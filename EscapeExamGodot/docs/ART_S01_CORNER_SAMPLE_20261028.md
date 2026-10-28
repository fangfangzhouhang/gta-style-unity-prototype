# 【美术交付】ART-S01-CORNER · 教室角落样板（墙裙 / 低柜遮挡 / 窗边冷暖）

- **任务 ID**：`ART-S01-CORNER`
- **分支**：`art/chapter01-module-pass`（worktree `/workspace/wt-art/EscapeExamGodot`）
- **基线 tip（开工时）**：`5a94a02`（SHOT 文档锁定）；本交付叠在 tip `204e135`（prologue VO 文档）之上
- **本交付 Commit**：见文末 stamp（message 含 `ART-S01-CORNER`）
- **日期**：2026-10-28（Asia/Shanghai）
- **Godot**：4.7.2 stable · box 实跑 `gl_compatibility` + llvmpipe（OpenGL 4.5 Mesa）
- **范围**：共享教室色板材质 + `scenes/modules` 遮挡柜/桌侧低柜 + chapter 教室墙裙/玻璃/讲台面板色对齐 + 课桌木色 + S01 固定机位证据；**未改** `teacher.gd`、game/chapter 主流程、NavMesh、房间拓扑大改、Lighting/AudioBus/后期、Alert UI
- **状态**：材质/道具可读提升完成 · 盒上 S01 before/after 已拍 · **视觉 PASS 待 @游戏审核 forward+ 复看**（盒端不报视觉 PASS）

## 0. 背景

S01（坐位→讲台/门）是第一章样板验收轴。灰盒教室墙裙色偏暗、遮挡柜与墙裙同绿扁平、窗玻璃冷色不足，遮挡意图（低柜后 / 桌组侧）不够清楚。本轮按 `ART_DIRECTION` 色板做**一致性优先**的角落样板，不追求单模英雄精细度。

## 1. 改动清单

| # | 路径 | 改动 |
| --- | --- | --- |
| 1 | `assets/materials/mat_wall_paint.tres` | 墙涂料 `#D8D3C5` |
| 2 | `assets/materials/mat_wainscot.tres` | 墙裙 `#71867A` |
| 3 | `assets/materials/mat_dado_cap.tres` | 墙裙压条浅中性，强化 1.15m 层次线 |
| 4 | `assets/materials/mat_window_cold.tres` | 雨窗冷 `#607887` α≈0.22 |
| 5 | `assets/materials/mat_wood_school.tres` | 校木 `#9E7350` |
| 6 | `assets/materials/mat_cabinet_{body,door,metal}.tres` · `mat_baseboard.tres` | 柜体/门板/暗金属把手/踢脚（非交互金属族；交互仍 `#B78C48`） |
| 7 | `scenes/modules/cover_cabinet.tscn` | 低柜遮挡模块：体量同旧 `CoverCabinet`（0.6×1.54×1.35）+ 四门板/把手/顶盖/踢脚/侧条；碰撞外轮廓不变 |
| 8 | `scenes/modules/desk_side_cover.tscn` | 桌组侧低柜（≈0.78m 高）· 窗边冷暖对照用 |
| 9 | `scenes/chapters/chapter_01.tscn` | `r1/r2/r6/r14` 对齐色板；DadoCap×33 改 `mat_dado_cap`；`CoverCabinet`→模块实例；新增 `DeskSideCover` @ `(-9.48,0,-0.55)`；讲台 `FrontPanel` 改校木 |
| 10 | `scenes/props/desk.tscn` | 桌面木色对齐 `#9E7350` |
| 11 | `tools/capture_art_s01_corner.gd` | S01 单镜复拍（锁定机位同 `SHOT_BLOCKS_CHAPTER01`） |
| 12 | `evidence/screenshots/art-review/art-s01-{before,after}.png` | before=改前基线；after=本轮引擎截图 |
| 13 | `docs/ART_S01_CORNER_SAMPLE_20261028.md` | 本交付 |

**未改（声明）**

- [x] `scripts/actors/teacher.gd` 侦测 / 状态机
- [x] game / chapter 主流程脚本与进度字段
- [x] `resources/chapter_navigation.tres` NavMesh
- [x] 房间外壳拓扑（墙段/门洞位置未搬）
- [x] `Lighting/*` / Omni 参数 / AudioBus / Environment 后期
- [x] Alert `?`/`!`/嫌疑条：**保持 hidden**（capture 强制关闭；产品不重开）
- [x] 未新增 Omni；未写实血污；无过强描边；不抄你好邻居资产

## 2. 机位（锁定）

与 `docs/SHOT_BLOCKS_CHAPTER01.md` / 旧 `capture_art_s01_s05.gd` 一致：

| 项 | 值 |
| --- | --- |
| ID | **S01** · 坐位看讲台与门 |
| player | `(-6.8, 0.04, -0.85)` · eye y≈1.62 · FOV **74** |
| look_at | `(-5.4, 1.55, -5.3)` |
| teacher | `(-6.35, 0.02, -5.55)` yaw 0 |
| 分辨率 | 1280×800 |
| 渲染器（盒） | **`gl_compatibility` / opengl3 / llvmpipe** |
| 目标机 | 可用 `forward_plus` 复拍同机位 |

复跑：

```bash
cd EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_s01_corner.gd
```

## 3. Omni / Alert

| 项 | 本轮结果 |
| --- | --- |
| **新增 Omni** | **0**（硬约束：不新增） |
| **场景既有 Omni** | Fixture0–8 + FixtureS = **10**（既有 lookdev/音频侧溢遗留；**本任务未改** Lighting） |
| **交付核对「Omni≤4」** | 基线 tip 已 >4；本轮遵守「不新增」。若产品仍要压到 4，需另开灯光任务（非本美术样板范围） |
| **Alert / 嫌疑条** | **hidden = true**（capture 日志 `alert_hidden=true`） |

## 4. 证据截图

目录：`EscapeExamGodot/evidence/screenshots/art-review/`

| 文件 | 角色 | 说明 |
| --- | --- | --- |
| `art-s01-before.png` | before | 改前同机位基线（自开工时 after 拷贝） |
| `art-s01-after.png` | after | 本轮角落实例 · **渲染器 `gl_compatibility`** |

验收看点（人工 / forward+）：墙裙与压条层次；木柜 vs 绿墙裙分离；窗边冷玻璃 vs 室内暖灯管；遮挡柜/桌侧低柜体量可读；讲台门方向仍清；老师高瘦剪影仍在；无 Alert UI。

## 5. 请 @游戏开发 / @游戏审核

1. **@游戏开发**：请将本 tip **merge / cherry-pick** 进 `feat/chapter01-module-pass`（材质 + modules + chapter 家具实例 + desk 木色 + 文档/证据/捕获脚本）。勿整文件覆盖非美术逻辑；**勿**用本提交改 NavMesh/灯。美术 **不自行 push / 不合** feat/main。
2. **@游戏审核**：合入后 **forward+** 同机位复看 S01；确认遮挡意图与色板一致性。若柜门朝向或窗边低柜挡路，开微调任务（本轮碰撞外轮廓对齐旧柜，窗边低柜贴西墙内侧）。

## 6. Commit stamp

- **SHA**：_pending_local_commit_
- **message**：含 `ART-S01-CORNER`

---

*ART-S01-CORNER · 美术交付 · 盒证据完成 · 视觉 PASS 待 forward+ 复看*
