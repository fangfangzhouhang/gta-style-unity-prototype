# 逃离考场 · Godot

当前仅使用 Godot 开发游戏。main 已移除弃用的 Unity 工程、文档与 Unity 专用技能；旧内容保留在 Git 历史中。

## 分支边界

- **main**：保留此前已经合入的 Godot 原型。本次仅清理旧 Unity 内容，不引入开发分支的新玩法或美术。
- **feat/chapter01-module-pass**：保留当前开发成果与原有目录，用户确认前不合入 main。本分支的误清理已通过 c93930a 撤销，文件树恢复为清理前状态。

## 打开 Godot

安装 Godot 4.7.2 标准版，导入 `EscapeExamGodot/project.godot`，等待资源导入后按 F5。无需 Unity 或 .NET。操作和测试入口见 [Godot README](EscapeExamGodot/README.md)。

AI 工具以 `EscapeExamGodot/` 为工作目录，阅读该目录的 README、AGENTS、PROJECT_STATUS 和 DESIGN。子目录保留原版本文档，其中双引擎交付、旧目录与分支说明属于历史，以本页的当前分支边界为准。

```sh
git clone https://github.com/fangfangzhouhang/gta-style-unity-prototype.git
```

查看最新研发版时另行检出 `feat/chapter01-module-pass`，不要将 main 的旧 Godot 覆盖回开发分支。私有仓库协作者需先取得访问权限。

## 本次 main 清理（2026-10-27）

基线 `7a85e68bfc476cfd2da12e7ffeb6452019f1803c`。移除 Assets/、Packages/、ProjectSettings/、Docs/、.agents/、.cursor/ 与 skills-lock.json，更新根目录 README/AGENTS/.gitignore；EscapeExamGodot/ 的已跟踪内容保持完全不变，不重写历史，不修改默认分支，不合并研发分支。

清理后在无旧缓存的独立副本重新导入 Godot，并执行 Check：38 项通过、0 失败。此为 main 原型的引擎自动检查，不是开发分支的 48 项，也不代表人工试玩。本机日志存于 Maintenance/main-cleanup-check-20261027.log。
