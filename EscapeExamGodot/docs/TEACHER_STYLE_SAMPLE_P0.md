# 教师角色风格样板 · P0

- **任务 ID**：P0-TEACHER-STYLE → 续修 **P0-TEACHER-SLIM**（体型高瘦）
- **基准**：`feat/chapter01-module-pass` @ `6477111`（art 分支已 merge）
- **日期**：2026-10-27（Asia/Shanghai）
- **Skill**：character-visual-design · create-game-assets

## 一句话设定

外表正常、行为不安的监考老师：蓝灰长大衣、苍白脸、细框眼镜、礼貌不到眼的异常微笑；**体型 = 高瘦监考感（非敦实/块状偏胖）**；**不是**怪物，**无**血污，**无**过强描边。

## 交付清单

| # | 项 | 路径 | 类型 |
| --- | --- | --- | --- |
| 1 | 正/侧/背概念板 | `evidence/screenshots/art-review/teacher-p0-concept-turnaround.jpg` | **概念 only（AI）** |
| 1b | 引擎 turnaround | `teacher-p0-turnaround-{front,side,back}.png` | **引擎渲染** |
| 2 | 教室暖 vs 走廊冷概念 | `teacher-p0-concept-lighting.jpg` | **概念 only（AI）** |
| 2b | 引擎教室/走廊光 | `teacher-p0-classroom-warm.png` · `teacher-p0-corridor-dim.png` · `teacher-p0-corridor-8m.png` | **引擎渲染** |
| 3 | Idle/Walk/Chase 概念 | `teacher-p0-concept-poses.jpg` | **概念 only（AI）** |
| 3b | 引擎三姿态 | `teacher-p0-poses-engine.png` | **引擎渲染（摆姿势）** |
| 4 | 8m 轮廓可读 | `teacher-p0-read-8m.png` | **引擎渲染** |
| 5 | Godot 可用 blockout | `scenes/actors/teacher.tscn` Visual/* · `scenes/actors/teacher_visual_body.tscn` · `assets/models/teacher/teacher_p0_blockout.glb` | **引擎可用（非最终英雄模）** |

样板场景：`scenes/actors/teacher_visual.tscn`（灰底三视图相机 + 教室/走廊灯参切换）。  
截图脚本：`tools/capture_teacher_p0_runner.gd` + `scenes/actors/teacher_p0_capture.tscn`。

## 比例与剪影

- **体型 = 高瘦监考感，非敦实**（P0-TEACHER-SLIM）：收窄 + 拉高，禁止只整体等比放大。
- 头身比目标约 **1:7–1:8**（头略小、腿更长、躯干相对缩短）。
- 肩宽适中偏窄；肩≈略宽于髋；胸腹深度明显变薄；大衣下摆勿鼓成圆桶。
- 大衣膝长 + 轻度底摆 flare + 翻领，远距靠修长肩袖与大衣读朝向。
- 细框圆眼镜 + 上扬微笑角：近距 uncanny，不靠血污。
- 红笔 + 牛皮纸卷夹：记忆点缀。
- 引擎 Visual 关键：大衣约 `0.48×1.02×0.24`；头心约 `y=2.02`；腿根约 `y=1.05`。

## 模型 / 材质 / 骨骼预算（建议）

| 项 | P0 blockout（已交付） | 下一档英雄模（建议） |
| --- | --- | --- |
| 三角面 | CSG/原语合计 ≈1–3k；glTF blockout ≈0.6k（slim） | **15k–30k**（与 ART_DIRECTION 一致） |
| 材质数 | 9 个 `.tres`（coat/trousers/skin/hair/shoes/paper/pen/glasses/collar） | ≤8–10，可图集 |
| 贴图 | 无（纯色 StandardMaterial3D） | 主 1K，脸/标识可 1K–2K；密度约 256px/m |
| 骨骼 | **无**（Visual 节点旋转摆姿态） | Humanoid 简化：Root/Hips/Spine/Chest/Neck/Head + 四肢 3 段；表情有限 blend 或骨控微笑 |
| 导出 | `assets/models/teacher/*.glb`（Y-up） | 同路径替换；Godot 导入后挂到 Visual |

## 警戒 UI（写死）

头顶 `?` / `!` / 嫌疑条：**已取消**。`AlertIcon` / `SuspicionBar` 节点保留且 `visible = false`；`teacher.gd` 强制隐藏。**禁止重开**。可读性依赖步态 / 停转 / 音效 / 剪影。

## 许可与 provenance

| 资产 | 许可 |
| --- | --- |
| `teacher.tscn` Visual 原语、`teacher_visual_body.tscn`、`mat_*.tres`、`teacher_p0_blockout.glb` | **原创原语 = 本项目自有** |
| `teacher-p0-concept-*.jpg` | **AI 生成概念参考 only**，不得当作最终网格或直接贴图进包体生产链路 |
| 禁止 | 复制《你好邻居》具体角色/物件/场景；第三方扫描网未入库 |

## 验收自检（本轮）

- [x] 8m 外轮廓/朝向可读（`teacher-p0-read-8m.png`）
- [x] Idle / Walk / Chase 姿态有别（引擎摆姿势图）
- [x] 无严重悬空；碰撞胶囊仍 2.05m（Visual 略高瘦，不改侦测）
- [x] 正/侧视一眼比旧版更瘦更高（大衣深度变薄）
- [x] 不依赖警戒 UI
- [x] 概念图 vs 引擎渲染路径已分表标注
- [ ] 尚未绑骨 / 尚未最终拓扑 UV / 未在 chapter_01 全关卡人工乐趣验收

## 需要协作

- **开发**：可将 `teacher_visual_body` 或未来 `teacher_hero.glb` 挂入 `teacher.tscn` Visual；勿改侦测逻辑。
- **光影**：S02/S03 冷廊侧溢后复检大衣剪影（见 KNOWN_ISSUES）；本 P0 样板灯仅在 `teacher_visual.tscn` 内，**未改** chapter 灯光。
