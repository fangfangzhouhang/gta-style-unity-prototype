# 环境与只读参考检查

检查日期：2026-10-25（Asia/Shanghai）。工作目录 `D:\Unity`。

## 独立目录与边界

首次检查 `D:\Unity\EscapeExamGodot` 不存在；创建前再次检查仍不存在，随后新建。没有覆盖现有项目内容。

`D:\Unity` 未检出 Git 仓库根。本项目单独初始化本地 Git，仓库根应为 `D:\Unity\EscapeExamGodot`；不与 Unity 使用同一 Git 工作区、分支、存档或资产软链接。

检查了 `D:\AGENTS.md`、`D:\Unity\AGENTS.md`，均不存在；`rg --files -g AGENTS.md` 在本次搜索范围未返回文件。Unity 内部开发规则不被迁移成新项目需求。新目录的 AGENTS.md 仅记录本次边界与验收要求。

## 引擎

安装目录：`D:\Unity\Editors\Godot`。

| 项目 | 本轮结果 |
| --- | --- |
| 控制台程序 | `Godot_v4.7.2-stable_win64_console.exe` |
| 主程序 | `Godot_v4.7.2-stable_win64.exe`，180858888 字节 |
| 实际执行 `--version` | `4.7.2.stable.official.ed1daf0bf` |
| 主程序 SHA-256 | `AB1824F85BFD8E0E4128182C000C4003A3E042245B2967848D089B2A04B22424` |
| 标准/.NET | 使用标准版程序；GDScript 方案不需要 .NET |
| Git | `git version 2.53.0.windows.1` |
| rg | 可调用 |
| Blender | PATH 查询未返回；未全面查找安装盘，不能认定未安装 |
| GPU、OS CIM 查询 | `Get-CimInstance` 被系统拒绝访问；没有提权或改系统设置 |
| 旧渲染日志线索 | `D:\Unity\godot201-render-final.log` 记载 Compatibility / OpenGL / NVIDIA GeForce RTX 5060 Laptop GPU；仅是历史记录，不作为当前性能验证 |
| 导出模板 | 未核实新项目可用模板；旧目录中虽有同名目录，不能据目录存在判定可导出 |

版本命令没有打开项目。未启动旧项目编辑器、未重跑旧验收、未写旧 `.godot` 缓存或改引擎文件。SHA 仅为本地标识，本轮未下载官方二进制逐字节验证签名。

对应官方资料已在线打开：4.7 CharacterBody3D、NavigationAgent3D、渲染器、保存、暂停、NavigationObstacles、AudioStreamPlayer3D。采用版本固定的 `/en/4.7/`，避免把滚动的 latest 文档当成安装版本。链接与设计用途见 [技术方案](TECHNICAL_PLAN.md)。

## 只读检查的旧资料

来源根：`D:\Unity\gta-style-unity-prototype`。下列为阅读/观察，未复制实现到新项目。

| 资料 | 得到的经验 | 本轮没有声称的结论 |
| --- | --- | --- |
| 根 README、Docs 目录索引/文件列表 | 旧版已有多阶段内容，自动通关与人工试玩明确不同 | 没有将旧任务列表转成新需求，也没有审计全部 Unity 源码 |
| `GodotPilot201/project.godot`、README | 旧小样 4.7 / Compatibility，显式场景，密码柜流程，无跨退出存档 | 不复制原房间、密码或入口 |
| `GodotPilot201/scripts/teacher.gd` | 视线射线、简单警觉和导航已尝试；追逐状态的目标位置仍取玩家全局位置，直到失视计时结束 | 未运行验证该行为；新方案需要最后目击点而非照搬此实现 |
| `GodotPilot201/evidence/acceptance.txt` | 23 条历史 PASS，标注了视口事件/定位夹具与非人工性质 | 不算新项目通过 23 项测试 |
| `GodotPilot201/evidence/classroom-preview.png` | 实际打开观察：完整顶面、尺度线索清楚；大块纯色/偏平亮度、简化教师和较大 HUD 仍像原型 | 是旧项目历史图片，不是本项目渲染、性能测试或最终美术 |
| `Docs/07-待验证清单.md` | 控制器、UI、碰撞与人工验证不能混写 | 未将历史勾选重新认证 |
| `Docs/17-ArtLab201技术审查与验收.md` | 先样板、后推广；材质被运行时覆盖、许可不明、未烘焙等需要实证 | 未复用其资产许可结论，未执行 Unity 修改 |

本轮只对上述有限资料作检查；另一个聊天可能同时更新 Unity，不能把本项目的检查写成对那边仓库整体不变的证明。本轮工具未对其执行写入、Git 切换、提交或合并。

## 尚需实现期核实

新项目 Forward+ 真实启动、当前 GPU/驱动、目标最低硬件、导出模板、素材制作工具、全屏与输入法、音频设备、独立存档写入和性能。都不是 D0 的已完成验证。
