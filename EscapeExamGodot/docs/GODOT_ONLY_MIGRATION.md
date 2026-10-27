# Godot 单引擎整理 · 2026-10-27

## 分支与基线

用户指定唯一开发分支 `feat/chapter01-module-pass`，以远端 `1af8e06ed75672754fa0e09ba92709b5f2376edd` 为清理基线。提交前再次核对远端未前进。main 当时为 `7a85e68bfc476cfd2da12e7ffeb6452019f1803c`，本轮不修改、不合并。

## 仓库内容

删除 Unity 的 Assets/、Packages/、ProjectSettings/、Docs/，以及 Unity 专用 .agents/、.cursor/ 和 skills-lock.json。更新根 README/AGENTS/.gitignore，仅以 EscapeExamGodot/ 为游戏项目。保留 Godot 项目子目录，避免破坏既有路径和自动化引用。仓库不更名、不重写历史、不删除历史分支。

保留远端 P1 门楔/侧室/卷纸架/夹道和氛围音/教师/灯光迭代，未用旧本机 A-M2.1 覆盖；未修改任何游戏脚本、场景、模型、材质、音频或测试逻辑。首次导入生成的 GDScript UID 作为元数据补齐。历史 Godot 研究文档中的 Unity 引用只是来源/历史，不是运行依赖。

## 本地整理

当前开发仓库：D:/Unity/EscapeExamRepository，项目：EscapeExamGodot/project.godot。

删除用户明确指定的 ArtLab201Source、ArtLab201Evidence、project 和三个 artlab201 日志：6,799 个文件，3,839,781,856 字节。清理清单保存在本机 D:/Unity/Maintenance/deleted-artlab-project-20261027.json。根目录另外 56 个旧日志/辅助脚本归档到 Maintenance/LegacyFiles，映射表保留。

原 Unity 主工程、验证工作树、引擎安装保留；其未提交内容未重置或上传。旧 D:/Unity/EscapeExamGodot 保留为历史原型（含本地存档、旧构建与隔离引擎）；新增本机说明禁止将其当最新源码。以后使用新仓库开发，只推送指定功能分支。

## 验证

- 在移除全部根目录 Unity 内容之后，全新 Godot 4.7.2 headless editor import 成功。
- tools/run.ps1 -Mode Check：48 项通过、0 失败，退出码 0，包含 P1 门楔/夹道双向通行与无门楔离场；见 ../evidence/logs/godot-only-20261027-check.txt 和同名 .log。
- 测试有沙箱根证书读取错误，不是游戏脚本编译/运行错误；未声明完整日志零错误。
- 静态检查保留项目的 res:// 固定资源引用，清理不改变场景/玩法。没有本轮人工试玩、真实键鼠、画面或发行包验收。

本次只做目录、协作入口与分支整理，不以清理为由改变游戏体验或扩大范围。
