# 【光影音效交付】恐怖氛围 Ambient 床抬高 + 门声软化 · AUDIO-HORROR-BED-DOOR-SOFT-20261028

- **任务 ID**：`AUDIO-HORROR-BED-DOOR-SOFT-20261028`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-28（Asia/Shanghai）
- **分支**：`audio/horror-bed-door-soft`（worktree `/workspace/wt-audio-bed-door`；**未**合并进 `feat/chapter01-module-pass` / main）
- **基线 tip**：`c93930a2944bb74dc33b9e45fda6accabf3f1017`（`origin/feat/chapter01-module-pass`；≈先前口述 tip `1af8e06` 之后的 revert 恢复 tip）
- **本交付 Commit**：（）
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）
- **约束遵守**：未改光照 / 教师 AI / 房间拓扑 / Alert UI；Ambient 床仍只走 `Ambient` bus，**不**进 `acoustics.emit_sound` / AI 听力；未抬脚步 / tick / free-roam SFX / Sting / Master。
- **状态**：代码完成 · acceptance 机检见下 · **不报人耳 PASS**（box headless；无录音、无真人试听）

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/scripts/systems/ambience.gd` | `BED_RAIN_DB` / `BED_HUM_DB` 抬高；所有恢复路径已用常量 |
| `EscapeExamGodot/scripts/systems/acoustics.gd` | `kind == "door"` 额外衰减 + `pitch_scale` |
| `EscapeExamGodot/scripts/props/door.gd` | 铰链 tween 时长放缓 |
| `EscapeExamGodot/docs/AUDIO_HORROR_BED_DOOR_SOFT_20261028.md` | 本交付说明 |

**未改**：`teacher.gd` / Unlock / wedge 逻辑（仅门开合动画时长）/ step·tick 音量 / Sting / Master / Lighting / NavMesh。

---

## 2. Before → After（精确数值）

### 2.1 Ambient 恐怖氛围床（非 Music 轨；雨 + 灯管 hum）

| 常量 | Before | After | Δ |
|------|--------|-------|---|
| `BED_RAIN_DB` | `-14.0` | **`-9.0`** | **+5.0 dB** |
| `BED_HUM_DB` | `-18.0` | **`-13.0`** | **+5.0 dB** |
| Bus | `Ambient` | `Ambient` | 不变 |
| AI noise | 否 | 否 | 床仍不调 `emit_sound` |

恢复路径（均读常量，无硬编码旧 dB）：`_ready` 初值、`set_loop_quiet(false)` → `_tween_beds(BED_*)`、`reset_audio_state` 非 quiet 分支。

### 2.2 门声（`kind == "door"`，含开/关；同 clip 的 hatch/wedge/rack 一并软化）

| 项 | Before | After |
|----|--------|-------|
| 基础 clamp | `[-28, -2]` | 同左，再对 door 处理 |
| Door 额外 | （无） | **`volume_db -= 4.0`**，再 clamp **`[-32, -6]`** |
| `pitch_scale` | `1.0`（默认） | **`0.94`** |
| 非 door kinds | 不变 | 脚步 / tick / gate / notice / broadcast **未动** |

### 2.3 门铰链动画

| 项 | Before | After | Δ |
|----|--------|-------|---|
| `door.gd` hinge tween | `0.45` s | **`0.60` s** | **+0.15 s** |

Unlock / AI `request_teacher_open` 阈值门槛 / wedge 逻辑未改（仅 `set_open` 动画时长）。

---

## 3. 验收

```bash
cd /workspace/wt-audio-bed-door
godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd
```

**RESULT**：`passed=48 failed=0`（exit 0）

人耳 / 氛围听感：**未验证**（headless + dummy audio；**≠ ear PASS**）。

---

## 4. 给 ark 的一句话（中文草稿）

【光影音效交付】已在分支 `audio/horror-bed-door-soft` 抬高 Ambient 恐怖氛围床（雨 −14→−9 dB、灯管 hum −18→−13 dB，仍只走 Ambient、不进 AI 听力），并把开门声软化（door kind 额外 −4 dB、上限 −6、pitch 0.94）且铰链 tween 0.45→0.60s；未动脚步/tick/Sting/Master/光照/AI。机检 acceptance 见本文；**人耳听感未 PASS**，待真机试听确认氛围是否够烘托、门声是否仍刺耳。
