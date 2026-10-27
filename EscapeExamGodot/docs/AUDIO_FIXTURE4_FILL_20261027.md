# 【光影音效交付】西廊 Fixture4 无阴影填充抬升 · AUDIO-FIXTURE4-FILL-20261027

- **任务 ID**：`AUDIO-FIXTURE4-FILL-20261027`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-27（Asia/Shanghai）
- **分支**：`audio/fixture4-west-fill`（独立 worktree `/workspace/wt-audio-f4`；**未**合并进 `feat/chapter01-module-pass`）
- **基准 Commit（feat tip）**：`60fd24a6b0abcf5716964873e2b77c02d662df84`（`60fd24a` · `docs: stamp AUDIO-COLD-SPILL delivery commit SHA`）
- **本交付 Commit**：`6ddedaa949d29ba7561919bdbbdf1c50e6059ebe`（`6ddedaa` · `audio: Fixture4 west-corridor no-shadow fill bump`）
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）
- **约束遵守**：仅改 `Lighting/Fixture4` Omni 参数；`shadow_enabled=false` 保持；未新增第 5 盏阴影 Omni；未重开 AlertIcon/SuspicionBar；未改教师 AI / 拓扑 / 玩家 / 解谜 / 非光照场景；**未改** ambient / WorldEnvironment。

---

## 0. 西廊遮挡与杠杆选择（rationale）

S02 西廊门口教师大衣剪影联检仍失败：东侧冷英雄灯 **Fixture5 / Fixture7** 到西廊视点被 **服务核（service core）** 几何遮挡，冷侧溢对「纯西廊门口背光剪影」增益有限（见 `AUDIO_COLD_CORRIDOR_SPILL_20261027`）。美术+QA 约定下一杠杆：**Fixture4 无阴影填充抬升**——Fixture4 位于西廊口上方 `(-1.7, 3.15, 1.3)`，与 S02 教师 `(-1.75, ·, -1.0)` 同轴近距，可作冷邻填充 rim，**禁止**为剪影加第 5 盏阴影 Omni。

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/scenes/chapters/chapter_01.tscn` | **仅** `Lighting/Fixture4/Light` Omni 参数 |
| `EscapeExamGodot/tools/capture_audio_fixture4_fill_s02_s03.gd` | S02/S03 锁定机位截图夹具（同 spill pose） |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-fill/s02-before.png` | before：复制 cold-spill `s02-after.png`（feat tip 联检失败态） |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-fill/s03-before.png` | before：复制 cold-spill `s03-after.png` |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-fill/s02-after.png` | after：本分支 Fixture4 + 同 S02 pose |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-fill/s03-after.png` | after：本分支 Fixture4 + 同 S03 pose |
| `EscapeExamGodot/evidence/logs/audio-fixture4-fill-capture.log` | xvfb 捕获引擎日志 |
| `EscapeExamGodot/docs/AUDIO_FIXTURE4_FILL_20261027.md` | 本交付说明 |

**未改**：Fixture0–3/5–8 / FixtureS / Sun / ambient / teacher.gd / Alert UI / ambience / 地图拓扑。

---

## 2. Fixture4 before → after

Fixture 父节点 `position = (-1.7, 3.15, 1.3)` **未动**；Light local `position = (0, -0.25, 0)` **未动**。

| Param | before（feat `60fd24a`） | after |
|-------|--------------------------|-------|
| `light_color` | `(0.55, 0.62, 0.7)` 中性冷灰 | **`(0.48, 0.66, 0.88)`** 冷邻填充（西廊 rim） |
| `light_energy` | `0.22` | **`0.55`**（~+150%；仍远低于 F5 hero `2.45`，近于教室填充 F1 `0.42`） |
| `light_size` | `0.3` | **`0.38`**（略软高光） |
| `omni_range` | `3.2`（刚够 ~3 m 到大衣，衰减后近零） | **`5.4`** |
| `omni_attenuation` | `1.6`（陡） | **`1.15`**（更软侧溢） |
| `shadow_enabled` | `false` | **`false`**（不变 · fill tube） |

**Shadow Omni 计数**：仍 **4**（Fixture0 / 5 / 6 / 7）≤5。未新增阴影灯。离线校验：`python3 tools/check_shadow_omni_budget.py` → `PASS … measured=4`。捕获日志：`AUDIO_FIXTURE4_FILL_CAPTURE_OK … shadow=4 f4_shadow=false`。

**Ambient**：未改（仍 `ambient_light_energy = 0.045`）。

---

## 3. 截图路径与捕获

| 文件 | 含义 |
|------|------|
| `evidence/screenshots/audio-fixture4-fill/s02-before.png` | cold-spill after 基线（西廊仍失败联检） |
| `evidence/screenshots/audio-fixture4-fill/s02-after.png` | Fixture4 fill bump · S02 |
| `evidence/screenshots/audio-fixture4-fill/s03-before.png` | cold-spill after 基线 |
| `evidence/screenshots/audio-fixture4-fill/s03-after.png` | Fixture4 fill bump · S03 |

机位（与 `docs/SHOT_BLOCKS_CHAPTER01.md` / cold-spill 夹具一致）：

- **S02**：player `(-1.75, 0.04, 1.5)` look `(-1.75, 1.55, -1.0)` FOV 72；teacher `(-1.75, 0.02, -1.0)` suspicious
- **S03**：player `(-3.0, 0.04, 0.2)` crouch Head.y=0.94 look `(-1.75, 1.35, -1.0)` FOV 70

```bash
cd /workspace/wt-audio-f4/EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_audio_fixture4_fill_s02_s03.gd
# → AUDIO_FIXTURE4_FILL_CAPTURE_OK … shadow=4 f4_shadow=false
```

**渲染注记**：box 无 Vulkan → 实际 `gl_compatibility` + llvmpipe。目标机可用 `forward_plus` 复跑同夹具。before 为 cold-spill 同环境捕获复制；after 为同 pose 本分支捕获。

盒上粗测（中心大衣 ROI / 地板 ROI 平均亮度 0–255，**仅供参考、非验收**）：

| Shot | coat ROI before → after | floor ROI before → after |
|------|-------------------------|--------------------------|
| S02 | 16.3 → 34.7 | 1.8 → 5.8 |
| S03 | 7.8 → 25.5 | 1.8 → 5.9 |

大衣区明显抬亮；地板仍极暗（无 bloom 迹象）。教室暖窗侧未动 Fixture2。

---

## 4. 验收状态

| 项 | 状态 |
|----|------|
| 灯参 diff 仅 Fixture4 | 是 |
| Fixture4 `shadow_enabled=false` | 是 |
| Shadow Omni ≤5（实测 **4**） | 是（脚本 + 捕获日志） |
| S02/S03 after 已落盘 | 是 |
| **人眼联检（大衣轮廓可读 / 未爆地板 / 教室暖不漂）** | **未验证**（待 @美术 / 联机 forward+ 复检） |
| 耳感 / mix | 本任务无关；未声称 |
| 帧率 / perf | **未测**；无虚构 FPS |

---

## 5. 给 @游戏开发 的合入提示

1. 独立分支，请 cherry-pick / merge **仅** `Lighting/Fixture4/Light` 灯参段进 `feat/chapter01-module-pass`，勿整文件覆盖非 Lighting 节点。
2. 合入后请 @美术 在 forward+ 下复看 S02/S03 西廊大衣剪影；若仍软，下一候选需新任务授权（仍勿加第 5 盏阴影 Omni）。
3. 静态门禁：`python3 EscapeExamGodot/tools/check_shadow_omni_budget.py`。

---

*AUDIO-FIXTURE4-FILL-20261027 · 光影音效交付 · 人眼联检未验证*
