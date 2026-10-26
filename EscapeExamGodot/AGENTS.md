# 项目协作边界

- 使用中文沟通。用户已选定 A「最后一遍铃」，现为 A-M2.1 可玩原型，先读 PROJECT_STATUS，不要将 D0 历史提案视为等待批准。
- 写入范围只限本项目及任务授权的临时目录。Unity 项目、其 GodotPilot201、原分支与引擎安装目录只读；不要运行可能在旧项目中写入 `.godot`、导入缓存或日志的编辑器/测试命令。
- 用户于 2026-10-26 授权将本项目源码发布到现有 Unity 仓库的 EscapeExamGodot/ 子目录。原本机独立工程仍用于本对话开发；仓库副本是交付快照，不是同时编辑的第二工作副本。后续同步应明确源与目标，避免覆盖新改动。未经明确要求不合并主干。
- 克隆仓库后将 AI 工具工作目录设为 EscapeExamGodot，Godot 直接打开该目录的 project.godot。勿打开旧 GodotPilot201 代替本项目。本项目不依赖 Unity；不修改仓库根目录 Assets/、Packages/、ProjectSettings/。
- 需要 Godot 4.7.2 标准版，不需要 .NET、Blender、Python 或原机器 D 盘路径即可运行。可用 GODOT_BIN 指定本机引擎，再运行 tools/run.ps1 -Mode Editor；引擎与缓存不纳入 Git。
- 自动测试必须用 tools/run.ps1 -Mode Check（隔离测试存档）；不得让测试覆盖玩家存档。工具 author_scene.py 仅供离线重新生成原型，会覆盖场景，不能作为启动步骤。手动编辑场景后不要随意重跑它。
- 先读 README、PROJECT_STATUS、DESIGN，再执行当前阶段。场景与参数应可在 Godot 编辑器中检查，不用一个运行时巨型脚本生成整章。
- 重要视觉修改必须实际渲染/运行检查，截图记录版本、机位、渲染器与验证类型。
- 分开记录静态检查、引擎自动测试、脚本辅助通关、操作系统真实输入测试、人工试玩；没有执行的项目不得标记完成。
- 维护 ASSET_CREDITS。网页参考不等于可分发素材；授权未核实的素材不得进入游戏。
