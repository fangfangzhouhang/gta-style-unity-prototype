# 逃离考场 · Godot 独立版

当前版本 **A-M2.1，可玩原型**（2026-10-26）。用户已选择 A「最后一遍铃」，体验参考确认为《你好，邻居》。观察监考者，利用计时器与广播改变调查方向，再分段抬起出口卷栅。完整美术、人工首玩节奏和发行验收尚未完成。

## 克隆后直接打开

本目录是完整 Godot 工程，入口 `project.godot`，不是 Unity 插件；运行不依赖旁边的 Unity Assets，也不需要旧 GodotPilot201。

1. 安装/解压 **Godot 4.7.2 标准版**：[官方归档](https://godotengine.org/download/archive/4.7.2-stable/)。不需要 .NET。
2. Godot 项目管理器选择“导入”，选中本目录 `project.godot`，打开后等待首次资源导入。
3. 按 F6 运行当前场景，或 F5 运行主场景 `scenes/chapters/chapter_01.tscn`。主菜单选择“开始新的试走”。

命令行也可运行（`godot` 代表本机 Godot 可执行文件）：

```sh
godot --editor --path EscapeExamGodot
godot --path EscapeExamGodot
```

Windows PowerShell 可在本目录执行：

```powershell
./tools/run.ps1 -Mode Editor -Godot 'C:/Tools/Godot/Godot_v4.7.2-stable_win64.exe'
./tools/run.ps1 -Mode Run -Godot 'C:/Tools/Godot/Godot_v4.7.2-stable_win64.exe'
```

也可以设置 `GODOT_BIN` 为引擎绝对路径。脚本依次使用参数/环境变量、本项目 tools/runtime、本机 PATH 的 godot 或 godot4，不要求原电脑 D 盘目录。现有中文 cmd 是原本机快捷入口；换电脑优先使用上述编辑器导入或 run.ps1。引擎文件、导入缓存、存档、试玩构建不纳入源码仓库。

## 操作

| 操作 | 按键 |
| --- | --- |
| 移动 / 观察 | WASD / 鼠标 |
| 跑 / 蹲 | Shift / Ctrl |
| 调查、开关门、拾取 | E |
| 抬出口卷栅 | 对准把手持续按 E，松开可撤退；每完成一档保留 |
| 计时器上弦 / 停止 | Q（手持或靠近已放置的计时器） |
| 放计时器 | 左键，对准可放置平面 |
| 已发现事实 / 暂停 | Tab / Esc |
| 截图 | F8 |

暂停菜单提供灵敏度、视野、音量与声音字幕。无跳跃、战斗、联网或大模型追捕。

## AI 接手入口

把 AI 编程工具的工作目录设为本目录，让它依次阅读 `AGENTS.md`、`PROJECT_STATUS.md`、`DESIGN.md`、`evidence/ACCEPTANCE.md`，再打开 `project.godot`。有本地文件与运行权限的工具可以修改 GDScript/场景并调用引擎；单纯聊天工具不会自动安装或操控引擎。

场景、门、桌椅、角色与 HUD 均为可在编辑器查看的 tscn，参数在脚本 export 字段中。`tools/author_scene.py` 是历史离线场景制作工具，不是运行依赖；重新执行会覆盖编辑后的场景并要求重烘焙导航。

## 验证与限制

隔离测试存档的 Windows 命令（先指定 GODOT_BIN，或传 -Godot）：

```powershell
./tools/run.ps1 -Mode Check
./tools/run.ps1 -Mode Route
./tools/run.ps1 -Mode TimerRoute
```

Check 是夹具式引擎测试；Route / TimerRoute 是使用真实角色物理与活动 AI 的脚本辅助路线，不是实体键鼠或人工试玩。请勿直接以玩家存档目录运行 acceptance.gd；测试会写入测试档。

历史记录为 38 项引擎检查通过；广播和纯计时器各有脚本通关记录。失败路线也保留供分析。15—25 分钟只是首玩目标，脚本几十秒通关不能证明体验成立。真实电脑输入检查被用户物理 Escape 中止，不能标记为完成。

原型资产为自制几何、程序材质、合成音效，教师和广播演出仍需提升。使用 Forward+，已在 RTX 5060 Laptop 上实际渲染；其他 GPU/系统尚未验收。字体通过系统字体回退，不打包商业字体。

## 文档与数据

- [DESIGN](DESIGN.md)：核心体验、地图、谜题依赖与追捕规则。
- [PROJECT_STATUS](PROJECT_STATUS.md)：当前完成范围和下一步。
- [验收](evidence/ACCEPTANCE.md)、[已知问题](docs/KNOWN_ISSUES.md)、[素材来源](ASSET_CREDITS.md)。
- 通过 run.ps1 / 本机 cmd 启动的数据位于 local_data；测试位于 test_userdata。直接从编辑器运行则使用系统 user://EscapeExamGodot。两者不共享，均不入库。
- 仓库中的 EscapeExamGodot/ 是独立源码目录，不含嵌套 .git 或子模块；克隆一次即可得到所需源码。
