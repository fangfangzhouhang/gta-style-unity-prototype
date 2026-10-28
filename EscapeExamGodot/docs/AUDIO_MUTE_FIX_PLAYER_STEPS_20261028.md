# 【光影音效交付】Mute 排查硬化 + 玩家脚步 −16 基线 · AUDIO-MUTE-FIX-PLAYER-STEPS-20261028

- **任务 ID**：`AUDIO-MUTE-FIX-PLAYER-STEPS-20261028`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-28（Asia/Shanghai）
- **分支**：`audio/mute-fix-player-steps`（worktree `/workspace/wt-audio-bed-door`；**未**合 main）
- **基线 tip**：`55d491f`（含 `2344404` 脚步 −16）（`origin/feat/chapter01-module-pass`；含 游戏开发 玩家脚步 −11→−16）
- **本交付 Commit（代码）**：见分支 tip（本文提交后 `git rev-parse audio/mute-fix-player-steps`）
- **Godot**：4.7.2 stable
- **约束遵守**：未改 AI / 拓扑 / 解谜 / Alert UI / 教师距离曲线；床仍只走 Ambient、**不**进 acoustics AI；**未再改**玩家脚步 −16 行（已由 2344404 落地）。
- **状态**：代码硬化完成 · **不报人耳 PASS**（box headless/dummy audio；待 ark Windows `play.cmd` 复测）

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/scripts/systems/ambience.gd` | 床/刺音 `PROCESS_MODE_ALWAYS`；`_clear_bus_mute_flags`；`reset_audio_state` 清 mute + 抬 Ambient=0 |
| `EscapeExamGodot/scripts/systems/game.gd` | `enter_b0` 再调 `Ambience.reset_audio_state`；`load_settings` Master ≤−24 视为误存→0，clamp 下限 −24 |
| `EscapeExamGodot/scripts/systems/acoustics.gd` | door 软化 −4/−6 → **−2/−3**（仍软于未软化）；**不改** player −16、不改 teacher 曲线 |
| `EscapeExamGodot/assets/audio/rain_bed.wav.import` | `compress/mode` 2→**0**（PCM 长循环） |
| `EscapeExamGodot/assets/audio/tube_hum.wav.import` | 同上 |
| `EscapeExamGodot/docs/AUDIO_MUTE_FIX_PLAYER_STEPS_20261028.md` | 本交付 |
| `EscapeExamGodot/docs/AUDIO_LIGHTING_AGENT_CONTEXT_20261028.md` | 下任 agent 交接 |

**未改**：教师 AI、Omni 灯、cabinet/gate/prologue 运动、Alert UI、Unity Assets、玩家脚步公式（保持 −16）。

---

## 2. Root cause（已验证假设 · 非设备终局结论）

| 检查项 | 结果 |
|--------|------|
| `default_bus_layout.tres` | Music/Ambient/SFX/Sting → Master；mute=false；volume=0 |
| Master/SFX「全死」？ | **否** — ark 能大声听到玩家脚步 ⇒ Master+SFX 通路活着 |
| Ambient 床代码 | `Ambience` autoload；`BED_RAIN_DB=-9` / `BED_HUM_DB=-13`；不调 `emit_sound` |
| 门/闸/notice 发射 | `door.gd` `set_open` / unlock 调 `emit_sound`；gate/notice 同通道 |
| WAV | 源峰值正常；床 import 原为 IMA-ADPCM（mode=2）长循环 — Windows 风险点，本轮改 PCM |
| `enter_b0`（跳过前情后） | **原先不** `reset_audio_state` → Ambient 可能留在 duck −22；本轮补调用 |
| `user://settings.cfg` via `试玩.cmd` `APPDATA=local_data` | Volume 滑条 min=−30；误拖到 −30 时床/环境几乎死、瞬态脚步仍可能「听得到」 |
| 相对响度 | 历史玩家脚步仅 −7/−11；door 曾额外 −4 且上限 −6；连续床源 ≈−25 dBFS → 主观「无 SFX/无氛围」 |

**结论（代码侧）**：不是「SFX bus 静音」单因；是 **相对混音失衡（脚步盖过）+ 可选 Master 误存过低 + Ambient duck/进 B0 未复位 + 长循环 ADPCM 风险**。设备/声卡仍须 ark 机上核对（见下）。

---

## 3. Before → After

| 项 | Before（feat ≤55d491f / 2344404 音频） | After（本分支） |
|----|------------------------|-----------------|
| 玩家脚步 offset | **−16**（游戏开发） | **−16 不变** |
| door 额外衰减 / 上限 | −4 / clamp max −6 | **−2 / clamp max −3** |
| teacher 距离曲线 | 保持 | **不变** |
| beds dB | rain −9 / hum −13 | **不变** |
| `enter_b0` | 无 Ambience reset | **`reset_audio_state()`** |
| Master load | clamp [−30,0] | ≤−24→0；clamp [−24,0] |
| 床 WAV compress | mode=2 | **mode=0** |
| Ambient/SFX mute flags | 未强制清 | **`_clear_bus_mute_flags`** |

---

## 4. 给 ark 机上核对（Windows）

1. 删或检查 `D:\game\EscapeExamGodot\local_data\**\settings.cfg` 里 `audio/volume`（若 ≤−24 本版会忽略并按 0）。
2. `试玩.cmd` / Forward+；新开局或 Skip 前情 → B0：应闻雨/灯管床（Ambient），开门应有门轴声。
3. 玩家脚步应明显低于改 −16 前；**勿**把 box 机检写成耳听 PASS。
4. 若仍无床：Audio 面板确认 Ambient 未 mute、Master≈0；Godot 输出是否 dummy/设备切换。

---

## 5. 验收

```bash
cd /workspace/wt-audio-bed-door
godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd
```

人耳：**未验证**（≠ ear PASS）。
