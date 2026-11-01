#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
gen_v2_audio.py — 《逃离考场》V2 程序化音频素材生成（纯 Python stdlib）

只用 wave / struct / math / random，不依赖 numpy 或任何第三方采样。
全部素材为本项目原创合成，无外部采样、无模型权重，许可状态见 ASSET_CREDITS.md。

约定：22050 Hz / 单声道 / 16-bit PCM（与既有 SFX step_hard/bell 同规格）。
响度克制：所有文件峰值归一到 <= 0.50（约 -6 dBFS），给混音留头room；
循环素材靠"整数周期 / 环绕衰减 / 首尾交叉淡化"保证无缝，不靠听运气。

生成文件（写入 ../assets/audio/）：
  heartbeat.wav      心跳 lub-dub 两拍 one-shot（约 0.85 s）
                     两下低频正弦衰减包络：lub 58 Hz / 0.22 s，间隔 0.28 s 后
                     dub 52 Hz / 0.18 s、幅度 0.7；AdaptiveAudio 按 BPM 计时重播。
  keys.wav           钥匙串轻晃（约 0.38 s）：5 次短金属 ping（3.1–6.2 kHz
                     谐振正弦，指数衰减 30–60 ms）+ 高通短噪声擦声，间隔 45–80 ms。
  pulse_loop.wav     追击脉冲循环（4.0 s，无缝）：55 Hz 正弦脉冲，每 1 s 一拍，
                     包络在 1 s 内衰完且拍数整除时长，循环点天然对齐；另叠
                     110 Hz 弱谐波（0.25 幅度）。
  tension_strings.wav 弦乐擦弦张力循环（8.0 s，无缝）：带通噪声（双一阶滤波，
                     中心约 900 Hz / 2.4 kHz 两层）+ 0.25 Hz 慢颤音（时长内
                     恰 2 个周期）；噪声用最后 1.5 s 与开头交叉淡化做环绕无缝。
  drone_loop.wav     低频氛围嗡鸣（8.0 s，无缝）：55 / 82.5 / 110 / 165 Hz 正弦
                     叠加（均为 0.125 Hz 的整数倍，8 s 内整周期）+ 0.125 Hz
                     慢呼吸调幅（恰 1 周期），相位闭合即无缝。

用法：python3 tools/gen_v2_audio.py [--out assets/audio]
重跑是确定性的（random 固定种子 20261102）。
"""

import math
import os
import random
import struct
import sys
import wave

SR = 22050
PEAK = 0.50  # -6 dBFS 头room


def write_wav(path, samples):
    frames = bytearray()
    for s in samples:
        v = int(max(-1.0, min(1.0, s)) * 32767)
        frames += struct.pack("<h", v)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(bytes(frames))
    print("wrote %s (%.2f s, %d samples)" % (path, len(samples) / SR, len(samples)))


def normalize(samples, peak=PEAK):
    m = max((abs(s) for s in samples), default=0.0)
    if m <= 0:
        return samples
    g = peak / m
    return [s * g for s in samples]


def heartbeat():
    dur = 0.85
    n = int(SR * dur)
    out = [0.0] * n

    def thump(start, freq, length, amp):
        ln = int(SR * length)
        for i in range(ln):
            idx = start + i
            if idx >= n:
                break
            t = i / SR
            env = math.exp(-t * 14.0) * (1.0 - math.exp(-t * 90.0))
            out[idx] += amp * env * math.sin(2.0 * math.pi * freq * t)

    thump(0, 58.0, 0.30, 1.0)               # lub
    thump(int(SR * 0.28), 52.0, 0.26, 0.7)  # dub
    return normalize(out, 0.55)


def keys():
    rng = random.Random(20261102)
    dur = 0.38
    n = int(SR * dur)
    out = [0.0] * n
    t0 = 0.0
    for k in range(5):
        freq = rng.uniform(3100.0, 6200.0)
        freq2 = freq * rng.uniform(1.48, 1.53)
        length = rng.uniform(0.03, 0.06)
        ln = int(SR * length)
        start = int(SR * t0)
        amp = rng.uniform(0.45, 0.8)
        for i in range(ln):
            idx = start + i
            if idx >= n:
                break
            t = i / SR
            env = math.exp(-t / (length * 0.35))
            out[idx] += amp * env * (math.sin(2 * math.pi * freq * t)
                                     + 0.5 * math.sin(2 * math.pi * freq2 * t))
            # 金属擦声：短高频噪声，同包络
            out[idx] += 0.35 * amp * env * (rng.random() * 2.0 - 1.0)
        t0 += rng.uniform(0.045, 0.08)
    return normalize(out, 0.42)


def pulse_loop():
    dur = 4.0
    n = int(SR * dur)
    out = [0.0] * n
    beat = 1.0  # 每 1 s 一拍，4 拍整除 4 s 时长
    for i in range(n):
        t = i / SR
        lt = math.fmod(t, beat)
        env = math.exp(-lt * 7.0) * (1.0 - math.exp(-lt * 60.0))
        out[i] = env * (math.sin(2 * math.pi * 55.0 * t)
                        + 0.25 * math.sin(2 * math.pi * 110.0 * t)
                        + 0.08 * math.sin(2 * math.pi * 165.0 * t))
    return normalize(out, 0.5)


class OnePoleBP:
    """极简谐振带通（双一阶近似），够本项目噪声塑形用。"""

    def __init__(self, freq, q=6.0):
        w = 2.0 * math.pi * freq / SR
        self.alpha = math.sin(w) / (2.0 * q)
        self.cosw = math.cos(w)
        self.b0 = self.alpha
        self.b2 = -self.alpha
        self.a1 = -2.0 * self.cosw
        self.a2 = 1.0 - self.alpha
        self.x1 = self.x2 = self.y1 = self.y2 = 0.0

    def step(self, x):
        y = (self.b0 * x + self.b2 * self.x2
             - self.a1 * self.y1 - self.a2 * self.y2) / (1.0 + self.alpha)
        self.x2, self.x1 = self.x1, x
        self.y2, self.y1 = self.y1, y
        return y


def loop_crossfade(samples, fade_s=1.5):
    """标准交叉淡化循环：输出长度 = 原长 - fade；头部由原尾部淡出+原头部淡入
    混合而成，文件末尾与头部在原素材中本就相邻，循环点连续。"""
    f = int(SR * fade_s)
    n = len(samples)
    out = list(samples[: n - f])
    for i in range(f):
        w = i / f
        out[i] = samples[i] * w + samples[n - f + i] * (1.0 - w)
    return out


def tension_strings():
    rng = random.Random(7402)
    dur = 9.5  # 多生成 1.5 s 供 loop_crossfade 截断，最终恰 8.0 s
    n = int(SR * dur)
    bp1 = OnePoleBP(900.0, 7.0)
    bp2 = OnePoleBP(2400.0, 9.0)
    out = [0.0] * n
    for i in range(n):
        t = i / SR
        noise = rng.random() * 2.0 - 1.0
        # 0.25 Hz 慢颤音：8 s 内恰 2 周期，循环点相位闭合
        trem = 0.72 + 0.28 * math.sin(2 * math.pi * 0.25 * t - math.pi / 2)
        bow = bp1.step(noise) * 1.6 + bp2.step(noise) * 0.9
        out[i] = bow * trem
    out = loop_crossfade(out, 1.5)
    return normalize(out, 0.34)


def drone_loop():
    dur = 8.0
    n = int(SR * dur)
    out = [0.0] * n
    # 频率均为 0.125 Hz 整数倍 → 8 s 内整周期，相位闭合
    parts = [(55.0, 1.0), (82.5, 0.45), (110.0, 0.55), (165.0, 0.18)]
    for i in range(n):
        t = i / SR
        breath = 0.82 + 0.18 * math.sin(2 * math.pi * 0.125 * t - math.pi / 2)
        s = 0.0
        for f, a in parts:
            s += a * math.sin(2 * math.pi * f * t)
        out[i] = s * breath
    return normalize(out, 0.4)


def main():
    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "assets", "audio")
    if "--out" in sys.argv:
        out_dir = sys.argv[sys.argv.index("--out") + 1]
    out_dir = os.path.normpath(out_dir)
    os.makedirs(out_dir, exist_ok=True)
    write_wav(os.path.join(out_dir, "heartbeat.wav"), heartbeat())
    write_wav(os.path.join(out_dir, "keys.wav"), keys())
    write_wav(os.path.join(out_dir, "pulse_loop.wav"), pulse_loop())
    write_wav(os.path.join(out_dir, "tension_strings.wav"), tension_strings())
    write_wav(os.path.join(out_dir, "drone_loop.wav"), drone_loop())


if __name__ == "__main__":
    main()
