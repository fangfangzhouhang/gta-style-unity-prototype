# QA · feat/chapter01-module-pass @ a7427d8

- **日期**：2026-10-27（Asia/Shanghai）
- **审核**：游戏审核 · godot-game-review + stealth / level-design / puzzle 检查点
- **结论**：**打回**（未 OK；必须改项见下，改完再喊审）

## 环境

Godot **4.7.2** · `forward_plus` · viewport 1280×800 · stretch `canvas_items` · box headless 复跑夹具

## 六项门禁

| 项 | 结果 | 证据 |
| --- | --- | --- |
| launch | PASS | acceptance boot |
| render | PASS* | 夹具；暗场景人工/截图仅部分（见 limitation） |
| input | PASS | 夹具 crouch / interact 路径 |
| coreLoop | PASS | 38/38；无楔路径仍通关 |
| outcome | PASS | chapter_complete 经抬栅穿越 |
| restart | PASS | catch recovery / save load |

\* 耳听停播 duck / 刺音、正面 `?`/`!`、冷廊剪影：本轮无完整人工试玩证据。

## 检查点对照

| 点 | Skill | 判定 |
| --- | --- | --- |
| 1–2 声诱/视线/门 | stealth | 骨架夹具 PASS；P1 ajar/peek **无夹具**；雨窗曲柄按约定未做 (P2) |
| 3–5 环路/节奏/关键路径 | level-design | 主环代码仍在；15–25 分钟未验（limitation） |
| P0–P3 依赖 | puzzle | S/楔/架/H 已灰盒进仓；**可选非前置**仅由「旧 38 项不碰 P1」间接证明，缺显式断言 |

## 问题（按严重度）

### [Major] P1 灰盒零夹具覆盖
- **现象**：`tests/acceptance.gd` 无 wedge / Storage / PushableRack / Hatch 断言。
- **复现**：通读 acceptance；本轮复跑仍 `passed=38 failed=0`，但无 P1 名。
- **影响**：KNOWN_ISSUES 要求的「无楔无暗道通关 / ajar 不可挤 / H 可逆」无法回归锁死，后续合并易 silently 破约定。
- **归属**：code · @游戏开发
- **必须改**：至少补 3 条夹具——(1) 从未拾楔仍可抬栅离场；(2) ajar 下角色胶囊不可穿门；(3) HatchM↔HatchG 开盖后双向可达（或等价 nav/位置断言）。

### [Minor] 冷廊剪影糊墙（已知 follow-up）
- **现象**：S02/S03 复检：无暖光垫底时大衣剪影易糊进墙。
- **归属**：art + audio/lighting · 已认领 Fixture5·7 侧溢
- **必须改**：落地一笔 `audio:`/`art:` 或写入 KNOWN_ISSUES 并附截图前后对比后再喊终审。

### [Minor] 警戒 `?`/`!` 正面未复检
- **现象**：art 自报后视角未看清 Alert；合进 feat 后未补正面 evidence。
- **归属**：art
- **必须改**：`evidence/screenshots/` 补一张正面/近距 `?` 与 `!`。

### [Nit / Limitation] 失败证据句未挂 UI
- plot docs 成文 OK（P0 约定只交 docs）；`teacher.gd` 仍用旧 generic evidence。
- **不挡本轮 P1 灰盒**，但列入下一切片：按 `PLOT_P0_BEATS_AND_FAIL_LINES.md` 挂接。

### [Limitation] 未验
人工 15–25 分钟乐趣、耳听 duck/sting（box 无可靠听感）、跨分辨率/安全区、雨窗 P2。

## 已核对为符合设计的点（不打回）

- 门楔 `AJAR_ANGLE≈17°` + toast「挤不过去」；教师会踹开楔防 AI soft-lock
- 床音注释与实现不进 acoustics AI 事件
- `Game.caught → Ambience.play_catch_sting`；broadcast ↔ `set_loop_quiet`
- 阴影 Omni=4；雨窗曲柄未做符合 P2
- 教具室 S / 推架 / 双 Hatch 节点在 `chapter_01.tscn`

## 下一步

@游戏开发 按 Major（+ 两条 Minor）改完并 push → 再 priority 喊 @游戏审核。通过前 **不要** 合 `main`。
