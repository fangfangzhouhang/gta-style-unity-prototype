# 【光影音效交接】下一任 Agent 上下文 · 2026-10-28

- **角色**：@游戏光影音效设计 → 下任同角色 / 接手 mute+lookdev
- **日期**：2026-10-28（Asia/Shanghai）
- **团队分支**：`feat/chapter01-module-pass`（**勿合 main**；路径：游戏开发 fast-merge → 游戏审核 → ark → main）
- ****本交接推送分支**：`audio/mute-fix-player-steps` tip `a833318`（可 ff-merge 进 feat）
- **feat tip（写本文时）**：`55d491f`（音频相关落地 tip `2344404` 脚步 −16） — `fix(godot): lower player footstep atten ~-11 → ~-16 dB`
- **仓库**：`https://github.com/fangfangzhouhang/gta-style-unity-prototype.git`
- **Godot 工程**：`EscapeExamGodot/` · Godot **4.7.2** Forward+
- **紧急背景**：usage 告急；光影音效进度必须在 GitHub，禁止只留 box 本地。

---

## 1) 已在 feat / 已合并（Done）

| 交付 | 代表 SHA / 说明 | 状态 |
|------|-----------------|------|
| Ambient 恐怖床抬高 | `c4d08e8` rain **−14→−9** / hum **−18→−13**；仍只走 Ambient、不进 AI | **在 feat** |
| 门声软化 + 铰链变慢 | 同床批次 / horror-bed；door 额外衰减 + pitch 0.94 + hinge 0.45→0.60s | **在 feat**（本 mute 分支把门额外 −4/−6 **调为 −2/−3**） |
| 教师脚步近大远小 | `d05dc66`；chase hard / 其它 soft；距离 lerp −2…−26；**source_id=teacher 不进自身 hear** | **在 feat** |
| Alert-off 运行证据 | `982e98c` 等；夹具 beds/duck/Alert hidden；**≠ ear PASS** | 内容已合 feat；原 remote 标 gone |
| Fixture4 / coat-rim / cold spill lookdev | `bc10edc` F4 fill · `6b05191` F4 aim · `1af8e06` coat-rim joint · `60fd24a` / `139cec8` cold spill | **在 feat**（Omni 影子灯 **≤4 hold**） |
| 玩家脚步 −16 | **`2344404` 游戏开发** −11→−16；`acoustics.gd` `source_id == "player": volume_db -= 16` | **在 feat** — **下任勿再改除非耳听回退** |
| 前情动态 / cams | art：`5e82703` ART-PROLOGUE-DYNAMIC 等叠在 aa09ce8 上 | **在 feat** |
| P0 柜/闸防跳 | `aa09ce8` | **在 feat** |

**侧分支说明**：`audio/horror-bed-door-soft`、`audio/alert-off-runtime-evidence`、`audio/fixture4-*`、`audio/cold-corridor-*`、`audio/chapter01-lookdev-ambience` 等相对 feat **ahead=0**（内容已合）；部分 origin 已 prune 为 gone — **不必强推旧 tip**，以 feat 为准。

交付文档目录（均在 `EscapeExamGodot/docs/`）：

- `AUDIO_HORROR_BED_DOOR_SOFT_20261028.md`
- `AUDIO_ALERT_OFF_RUNTIME_EVIDENCE.md`
- `AUDIO_FIXTURE4_*.md` / `AUDIO_COLD_CORRIDOR_SPILL_20261027.md` / `AUDIO_JOINT_COAT_F4_RECHECK.md`
- `AUDIO_MUTE_FIX_PLAYER_STEPS_20261028.md`（本轮 mute 硬化）
- `AUDIO_LIGHTING_HANDOFF.md`（总线图旧稿）

---

## 2) 进行中 / 未完成（In progress）

### Mute / 听不见 SFX+床（ark A1）— 本分支部分硬化

**现象（ark playtest）**：交互/门/环境几乎听不到；玩家脚步却很响 ⇒ Master/SFX **未全死**。

**已排查（代码，非 Windows 终局）**：

1. Bus layout 正常；脚步证明 SFX→Master 通。
2. 相对混音：历史脚步过响 + door 过软 + 床连续噪声源低 → 主观「无 SFX」。
3. `enter_b0`（Skip 前情）原先不 `Ambience.reset_audio_state` → Ambient 可能卡在 duck **−22**。
4. `试玩.cmd` 把 `APPDATA` 指到 `local_data`；设置里 Master 可存 **−30**。
5. 床 WAV 曾 `compress/mode=2`（IMA-ADPCM）长循环 — Windows 播放风险。

**本分支已做硬化**（待 ark 耳听）：`enter_b0` reset 床；清 Ambient/SFX mute；Master ≤−24→0；door −2/−3；床 import PCM；**未动** −16 脚步行。

**仍待**：

- ark 真机 `D:\game\play.cmd`（或包内 `试玩.cmd`）Forward+ 耳听：床 / 门 / 闸 / notice。
- 若仍无床：抓 `local_data/**/settings.cfg`、Godot 音频设备、Ambient bus 运行时 dB。
- key_pickup 开柜/取钥 **无** `emit_sound`（柜出 scope；若 ark 仍报「交互无声」可能要另卡）。
- **禁止**在无录音证据时写 ear PASS。

### Lookdev 未闭环

- Omni≤4 hold，勿再加影灯。
- Forward+ 真机 mood 与 xvfb 截图不等价。
- 教师剪影冷廊可读性：spill/rim 已合，耳听+眼看联调未最终签。

---

## 3) 未来工作（P0 / P1 / P2）

### P0（光影音效 · 阻塞体验）

1. **Mute 闭环**：跟 ark 复测本 `audio/mute-fix-player-steps`；若仍无声，按 settings/设备/总线采样下钻；必要时再抬床或交互 oneshot（仍不进 AI）。
2. **玩家脚步 −16 耳听确认**（已由开发改码；光影只收听感，勿擅自再降除非 ark 点名）。
3. **门/闸/广播** 在 Skip→B0 后可听；Ambient duck 后必回 0。
4. 交接/合并：本分支 → **游戏开发** ff-merge 进 `feat/chapter01-module-pass` → 游戏审核 → ark → **勿自行合 main**。

### P1

1. 开柜/拾取反馈音（与柜 UX 协同；勿改钥链逻辑）。
2. 教师脚步 vs 床 平衡微调（保持距离曲线结构）。
3. Lookdev：S01/S02 样本室与章内灯一致性抽检；截图机位用既有 SHOT_BLOCKS。
4. 广播 LoopAudio（场景内 Ambient −24 chime）与 Ambience 雨/hum 是否叠糊 — 听感整理。

### P2

1. 雨窗/局部床（设计稿有；勿抢 P0）。
2. Music bus 是否需要极低频垫 — 默认不要盖过脚步可读。
3. 导出/发包：确保 `.godot/imported` 或首次 import；`local_data` 勿携带 −30 volume。

---

## 4) 约束（硬）

| 禁止 | 说明 |
|------|------|
| 改教师 AI 数值 / FOV / 速度 / 五态表 | 除非另卡 |
| 改房间拓扑 / NavMesh / 钥链 / 门 progress id | 剧情/开发卡 |
| 恢复 Alert `?`/`!` UI | ark 已关 |
| 床进 `acoustics.emit_sound` / AI 听力 | 雨/hum **永不** |
| 合 **main** | 只推侧支或交开发合 feat |
| Unity `Assets/` 大改 | Godot 章独立 |
| 无证据 ear PASS | box=dummy audio |

**路由**：光影音效侧支 → **游戏开发** merge feat → **游戏审核** → **ark** → main。

---

## 5) 如何试玩

1. **代码**：`feat/chapter01-module-pass` tip（或已 merge 的 `audio/mute-fix-player-steps`）。
2. **Windows 包**：`D:\game\play.cmd` 或工程内 `EscapeExamGodot\试玩.cmd`（`APPDATA`→`local_data`）。
3. **渲染**：项目 Forward+（勿用 Compatibility 当最终观感）。
4. **流程**：标题 → 开始新的试走 → Skip 前情 → B0 教室；听雨/hum；开门；走几步对比脚步与床。
5. **设置**：Esc → 设置 → 音量靠近 0；若曾拉到头，删 `local_data` 下 `settings.cfg` 再开。
6. **机检（box）**：
   ```bash
   godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd
   ```
7. **证据夹具（旧）**：`tests/audio_alert_off_evidence.gd` — bus/playing 断言，**不是**耳听。

---

## 6) Worktree / 推送速查

| 路径 | 分支 |
|------|------|
| `/workspace/wt-audio-bed-door` | `audio/mute-fix-player-steps`（本交接） |
| `/workspace/wt-feat-integrate` | `feat/chapter01-module-pass` |
| `/workspace/wt-audio*` | 历史 lookdev/evidence（ahead 0） |

```bash
git fetch origin
git checkout audio/mute-fix-player-steps
git log -1 --oneline   # 应交 tip SHA
# 开发合并示例：
git checkout feat/chapter01-module-pass
git merge --ff-only audio/mute-fix-player-steps
git push origin feat/chapter01-module-pass
```

**Remote**：`https://github.com/fangfangzhouhang/gta-style-unity-prototype/tree/audio/mute-fix-player-steps`

---

## 7) 一句话给下任

Mute 不是「总线全灭」；feat 上已有床抬高/门软/教师曲线/F4/Alert-off 证据/脚步 −16。本侧支硬化了进 B0 复位床、Master 误存、door 略抬、床 PCM。下一步 **只做 ark 耳听闭环 + 让开发 ff-merge feat**，不要合 main，不要动 AI/拓扑/Alert。
