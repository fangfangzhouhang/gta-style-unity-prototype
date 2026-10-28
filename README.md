# 逃离考场 · 最后一遍铃

第一人称校园潜行解谜游戏，现仅使用 **Godot**。

## 开发入口

- 当前研发分支：`feat/chapter01-module-pass`。用户确认前不合并 main。
- 游戏工程：**EscapeExamGodot/project.godot**。安装 Godot 4.7.2 标准版，导入此文件后按 F5 运行。
- AI 工具以 `EscapeExamGodot/` 为工作目录，先读其中的 README、AGENTS、PROJECT_STATUS、DESIGN，以及最新设计文档。
- 本机工作目录：`D:/Unity/EscapeExamRepository/EscapeExamGodot`。其他电脑直接使用克隆目录，无需同样的盘符。

```sh
git clone --branch feat/chapter01-module-pass https://github.com/fangfangzhouhang/gta-style-unity-prototype.git
```

仓库为私有，协作者需取得访问权限。GitHub 下载 ZIP 时请选择研发分支。引擎、导入缓存、本地存档和发行构建不入库。

## 目录与本次整理

根目录仅保留 EscapeExamGodot/、README.md、AGENTS.md 与 .gitignore。操作说明见 [游戏 README](EscapeExamGodot/README.md)。

2026-10-28 用户授权清理现有研发分支的非 Godot 内容。移除根目录旧 Unity Assets/、Packages/、ProjectSettings/、Docs/、.agents/、.cursor/ 和 skills-lock.json；Git 历史保留。此次整理以远端 `9588e50` 为基线，完整保留 Godot 子目录，不回退之后已接入的序章、钥匙链、声音和美术成果。

本次仅整理文件，不重新设计玩法、不修改 Godot 脚本或资产、不改变 main，也不将研发分支合入 main。子目录中较早的双引擎交付及旧机器路径说明属于历史，以本页的当前开发入口为准。游戏仍在研发，历史自动测试不能替代人工试玩与发行验收。
