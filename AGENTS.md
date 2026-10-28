# Godot 仓库协作边界

- 使用中文。当前游戏仅使用 Godot，工程入口 EscapeExamGodot/project.godot。
- 当前改动提交到 feat/chapter01-module-pass；用户明确确认前不得合并或推送 main。
- 2026-10-28 用户明确授权清理本研发分支的非 Godot 目录。本次仅删除旧 Unity 内容，保留 Godot 子目录原样。
- 进入 EscapeExamGodot 阅读 README、AGENTS、PROJECT_STATUS、DESIGN 和最新设计文档；旧双引擎/旧本机目录声明按历史记录理解。当前本机源是 D:/Unity/EscapeExamRepository/EscapeExamGodot，不用旧 D:/Unity/EscapeExamGodot 覆盖新成果。
- 不恢复 Unity Assets/Packages/ProjectSettings，不要求 Unity 编辑器，不自动生成或重写游戏场景。
- 自动检查使用 tools/run.ps1 -Mode Check，隔离测试存档并带 -- --test；不能让测试覆盖玩家存档。
- 不提交 .godot、引擎、构建、存档或凭据。既有已跟踪证据保留；未经用户要求不要批量提交本地自动截图。
- 区分静态核对、引擎检查、脚本通关、真实输入与人工试玩。目录清理不等于新版游戏完成验收。
