# 第一章改良路线 · 群协作版（A-M2.1）

- **日期**：2026-10-27（Asia/Shanghai）
- **分支**：`feat/chapter01-module-pass`（本轮改动专用；合主干须 ark 目视 OK）
- **循环**：模块 worktree 完成 → 交 @游戏开发 合并到功能分支 → @游戏审核 QA → 不通过则 @游戏开发 按问题改策划/重派 → 审核说 OK → ark 再合 `main`
- **验收锁**：检查点 1–2 用 `godot-genre-stealth`；3–5 用 `level-design`；P0–P3 依赖链用 `godot-genre-puzzle`（与 agent工作流搭建 对齐）

## 1. 现有缺点（基于评审 + 氛围复检）

| 短板 | 表现 |
| --- | --- |
| 危险不可读 | 老师有 suspicion，缺稳定 `?`/`!` 体感；单点 LoS；无 peek |
| 邻居味不足 | 无规则化惊吓；盒子人轮廓弱；程序几何离概念图远 |
| 谜题偏浅 / 时长塌 | 熟练几分钟可出；缺教→练→考节拍、假安全、出口布置窗 |
| 声光未闭环耳听 | 床/刺音/duck 代码已接，本机试玩与分区光打磨未完成 |
| 人工乐趣未验 | 38 夹具绿 ≠ 好玩；无三次人工首玩 |

## 2. 对标大型/同类项目「拿什么优点」（不抄关）

| 参考 | 可借的优点 | 落到本章 |
| --- | --- | --- |
| 《你好，邻居》 | 停-听-开门-peek；规则惊吓可学会；挪物改设施 | peek + 一种规则突脸；可选可推卷纸架 |
| 潜行佳作（Splinter Cell 系） | 渐进侦测表、警戒可读 | 头顶警戒 + 三点 LoS |
| 叙事解谜（Return of the Obra Dinn / 短读物系） | 短信息、证据句 | 失败证据句、四处短读物打磨 |
| 氛围恐怖（Outlast 系气质，不抄机制） | 稀疏灯、冷暖对比、声床服务紧张 | 昏暗分区 + 停播真静 + 抓捕刺音 |

**明确不借**：第二敌人、光级潜行、黑箱自学习 AI、加锁凑时长、血腥。

## 3. 模块分工与边界（文件/产出锁死）

### A · @游戏情节和机制设计
- **产出**：设计案 / 节拍表 / 文案稿 / 对 `DESIGN.md` 的改动建议（Markdown）；**不直接改代码**（含 `teacher.gd`、通关骨架）。
- **范围**：开场观察窗、教→练→考、假安全与出口布置窗、失败证据句、广播空间推理文案、时长目标（10–15 vs 15–25）确认项。
- **禁止**：改 GDScript/tscn 逻辑；加钥匙/第二敌人提案若无 ark 书面同意不得进本分支。
- **Worktree 焦点路径**：`EscapeExamGodot/docs/`、`DESIGN.md` 建议补丁（PR 描述贴 diff 即可）。

**定稿锁定（ark · 2026-10-27）**
- 侧室 **S「教具储藏」**：优先挂转角 **J**
- **门楔**：仅窥视 + 撑卷纸架；**不允许**挤过半开门
- 夹道 **H「卷纸检修」**：要做；全可选、无 soft-lock
- **雨窗曲柄**：标 **P2**，不进本轮 P0/P1
- 排期：P0 = 情节写 docs（节拍表 + 失败证据句）；**P1 = @游戏开发 实现** S + 门楔 + 推架露盖 + H；P2 = 雨窗曲柄声学
- 草案：`/workspace/escape-exam-docs/design-proposal-puzzle-map-expand-20261027.md`（实现前对照）

### B · @游戏光影音效设计
- **产出**：Environment/灯具参数、AudioBus、环境床、停播 duck、抓捕刺音、试听区灯；可改 `chapter_01` 灯光节点、`ambience`、`default_bus_layout`、`assets/audio/*`。
- **禁止**：改老师 AI 状态机数值；重写地图拓扑；重跑 `tools/author_scene.py`（会盖光照）。
- **Skill**：`godot-3d-lighting`、`godot-audio-systems`、`audio-design`。
- **Worktree 焦点**：`scenes/chapters/chapter_01.tscn`（仅 Lighting/WorldEnvironment）、`scripts/systems/ambience.gd`、`scripts/props/broadcast.gd`（仅静音钩子）、`assets/audio/`、`default_bus_layout.tres`。

### C · @人物形象和画面组块设计
- **产出**：老师混搭恐怖谷低模/材质规范、警戒 UI 视觉升级、样板房材质铜点、关键镜头组块（观察窗剪影、突脸 1s、檐下结算）；可改 `teacher.tscn` 视觉节点、材质、导入 mesh；构图文档。
- **禁止**：改巡逻点/侦测公式/存档 schema；擅自改通关条件。
- **Skill**：`character-visual-design`、`shot-block-composition`、`game-art-direction`、`blender-skills`。
- **Worktree 焦点**：`scenes/actors/teacher.tscn`（Visual/*）、新 `assets/models/`、`docs/ART_DIRECTION.md` 增补、镜头表 Markdown。

### D · @游戏开发（本角色）
- **产出**：功能分支维护、冲突合并、把设计案落地到代码（三点 LoS/peek/突脸钩子等跨模块粘合）、夹具不回归、按审核问题重写本策划案并重派。
- **禁止**：在审核未 OK 前合 `main`；跳过审核循环。

### E · @游戏审核
- **产出**：按严重度排序的 QA 报告（机制闭环、Bug、光线、分辨率）；对照 stealth / level-design / puzzle 检查点。
- **时机**：仅在 @游戏开发 声明「本轮已合并进 `feat/chapter01-module-pass`」之后。
- **结论**：`OK` 或 `打回`（列必须改项）；打回则开发改策划/重派，**不得**直接合主干。

## 4. Git / Worktree 约定

```text
# 功能分支（已由游戏开发创建，以远端为准）
git fetch origin
git checkout feat/chapter01-module-pass

# 成员示例（路径自定，勿共用同一工作区）
git worktree add ../wt-plot   feat/chapter01-module-pass   # 情节机制：只提交 docs
git worktree add ../wt-audio  feat/chapter01-module-pass   # 光影音效
git worktree add ../wt-art    feat/chapter01-module-pass   # 人物画面
```

- 分支命名：`feat/chapter01-module-pass`
- 成员提交前缀：`plot:` / `audio:` / `art:` / `dev:` / `qa:`
- **冲突高发区**：`teacher.gd`、`chapter_01.tscn` 整文件——情节不碰代码；光影只碰 Lighting；人物只碰 Visual；跨切改动开 Issue 由开发串。
- 合并顺序建议：`plot` 文档 → `audio` → `art` → `dev` 粘合 → 夹具 → 提审。

## 5. 本轮 P0 交付切片（审核用）

1. 危险可读：`?`/`!` + suspicion 可见；半身遮挡不全满表（三点 LoS 若本轮来得及）
2. 声光：昏暗分区耳听通过；停播安静；抓捕刺音
3. 老师：混搭轮廓可读（可仍非最终绑骨）
4. 情节：节拍表 + 失败句成文，不强制本轮全长 15–25
5. 夹具 38 项不回归

## 6. 硬边界

不重写整章地图；不加第二敌人；不加锁凑时；不做光级潜行；Unity 根玩法本轮不动。
