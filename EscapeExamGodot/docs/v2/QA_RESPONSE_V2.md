# QA 审计处置 · 集成方回应（对 QA_AUDIT_V2.md）

> 审计：`claude/v2-qa` @f4e8615，总判定 TECH PASS（附条件）。以下为集成方（claude/v2-redesign）在推送前的逐项处置。审计报告原文不改动。

## 中级问题

1. **chalk.wav 来源链断裂 → 已清**。粉笔声的合成函数已补入 `tools/gen_v2_audio.py` 的 `chalk()`（种子 20261102，参数注释在脚本内），重跑生成与仓库文件**逐字节 MD5 一致**（1fa90fe14f3b3a4d2d4bb1235a944e7d）；ASSET_CREDITS 的 V2 素材表已补登记该文件。此前其余 5 个 V2 wav 经审计实跑验证逐字节一致，本次复验亦然。
2. **V2 新机制零新增验收断言 → 已清**。新增独立专项 `tests/v2_acceptance.gd`（不改动原 95 项断言集），覆盖：三层脑自注册、Belief 目击坍缩/噪声观测/搜索队列、Director tension 接口与范围、Profile 热力与反制接口、AdaptiveAudio/VisualMood 自注册、chalk/keys 的 CLIPS 注册、探头偏移与回正、粉笔拾取/投掷计数、红笔随抬栅第一档出现、广播异变触发。实跑结果：**RESULT passed=24 failed=0**；同轮基线验收复跑 **95/95**。
3. **route_trial 两策略全版本失败（既有债务）→ 接受审计结论，不在 V2 修**。审计已独立证实基线同败。列入后续技术债：route_trial 更新 + Classroom navmesh 跨锁洞（原 HANDOFF 的 G1/G2 项）。

## 低级问题

- **keys 两处发射与注释不符 → 已据实改注释**（acoustics.gd CLIPS 处注明 AdaptiveAudio 节流发射 + prologue P06 一次性节拍发射，两处均 source_id="teacher"）。行为不改：prologue 那处是情节设计（开场门外钥匙声），且受教师 frozen 门控无副作用——审计亦确认当前无实际副作用。
- **文件头注释漂移 → 已改**（adaptive_audio.gd、visual_mood.gd 的 tension 来源说明已更新为集成后的 Director 接管口径）。
- **red_marks 与 patrol "mark" flavor 两套标记互不连通 → 已知，留待真机校准后统一**（当前红笔标记=进程可视化，巡逻 mark=教师口味停顿，两者语义不同；盲测若证明玩家混淆，再合并为同一系统）。
- **退出时 ObjectDB 泄漏警告 → 基线既有**（KNOWN_ISSUES 已载），不在 V2 范围。

## 审计提示的留观项（记入真机清单）

- Director 提示为玩家**当前准确房间**（圣经许可内最"全知"的一处）：盲测重点观察是否被玩家感知为作弊；若是，第一刀砍它（改邻接房间模糊提示）。
- 体验栏 11 项全部未验证，以推送时给 ark 的真机清单为准，谁都不许替 ark 下结论。

## 处置后状态

- 基线验收 95/95 + V2 专项 24/24（本机 Godot 4.7.2 headless 实跑）。
- TECH 附条件两项中级问题已清；route_trial 债务与体验验证如实保留为未完成项。
