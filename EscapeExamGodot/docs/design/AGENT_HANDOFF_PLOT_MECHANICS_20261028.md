# 《逃离考场》情节/机制设计 — 新 Agent 交接上下文

> 日期：2026-10-28（Asia/Shanghai）  
> 作者角色：游戏情节和机制设计（首席策划 / 叙事 / 关卡；只交设计，不改代码）  
> 仓库：`fangfangzhouhang/gta-style-unity-prototype`  
> **总分支（当前集成，不合 main）**：`feat/chapter01-module-pass`  
> **交接基准 tip**：`2344404`（叠前情动态 `5e82703` + P0 硬门禁 `aa09ce8` + 脚步 ≈−16）  
> **main**：禁止合入，直至 ark 体验 PASS 明确批准

本文是给**下一个情节/机制 Agent**（以及需要策划上下文的开发/审核/美术/音效）的单一入口。实现仍归 @游戏开发；本角色不写 `.gd` / `.tscn` / 不覆盖根 `DESIGN.md`。

---

## 1. 角色边界（硬）

- **可做**：读代码/拓扑/QA 报告；写 `EscapeExamGodot/docs/design/` 提案、节拍表、文案、任务卡；群「游戏」用【策划交付】模板交活。
- **不可做**：改 `.gd` / `.tscn` / save schema；改老师 AI 参数；私自加硬挂锁钥匙；推翻已批准核心环；自动合 GitHub；把「设计文档完成」标成「代码完成 / 体验通过」。
- **协作**：统一走 开发集成 → 审核 → 用户批准 → 再合 main。文档完成 ≠ 代码完成 ≠ 测试通过 ≠ 体验通过。
- **五 Agent 规则 V2.0（ark 2026-10-27）**：本轮只打磨 Godot 第一章可玩体验；P0 稳定/审核/人工试玩 → P1 危险反馈/轻漫画样板/声光统一 → P2 谜题节奏/可选机关；暂缓第二敌人、大扩建、大规模 AI、全面重写玩法；Unity 与 Godot 禁止未批准跨项目覆盖。

---

## 2. 已锁定产品方向（勿回退）

| 锁定项 | 内容 | 状态 |
|--------|------|------|
| 第一章进度幻想 | **最小钥匙链 gated**：C 出廊硬锁 + K1/K2/K3；S 必经；老师首次开 Classroom 后延迟走廊巡逻上场 | ark「用户已批准」2026-10-28；**推翻** 2026-10-27 自由回路 A |
| 强制链 | `C → K1 → K2 → S → K3 → G`；无 K3 不得开 GateAntechamber / Hatch 捷径 | 销钉 + 代码 tip `aa09ce8` TECH PASS |
| 时长框 | 熟练 10–15 / 首通目标 15–25（仍待真机校准） | 已框，体验未 PASS |
| 类型 | Godot 4.7.2；A-M2.1「最后一遍铃」；第一人称潜行 + 声饵谜题（非战斗）；Hello Neighbor 式空间/风险回报 | 长期 |
| 叙事手段 | 短读物/物件；无对话树、无多结局任务树 | 已定 |
| 通关骨架 | 计时器 → 运卷 → 广播 → 卷栅 → 离场 | 已定（实现归开发） |

**未批准 / 勿当已定**：手册五态软补偿；半锁方案；维持自由回路 A。

---

## 3. 已完成（策划侧，已在 Git）

| 文档 / 交付 | 路径 / tip | 说明 |
|-------------|------------|------|
| 最小钥链重设计 + 批准戳 | `docs/design/CHAPTER01_GATED_PROGRESSION_REDESIGN.md` | 产品方向 |
| 实现任务卡 | `docs/design/CHAPTER01_GATED_IMPL_CARD.md` | 给开发 |
| 空间解锁 A/B（A 已废） | `docs/design/SPACE_UNLOCK_AB.md` | 横幅注明 A  superseded |
| 前 10–15 分钟节拍表 | `docs/design/CHAPTER01_BEATS_10_15.md` | 对照活代码 |
| 失败证据句任务卡 | `docs/design/FAIL_EVIDENCE_IMPL_CARD.md` | 可交开发，非本批 P0 |
| 硬句补丁（gated） | `docs/design/DESIGN_HARD_SENTENCE_PATCHES_GATED.md` | 建议句，勿当已合 DESIGN.md |
| 前情 VO P01–P09 定稿 | `docs/design/PROLOGUE_VO_FINAL_20261028.md`（设计 tip 曾 `c7c6a50`） | 已交美术 Timeline |
| 真机试玩补丁销钉 | `docs/design/CHAPTER01_PLAYTEST_PATCH_20261028.md` @ `9b872df`（docs 旁支）/ 内容已进 feat | P0 门禁+柜；P1 前情动/音效条 |
| P0 代码落地 | tip **`aa09ce8`** | open_first 柜取钥；无 K3 拒 G/Hatch/jamb；验收 95/95 |
| 前情动态 | tip **`5e82703`**（镜缓推+镜间淡入淡出，Skip 可用） | 对上销钉 P1；视觉 PASS 等 ark 真机 |
| 脚步 ≈−16 dB | tip **`2344404`** | 开发交付；音效仍查「听不见 SFX」 |

**审核状态（截至交接）**：`aa09ce8` P0 **TECH PASS**；体验 **NOT PASS**（等 ark 盲测）；不合 main。复验钉 tip `2344404`（动镜+脚步+门禁）。

---

## 4. 未完成 / 仍开（按优先级）

### P0（挡体验 PASS / 合 main）

1. **ark 真机再盲测** tip `2344404`（至少覆盖：钥链是否卡死、柜→钥、无 K3 终门、Skip→B0、脚步是否仍炸耳、前情是否还像幻灯片）。
2. **SFX 听不见** — @游戏光影音效设计 从 `2344404` 旁支查根因；合回 feat 后再审。
3. 体验未 PASS → **禁止合 main**。

### P1（本轮可做、非挡门）

1. 前情动态真机观感（是否仍像幻灯片）— 审核盒端不报视觉 PASS，以 ark 为准。
2. 失败证据句落地（状态/触发/误判/回退/学习）— 文档已有任务卡，待开发排期；勿在未批准下扩玩法。
3. 危险反馈 / 轻漫画样板 / 声光统一 — V2 P1 桶，需与美术/音效对齐，情节只出 brief。

### P2 / 暂缓

- 谜题节奏细调、可选机关（雨窗曲柄等曾标 P2）。
- 第二敌人、大扩建、大规模 AI、全面重写玩法。
- Handbook 五态软补偿（仍 **待批准**）。

### 已知技术债（非情节独有，交接时知悉）

- 历史缺口：`route_trial`、Classroom 锁洞 nav（曾记于钥链落地后）。
- 盒端 gl_compat 等导致视觉 NOT PASS — 不由策划改渲染。

---

## 5. 给下一个情节 Agent 的「未来工作」清单

按顺序做，**不要私自改产品锁**：

1. **跟盲测**：ark / 审核对 `2344404` 的体验结论出来后，只分拣「设计问题 vs 实现问题」；设计缺口再写销钉补丁（新 `CHAPTER01_*_PATCH_*.md`），仍交开发，不改代码。
2. **体验 PASS 后**：协助写「可合 main 条件」核对表（钥链、Skip→B0、SFX、脚步、前情动、acceptance），交 ark 拍板；**情节不得自行合 main**。
3. **失败证据句**：若 ark 批准进本轮，把 `FAIL_EVIDENCE_IMPL_CARD.md` 按活代码再对一版触发表，交开发。
4. **P1 危险反馈 brief**：短文案 + 状态映射（勿扩第二敌人）。
5. **时长校准**：盲测后若熟练/首通偏离 10–15 / 15–25，出一页节拍微调建议（加减诱饵/读物密度），仍要 ark 批。
6. **保持文档为唯一真相源**：本地草稿必须进 `docs/design/` 并 push 到 `feat/chapter01-module-pass`（或文档旁支再合进 feat）；**禁止只留本地**。

---

## 6. 关键文档索引（`EscapeExamGodot/docs/design/`）

- `CHAPTER01_GATED_PROGRESSION_REDESIGN.md` — 钥链产品锁  
- `CHAPTER01_GATED_IMPL_CARD.md` — 开发实现卡  
- `CHAPTER01_PLAYTEST_PATCH_20261028.md` — 真机反馈销钉  
- `CHAPTER01_BEATS_10_15.md` — 节拍  
- `PROLOGUE_VO_FINAL_20261028.md` — VO  
- `FAIL_EVIDENCE_IMPL_CARD.md` — 失败句  
- `SPACE_UNLOCK_AB.md` — A/B（A 已废）  
- `DESIGN_HARD_SENTENCE_PATCHES_GATED.md` — 建议硬句  

根目录另有美术/音效/QA 文档（`ART_*`、`AUDIO_*`、`QA_*`）；情节 Agent 可读，勿覆盖他人域文档除非协作约定。

---

## 7. 群协作短约定

- 交活前缀：`【策划交付】`  
- 点名：开发落地、审核 TECH、美术镜头、音效听感；体验最终由 **ark** 定。  
- tip 以 feat 最新为准；交接时钉 **`2344404`**，后续 tip 请在本文件顶部「交接基准 tip」行追加一行更新（新 Agent 接手后维护）。

---

## 8. 一句话现状

> 最小钥链已 TECH PASS（`aa09ce8`+叠层至 `2344404`）；前情动与脚步已上 feat；SFX 听不见与 ark 盲测仍开；**体验未 PASS，不合 main**。下一任情节 Agent 从盲测分拣与文档销钉继续，不改代码、不回退钥链锁。
