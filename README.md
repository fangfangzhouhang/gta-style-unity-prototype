# 逃离考场 · 最后一遍铃

本项目现在仅使用 **Godot** 开发第一人称校园潜行解谜游戏。Unity 版本已停止开发，其源码从当前开发分支移除；历史提交仍可追溯。

## 当前开发分支

使用 **feat/chapter01-module-pass**。所有当前改动提交到此分支；在用户明确确认前，不合并或推送 main，也不改变默认分支。main 暂时保留旧内容，这是评审边界，不是清理遗漏。

```sh
git clone --branch feat/chapter01-module-pass https://github.com/fangfangzhouhang/gta-style-unity-prototype.git
```

仓库为私有，协作者需要访问权限。GitHub 下载 ZIP 时也先选择上述分支。仓库名称暂时保留，以免现有链接失效。

## 打开与继续开发

1. 安装 Godot 4.7.2 标准版（不需要 .NET）。
2. Godot 项目管理器导入 **EscapeExamGodot/project.godot**，等待资源导入后按 F5。
3. AI 工具以 **EscapeExamGodot/** 为工作目录，先读该目录的 AGENTS.md、README.md、PROJECT_STATUS.md 与 DESIGN.md。

本地当前工作副本：`D:/Unity/EscapeExamRepository`；游戏子目录为 `D:/Unity/EscapeExamRepository/EscapeExamGodot`。其他电脑无需相同路径。旧本机 `D:/Unity/EscapeExamGodot` 是旧原型，勿复制回覆盖本分支新成果。

## 目录

- `EscapeExamGodot/`：Godot 场景、脚本、资产、设计文档、测试和证据；[启动与操作](EscapeExamGodot/README.md)。
- `AGENTS.md`：分支与 AI 协作边界。
- `.gitignore`：排除缓存、构建、引擎与存档。

最新保留内容包含 P1 侧室/门楔/卷纸架/检修夹道、氛围音与视觉迭代；以游戏内 [PROJECT_STATUS](EscapeExamGodot/PROJECT_STATUS.md) 及提交记录为准，仍是研发版本。历史自动检查不是当前人工试玩或发行验收。

[本轮清理与验证](EscapeExamGodot/docs/GODOT_ONLY_MIGRATION.md)。引擎、玩家存档与导入缓存不入库。历史 Unity 源码只留在 Git 历史和原本机旧工程，不作为本游戏依赖。
