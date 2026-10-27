# 【光影音效交付】Alert-off 运行证据 · AUDIO-ALERT-OFF-EVIDENCE-20261027

- **任务 ID**：`AUDIO-ALERT-OFF-EVIDENCE-20261027`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-27（Asia/Shanghai）
- **分支**：`audio/alert-off-runtime-evidence`（独立 worktree `/workspace/wt-audio-evidence`；**未**合并进 `feat/chapter01-module-pass`）
- **基准 Commit（feat tip，本分支起点）**：`e13865320612d09263def9ec4808835c2d98bcd4`（`e138653` · `plot: chapter01 beats, fail-evidence task card, unlock A/B`）
- **本交付 Commit（证据内容）**：`4fbe9e62f94d59d19f7c3a5ac86d37b3d3a0ace7`（`4fbe9e6` · `audio: Alert-off runtime evidence probe + log`）；分支 tip 以 `git rev-parse audio/alert-off-runtime-evidence` 为准（可含本文 SHA 戳记提交）
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）
- **约束遵守**：未重开 AlertIcon/SuspicionBar；未改教师 AI 参数 / 房间拓扑 / 玩家速度 / 解谜通关；未整文件重写 `chapter_01.tscn` 非光照部分；本轮**仅新增**夹具脚本 + 证据日志/截图 + 本文档。

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/tests/audio_alert_off_evidence.gd` | SceneTree 运行夹具（镜像 `acceptance.gd` 风格） |
| `EscapeExamGodot/tests/audio_alert_off_evidence.gd.uid` | Godot UID |
| `EscapeExamGodot/evidence/logs/audio-alert-off-evidence.log` | PASS/FAIL 结构化日志 |
| `EscapeExamGodot/evidence/logs/audio-alert-off-engine.log` | 引擎 stdout/stderr 实录 |
| `EscapeExamGodot/evidence/screenshots/audio-alert-off/classroom-warm.png` | 教室暖窗 mood |
| `EscapeExamGodot/evidence/screenshots/audio-alert-off/corridor-cold.png` | 走廊冷光 mood |
| `EscapeExamGodot/evidence/screenshots/audio-alert-off/exit-pressure.png` | 出口压迫 mood |
| `EscapeExamGodot/docs/AUDIO_ALERT_OFF_RUNTIME_EVIDENCE.md` | 本交付说明 |

**未改**：`ambience.gd` / `broadcast.gd` / `teacher.gd` / `chapter_01.tscn` / AI / 拓扑（本任务为运行证据，不改玩法）。

---

## 2. 运行命令与结果

```bash
cd /workspace/wt-audio-evidence
xvfb-run -a godot --path EscapeExamGodot -s res://tests/audio_alert_off_evidence.gd
# process exit code: 0
```

**RESULT**：`passed=27 failed=0`

关键 PASS 行（摘自 `evidence/logs/audio-alert-off-evidence.log`）：

```
PASS AlertIcon.visible==false during patrol
PASS AlertIcon.visible==false during investigate
PASS AlertIcon.visible==false during suspicious
PASS AlertIcon.visible==false during chase
PASS true quiet: RainBed not playing after set_loop_quiet(true)
PASS true quiet: TubeHum not playing after set_loop_quiet(true)
PASS beds restart: RainBed.playing after set_loop_quiet(false)
PASS Ambient bus ducked (≤ -12 dB shortly after sting; got -22.00)
PASS Ambient bus restored toward 0 (≥ -6 dB; got 0.00)
PASS reset_audio_state: Ambient bus ~0 (got 0.00)
PASS ambience.gd does not call acoustics.emit_sound (static)
```

NOTE（总线采样）：

```
NOTE Ambient bus before sting = 0.00 dB
NOTE Ambient bus shortly after sting = -22.00 dB (target ~-22)
NOTE Ambient bus after restore window = 0.00 dB (target ~0)
NOTE shadow OmniLight3D count under Lighting = 4 (budget ≤5; expect 4)
NOTE human ear / playfeel: 未验证 (box headless/xvfb; no audio recording claimed)
```

---

## 3. 光照参数（引用既有 lookdev；本轮未改灯）

| 项 | 值 | 证据 |
|----|-----|------|
| Warm window hero | `Lighting/Fixture0` Color `(1.0, 0.72, 0.45)` energy `2.0` shadow on | 既有 `chapter_01.tscn` + 截图 `classroom-warm.png` |
| Cold exit heroes | `Fixture5` + `Fixture7` Color `(0.45, 0.7, 1.0)` shadow on | `corridor-cold.png` / `exit-pressure.png` |
| Shadow Omni 实测 | **4**（Fixture0/5/6/7）≤5 | 夹具 `count_shadow_omnis` NOTE |
| Ambient env | Color `(0.16, 0.18, 0.22)` energy `0.045` | 既有 scene（未改） |

截图机位（夹具 teleport）：

- classroom-warm：`(-8.9, 0.04, 1.7)` look `(-5.6, 1.5, -5.8)`（近 Fixture0 暖窗）
- corridor-cold：`(-1.75, 0.04, 1.5)` look `(2.7, 1.4, 1.2)`（向 Fixture5 冷廊）
- exit-pressure：`(5.8, 0.04, -2.6)` look `(7.0, 1.4, 1.5)`（近 Fixture7 / 出口压迫）

截图为 xvfb + llvmpipe OpenGL 兼容渲染；**不是**最终美术同机位验收片，仅作 mood / 灯色存在性运行证据。像素均值非纯黑（classroom≈25 / corridor≈20 / exit≈27 luma）。

---

## 4. 音频 Bus / 钩子（代码/夹具运行证据）

| 钩子 | 验证方式 | 结果 |
|------|----------|------|
| Buses Ambient / Sting | `AudioServer.get_bus_index` | PASS 存在 |
| 开局雨/hum 床 | `RainBed`/`TubeHum`.playing | PASS true |
| Broadcast true-quiet | `set_loop_quiet(true)` 后 `.playing==false`（非仅降音量） | PASS |
| 恢复床 | `set_loop_quiet(false)` 后 `.playing==true` | PASS |
| Catch sting duck | `play_catch_sting()` → Ambient ≈ **-22 dB** | PASS（采样 -22.00） |
| Duck 恢复 | ~1.0s+ 后 Ambient ≈ **0 dB** | PASS（采样 0.00） |
| `reset_audio_state` | 打断 duck 后 Ambient ~0、sting 不卡住 | PASS |
| 床不进 AI | `ambience.gd` 静态无 `emit_sound`；注释声明不进 acoustics | PASS |
| Alert UI 强制隐藏 | patrol / investigate / suspicious / chase + `_update_alert_ui` | PASS visible==false |

引擎侧：本 box **无 ALSA 声卡**，Godot 回退 **dummy audio driver** — 玩家状态与 bus dB 可断言；**波形/耳听不可断言**。

---

## 5. 截图证据 paths

- `EscapeExamGodot/evidence/screenshots/audio-alert-off/classroom-warm.png`
- `EscapeExamGodot/evidence/screenshots/audio-alert-off/corridor-cold.png`
- `EscapeExamGodot/evidence/screenshots/audio-alert-off/exit-pressure.png`

---

## 6. 实际耳听结果

**未验证**（box headless/xvfb + dummy audio；无录音、无真人试听）。

不得把 bus dB / `.playing` 夹具 PASS 写成「耳听通过」。

---

## 7. 性能

- Shadow Omni 实测：**4**（夹具计数；未测 FPS，不虚构帧率）。
- 渲染：OpenGL compatibility / llvmpipe（无 Vulkan surface）。

---

## 8. 未完成 / 交给 @游戏开发

1. **Human ear / playfeel** 未验证 — 需有声设备或导出包真人试听。
2. **与美术联合 S02/S03** 同机位验收（剪影侧溢光 / 冷廊轮廓）— 本轮 mood 图非正式 shot-block 终审。
3. **盲测**（无警戒 UI 补偿是否可读）— 见 `BLIND_PLAYTEST_PROTOCOL_v1.md`；本夹具不代替。
4. **合入**：请 @游戏开发 审 PR / cherry-pick 本分支进 `feat/chapter01-module-pass`；光影侧不自行 merge。

---

## 9. 复跑

```bash
git fetch origin
git worktree add /workspace/wt-audio-evidence -b audio/alert-off-runtime-evidence origin/audio/alert-off-runtime-evidence
# or checkout this branch
xvfb-run -a godot --path EscapeExamGodot -s res://tests/audio_alert_off_evidence.gd
# expect: RESULT passed=27 failed=0 ; exit 0
# log: EscapeExamGodot/evidence/logs/audio-alert-off-evidence.log
```
