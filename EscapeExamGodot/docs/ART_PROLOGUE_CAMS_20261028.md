# 【美术交付】前情 P01–P09 定稿机位 · ART-PROLOGUE-CAMS

- **任务 ID**：`ART-PROLOGUE-CAMS`
- **分支**：`art/chapter01-module-pass`（worktree `/workspace/wt-art/EscapeExamGodot`）
- **基线 tip**：`73611ba`（全长 `73611ba0a37d2cecb7ec6a4e9351b816019370a2`）· art tip 祖先含此 SHA（merge `73611ba` → art 后再改机位）
- **本交付 tip**：`d73f56e`（全长 `d73f56ef0f6936b5dc5d647645b8fa2d4f6da7e0`）· message 含 `ART-PROLOGUE-CAMS`
- **日期**：2026-10-28（Asia/Shanghai）
- **镜头真源**：`docs/ART_PROLOGUE_CINEMATIC_20261028.md`（V2）
- **驱动**：`scripts/systems/prologue.gd` → `const SHOTS`（`PrologueCamera` 运行时 `eye` / `look_at` / `fov`；非 AnimationPlayer / Marker3D）
- **状态**：占位机位已换成教室几何对齐的定稿姿态 · 盒端 gl_compat 证据已拍 · **不报视觉 PASS**

---

## 1. 每镜改动摘要

| ID | 意图 | eye | look | FOV | 相对占位的关键修正 |
| --- | --- | --- | --- | --- | --- |
| **P01** ★ | 窗外大雨 | `(-8.75, 1.58, -5.35)` | `(-15.5, 1.05, -5.35)` | **58** | 占位瞄西墙实缝 z≈-2.75；改穿窗洞 z≈-5.4，露铝框+窗外树球 |
| **P02** | 雨窗→倒影 | `(-8.55, 1.56, -4.95)` | `(-9.85, 1.40, -3.80)` | **63** | 同窗轴微横移，看向玻璃内课桌方向 |
| **P03** ★ | 校牌/线索 | `(-1.95, 1.68, -0.55)` | `(-2.88, 2.08, -0.50)` | **48** | 对准 `WClockLabel`「考场←」（廊侧可读）；占位曾瞄柜体虚空 |
| **P04** | 规章笔迹 | `(-7.55, 1.35, -6.90)` | `(-7.80, 1.35, -7.73)` | **44** | 对准 `ExamSheet` 终场须知；占位曾瞄课桌纸 |
| **P05** | 卷面印章 | `(-8.30, 1.38, -2.30)` | `(-8.50, 0.78, -3.05)` | **47** | Desk00/`Paper00` 俯角（微调） |
| **P06** | 讲台空场 | `(-6.50, 1.62, -1.35)` | `(-6.50, 1.55, -6.55)` | **72** | 走道中景→讲台；教师 frozen 剪影 |
| **P07** ★ | 监考微笑 | `(-6.50, 1.78, -5.05)` | `(-6.50, 2.02, -6.34)` | **38** | look 抬至头/眼镜≈y2.02（占位 y1.78 打胸）；非跳脸缓近景 |
| **P08** | 铃未响 | `(-6.05, 1.40, -5.45)` | `(-6.50, 0.95, -6.65)` | **52** | `ExamTimer` 未响中近景 |
| **P09** | 进 B0 | `(-4.25, 1.55, -0.55)` | `(-3.05, 1.42, 1.10)` | **74** | 低柜后→考场门；FOV 近玩法默认 |

VO 九句 **未改**（与 `docs/design/PROLOGUE_VO_FINAL_20261028.md` 一致）。时长仍在 V2 建议窗内。

教师摆拍（仅视觉）：仍 `(-6.5, 0.04, -6.45)` · yaw `PI` · `frozen` · Alert forced-off（`prologue.gd` `_pose_teacher_for_shots`）。

---

## 2. 改动路径

| 路径 | 作用 |
| --- | --- |
| `scripts/systems/prologue.gd` | **唯一运行时真源**：`SHOTS` eye/look/fov |
| `tools/capture_art_prologue_cams.gd` | 盒端复拍 P01/P03/P07（gl_compat） |
| `evidence/art-prologue/p01-rain-window.png` | P01 证据 |
| `evidence/art-prologue/p03-plaque-clue.png` | P03 证据 |
| `evidence/art-prologue/p07-teacher-smile.png` | P07 证据 |
| `evidence/screenshots/art-review/art-prologue-p0*.png` | 同三镜副本 |
| `docs/ART_PROLOGUE_CAMS_20261028.md` | 本交付 |

**开发挂接**：已在 feat `73611ba` 的 `prologue.gd` / HUD / Skip→B0 骨架上；合 art→feat 时只收 `SHOTS` 数值 + 文档/证据/capture 工具即可，**勿**整文件覆盖 `game.gd` / `hud.gd` 非美术逻辑。

---

## 3. 未改清单

- [x] `teacher.gd` 侦测 / 追逐 / 状态机
- [x] game/chapter 主流程、Skip→B0 行为、进度字段
- [x] NavMesh / 房间拓扑
- [x] Lighting / Omni（**未新增**）/ AudioBus / 后期
- [x] Alert `?`/`!`/嫌疑条：**保持 hidden**
- [x] 铃（不在前情响铃）
- [x] VO 九句文案
- [x] 未合 main / feat（本分支仅 push art）

---

## 4. 盒端证据

```bash
cd EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_prologue_cams.gd
```

- Godot **4.7.2** · **gl_compatibility** / llvmpipe  
- `alert_hidden=true`  
- **不报视觉 PASS**；请 @游戏审核 目标机 forward+ 复看三拍高光 + Skip→B0

---

## 5. 风险

| 风险 | 说明 |
| --- | --- |
| 窗外无雨丝粒子 | 盒端窗外偏暗；雨丝/音景属后续光影·音频，本任务未加 Omni/VFX |
| P07 微笑网格细 | 近景靠眼镜反光+礼貌嘴线；forward+ 再验「笑意不到眼」 |
| P03 标牌「歪」 | 现标仍轴对齐；「略歪」若要几何倾斜需另开小道具任务 |
| P09 vs B0 spawn | P09 为低柜→门观察构图；`enter_b0` 仍传送 `Checkpoints/Classroom`（开发侧，未改） |
| art 含 S01-CORNER | tip 在 `73611ba` 之上叠有 ART-S01-CORNER；合 feat 时一并收材质/模块 |

---

## 6. 请协作方

1. **@游戏开发**：可将本 tip merge → `feat/chapter01-module-pass`（优先 `prologue.gd` SHOTS）；勿覆盖 Skip/侦测。  
2. **@游戏审核**：机位换完可复验 Skip→B0；三拍看 P01 雨窗框 / P03 考场标 / P07 微笑近景（盒图不作 PASS）。

---

*ART-PROLOGUE-CAMS · 定稿机位落地 · 盒证据完成 · 视觉 PASS 待 forward+*
