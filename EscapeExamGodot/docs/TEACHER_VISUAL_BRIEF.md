# 老师混搭视觉简报（P0）

- **Skill**：[character-visual-design](sand-workflow:character-visual-design) · [game-art-direction](sand-workflow:game-art-direction)
- **样板交付**：见 [TEACHER_STYLE_SAMPLE_P0.md](./TEACHER_STYLE_SAMPLE_P0.md)
- **参考概念**：`evidence/screenshots/art-review/teacher-p0-concept-*.jpg`（AI 概念 only）+ 历史 `/workspace/escape-godot-art-20261027/teacher-hybrid-ab.jpg`
- **禁止**：改 `teacher.gd` 侦测/巡逻；血腥；版权角色像；重开头顶警戒 UI

## 锁定混搭
| 层 | 取自 | 要点 |
| --- | --- | --- |
| 脸 | 方案 A | 塑料苍白、礼貌不到眼的空笑、细框眼镜 |
| 身/衣 | 方案 B | 偏高、蓝灰长大衣、肩袖轮廓 |
| 点缀 | 共用 | 红笔 + 卷夹；无血污贴图 |

## P0 完成定义
- 8m 外能辨朝向与大衣剪影（引擎 `teacher-p0-read-8m.png`）
- Idle / Walk / Chase 轮廓差可读（可暂无绑骨精修）
- 头顶 `?`/`!` + suspicion 条：**已按 ark 关闭且写死**（节点保留 hidden）；警戒只靠行为/音效/字幕
- 导入优先 `.glb` → `assets/models/teacher/`；`teacher.tscn` **只动 `Visual/*`**（Alert 保持 hidden）

## 引擎资产（当前）
- 材质：`assets/materials/teacher/mat_*.tres`
- 样板场景：`scenes/actors/teacher_visual.tscn` · `teacher_visual_body.tscn`
- Blockout glTF：`assets/models/teacher/teacher_p0_blockout.glb`（≈1.1k tris，未蒙皮）

## 与光影交界
灯参 / Environment 归 @游戏光影音效设计；样板灯仅存在于 `teacher_visual.tscn`。mesh 交付后请他们在 S02/S03 复检轮廓明暗差。
