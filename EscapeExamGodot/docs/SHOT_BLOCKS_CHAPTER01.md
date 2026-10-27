# 第一章镜头组块 · S01–S05（画面验收）

- **分支工作区**：`art/chapter01-module-pass` → 合入 `feat/chapter01-module-pass`
- **Skill**：[shot-block-composition](sand-workflow:shot-block-composition)
- **规则**：改光/改老师/改样板材质后，必须同机位复拍再比；勿用自由机位当验收。
- **Capture runner**：`tools/capture_art_s01_s05.gd`（Godot 4.7.2 · 1280×800 · `gl_compatibility`/`opengl3` on box；目标机可用 `forward_plus`）
- **任务 ID**：`V2-S01-S05-SHOTS`

| ID | 意图 | 锁定机位（player pos / look_at / FOV / eye） | 教师放置 | 验收要点 |
| --- | --- | --- | --- | --- |
| S01 | 坐位看讲台与门 | `(-6.8, 0.04, -0.85)` → look `(-5.4, 1.55, -5.3)` · FOV **74** · eye y≈1.62 | `(-6.35, 0.02, -5.55)` yaw 0（讲台前朝课桌） | 黑板/讲台可读；门在画面右；老师高瘦肩宽轮廓入画 |
| S02 | 门口看走廊转角 | `(-1.75, 0.04, 1.5)` → look `(-1.75, 1.55, -1.0)` · FOV **72** · eye y≈1.62 | `(-1.75, 0.02, -1.0)` yaw 0 · suspicious | 转角暗区 vs 冷暖；远处老师朝向可辨（与 before 同机位） |
| S03 | 柜后蹲看教师 | `(-3.0, 0.04, 0.2)` crouch Head.y=0.94 → look `(-1.75, 1.35, -1.0)` · FOV **70** | 同 S02 | 半身/低机位危险距离；Alert 保持 hidden（不验收 `?` 头顶） |
| S04 | 近看计时器/纸 | `(-6.5, 0.04, -5.55)` → look `(-6.5, 1.0, -6.65)` · FOV **55** · eye y≈1.62 | 移出画外 `(2.5, 0.02, -4.0)` | 计时器位数可读；铜点/握位强调（`#B78C48` 族） |
| S05 | 廊反看教室 | `(-2.85, 0.04, 1.10)` → look `(-6.5, 1.50, 0.95)` · FOV **72** · eye y≈1.62（LoS 穿考场门 z≈1.1） | `(-6.4, 0.02, 0.95)` yaw π/2 | 廊冷→室内；老师侧影距离可读；出口方向不糊 |

## 可选电影短镜（机制稳后再做）
- **C-Scare**：突脸 ~1s（同侧开门规则，由开发挂钩）
- **C-Settle**：檐下结算静帧

## 截图清单（引擎真实截图，概念图不算）

目录：`EscapeExamGodot/evidence/screenshots/art-review/`

| 文件 | 角色 | 说明 |
| --- | --- | --- |
| `art-s01-after.png` | after | 首次锁定机位（无历史同机位 before） |
| `art-s02-before.png` | before | 自 `s02-door-corridor.png`（预高瘦 Visual · 同 S02 pose） |
| `art-s02-after.png` | after | 高瘦 Visual @ tip `91204e2`+ |
| `art-s03-before.png` | before | 自 `s03-cabinet-teacher.png`（预高瘦 · 同 S03 pose） |
| `art-s03-after.png` | after | 高瘦 Visual · 蹲姿 |
| `art-s04-after.png` | after | 首次锁定机位（无 before） |
| `art-s05-after.png` | after | 首次锁定机位（无 before） |

**缺 before**：S01 / S04 / S05 标注「仅 after（首次锁定机位）」；S02 / S03 成对。

## 风格自检（轻漫画校园微恐）
- 仍是真实比例灰盒 + 块状教师；高瘦监考剪影（非敦实）在 S01/S02/S03/S05 可读。
- 冷廊暗部剪影仍偏糊（KNOWN_ISSUES 冷廊侧溢 follow-up）；本轮**未改** chapter 灯光 / AudioBus / NavMesh / teacher.gd 逻辑。
- Alert / 嫌疑条保持 hidden。

## 合入说明（待开发）
1. Art 分支完成本清单后 **push `art/chapter01-module-pass`**（勿强推 feat）。
2. 开发将 `art/chapter01-module-pass` merge → `feat/chapter01-module-pass`。
3. **待开发 merge art→feat 后的完整 SHA**：本交付时尚无（art tip 见本 commit）；交审理想态为 feat 合入后的完整 SHA。
4. 复跑：`godot --path EscapeExamGodot --rendering-driver opengl3 --rendering-method gl_compatibility -s res://tools/capture_art_s01_s05.gd`

## 本轮 art 交付核对
1. ✅ 老师 Visual 高瘦混搭轮廓（P0-TEACHER-SLIM · `91204e2`）
2. ✅ 警戒 Label3D 保持 hidden（产品取消，不重开）
3. ✅ S01–S05 引擎复检截图 `art-s0x-*.png` 已入库
