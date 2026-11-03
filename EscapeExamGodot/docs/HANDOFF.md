# HANDOFF — 《逃离考场·最后一遍铃》开发交接（2026-11-02 首版，2026-11-03 更新推送状态）

> 给接手本项目的 AI 编程助手（Claude Code / Codex 等）：读完这份文档，你就拥有与上一任
> 助手（Muse）相同的项目上下文。先读仓库根 `AGENTS.md` 与 `EscapeExamGodot/AGENTS.md`
> （协作边界以那两份为准），再读 `docs/v2/V2_DESIGN_BIBLE.md`（设计圣经），然后看本文的
> 「未决事项」决定你要做什么。**任何动手前，先向 ark 确认任务范围。**

## 项目一句话

Godot 4.7.2 第一人称微恐潜行游戏：学生在考场用延时计时器 + 分区广播的声音诱导躲避
监考教师，分段抬起出口卷栅离场。参考《你好，邻居》（只借"观察追捕者、声诱、绕路"的
玩法骨架）。仓库名 `fangfangzhouhang/gta-style-unity-prototype` 是历史遗留，Unity 已弃用，
本体在 `EscapeExamGodot/`，不要碰任何 Unity 目录。

## 代码在哪、是什么状态

- 本机（ark 的电脑）源：`D:/Unity/EscapeExamRepository/EscapeExamGodot`（以仓库 AGENTS 为准）。
- `main` @280acb9 是 A-M2.1 旧基线。**红线：未经 ark 本人批准，不得合并或推送 main；
  不得用 main 旧基线覆盖 feat 的开发成果。**
- 开发基线：`feat/chapter01-module-pass` @895f985（原自动验收 95/95，但人工体验 NOT PASS、无人工盲测）。
- V2 集成：`claude/v2-redesign` @f4b2ee5（四域合入 + QA 报告）。
- **当前最新：`claude/v2-model`（V3 建模提交 e9f8712，其上还有若干文档提交，tip 以 git log 为准）**（在集成之上完成 V3 教师建模），从这里继续。
- 其余 claude/v2-* 分支是 V2 的分域工作分支与 QA 分支（v2-teacher-ai / v2-audio / v2-visual /
  v2-story / v2-qa），内容均已合入集成，不要再往这些分支提交。
- **推送状态（2026-11-03 晚）：7 个 claude/v2-* 分支（含完整提交历史）已经上一任助手
  送进游戏本的本地 clone（`D:\game\gta-style-unity-prototype`），只差最后一步 push：**
  该机 Git 凭据管理器里没存 GitHub 登录，远程无头会话弹不出登录窗，必须在交互终端
  跑一次（首次会弹浏览器登录）：
  `git push origin 'refs/heads/claude/*:refs/heads/claude/*'`
  ——由 ark 本人跑，或由你（Codex 在桌面交互会话里）代跑，效果相同。推完后接手
  第一步：`git fetch origin` → `git checkout claude/v2-model`，即是最新工作线。
  （上一任助手所在 VM 无 GitHub 凭据、其 SSH 密钥未获 ark 的 GitHub 账号授权，
  直推与连接器快照推送都走不通，才用的这个借道方案；本地若还留有
  escape-exam-v2.bundle，那是离线备份。）

## 设计口径（不可自行推翻，要改先问 ark）

自有风格「终场秩序」——中国考场秩序恐怖：
- 配色：荧光绿白 × 试卷米白 × 印章红。**印章红是唯一高饱和色，只属"秩序"**
  （红笔、红笔标记、印章），不得拿它做装饰或 UI 强调。
- 教师是认真执行"考试未结束"的普通人：平静、公事公办的台词，不是鬼、不是疯子。
  恐怖感来自秩序本身，不来自怪物化。
- 招牌意象「红笔标记」：教师在门框打勾/划叉、留"已查"纸条——他的适应与反制行为
  必须经红笔/纸条**可视化**给玩家，不许做成不可见的数值惩罚。
- 借鉴纪律：第五人格只借信息层（张力信息传递方式），邻居只借适应系统思路，
  异形隔离只借双脑结构（信念 + 导演）；**只借标准与结构，不借皮**——不复制任何
  现成游戏的角色、美术、台词、关卡。

## V2 系统地图（都在 claude/v2-redesign 及之后）

- **教师三层脑**（`scripts/actors/teacher.gd` + `scripts/systems/`）：
  `belief.gd` 房间图离散贝叶斯信念；`director.gd` 紧张度导演（按 NavigationServer 路径长
  算 menace，节拍 calm/build/peak/release；无遭遇 ≥40s 给房间级提示、冷却 45s；
  menace ≥0.85 持续 5s 下撤离令）；`player_profile.gd` 跨局玩家热力（只启用一件反制：
  最高热房间首查）。三者在 teacher `_ready` 自注册。
  **公平红线（已过 QA 逐行审计）：全栈只有 director 读玩家真实位置，且只输出房间名；
  任何新感知/反制都必须遵守——教师只能用"能合法知道"的信息行动。**
  对外接口：`teacher.get_tension()`（0–1），音效与画面都消费它，不要另建张力来源。
- **自适应音效**：`scripts/systems/adaptive_audio.gd`（挂 `/root/Ambience/AdaptiveAudio`），
  分层音乐 + 心跳 BPM 50–125 随 tension 变，脱战 7–8s 慢衰减；`SFXOccluded` 总线
  （LowPass 800Hz + Reverb wet 0.18）；`scripts/systems/acoustics.gd` 是原有的房间-门口
  声学传播图（项目亮点），**播放器池化可以动，传播逻辑不要动**。
  程序生成音效在 `tools/gen_v2_audio.py`（纯 stdlib，种子固定、逐字节可复现，
  新增音效必须在脚本里补生成函数并登记 ASSET_CREDITS）。
- **画面氛围**：`scripts/systems/visual_mood.gd`（chapter.gd `_ready` 自注册）：指数雾、
  曝光/饱和随 tension、逐灯闪烁、后期（暗角/颗粒/色差）、浮尘与窗外雨幕。
  当前数值是**待真机校准的初值**，不是定稿。
- **新机制**（`scripts/actors/player.gd`、`scripts/systems/red_marks.gd`、
  `scripts/props/broadcast.gd` 及 props/chalk）：
  探头 lean（Q/E，±0.35m，shapecast 撞墙截断；调查键由 E 不变、蓄力键改 R）；
  粉笔投掷（讲台 2 支，右键投，落地发 `kind="chalk"` 声响，`Game.chalk_count`/CHALK_MAX=2，不入存档）；
  红笔标记严重度 severity = gate_stage + teacher_summoned + shortcut + loop_stopped
  + min(caught, 2)，clamp 0–5；广播终局异变（loop_stopped ∧ gate_stage≥2 触发一次，
  标记存 `Game.facts["broadcast_glitched"]`）；序章 P06 加了 door_shadow_keys 节拍
  （P01–P09 的 VO 断言一个字都不要动）。

## V3 教师模型（最新工作）

- `assets/models/teacher/teacher_model.glb`：Blender 真建模的教师全身（V2 只是材质级
  改版，已被 ark 否决，不要再走"只改材质/几何块"的路线）。
- **模型是脚本生成的**：`tools/model_teacher.py`（Blender 4.2 无头：
  `blender --background --python tools/model_teacher.py -- assets/models/teacher`）。
  改模型 = 改脚本参数后重生成，**不要直接手编 glb**。
- 接入：teacher.tscn 的 `Visual` 改为实例化该 glb；节点名/层级/枢轴与旧版一致
  （髋 1.05、头 2.02；肩由 ±0.34 内收至 ±0.325 是模型内部调整），teacher.gd 动画零改动兼容。
- 建模的坑（脚本注释与 `docs/v2/MODEL_V3.md` 有全记录，别重踩）：Blender→Godot 朝向
  镜像标定；裤腿顶端必须止于髋下否则穿出大衣；Blender 测试渲染左髋暗影是测试光源
  投影、不是穿模（已排除法确认，勿追）。
- 设计标准参考第五人格监管者（约瑟夫/红夫人）：只借比例（约 8 头身）、收腰剪影、
  苍白空白脸、一主色+金属点缀四条纪律；身份道具是红笔/密封试卷筒/带齿钥匙串。
  已知未完：手部未分指（长掌+指块）。

## 验证方法（结论口径，违反 = 交付不合格）

- 自动验收（必须带 `-- --test`，裸跑会让 prologue 污染夹具）：
  `godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd -- --test` → 基线 95/95；
  `... -s res://tests/v2_acceptance.gd -- --test` → V2/V3 专项 24/24。
  或用仓库的 `tools/run.ps1 -Mode Check`。**不得让测试覆盖玩家存档（用隔离存档）。**
- **自动测试 ≠ 人工试玩**：报告结论必须分开写。当前体验侧 11 项未验证（见下），
  不许把"测试全过"说成"体验已过关"。
- 截图验证的坑（任何 agent 都会踩）：① 截图脚本必须先 `game.test_mode = true` 再
  `start_new()`，否则进序章污染机位；② 教师未被召唤时被 game.gd 停在 (0,−40,0) 且
  invisible，摆拍要手动 `visible = true` 并摆位；③ 无 GPU 的机器上用
  xvfb + `--rendering-driver opengl3`（gl_compatibility）截图，截出的不是 Forward+
  真机效果，汇报时必须标注渲染器，不得报"视觉 PASS"。
- `tests/route_trial.gd` 中 broadcast 策略失败是**既有 G1/G2 债务**（已在未改基线上
  对照复跑证实），不是 V2/V3 回归，不要顺手去"修"它，除非 ark 单独布置。

## 未决事项（需要 ark 拍板或真机，不要自行决定）

1. **K2 钥匙绑巡逻升级的数值口径**：设计已定（钥匙链改"节拍器不是清单"，见
   `docs/v2/PLOT_V2.md`），具体数值等 ark 拍板。
2. **真机试玩清单（Forward+）**：S01–S05 机位画面校准（教师脸部可读性、红笔在背光侧
   是否醒目是已知弱点）、耳机混音与心跳距离感、探头与调查同为 E 键附近的手感、
   完整一局的压迫感与红笔标记可读性、单局时长计时。
3. Director 会向教师提示玩家**当前准确房间**——是否太强，列为盲测重点观察项，
   等盲测数据再调，不要先改。
4. `red_marks.gd` 的标记与巡逻系统的 "mark" flavor 目前是两套未连通的系统（已知留观）。
5. 章节钥匙链与 DESIGN 当初"反对三把钥匙"的立场冲突已按"节拍器"方案处理，
   若 ark 改口径，以 ark 最新说法为准并回写 PLOT_V2.md。

## 候选任务（ark 确认后可直接开工）

- 教师手部分指细化（改 model_teacher.py 的手部段，重生成 + 重跑验收 + 同机位截图对比）。
- 配合 ark 真机数据做 VisualMood / 混音数值校准（只调数值，不动结构）。
- 按盲测结论调整 Director 房间提示的粒度（房间 → 区域）。

## 上一任助手的身份说明

V2/V3 由 Muse（Meta 的个人 agent，运行在 ark 的云端 VM 上）完成，与你（本机
Claude Code / Codex）不共享记忆；本文件就是全部交接。ark 的长期偏好：执行/汇报要
简短直接、先做事再报结果；研究/设计类要讲透（原理、对比、标注不确定性）；
交付不许用"差不多/基本重合"式表述，数字要逐项对账。
