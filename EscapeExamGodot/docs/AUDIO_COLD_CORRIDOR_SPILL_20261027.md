# 【光影音效交付】冷廊教师大衣剪影侧溢 · AUDIO-COLD-SPILL-20261027

- **任务 ID**：`AUDIO-COLD-SPILL-20261027`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-27（Asia/Shanghai）
- **分支**：`audio/cold-corridor-silhouette-spill`（独立 worktree `/workspace/wt-audio-spill`；**未**合并进 `feat/chapter01-module-pass`）
- **基准 Commit（feat tip，含 art `88e91f6`）**：`b6257f47e58bc3120616602822509ba9133f4114`（`b6257f4` · `merge: origin/art/chapter01-module-pass into feat/chapter01-module-pass`）
- **本交付 Commit**：`3b0100e73d71fc3b239edbfeb8c5af562a759689`（`3b0100e` · `audio: cold corridor Fixture5/7 silhouette spill`）
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）
- **约束遵守**：仅改 `Lighting/Fixture5` + `Lighting/Fixture7` Omni 参数；未重开 AlertIcon/SuspicionBar；未改教师 AI / 拓扑 / 玩家速度 / 解谜 / 非光照场景；未新增第 5 盏阴影 Omni。

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/scenes/chapters/chapter_01.tscn` | Fixture5 / Fixture7 Omni 参数 only |
| `EscapeExamGodot/tools/capture_audio_cold_spill_s02_s03.gd` | S02/S03 锁定机位截图夹具 |
| `EscapeExamGodot/tools/check_shadow_omni_budget.py` | 离线静态：Lighting 下阴影 Omni ≤5（期望 4） |
| `EscapeExamGodot/tools/check_shadow_omni_budget.gd` | 可选 Godot 侧同口径计数 |
| `EscapeExamGodot/evidence/screenshots/audio-cold-spill/s02-before.png` | before：复制 art `art-s02-after.png` |
| `EscapeExamGodot/evidence/screenshots/audio-cold-spill/s03-before.png` | before：复制 art `art-s03-after.png` |
| `EscapeExamGodot/evidence/screenshots/audio-cold-spill/s02-after.png` | after：本分支灯参 + 同 S02 pose |
| `EscapeExamGodot/evidence/screenshots/audio-cold-spill/s03-after.png` | after：本分支灯参 + 同 S03 pose |
| `EscapeExamGodot/evidence/logs/audio-cold-spill-capture.log` | xvfb 捕获引擎日志 |
| `EscapeExamGodot/docs/AUDIO_COLD_CORRIDOR_SPILL_20261027.md` | 本交付说明 |

**未改**：`teacher.gd` / Alert UI / ambience / broadcast / 地图拓扑 / Fixture0–4/6/8 / WorldEnvironment ambient。

---

## 2. Fixture5 / Fixture7 before → after

基准 = feat tip `b6257f4` 场景内既有值（此前 AUDIO 侧溢已合入 feat：energy 1.8/1.95 等）。本轮在 art 合并后仍报「大衣过暗」上再推一档。冷色族保持 ≈`(0.45, 0.7, 1.0)`（微提亮至 `(0.48, 0.72, 1.0)`）。

| Param | Fixture5 before → after | Fixture7 before → after |
|-------|-------------------------|-------------------------|
| `light_color` | `(0.45, 0.7, 1.0)` → **`(0.48, 0.72, 1.0)`** | 同左 |
| `light_energy` | `1.8` → **`2.45`**（~+36%） | `1.95` → **`2.5`**（~+28%） |
| `light_size` | `0.35` → **`0.45`** | `0.35` → **`0.45`** |
| `omni_range` | `6.3` → **`8.0`** | `6.0` → **`7.8`** |
| `omni_attenuation` | `1.1` → **`0.9`**（更软衰减 / 更远侧溢） | `1.1` → **`0.9`** |
| `shadow_enabled` | `true`（不变） | `true`（不变） |
| fixture `position` | `(2.7, 3.15, 1.2)` 未动 | `(6.4, 3.15, -2.5)` 未动 |
| Light local `position` | `(0, -0.25, 0)` 未动 | `(0, -0.25, 0)` 未动 |

**Shadow Omni 计数**：仍 **4**（Fixture0 / 5 / 6 / 7）≤5。未新增阴影灯。离线校验：`python3 tools/check_shadow_omni_budget.py` → `PASS … measured=4`。

---

## 3. 截图路径与捕获

| 文件 | 含义 |
|------|------|
| `evidence/screenshots/audio-cold-spill/s02-before.png` | art 基线（`art-review/art-s02-after.png` 复制） |
| `evidence/screenshots/audio-cold-spill/s02-after.png` | 本分支灯参 · S02 |
| `evidence/screenshots/audio-cold-spill/s03-before.png` | art 基线（`art-review/art-s03-after.png` 复制） |
| `evidence/screenshots/audio-cold-spill/s03-after.png` | 本分支灯参 · S03 |

机位（与 `docs/SHOT_BLOCKS_CHAPTER01.md` / `tools/capture_art_s01_s05.gd` 锁定一致）：

- **S02**：player `(-1.75, 0.04, 1.5)` look `(-1.75, 1.55, -1.0)` FOV 72；teacher `(-1.75, 0.02, -1.0)` suspicious
- **S03**：player `(-3.0, 0.04, 0.2)` crouch Head.y=0.94 look `(-1.75, 1.35, -1.0)` FOV 70

```bash
cd /workspace/wt-audio-spill
xvfb-run -a godot --path EscapeExamGodot \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_audio_cold_spill_s02_s03.gd
# → AUDIO_COLD_SPILL_CAPTURE_OK … shadow=4
```

**渲染注记**：box 无 Vulkan → 实际 `gl_compatibility` + llvmpipe。目标机可用 `forward_plus` 复跑同夹具。before 为 art 机位图（可能不同驱动），after 为同 pose 本环境捕获；**不可当作严格像素 A/B 性能/画质结论**。

盒上粗测（中心大衣 ROI 平均亮度，仅供参考、非验收）：S02 coat mean before/after 均极低；地板 ROI 略升（spill 到地面）。东侧 Fixture5/7 与西廊 S02 教师之间有服务核遮挡，侧溢对「纯西廊门口背光剪影」增益有限；对东廊/出口冷区与更近 F5 的读边更有效。未改 Fixture4（西廊无阴影填充，越权）。

---

## 4. 验收状态

| 项 | 状态 |
|----|------|
| 灯参 diff 仅 Fixture5/7 | 是 |
| Shadow Omni ≤5（实测 4） | 是（脚本 + 捕获日志） |
| S02/S03 after 已落盘 | 是 |
| **人眼联检（大衣轮廓可读 / 未爆地板）** | **未验证**（待 @美术 / 联机 forward+ 复检） |
| 耳感 / mix | 本任务无关；未声称 |
| 帧率 / perf | **未测**；无虚构 FPS |

---

## 5. 给 @游戏开发 的合入提示

1. 独立分支，请 cherry-pick / merge 灯参段进 `feat/chapter01-module-pass`，勿整文件覆盖非 Lighting 节点。
2. 合入后请 @美术 在 forward+ 下复看 S02/S03；若西廊门口仍糊，下一步候选是 **无阴影** Fixture4 微抬（需新任务授权），或教师东移取样点，**不要**为剪影加第 5 盏阴影 Omni。
3. 静态门禁可跑：`python3 EscapeExamGodot/tools/check_shadow_omni_budget.py`。

---

*AUDIO-COLD-SPILL-20261027 · 光影音效交付 · 人眼联检未验证*
