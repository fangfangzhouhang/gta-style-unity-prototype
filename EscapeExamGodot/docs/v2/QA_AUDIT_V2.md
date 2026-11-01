# V2 集成独立复验报告（QA_AUDIT_V2）

> 审核方：独立审核 agent（未参与任何一域开发）。立场：各域自报与集成提交说明均为被审对象，本报告只认本次实测与代码证据。
> 被审对象：集成分支 `claude/v2-redesign` tip `b3cfc7d`（四域合并 + 集成提交），基线 `73d72b3`（圣经）/ `895f985`（feat 基线）。
> 环境：本机 Linux，Godot 4.7.2 headless。审核日期：2026-11-02。
> 权限执行情况：未修改任何游戏代码/场景/测试；路线与音频证据均在 /tmp 临时副本中运行，临时基线 worktree 已清理；`--import` 产生的 .import 副产物已全部还原/清理，工作区除本报告外干净。

## 总判定

**TECH：PASS（附条件）** —— 既有验收全绿、无回归、公平红线守住；但有 2 个中级问题（chalk 素材来源登记断裂、V2 新机制零新增验收断言）必须在考虑合 main 之前清掉。
**体验：未验证** —— 本报告不对画面质感、听感、压迫感、难度、时长下任何结论，清单见 §8，全部留 ark 真机。

---

## 1. 验收复跑

| 项 | 结果 | 证据 |
|---|---|---|
| `--import` | 成功（exit 0） | 本机实跑 |
| `tests/acceptance.gd -- --test` | **PASS** | 原文：`RESULT passed=95 failed=0`；exit 0；日志中 SCRIPT ERROR / Parse Error 计数 = 0 |
| `tests/audio_alert_off_evidence.gd` 非截图部分 | **PASS（部分）** | 在 /tmp 副本运行：断言 **27 PASS / 0 FAIL**（Alert 图标三态隐藏、环境床真安静/重启、sting duck −22.00 dB、reset 恢复等）后，脚本进入截图阶段（其自身注释即称纯 headless 下可能失败），等待视口帧超过 3 分钟无输出，被审核方终止。**截图部分未完成，不计 PASS 也不计 FAIL。** |

附带观察（低）：acceptance 与 route 运行退出时均有 `ObjectDB instances were leaked` / `resources still in use` 警告。此为 feat 基线 KNOWN_ISSUES 已记录的既有现象，非 V2 新增，但长会话影响仍未验证（见 §8）。

## 2. 路线回归对照（集成者声称"broadcast 策略失败在未改基线上同样复现"）

**判定：声称属实（PASS）——route_trial 本身在两版上同等失败，属既有债务，非 V2 回归。**

在 /tmp 临时副本分别实跑（均 `-- --test`）：

| 策略 | 集成分支 b3cfc7d | 未改基线 73d72b3 |
|---|---|---|
| broadcast | `ROUTE_RESULT {"captures":0,"gate":0,"seconds":18.579,"strategy":"broadcast","success":false}` | `ROUTE_RESULT {"captures":0,"gate":0,"seconds":18.581,"strategy":"broadcast","success":false}` |
| timer | `{"captures":0,"gate":0,"seconds":18.581,"strategy":"timer","success":false}` | `{"captures":0,"gate":0,"seconds":18.579,"strategy":"timer","success":false}` |

失败模式亦相同：两版日志均为 `ROUTE_STUCK`——脚本玩家在从 (-3.30, 1.15) 移向 (-1.75, 1.1) 一步卡住（路径存在、移动停滞），与 feat 交接文档记录的 G1/G2 `route_trial` 技术债一致。**结论：route_trial 当前在任何版本上都不能用作通关证据；它恢复可用之前，"脚本通关"这一证据类型整体缺席。**

## 3. 公平红线代码审计（全仓 `Game.player` 读取点逐处定性）

**判定：PASS（附 1 条提示）。** 教师决策路径未发现违规读取。

| 文件:行 | 读取内容 | 定性 |
|---|---|---|
| `scripts/systems/belief.gd` 全文 | **零** `Game.player` 读取（仅注释声明） | ✅ 信念层只收教师感官喂入的 `observe_sighting/noise/absence`；`best_search_rooms(n, from_pos)` 的 from_pos 是教师自身位置（belief.gd:160） |
| `scripts/systems/director.gd:143-155` | `room_at(Game.player.global_position)`、`map_get_path(..., Game.player.global_position)` | ✅ 圣经许可的唯一例外：仅 Director 内部量路径长度（menace）与玩家房间名（提示）；提示门槛 quiet ≥40 s + 冷却 45 s（director.gd:24-25），输出只有房间名字符串，坐标不离开该节点 |
| `scripts/systems/player_profile.gd` 全文 | **零** `Game.player` 读取 | ✅ 只存教师亲历事件（目击/抓捕的房间，由 teacher 以 `room_at(last_seen)` 传入）；反制恰一件（首查房间）、巡逻 dwell 偏置上限 ×2.0、无加速/扩感知（player_profile.gd:37-45） |
| `scripts/actors/teacher.gd:121-133` | 玩家位置/蹲姿/lean 偏移 → 视线射线目标点 | ✅ 感官（射线命中才算看见） |
| `scripts/actors/teacher.gd:209-241` `_exposure_rate()` | 玩家位置/速度/蹲姿 | ✅ 感官精细化——仅在当帧 `visible==true` 分支内被调用计 suspicion（teacher.gd:247）；注：它读的是真值而非感知采样值，但门控在已确认目击之内，不构成决策作弊 |
| `scripts/actors/teacher.gd:320-322` | 追击截击读 `Game.player.velocity` | ✅ 仅 `visible and visible_time>0` 时生效，lead ≤0.35 s；断视线立即回落 last_seen |
| `scripts/actors/teacher.gd:379` | 抓捕判定读玩家实际距离 | ✅ 既有契约（基线同逻辑），且要求 visible + chase；profile 记的是 `room_at(last_seen)` 而非实时位置 |
| `scripts/actors/teacher.gd` 搜索/巡逻/反制 | search 队列自 belief 效用排序 + profile 反制房间（teacher.gd:190-213）；detour = `belief.room_center(hint_room)`（:296-298）；retreat = 以教师自身位置算最远巡逻点（:306-316）；dwell 偏置自 profile 热度 | ✅ 全部源于信念/亲历统计/自身位置 |
| `scripts/actors/teacher.gd:43,277` `tension` | 只镜像 director.tension 并经 `get_tension()` 供音画消费 | ✅ 教师本体不以 tension 做任何决策 |
| 其余文件 | props 的交互距离/手持物变换（door/exam_timer/door_wedge/broadcast 距离门）、acoustics 的听者位置（混音/字幕方位）、adaptive_audio:121-141 与 visual_mood:69 的自算兜底（表现层）、prologue 相机开关、hud 设置项 | ✅ 均为既有契约/表现层，不回流教师决策 |

提示（非违规，提请 ark 知情）：Director 的房间提示给的是玩家**当前所在的准确房间**（observe_noise 0.5 置信 + patrol 时直接 detour 到该房间中心）。圣经许可"只到房间粒度"，现实现在许可之内，但它是本作公平体系里最"全知"的一处，盲测时应重点观察玩家是否觉得"他总能找对楼层"。

## 4. 跨域契约核对

| # | 契约 | 判定 | 证据 |
|---|---|---|---|
| ① | `teacher.get_tension()` 真实存在且被 adaptive_audio、visual_mood 消费 | **PASS** | teacher.gd:474-475 定义；teacher.gd:277 每 tick 镜像 `director.tension`；adaptive_audio.gd:109-110 与 visual_mood.gd:62-65 均在 game 态调用 `Game.teacher.get_tension()`，域外/无教师时回落自算 |
| ② | 粉笔 kind `"chalk"` 已在 acoustics CLIPS 注册且有 wav | **PASS** | acoustics.gd:19 `"chalk": preload("res://assets/audio/chalk.wav")`；文件实测存在：0.22 s / 22050 Hz / 单声道 / 16-bit（与全仓合成音规格一致） |
| ③ | `"keys"` 只有 AdaptiveAudio 一处发射 | **FAIL（字面不符，影响低）** | 全仓 `emit_sound(... "keys" ...)` 共 **两处**：adaptive_audio.gd:202（source_id="teacher"）与 **prologue.gd:188**（source_id="prologue"，V2 情节域 P06 节拍新增）。acoustics.gd:14-16 注释"AdaptiveAudio 节流发射"与事实不符。影响分析：prologue 发射时教师处于 frozen（hear_noise 在 teacher.gd:136 对 frozen 早退）且前情先于上场/重置，当前流程下无玩法副作用；但该发射 source_id≠"teacher"，一旦 frozen 门控变化，它会成为真实的教师听觉输入——契约表述必须修正为两处并注明门控依赖 |
| ④ | teacher.tscn 的 $Visual 节点名与 teacher.gd 引用一致 | **PASS** | teacher.gd 全部引用 {$Visual/LeftLeg, RightLeg, LeftArm, RightArm, Head}（teacher.gd:422-432）在 teacher.tscn 中逐一存在（tscn:241/254/267/280/329）；视觉域新增 Coat/KeyChain/RedPen 等节点为纯场景节点，无脚本引用可断裂 |
| ⑤ | red_marks 触发条件在正常流程可达 | **PASS（附设计保真提示）** | severity()（red_marks.gd:36-46）= gate_stage（ratchet_gate.gd:34 写实进度，0-3）+ teacher_summoned（game.gd:258 置位）+ shortcut + loop_stopped（broadcast.gd:38）+ min(caught,2)，上限 8；doors 组由 door.gd:31 运行时注册，多扇门可标。仅抬栅一档即出第一枚标记，可达性无问题。提示：实现是**进程驱动、按门名字母序落点**，并非圣经字面的"教师巡查/怀疑过哪里就标哪里"——TECH 无碍，保真度问题记入 §7/§8 |
| ⑥ | 广播异变标记存 facts 与 `_read_valid_save` 校验兼容 | **PASS** | broadcast.gd `_maybe_glitch` 写 `Game.facts["broadcast_glitched"] = <字符串>`；game.gd:426-428 校验只要求 facts 为 String→String 且 key ≤128、值 ≤4096——key 长 18、值约 24 字，兼容；触发门控（loop_stopped + gate_stage≥2 + 靠近面板或操作面板 + 只触发一次）代码可推演可达 |

附带发现（低）：adaptive_audio.gd:4 与 visual_mood.gd:11-12 的文件头注释仍写"自己算 tension、不依赖其他 agent"，与集成后实际消费 Director 的代码不符；prologue.gd:185 注释"kind 未注册时回退 door"亦已过时（CLIPS 已注册）。均为文档漂移，不影响运行，但会误导后续接手者。

## 5. 许可与素材

| 项 | 判定 | 证据 |
|---|---|---|
| ASSET_CREDITS 登记 V2 音频 5 件（heartbeat/keys/pulse_loop/tension_strings/drone_loop） | **PASS（强证据）** | ASSET_CREDITS 新增 V2 表逐项登记；审核方实跑 `tools/gen_v2_audio.py` 重新生成，**5 个文件与仓库内 wav 逐字节 MD5 完全一致**——程序生成属实、固定种子可复现 |
| chalk.wav 来源 | **FAIL（登记断裂，中）** | chalk.wav **不在** ASSET_CREDITS V2 表中，也**不是** gen_v2_audio.py 的产物（该脚本只生成上述 5 件）。它由集成提交 b3cfc7d 加入；提交说明与 acoustics.gd:18 注释均称"合成参数见集成提交说明"，但该提交正文除标题外无任何参数记录——来源链断裂。其格式与全仓合成规格一致（见 §4②），疑似自制合成，但按本项目 ASSET_CREDITS 纪律"授权/来源未核实不得进入交付"，在补登记（生成方法/参数/作者）之前应视为未登记素材 |
| 其他新增文件 | **PASS** | `git diff 73d72b3..HEAD --stat` 全量核对：新增二进制仅 6 个 wav；其余为自有代码（post_process.gdshader、surface 改动）、自建场景（teacher.tscn 几何、chalk.tscn）与文档。无外部贴图/模型/字体混入。注：视觉域新 shader 未在 ASSET_CREDITS 另行登记（自有代码，低） |

## 6. 分支纪律

| 项 | 判定 | 证据 |
|---|---|---|
| tests/ 未被改动 | **PASS** | `git diff 73d72b3..HEAD -- EscapeExamGodot/tests/` 为空；`git diff 895f985..HEAD -- EscapeExamGodot/tests/` 亦为空。acceptance 断言一字未动 |
| 未合 main | **PASS（附证据范围说明）** | 本克隆无 main 引用可直接比对；就谱系证据：v2 线为 895f985（feat tip）→ 73d72b3 → 四域提交 → 四次域合并 → b3cfc7d，日志中无任何 main 合并提交，HEAD 不被 v2 之外任何分支包含 |
| 圣经 §6"新增机制同步加验收项" | **未执行（中）** | tests/ 零 diff 的另一面：Belief/Director/Profile/探头/粉笔/红笔/广播异变等全部 V2 新机制**没有任何新增验收断言**。§1 的 95/95 是**基线断言集**在新代码上仍通过，证明的是"没弄坏旧的"，不是"新的对了"。集成提交说明中的"95/95"表述本身属实，但各域与集成方均未注明这一口径差 |

## 7. 问题清单（按严重度）

| 严重度 | 问题 | 位置/证据 |
|---|---|---|
| 中 | chalk.wav 来源未登记：不在 ASSET_CREDITS、不由 gen_v2_audio.py 生成、声称的"集成提交说明"中无合成参数。补生成脚本/参数与登记，或替换为可复现生成 | b3cfc7d；acoustics.gd:18；ASSET_CREDITS V2 表缺行 |
| 中 | V2 新机制零新增验收断言，违反圣经 §6 自定纪律；当前自动证据覆盖不到三层脑/探头/粉笔/红笔/异变 | tests/ 对两基线均零 diff |
| 中（既有债务，非 V2 引入） | route_trial 两策略在基线与集成版上同等失败（ROUTE_STUCK），脚本通关证据整体缺席；G1/G2 债务未清 | §2 对照表 |
| 低 | "keys 只有一处发射"与事实不符（prologue.gd:188 为第二处），且 acoustics 注释误导；该处对 frozen 门控的隐性依赖应写明 | §4③ |
| 低 | 文档漂移：adaptive_audio/visual_mood 文件头仍称自算 tension；prologue.gd:185 注释称 keys 未注册 | §4 附带发现 |
| 低 | red_marks 为进程驱动+字母序落点，与圣经"教师查过/怀疑处留标记"的招牌语义有距离；教师 patrol 的 "mark" flavor（teacher.gd patrol_flavor）与 red_marks 系统互不连通 | red_marks.gd:36-70；teacher.gd:_start_patrol_flavor |
| 低 | 运行退出 ObjectDB/resource 泄漏警告持续出现（基线既有） | §1 附带观察 |

## 8. 体验未验证清单（headless 无法证明，必须 ark 真机/人工，一条无结论）

1. **画面质感**：VisualMood（雾/曝光/灯管闪烁/后期颗粒-暗角-色差）在 Forward+ 下的实际观感与数值校准——本轮及各域截图均为 gl_compatibility/xvfb，只证管线。
2. **教师形象可读性**：新剪影（大衣/苍白脸/红笔/钥匙串）在真实光照下 8 米外读朝向/状态、压迫感是否成立。
3. **听感**：自适应音乐三层（drone/pulse/strings）与心跳的混音平衡、keys 签名声可闻性、SFXOccluded 低通/混响的遮挡感、脚步声新参数——全部未过人耳。
4. **探头手感**：Q/E 探头与 E 轻点交互同键的区分是否误触、探头读危险是否真的可用。
5. **粉笔 utility**：抛物线落点可控性、声诱是否真能调动教师、与计时器的决策分工是否成立。
6. **对手压迫感与公平感**：Belief 搜索、Director 提示节奏（40 s/45 s）、跨局反制在真实对局中是"聪明"还是"作弊感"；§3 提示的准确房间提示尤其需要盲测观察。
7. **红笔标记可读性**：Forward+ 下门框标记是否看得清、玩家是否读得懂其含义。
8. **广播异变节拍**：触发时机、文案与声效的实际冲击力。
9. **时长与难度**：完整一局是否落在 15–25 分钟、被抓率与重试动机——无任何人工数据。
10. **性能与长稳**：目标硬件（RTX 5060 笔记本等）帧时间、分辨率适配、长会话下泄漏警告是否演变为实际问题。
11. **存档实玩链**：带 broadcast_glitched fact 的存档读档、player_profile 跨局衰减/反制在真实多局中的表现（代码审计通过 ≠ 实玩验证）。

---

## 附：本次实测命令与产物

- `--import` + acceptance：wt-qa 本地实跑，`RESULT passed=95 failed=0`（日志 /tmp 留存于审核会话，不入库）。
- audio_alert_off_evidence：/tmp 副本运行，27 PASS / 0 FAIL 后截图阶段挂起被终止。
- route_trial（broadcast/timer × 集成/基线）：/tmp 副本四跑，ROUTE_RESULT 见 §2；临时基线 worktree 已 `git worktree remove` 清理。
- gen_v2_audio.py 复现：/tmp 生成 5 文件，MD5 与仓库逐一一致。
