# 双引擎仓库协作边界

- 使用中文。先明确本次任务属于 Unity 还是 Godot；不要同时修改两套游戏逻辑。
- Unity 工程位于根目录 Assets/、Packages/、ProjectSettings/，交接见 Docs/19-开发过程与后续交接.md 与 Docs/20-V1.2手账叙事与存档恢复.md。
- 新 Godot 独立版位于 EscapeExamGodot/，先读其 AGENTS.md、README.md、PROJECT_STATUS.md；直接打开该目录 project.godot。
- Godot 不依赖 Unity 资产、不共享存档，不要将 Godot 文件放进 Unity Assets；旧 GodotPilot201 不代表新 Godot 版本。
- Godot acceptance：`godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd -- --test`（**必须** `-- --test`；`run.ps1 -Mode Check` 已带）。
- Unity 第五阶段已提交但人工/发行验收未完成；Godot 为 A-M2.1 原型，不能把脚本通关当人工试玩。
- 不提交 Library、.godot、引擎、导出包、本机存档或凭据。不推送和合并无关未提交实验；仅处理用户授权范围。
- 本交付分支保存 d5192ed Unity 基线与独立 Godot 快照；主干合并须有明确任务授权，不因存在 PR 自动合并。
