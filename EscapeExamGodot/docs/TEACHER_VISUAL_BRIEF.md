# 老师混搭视觉简报（P0）

- **Skill**：[character-visual-design](sand-workflow:character-visual-design) · [game-art-direction](sand-workflow:game-art-direction)
- **参考**：`/workspace/escape-godot-art-20261027/teacher-hybrid-ab.jpg`（A脸空笑 + B蓝灰大衣偏高）
- **禁止**：改 `teacher.gd` 侦测/巡逻；血腥；版权角色像

## 锁定混搭
| 层 | 取自 | 要点 |
| --- | --- | --- |
| 脸 | 方案 A | 塑料苍白、礼貌不到眼的空笑、细框眼镜 |
| 身/衣 | 方案 B | 偏高、蓝灰长大衣、肩袖轮廓 |
| 点缀 | 共用 | 红笔；无血污贴图 |

## P0 完成定义
- 8m 外能辨朝向与大衣剪影（S02）
- Idle / Walk 轮廓差可读（可暂无绑骨精修）
- 头顶 `?`/`!` + suspicion 条：**已按 ark 关闭**（节点保留 hidden）；警戒只靠行为/音效/字幕
- 导入优先 `.glb` → `assets/models/teacher/`；`teacher.tscn` 只动 `Visual/*` 与 Alert UI 节点

## 与光影交界
灯参 / Environment 归 @游戏光影音效设计；我交 mesh 后请他们在 S02/S03 复检轮廓明暗差。
