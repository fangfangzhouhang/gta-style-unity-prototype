# 游戏光影音效设计 Agent 上下文交接 · AUDIO_LIGHTING_AGENT_CONTEXT_20261028

- **日期**：2026-10-28（Asia/Shanghai）
- **交接目的**：给 **新的**「游戏光影音效设计」agent 一份可独立开工的现状包；**不要**只留本地。
- **本包分支**：`audio/agent-handoff-20261028`（从 feat tip `2344404` 切出；仅加本文档）
- **总分支（集成）**：`feat/chapter01-module-pass` @ **`234440437190c4612ddd80cdc63c0f5e2d06ff4c`**（`2344404`）
- **Repo**：https://github.com/fangfangzhouhang/gta-style-unity-prototype.git
- **硬约束**：**禁止** push / merge **`main`**。体验未 PASS、无 ark Forward+ 笔记前不得宣称耳听/观感 PASS。

---

## 1. Role scope（职责边界）

### 做（In scope）

| 域 | 内容 |
|----|------|
| 实时光照 | `WorldEnvironment`、Omni/Directional 颜色·能量·阴影；暗考场 lookdev；冷/暖对比；Omni shadow **≤4–5** 预算 |
| 音频总线 | `Master` → `Music` / **`Ambient`** / **`SFX`** / **`Sting`**（`default_bus_layout.tres` + `ambience.gd` `_ensure_buses`） |
| Ambient 床 | rain / tube_hum；duck / `set_loop_quiet`；catch sting；`reset_audio_state` |
| 玩法反馈音 | 门声软化、脚步衰减、教师近大远小（仍走 Acoustics/`emit_sound`，床 **不**进 AI 听力） |
| 证据 | 截图 / runtime 证据文档；**人耳 PASS 必须**有 ark Forward+ 实测笔记 |

### 不做（Out of scope）

- 地图拓扑 / NavMesh / 房间连通
- 教师 AI（巡逻速度、FOV、sight、suspicion）
- 玩家移动速度 / 探测半径
- 谜题逻辑、钥匙柜 UX、闸门/门禁逻辑
- **Alert UI 重开**（已确认 Alert-off；危险反馈走非 Alert 的 A/V）
- Unity 工程（**Unity ≠ Godot**；本线只维护 `EscapeExamGodot/`）

### 合并链（handoff 顺序）

```
游戏光影音效设计  →  游戏开发（merge 进 feat）  →  游戏审核  →  ark（Forward+ 人测）  →  main
```

本角色 **直接推 feat 仅限文档/已协调的小补丁**；功能音效修复优先走侧分支 → 请 **@游戏开发** merge 进 `feat/chapter01-module-pass`。**永远不要**自行 merge `main`。

---

## 2. Current baseline（当前基线）

| 项 | 值 |
|----|-----|
| Feat tip | **`2344404`** `fix(godot): lower player footstep atten ~-11 → ~-16 dB` |
| 含 P0 | `aa09ce8` hard gate anti-skip + real key cabinet UX（TECH PASS QA） |
| 含 prologue | `5e82703` ART-PROLOGUE-DYNAMIC（动态镜头）及之前 cams |
| 含脚步 | 玩家脚步 atten **-16 dB**（相对 path 公式结果再减） |
| **不是** | `main` / `origin/main` — 不要从 main 开分支做本章 |
| 游玩入口 | Windows：**`D:\game\play.cmd`**，Forward+ 渲染；box 上 Godot 4.7.2 headless **不能**替代 ark 人耳 |

祖先链摘要（新近 → 旧）：

```
2344404  footstep -11 → -16
5e82703  ART-PROLOGUE-DYNAMIC tip stamp
… 16daf3e prologue dynamic …
aa09ce8  P0 gate/cabinet + footstep -7 → -11（TECH PASS）
… art cams / prologue skip …
（更早：lookdev / coat-rim / horror bed+door soft 等已并入 feat 祖先）
```

---

## 3. Done（已完成，附已知 SHA）

### 3.1 Ambient 床 + 门软化 + 教师脚步曲线

| 项 | 值 / SHA |
|----|----------|
| 分支 | `audio/horror-bed-door-soft`（仍在 origin；内容 **已在** feat 祖先） |
| 代码 tip | **`d05dc66`** `audio: teacher footsteps louder when closer to player` |
| 文档 tip | **`2b95d84`** `docs: restore content SHA in AUDIO_HORROR_BED_DOOR_SOFT` |
| Rain bed | **−9 dB**（`BED_RAIN_DB`） |
| Hum bed | **−13 dB**（`BED_HUM_DB`） |
| Door | soft **−4** clamp **−6**；pitch **0.94**；hinge tween **0.60s** |
| Teacher step | distance→dB 曲线（`teacher_step` / `teacher_step_soft` via acoustics） |

交付文：`EscapeExamGodot/docs/AUDIO_HORROR_BED_DOOR_SOFT_20261028.md`

### 3.2 光照 / 轮廓 / Alert-off 证据

| 项 | SHA / 说明 |
|----|------------|
| Fixture4 coat-rim | **`1af8e06`**（joint S02/S03 证据；分支 `audio/fixture4-coat-rim` origin **gone**，commit **在** 2344404） |
| Cold corridor spill | Fixture5/7 冷出口 rim；见 `AUDIO_COLD_CORRIDOR_SPILL_20261027.md`（`60fd24a` stamp；origin gone，内容在 feat） |
| Alert-off runtime | **`982e98c`** stamp；`AUDIO_ALERT_OFF_RUNTIME_EVIDENCE.md` |
| Mood / lookdev | `AUDIO_LIGHTING_HANDOFF.md` + Fixture4 aim/fill 文档 |
| Omni 预算 | **Omni shadow ≤4 hold**（勿再加阴影 Omni） |
| Art metal keychain | 已更早并入相关 art/feat 线（钥匙链金属观感）；光影侧勿回退 |

**注意**：截图 / runtime 证据 ≠ 人耳/观感 **PASS**。无 ark Forward+ 笔记不得宣称 PASS。

### 3.3 Dev / QA 相关（feat 上）

| 项 | SHA |
|----|-----|
| 玩家脚步 −7 → −11 | 随 **`aa09ce8`**（P0 同提交 message） |
| 玩家脚步 −11 → −16 | **`2344404`** |
| P0 TECH PASS QA | **`aa09ce8`**（acceptance 机检绿；体验仍待 ark） |

---

## 4. Open / unfinished（未完成）

### P1 — 整体 SFX/氛围「人听几乎无听」Mute 诊断（进行中）

- **现象**：代码里 beds / duck / door soft 已存在，但人类试玩仍觉得整体环境声/部分 SFX **听不见**。
- **反证**：玩家 **脚步听得见** → **Master / SFX 总线并非整条死掉**。
- **排查方向（box executor 进行中）**：
  1. **Ambient 路径**：`ambience.gd` 是否真在进章时 `_ready` / 床是否 `play()`；bus 名是否落到 `Ambient`
  2. **`set_loop_quiet(true)`**：广播 stop / 某状态是否把床 fade→stop 后未恢复
  3. **`local_data` / 用户 Master volume**：Windows 存档是否把 Master 或 Ambient 打到极低
  4. **`D:\game\play.cmd` 启动路径**：是否打到旧包 / 非 Forward+ / 非本 tip 构建
- **协调**：另一 executor 可能在 **`audio/mute-fix-player-steps`**（worktree `/workspace/wt-audio-bed-door`）上修 mute — **不要抢同一分支改同一文件**；本交接包只写 CONTEXT，不碰 mute 修复。

### 耳听 / 观感 PASS

- 仍 **需要 ark Forward+** 笔记与实测。
- Experience **NOT PASS**；**无 ark → 不上 main**。

---

## 5. Future work（后续工作清单）

1. **完成 mute 根因修复** → push `audio/mute-fix-player-steps`（或等价分支）→ 请 **@游戏开发** merge 进 `feat/chapter01-module-pass`。
2. Mute 修好后做 **全链路耳听 QA**：beds 可闻 → duck → sting → reset/stop 广播后恢复；记录 dB/主观。
3. **P0 危险 A/V 反馈**（**不**重开 Alert UI）— 与 art 联合：抓到/逼近时的视觉+Sting/SFX。
4. 若光照再改，**刷新三mood样张**（暖窗 / 冷廊 / 中间态）。
5. **永不**在无实测下宣称 ear PASS。

---

## 6. Local worktrees on box（路径）& remotes

共享 box；多 agent 共用文件系统。相关 worktree：

| Path | Branch / HEAD | origin 状态 | vs `2344404` |
|------|---------------|-------------|--------------|
| `/workspace/wt-feat-integrate` | `feat/chapter01-module-pass` @ 2344404（本包切 `audio/agent-handoff-20261028`） | tracking | **baseline** |
| `/workspace/wt-audio-bed-door` | `audio/mute-fix-player-steps` @ 2344404 | → feat tip；**留给 mute 修复** | 无独有 commit |
| `/workspace/wt-audio` | `audio/chapter01-lookdev-ambience` @ 139cec8 | **gone** | 已祖先于 2344404 |
| `/workspace/wt-audio-joint` | `audio/fixture4-coat-rim` @ 1af8e06 | **gone** | 已祖先于 2344404 |
| `/workspace/wt-audio-spill` | `audio/cold-corridor-silhouette-spill` @ 60fd24a | **gone** | 已祖先于 2344404 |
| `/workspace/wt-audio-f4` | `audio/fixture4-west-fill` @ bc10edc | **gone** | 已祖先于 2344404 |
| `/workspace/wt-audio-f4-aim` | `audio/fixture4-aim-coat` @ 6b05191 | **gone** | 已祖先于 2344404 |
| `/workspace/wt-audio-evidence` | `audio/alert-off-runtime-evidence` @ 982e98c | **gone** | 已祖先于 2344404 |
| `/workspace/wt-art` | `art/prologue-dynamic` @ 5e82703 | behind feat 1 | prologue 已在 feat |
| （qa detached）`/workspace/wt-qa-*` | 各类 tip 快照 | n/a | 只读证据用 |

**Re-push 策略（本包执行时）**：

- 上表 **gone** 侧分支经 `git merge-base --is-ancestor <tip> 2344404` **全部已在 feat** → **无需**为独有 commit 重推。
- 仍在 origin 的音频相关：`origin/audio/horror-bed-door-soft`、`origin/feat/chapter01-module-pass`。
- **最低保证**：本 CONTEXT 文档必须在 GitHub（本分支 + 尽量 cherry-pick 到 feat）。

---

## 7. 仓库内关键文档（已有）

路径均相对 `EscapeExamGodot/docs/`：

| 文档 | 用途 |
|------|------|
| [`AUDIO_LIGHTING_HANDOFF.md`](./AUDIO_LIGHTING_HANDOFF.md) | MODULE B lookdev + bus + ambience 总交接 |
| [`AUDIO_HORROR_BED_DOOR_SOFT_20261028.md`](./AUDIO_HORROR_BED_DOOR_SOFT_20261028.md) | 床 −9/−13、门软化、教师脚步曲线 |
| [`AUDIO_FIXTURE4_COAT_RIM_20261027.md`](./AUDIO_FIXTURE4_COAT_RIM_20261027.md) | Fixture4 coat rim |
| [`AUDIO_FIXTURE4_AIM_20261027.md`](./AUDIO_FIXTURE4_AIM_20261027.md) | Fixture4 aim |
| [`AUDIO_FIXTURE4_FILL_20261027.md`](./AUDIO_FIXTURE4_FILL_20261027.md) | Fixture4 west fill |
| [`AUDIO_COLD_CORRIDOR_SPILL_20261027.md`](./AUDIO_COLD_CORRIDOR_SPILL_20261027.md) | 冷廊 silhouette spill |
| [`AUDIO_ALERT_OFF_RUNTIME_EVIDENCE.md`](./AUDIO_ALERT_OFF_RUNTIME_EVIDENCE.md) | Alert-off runtime 证据 |
| [`AUDIO_JOINT_COAT_F4_RECHECK.md`](./AUDIO_JOINT_COAT_F4_RECHECK.md) | coat+F4 联合复核 |
| [`QA_CHAPTER01_MODULE_PASS_20261027.md`](./QA_CHAPTER01_MODULE_PASS_20261027.md) | 模块 PASS QA 记录 |
| **本文** [`AUDIO_LIGHTING_AGENT_CONTEXT_20261028.md`](./AUDIO_LIGHTING_AGENT_CONTEXT_20261028.md) | 新 agent 总上下文（2026-10-28） |

---

## 8. 给下一任 agent 的起步检查清单

1. `git fetch origin`；从 **`origin/feat/chapter01-module-pass`（2344404 或更新 tip）** 开侧分支，**不要**从 `main`。
2. 读本文 §3–§5；再读 `AUDIO_LIGHTING_HANDOFF.md` + `AUDIO_HORROR_BED_DOOR_SOFT_20261028.md`。
3. 若修 mute：先确认 `/workspace/wt-audio-bed-door` / `audio/mute-fix-player-steps` 是否已有进行中改动，**避免冲突**。
4. Windows 验证用 **`D:\game\play.cmd` Forward+**；box headless 只做机检。
5. 交付走：侧分支 push → @游戏开发 merge feat → @游戏审核 → ark →（仅 ark 后）main。
6. Omni shadow 保持 ≤4；Ambient 床永不 `emit_sound` 进 AI。

---

## 9. English summary (secondary)

**Role:** realtime lighting + Ambient/SFX/Sting + play-feedback audio. **Not:** map topology, AI, player speed, puzzles, cabinets/gates, Alert UI reopen. Unity ≠ Godot. Pipeline: lighting/audio → 游戏开发 → 游戏审核 → ark → main.

**Baseline:** feat tip **`2344404`** (P0 `aa09ce8`, prologue `5e82703`, footsteps −16). Play via `D:\game\play.cmd` Forward+. **Not main.**

**Done:** horror beds −9/−13 + soft door + teacher distance curve (`d05dc66` / `2b95d84`); Fixture4 coat-rim `1af8e06`; cold spill; Alert-off evidence; Omni≤4; footsteps −7→−11→−16.

**Open:** P1 ambience still inaudible on human play despite code (mute diagnosis in progress; footsteps prove Master/SFX alive). Ear/visual PASS need ark Forward+. Experience NOT PASS.

**Next:** finish mute root-cause on `audio/mute-fix-player-steps` → merge to feat; full ear QA; P0 danger A/V without Alert UI; refresh mood shots if lighting changes; never claim ear PASS without measured playtest.

**Remotes:** several `audio/*` origin refs are **gone** but commits are ancestors of `2344404` — no unique re-push required. This CONTEXT doc must live on GitHub.
