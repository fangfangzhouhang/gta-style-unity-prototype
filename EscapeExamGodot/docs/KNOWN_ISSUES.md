# 已知问题 · A-M2.1

- 初玩时长、乐趣与三次人工完整试玩未验收；熟练脚本可很快通关，不能靠增加锁和重复收集凑时长。
- 近路导航已验证，但实际绕行收益未充分验证；开门后老师也会利用近路，返回响铃处的测试失败。
- 教师使用原型几何和简单摆臂；音效为合成占位，循环广播目前主要由字幕/提示音表达，没有真实配音。
- 房间与走廊已渲染检查，不等于最终美术。灯光噪点、材质/角色质量、窗外和结尾仍需提升。
- 真实 OS 键鼠测试被用户中止；跨平台、低帧率、分辨率、持续运行、正式导出和性能预算未验证。
- 沙箱引擎日志出现 root certificate store 读取错误；本游戏不使用网络。此前部分路线退出存在 ObjectDB/resource 泄漏警告，须与持续运行内存问题区分并继续调查。
- 旧开发日志含曾经失败的导入/测试，保留为诊断历史；最新验收摘要不能用历史错误自动覆盖，也不能抹除失败证据。
- tools/author_scene.py 会重写场景，重新生成后需重烘焙导航；普通开发应直接编辑 tscn，不要将它作为启动脚本。
- 初次打开需要 Godot 4.7.2 导入资源；源码仓库不包含编辑器、导出模板、存档或缓存。

- P1 灰盒风险：若门楔/夹道 H 被做成离场硬前置，将违反「禁止加锁凑时」；验收须保留无楔无暗道通关。若 ajar 门可侧身挤过，视为平衡回归，需 ark 重批。H 须保持 G→M / M→G 可逆，禁止无提示单向困死。

- 冷廊剪影（S02）：老师背光贴暖门口时轮廓清晰；纯冷廊暗区易糊进墙。不改拓扑；Fixture5·7 侧溢已合入 feat，仍待人工 after 复检。证据：`evidence/screenshots/art-review/s02-door-corridor.png`、`s03-cabinet-teacher.png`。
- Alert 正面证据（历史）：`evidence/screenshots/art-review/alert-front-q.png`（?）、`alert-front-bang.png`（!）。嫌疑条略压脸下半，不挡识别。

## Alert UI removed (2026-10-27)
ark decided floating `?`/`!` and the orange suspicion bar should not show. Teacher five-state logic (`patrol→suspicious→investigate→chase→search/return`) remains; readability relies on gait/stop/turn/audio/silhouette. Prior `art-review/alert-front-*.png` evidence is historical only. Nodes kept hidden; must not re-enable.
