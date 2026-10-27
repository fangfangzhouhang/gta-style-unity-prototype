# 项目协作边界

- 使用中文。当前唯一游戏引擎为 Godot；保留 feat/chapter01-module-pass 中的 P1 与氛围迭代，先读 PROJECT_STATUS，不拿旧原型覆盖。
- 2026-10-27 用户指定：当前改动只提交/推送 feat/chapter01-module-pass，等待用户确认后才能合并 main。不得自行更改默认分支。
- 当前本机工作副本 D:/Unity/EscapeExamRepository/EscapeExamGodot；旧独立工程 D:/Unity/EscapeExamGodot 已作为历史保留，不能继续当成最新开发源。其他电脑直接使用克隆得到的本目录。
- Unity 已停止开发，根目录已清理 Unity 资源/脚本/包/设置及专用技能；本项目不依赖它们，不再同步双引擎成果。
- 需要 Godot 4.7.2 标准版，不需要 .NET、Blender、Python 或原机器 D 盘路径即可运行。可用 GODOT_BIN 指定本机引擎，再运行 tools/run.ps1 -Mode Editor；引擎与缓存不纳入 Git。
- 自动测试必须用 tools/run.ps1 -Mode Check（隔离测试存档）；不得让测试覆盖玩家存档。工具 author_scene.py 仅供离线重新生成原型，会覆盖场景，不能作为启动步骤。手动编辑场景后不要随意重跑它。
- 先读 README、PROJECT_STATUS、DESIGN，再执行当前阶段。场景与参数应可在 Godot 编辑器中检查，不用一个运行时巨型脚本生成整章。
- 重要视觉修改必须实际渲染/运行检查，截图记录版本、机位、渲染器与验证类型。
- 分开记录静态检查、引擎自动测试、脚本辅助通关、操作系统真实输入测试、人工试玩；没有执行的项目不得标记完成。
- 维护 ASSET_CREDITS。网页参考不等于可分发素材；授权未核实的素材不得进入游戏。
