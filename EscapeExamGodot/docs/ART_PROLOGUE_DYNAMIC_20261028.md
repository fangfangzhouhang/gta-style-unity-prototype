# 【美术交付】前情镜头动态 · ART-PROLOGUE-DYNAMIC

- **任务 ID**：`ART-PROLOGUE-DYNAMIC`
- **分支**：`feat/chapter01-module-pass`（worktree `/workspace/wt-art/EscapeExamGodot` · 本地跟踪名 `art/prologue-dynamic`）
- **本交付 tip**：`16daf3e`（全长 `16daf3e2799dd9a5c6a165870b403d3bcafbeeaa`）· message 含 `ART-PROLOGUE-DYNAMIC`
- **叠放基线 tip**：`aa09ce8`（开发 P0 Gate anti-skip + Cabinet UX；含 CAMS `d73f56e`/`d6fec5e`）
- **对齐策划**：`docs/design/CHAPTER01_PLAYTEST_PATCH_20261028.md` §5 P1（文档 tip `9b872df` / feat 上 `016bf4f`）
- **日期**：2026-10-28（Asia/Shanghai）
- **范围**：**只改** `prologue.gd` 镜内运动 + 镜间短 crossfade + P07 头部微动 Visual；capture/证据/本交付文档
- **状态**：盒端 gl_compat 多帧证据已拍 · **不报视觉 PASS** · Skip→B0 语义未改

---

## 1. 问题与目标

ark 实机：「前情动画全是静态」。本交付在**现有每镜时长**内做可读、轻量的动态镜头语言（轻漫画微恐；不抄资产）：

1. **镜内微动**：Tween 驱动 `Camera3D` `eye→eye_end` / `look→look_end` / 可选 `fov→fov_end`（Cubic ease）
2. **镜间过渡**：短 crossfade（`fade_prologue` 约 0.16s 出 + 0.22s 入），避免硬切幻灯片感
3. **P07**：教师 `Visual/Head` 极轻 yaw 微动（纯 Visual；`frozen`；**非** jump-scare）
4. **保留**：定稿九句 VO、Skip→B0、Alert hidden、P07 非跳脸、不响铃

---

## 2. 每镜运动（对齐 playtest §5.3）

| Shot | 微动 | 实现要点 |
| --- | --- | --- |
| **P01** ★ | 极缓推近雨窗（穿窗感） | CAMS 起 → 略向雨幕推进；FOV 58→56 |
| **P02** | 轻摇跟随注目点 | 同窗侧微横移→室内倒影 |
| **P03** ★ | 轻摇/缓移对准校牌 | 略偏起 → CAMS 对准 `WClockLabel` |
| **P04–P05** | 轻推露纵深 | 近读缓推；FOV 变幅 ≤1.5° |
| **P06–P07** ★ | 慢推压迫（克制） | P07 略远起 → CAMS 近景；FOV 40→38；头微 yaw |
| **P08–P09** | 收回/落定到可玩衔接 | P08 铃未响轻收回；P09 落定观察窗；Skip 仍立刻→B0 |

VO 九句未改。时长仍在 V2 建议窗内。

---

## 3. 改动路径

| 路径 | 作用 |
| --- | --- |
| `scripts/systems/prologue.gd` | **运行时真源**：`SHOTS` + Tween 运动 + crossfade + P07 head micro |
| `tools/capture_art_prologue_dynamic.gd` | 盒端复拍 P01/P03/P07 start/mid/end |
| `evidence/art-prologue/*-dyn-*.png` | 九张动态证据 |
| `evidence/screenshots/art-review/art-prologue-*-dyn-*.png` | 同套副本 |
| `docs/ART_PROLOGUE_DYNAMIC_20261028.md` | 本交付 |

**未改**：钥链 / 门禁 / `teacher.gd` AI / NavMesh / Omni / AudioBus / 脚步 / 合 main。

---

## 4. 盒端复跑

```bash
cd EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_prologue_dynamic.gd
```

- Godot **4.7.2** · **gl_compatibility** / llvmpipe  
- `alert_hidden=true`  
- **不报视觉 PASS**；请 @游戏审核 目标机 forward+ 复看「镜头在动」+ Skip→B0

---

## 5. 真机成功标准

- 再播前情能明显感到镜头在动（非静态幻灯）
- Skip 任意时刻仍可用 → 淡出进 B0
- 无跳脸、无响铃、无 Alert `?`/`!`

---

## 6. 请协作方

1. **@游戏开发**：本 tip 已叠在 `aa09ce8` 之上 push feat；勿覆盖 P0 钥柜/门禁。  
2. **@游戏审核**：真机看 P01 缓推 / P03 移准 / P07 慢推+微头；盒图不作 PASS。  
3. **@情节**：已对齐 playtest patch §5；VO 未动。

---

*ART-PROLOGUE-DYNAMIC · 镜内运动+镜间 crossfade · 叠 aa09ce8*
