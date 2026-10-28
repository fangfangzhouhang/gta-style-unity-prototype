# 《逃离考场》第一章 · 开发 Agent 额度交接（2026-10-28）

> 时区：Asia/Shanghai  
> 作者角色：**游戏开发**（技术负责人 / 集成；本机 Git，不合 `main`）  
> 仓库：`https://github.com/fangfangzhouhang/gta-style-unity-prototype`  
> 工程子目录：`EscapeExamGodot/`  
> 协作房间：群「游戏」  
> 触发：ark 额度交接指令（push 进展 + 书面上下文给新 agent）

本文是给**下一个开发 / 集成 Agent** 的单一入口。角色专属交接另见：

- 审核：`docs/CHAPTER01_AGENT_HANDOFF_20261028.md`
- 情节/机制：`docs/design/AGENT_HANDOFF_PLOT_MECHANICS_20261028.md`
- 光影音效：`docs/AUDIO_LIGHTING_AGENT_CONTEXT_20261028.md`

---

## 1. 角色与门禁

| 角色 | 职责 | 硬边界 |
|------|------|--------|
| **开发（本角色）** | 技术负责人整合；旁支合入 `feat/chapter01-module-pass`；acceptance；本机/真机可复现 | 不擅自合 `main`；不整文件覆盖他人域 |
| **审核** | 独立复验 tip（绑 SHA）；TECH / 体验分栏 | 不自改被审核心玩法/美术；不合 `main` |
| **ark** | 产品拍板；体验 PASS；**合 `main` 唯一批准人** | 合 `main` 须 **ark + 审核** 都过 |
| 情节 / 美术 / 光影音效 | 各域文档与旁支；经开发集成进 feat | 文档完成 ≠ 代码完成 ≠ 体验通过 |

**本轮不合 `main`。** `main` ≈ `280acb9`（清 Unity 基线）；验收与推进一律钉 feat。

流程：

```
设计/旁支 → 开发集成 → acceptance → 游戏审核 → 人工/ark 体验 → ark 批准 → 才合 main
```

---

## 2. 当前总分支与 tip

| 项 | 值 |
|----|-----|
| **总分支** | `feat/chapter01-module-pass` |
| **交接时运行时基线 tip（代码）** | `234440437190c4612ddd80cdc63c0f5e2d06ff4c`（短 `2344404`）— 脚步 ≈−16 dB |
| **交接时 feat tip（含文档）** | 以 push 后 `origin/feat/chapter01-module-pass` 为准；本文件随 docs 提交叠在 `2344404` 之上 |
| **祖先要点** | `aa09ce8` P0 门禁+柜；`5e82703` 前情动镜；`9121693` 最小钥匙链；`46f9c27` Skip 骨架 |
| **main** | `280acb99dd72b913d00166d906ca0f1268f49fca` — **禁止 merge / push / force-push** |

### 如何开玩

- 玩家机：`D:\game\play.cmd`（尽量 **Godot 4.7.2 Forward+**）
- 开发：打开 `EscapeExamGodot/project.godot`；可用 `GODOT_BIN` + `tools/run.ps1 -Mode Editor`
- 钉 tip 验收，勿默默跟到无关清理提交（曾有 `e0a2cbd` 清 Unity 杂项，**不要钉它做真机**）

产品方向（已锁）：**最小钥匙链** `C → K1 → 出廊 → K2 → S → K3 → G`；S 必经；老师延迟上场；**G·K3 = `Doors/GateAntechamber`**。

---

## 3. 已合入进展时间线（feat）

| Tip（短） | 全长 SHA（前缀） | 内容 |
|-----------|------------------|------|
| `9121693` | `9121693209ffa…` | 最小钥匙链 gated（K1/K2/K3 + 延迟老师） |
| `46f9c27` | `46f9c271fb47a…` | 前情 Skip 骨架《还没响的铃》→ B0 |
| `9588e50` | `9588e50e494d0…` | 裸跑缺 `--test` 时强制 `test_mode`（acceptance 兜底） |
| `d6fec5e` / `d73f56e` | ART-PROLOGUE-CAMS | P01–P09 机位定稿 |
| `aa09ce8` | `aa09ce8805489…` | **P0**：无 K3 拒开 GateAntechamber / Hatch；柜 `open_first`；jamb 防 clip |
| `016bf4f` | playtest patch 销钉 | ark 真机反馈设计进 docs |
| `16daf3e` → `5e82703` | ART-PROLOGUE-DYNAMIC | 镜内 Tween + 镜间 crossfade |
| **`2344404`** | **`234440437190c4612ddd80cdc63c0f5e2d06ff4c`** | 玩家脚步 atten ≈−11 → **≈−16 dB** |
| `3516334` / `55d491f` / `27e83e0` | docs | 情节 / 审核 / 音效角色交接文档（无代码） |

另有已合美术/音频较早 tip：S01 转角样板、钥点金属读、大衣 S02、horror bed/door soft、冷廊 spill、Fixture4 系列等（多已祖先于 feat；部分旁支远程名已删但 tip 仍在 feat 可达）。

---

## 4. 验收命令与期望

```bash
godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd -- --test
```

或：`tools/run.ps1 -Mode Check`（隔离测试存档）。

| 期望 | 说明 |
|------|------|
| **95/95** | 当前绿线 |
| **必须带 `-- --test`** | CI/脚本写死；裸跑会 WARN + 强制 `test_mode`（自 `9588e50`），仍应 95/95，但勿依赖裸跑 |
| 不得覆盖玩家存档 | 用隔离 userdata |

TECH PASS ≠ EXPERIENCE PASS ≠ 可发布。

---

## 5. 未完成 / 进行中

| 项 | 状态 | 谁跟 |
|----|------|------|
| 审核复验 tip `2344404`（动镜+脚步+门禁） | 曾启动因交接中断 → **必须重跑** | 审核 |
| SFX「听不见」（非脚步） | 从 `2344404` 开光影旁支查；可能尚未交稿合 feat | 光影音效 → 开发合入 |
| 脚步 ≈−16 耳听 | 代码已上；待 ark 真机确认 | ark |
| 前情动镜观感 | 盒端 gl_compat **不报视觉 PASS**；等 Forward+ | ark / 美术 |
| 体验 PASS | **NOT PASS**；等 ark 盲测 | ark |
| `dev/fail-evidence-lines` @ `f708e50` | 远程可见；**尚未合 feat**（feat…该支 ≈ ahead62/behind1）→ **待 ark 批准再合** | 开发（等 ark） |
| G1/G2 `route_trial` + Classroom navmesh 跨锁洞 | 已知技术债 | 开发排期 |
| P1 场景拆分 / P3 代码健康 | 未开 | 开发 |
| 西廊 / ear 等氛围细项 | 部分证据在旁支；真机未锁 | 美术/音效 |
| 合 `main` | **禁止**，直至 ark 明确批准 | — |

---

## 6. 明确禁止

1. **不合 / 不 push / 不 force-push `main`**
2. **无审查勿整文件覆盖** `teacher.tscn` / `chapter_01.tscn`（优先小 diff、复用现有系统）
3. 优先复用：钥匙/门/柜/声学/ambiance/prologue 既有管线；勿平行重写
4. 不要用 CloudAgent 对本仓做「合 main」或擅自开 PR 合主干
5. Unity 根目录 `Assets/` 等只读；工作目录永远是 `EscapeExamGodot`
6. 文档/进度 **禁止只留本地** — 必须进 Git 并 push 到远程可见分支

---

## 7. 新 Agent 第一步清单

1. 读 `AGENTS.md` + `PROJECT_STATUS.md` + **本文**
2. 选读角色交接：审核 / 情节 / 音效三份 HANDOFF（见文首）
3. `git fetch origin`；checkout / ff `feat/chapter01-module-pass`；确认 tip ≥ `2344404`
4. 跑 acceptance（`-- --test`）确认仍 95/95
5. **等审核复验报告 + ark 体验结论** 再动玩法/合旁支；SFX 旁支交稿后再集成
6. 任何合 `main` 意图先停，问 ark

本机常见 worktree（Bot）：`/workspace/wt-feat-integrate`（集成）；另有 `wt-audio*`、`wt-art`、`wt-gated*`、`wt-qa*` 等，**不要无故整仓新 clone**。

---

## 8. 未来工作优先级建议（短）

1. **光影音效**：根治 SFX 听不见 → 合 feat → 耳听销钉  
2. **审核**：完成 `2344404+` TECH 复验（SHA 绑定）  
3. **ark**：Forward+ 复测柜 / 大门 / Skip / 脚步 / 动镜 / SFX → 体验结论  
4. **开发**：按新反馈开旁支；`fail-evidence` 仅 ark 批后合 feat  
5. **G1/G2**：`route_trial` 过时 + navmesh 锁洞（不挡体验 PASS 也可排）  
6. **P1 场景拆分 / P3 健康度**：体验 PASS 之后  
7. **合 main**：仅 ark 书面批准后由开发执行  

暂缓：第二敌人、大扩建、大规模 AI、全面重写玩法。

---

## 9. 旁支远程可见性（交接时核对）

已在 `origin` 且 tip 对齐或可达：

- `feat/chapter01-module-pass`（总）
- `art/chapter01-module-pass`、`audio/horror-bed-door-soft`
- `feat/chapter01-gated-min-keychain`、`docs/chapter01-gated-min-keychain`
- `dev/fail-evidence-lines`（**未合 feat**）

若干历史 `audio/*` / `feat/prologue-skip-skeleton` 远程分支名曾删，但 tip **已祖先于 feat**，提交仍远程可见。无「仅本地、远程不可达」的相关 tip（交接时核实）。

本地 `main` 可能 behind `origin/main` 1 — **只允许 ff 本地，禁止向 main push**。

---

## 10. 本文件写入说明

- 路径：`EscapeExamGodot/docs/HANDOFF_CHAPTER01_AGENT_20261028.md`
- 随 `feat/chapter01-module-pass` commit + **push origin**（不合 main）
- 新 tip 请在 §2 表追加一行，勿 silently 改历史 tip 语义
