# 【光影音效交付】恐怖氛围 Ambient 床抬高 + 门声软化 + 教师脚步近大远小 · AUDIO-HORROR-BED-DOOR-SOFT-20261028

- **任务 ID**：`AUDIO-HORROR-BED-DOOR-SOFT-20261028`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-28（Asia/Shanghai）
- **分支**：`audio/horror-bed-door-soft`（worktree `/workspace/wt-audio-bed-door`；**未**合并进 `feat/chapter01-module-pass` / main）
- **基线 tip**：`c93930a2944bb74dc33b9e45fda6accabf3f1017`（`origin/feat/chapter01-module-pass`；≈先前口述 tip `1af8e06` 之后的 revert 恢复 tip）
- **本交付 Commit（代码）**：`d05dc66530f2b4504cbd557341f7433935980ef7`（`d05dc66`）；分支 tip 以 `git rev-parse audio/horror-bed-door-soft` / 远程 tip 为准
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）
- **约束遵守**：未改光照 / 教师 AI 参数 / 房间拓扑 / Alert UI / 速度与探测；Ambient 床仍只走 `Ambient` bus，**不**进 AI 听力；教师脚步走既有 `Game.acoustics.emit_sound`（SFX），`source_id=teacher` 故不触发自身 `hear_noise`；未抬玩家脚步 / tick / Sting / Master。
- **状态**：代码完成 · acceptance 机检见下 · **不报人耳 PASS**（box headless；无录音、无真人试听）

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/scripts/systems/ambience.gd` | `BED_RAIN_DB` / `BED_HUM_DB` 抬高；恢复路径用常量 |
| `EscapeExamGodot/scripts/systems/acoustics.gd` | door 软化；`teacher_step` / `teacher_step_soft` 近大远小曲线 |
| `EscapeExamGodot/scripts/props/door.gd` | 铰链 tween 放缓 |
| `EscapeExamGodot/scripts/actors/teacher.gd` | 脚步 kind/reach/步距仅音频侧（chase→hard，其它→soft） |
| `EscapeExamGodot/docs/AUDIO_HORROR_BED_DOOR_SOFT_20261028.md` | 本交付说明 |

**未改**：`teacher` 巡逻/追逐速度、FOV、sight、suspicion、NavMesh、Unlock/wedge 逻辑（仅门开合动画时长）、玩家脚步公式、Sting、Master、Lighting。

---

## 2. Before → After（精确数值）

### 2.1 Ambient 恐怖氛围床（非 Music 轨；雨 + 灯管 hum）

| 常量 | Before | After | Δ |
|------|--------|-------|---|
| `BED_RAIN_DB` | `-14.0` | **`-9.0`** | **+5.0 dB** |
| `BED_HUM_DB` | `-18.0` | **`-13.0`** | **+5.0 dB** |
| Bus | `Ambient` | `Ambient` | 不变 |
| AI noise | 否 | 否 | 床仍不调 `emit_sound` |

恢复路径（均读常量）：`_ready`、`set_loop_quiet(false)` → `_tween_beds(BED_*)`、`reset_audio_state` 非 quiet 分支。

### 2.2 门声（`kind == "door"`）

| 项 | Before | After |
|----|--------|-------|
| Door 额外 | （无） | **`volume_db -= 4.0`**，再 clamp **`[-32, -6]`** |
| `pitch_scale` | `1.0` | **`0.94`** |
| hinge tween | `0.45` s | **`0.60` s**（+0.15 s） |

### 2.3 教师脚步近大远小（SFX · 既有 emit_sound 通道）

| 项 | Before | After |
|----|--------|-------|
| 发射 | `emit_sound(..., "teacher_step", 17, ...)` 每移动 1.0 m | 同通道；chase 步距 **0.85** m / 其它 **1.0** m |
| Clip | 一律 `step_hard.wav` | chase → **`teacher_step`** = hard；其它态 → **`teacher_step_soft`** = soft |
| Reach（仅玩家听距；`source_id=teacher` 不进 AI） | `17` | chase **`20`** / 其它 **`15`** |
| 玩家听音量 | 通用 `clamp(-cost*0.55 − muffled, -28, -2)` | **距离曲线覆盖**（见下） |
| Bus | `SFX` | `SFX` |
| 3D | `unit_size=5` `max_distance=45` | teacher steps：`unit_size=6.5` `max_distance=40` |

**衰减曲线（相对玩家听点 = camera / 必要时 portal `audio_origin`）**：

```
dist_m = listener.distance_to(sound.global_position)
t = clamp((dist_m - 0.75) / 17.25, 0, 1)   # 0.75 m → 18 m
volume_db = lerp(-2.0, -26.0, t)
if muffled: volume_db -= 6.0
if kind == "teacher_step": volume_db += 1.5   # chase/hard 略抬
volume_db = clamp(volume_db, -30.0, -1.0)
```

| 距离（约） | 未闷 | chase(+1.5) 约 |
|------------|------|----------------|
| 0.75 m | −2 dB | −1 dB（clamp 上限 −1） |
| ~5 m | ~−8 dB | ~−6.5 dB |
| ~10 m | ~−15 dB | ~−13.5 dB |
| 18 m+ | −26 dB（−32 闷） | −24.5 dB |

仍经 `audible_path` 门廊；`cost >= reach*1.4` 早退不播。玩家脚步 / tick / notice / broadcast **未改**通用公式。

---

## 3. 验收

```bash
cd /workspace/wt-audio-bed-door
godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd
# 需已有 EscapeExamGodot/.godot/imported（gitignore；可从其它 worktree 拷贝）
```

**RESULT**：`passed=48 failed=0`（exit 0；含教师脚步改动后复跑）

人耳 / 氛围听感：**未验证**（headless + dummy audio；**≠ ear PASS**）。

---

## 4. 给 ark 的一句话（中文草稿）

【光影音效交付】分支 `audio/horror-bed-door-soft`：Ambient 恐怖氛围床抬高（雨 −14→−9、hum −18→−13 dB，仍只走 Ambient、不进 AI）；开门软化（door −4 dB/上限 −6、pitch 0.94、铰链 0.45→0.60s）；教师脚步走既有 acoustics SFX，距离近大远小（约 0.75 m→−2 dB / 18 m→−26 dB，关门再 −6；chase 用 hard 略抬、其它 soft），未改 AI/速度/拓扑。机检 48/48；**人耳听感未 PASS**，待真机试听确认氛围与逼近脚步。
