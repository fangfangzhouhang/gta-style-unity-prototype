# 【实现任务卡】第一章 · 最小钥匙链 gated 推进（给 @游戏开发）

- **日期**：2026-10-28（Asia/Shanghai）
- **角色**：情节/机制/关卡策划 · **任务卡 only** · **禁止本 bot 改** `.gd` / `.tscn` / 存档 schema / 教师 AI 数值
- **批准源**：`docs/design/CHAPTER01_GATED_PROGRESSION_REDESIGN.md` · ark **「用户已批准」最小钥匙链** · 2026-10-28
- **Widget 原文**：「批准最小钥匙链：C出廊硬锁 + K1/K2/K3，S必经，老师开门后延迟上场」
- **代码状态**：**已落地** feat tip `9121693209ffa42a67c576b47b87ebedb7559978`（FF · `feat/chapter01-gated-min-keychain` / 已 tip 到 `feat/chapter01-module-pass`）· **体验未 PASS** · **未合 main**
- **G·K3**：**已锁定** `Doors/GateAntechamber`（**不是** Gate/Handle）
- **Progress**：schema2 `keys` {k1,k2,k3} · `doors_unlocked` {classroom,storage,gateantechamber} · `teacher_summoned`
- **排期**：实现切片已合 feat；后续跟进已知缺口 + 体验盲测；策划不改代码
- **引擎边界**：只动 EscapeExamGodot（Godot）；**禁止** Unity / Godot 交叉覆盖

---

## 0. 目标（一句话）

把第一章从「开场自由回路」改成：**教室 C 出廊硬锁 → K1 开门仪式 → 延迟召唤老师到走廊巡逻起点 → K2 开 S（必经）→ K3 开 G 区门禁 → 卷栅离场**；W–J–E 大体可走；Print/Shortcut/广播/运卷闩不改钥。

---

## 1. 已批准锁（产品）

| 项 | 内容 |
| --- | --- |
| 硬锁 | `Doors/Classroom`（需 **K1**）；`Doors/Storage`（需 **K2**）；**`Doors/GateAntechamber`**（需 **K3** · **已锁定**） |
| 必经 | **S**（取 K3） |
| 老师 | 首次成功打开 Classroom 后 **延迟** 上场于 **走廊巡逻起点**；**禁止**瞬移进教室 |
| 主环 | W–J–E **大体可走**（最小冲击） |
| 非钥 | Print / Shortcut / 广播 / CM 运卷闩 **不**转钥；M 靠短读软牵引；B/H 可选；H 仅线索动机 |
| 进度 | 钥进 progress；**被抓保留钥与门解锁**；无钥 soft-lock |

### 仍待定 · 设计默认（实现可按此做，ark 可覆盖 · **不是**新批准戳）

| 项 | 默认 |
| --- | --- |
| Phase0 误铃 | **不召唤老师**（本地声 / 远应一声仍不上场 OK） |
| `Doors/Print` | **保持可开** |
| 时长对外口径 | 保持已批 10–15 / 15–25；18–28 仅待测假设 |
| H | §7 线索包装 · 可选 |
| G·K3 节点 | **已锁定** · `Doors/GateAntechamber`（**不是** Gate/Handle）· tip `9121693…` |

---

## 2. 门表（对照 `scenes/chapters/chapter_01.tscn`）

| 节点 | 现况 / 基线 | 落地（feat tip `9121693…`） |
| --- | --- | --- |
| `Doors/Classroom` | 普通门 | **初锁** · **K1** · 首次打开 → 高噪声 + **延迟召唤老师** |
| `Doors/Storage` | 通 S | **初锁** · **K2** · 开锁噪声 |
| `Doors/Print` | 通 M | **保持可开**（默认） |
| `Doors/Shortcut` | `is_shortcut` 进度闩 | **不改钥**；仍 M 侧闩 |
| `Doors/GateAntechamber` | （feat 新建） | **K3 门禁 · 已锁定** · **不是** Gate/Handle |
| `Gate` / `Gate/Handle` | 三档卷栅 | **不**挂 K3；卷栅语义不变 |
| `HatchM` / `HatchG` | 可选夹道 | **不锁钥**；线索包装 |

锁门时禁用对应 `NavigationLink3D`（对齐 Shortcut 既有模式）。

---

## 3. 建议 progress 字段（供选型 · 非强制 API 名）

| 字段概念 | 用途 | 备注 |
| --- | --- | --- |
| `keys` / `keys[]` | 持有 K1/K2/K3 | 被抓 **不删**；读档幂等 |
| `doors_unlocked` / 每门 `unlocked` | Classroom / Storage / G 门禁 | 永久；对齐 shortcut 闩哲学 |
| `teacher_summoned` | 是否已延迟上场 | 与门解锁一致；半链存档必测 |
| （既有）shortcut / gate.stage / … | 不变语义 | 不借钥字段冒充 |

世界交互物：K1（C 讲台抽屉/柜）、K2（J 洗手池柜）、K3（S 教具柜）+ 指向短读。

---

## 4. 老师召唤规则

1. Phase0：**无老师激活**（或 AI 禁用 / 移出导航）。
2. 玩家用 K1 **成功解锁并完成第一次打开** `Doors/Classroom`。
3. 播门噪声（清晰/高等级 → `hear_noise` 路径听力）。
4. **延迟**（草案 2.5–4s，待测）后于 **走廊巡逻起点** 启用老师 + 远处脚步；**禁止**教室中心瞬移。
5. 玩家退回 C 关门：老师仍在廊侧调查门口。
6. Phase0 误响计时器：**默认不召唤**（见上表）。

**不**改教师五态转移数值表（除非开发另开任务）；本卡只规定 **何时上场**。

---

## 5. 防 soft-lock（验收必查）

1. 钥进进度后永不因被抓删除。
2. 门解锁进度永久。
3. 禁止「唯一钥在锁死房且无第二入口」（K3 不在 G 锁后）。
4. 读档恢复：`keys` + 门 `unlocked` + `teacher_summoned` 幂等。
5. 关键路径闭包：仅 Start 不可达 O；+K1 可达廊环；+K2 可达 S；+K3 可达 G→O。

---

## 6. 高可能炸掉的验收夹具（审核已点名）

| 风险 | 夹具 / 假设 |
| --- | --- |
| **高** | Initial route / shortcut-from-classroom / `route_trial`（开场即环假设失效） |
| **高** | 教师延迟到场与门/导航/progress 不同步的夹具 |
| **中** | 「无楔离场」文案 vs S 必经叙事 |
| **盲测** | 钥锁闭环、门声钉老师、半链存档、门声/铃声混淆 |

**acceptance ≠ 乐趣**：需关键路径门禁夹具 + 人工盲测。

---

## 7. 明确 Out of scope（本卡不做）

- **不加第 5 盏 Omni** / 不借本卡改 lookdev 灯数。
- **不恢复** Alert `?`/`!` / suspicion 条 UI。
- **不**由代码 bot **自动改写** `DESIGN.md` 正文；拓扑硬句改写走 `DESIGN_HARD_SENTENCE_PATCHES_GATED.md`，由 ark/开发审后合入。
- **不**把 Print / Shortcut / 广播 / 运卷闩改成钥匙。
- **不**做旧硬 B1（整主环层层锁）。
- **不**扩 Unity 大校园 / 第二敌人 / 对话树。
- **不**改失败证据句全文（可另挂「门声引正门」到失败卡；非本卡阻塞）。

---

## 8. 建议实现顺序

1. Progress / 存档字段 + 门 `key_id`（或等价）扩展（仿现有 `locked` / shortcut）。
2. 初锁 Classroom + K1 放置 + Phase0 无老师。
3. 首次打开 → 噪声 + 延迟召唤（走廊起点）+ `teacher_summoned`。
4. Storage + K2；S 内 K3；**`Doors/GateAntechamber`** K3 门禁（**已锁定**）。
5. 导航链接禁用；被抓保留；半链存档夹具。
6. 短读物 / 空态可读；Phase0 误铃默认。
7. 更新/作废失效 route 夹具；关键路径夹具；再约盲测。

---

## 9. 工期注记（征询摘要 · 非承诺）

游戏开发先前征询：教室锁 + 2～3 钥 + 开门 `hear_noise` ≈ **1–2 人日最小切片**；几何拓扑可不动。最易炸：钥匙 soft-lock、progress↔门/导航/到场不一致、**教师延迟到场夹具**。

**完整含夹具改写与盲测准备可能更长**；本卡不绑定排期日。


---

## 9b. 已知缺口（feat tip `9121693…` merge 后 · 非阻塞）

> 2026-10-28 Asia/Shanghai · 代码已 FF 到 feat tip；下列**不挡** feat 留存，但体验 **NOT PASS** / **未合 main** 前须跟进。

| # | 缺口 | 说明 |
| --- | --- | --- |
| G1 | `route_trial.gd` 未重定向 | 仍带旧「开场即环」假设；须按 gated Phase0 禁锢重写或作废/隔离 |
| G2 | 锁门 navmesh hole cut 未做 | 锁门时导航洞未切；现依赖 NavigationLink 禁用等权宜；正式洞切待做 |
| G3 | 体验 **NOT PASS** | acceptance 夹具绿 ≠ 乐趣；须关键路径人工盲测 |
| G4 | **未合 main** | 仅在 feat 线（`feat/chapter01-gated-min-keychain` / tip 于 `feat/chapter01-module-pass`） |

---

## 10. 协作约定

| 角色 | 做 | 不做 |
| --- | --- | --- |
| 策划（本文作者 bot） | 设计文档 / 任务卡 / 补丁句列表 | 不改代码；不宣称已实现 |
| 游戏开发 | 已定 G·K3=`Doors/GateAntechamber`；跟进已知缺口与夹具 | 不擅自扩钥墙 / 不恢复 Alert UI |
| 游戏审核 | 关键路径夹具 + 盲测 | 不以 acceptance 绿代替乐趣 |
| ark | 覆盖「仍待定」默认；批准 DESIGN 硬句合入 | — |

---

*设计面已「用户已批准」；**代码已落地** feat tip `9121693209ffa42a67c576b47b87ebedb7559978`；**G·K3**=`Doors/GateAntechamber`；见 §9b 已知缺口；**体验未 PASS** / **未合 main**。策划不改 `.gd` / `.tscn` / `DESIGN.md` 正文。*
