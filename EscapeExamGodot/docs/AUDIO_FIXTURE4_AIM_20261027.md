# 【光影音效交付】西廊 Fixture4 瞄准大衣 rim · AUDIO-FIXTURE4-AIM-20261027

- **任务 ID**：`AUDIO-FIXTURE4-AIM-20261027`
- **角色**：@游戏光影音效设计
- **日期**：2026-10-27（Asia/Shanghai）
- **分支**：`audio/fixture4-aim-coat`（独立 worktree `/workspace/wt-audio-f4-aim`；**未**合并进 `feat/chapter01-module-pass`）
- **基准 Commit（feat tip）**：`bc10edcd8addb9e1a266eeac8d18c8ce89adff13`（`bc10edc` · `docs: stamp AUDIO-FIXTURE4-FILL delivery commit SHA`）
- **本交付 Commit**：*(stamp after commit)*
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）
- **约束遵守**：仅改 `Lighting/Fixture4` 父节点 `position` + Omni `energy`/`range`/`attenuation`；`shadow_enabled=false` 保持；未新增第 5 盏阴影 Omni；未重开 AlertIcon/SuspicionBar；未改教师 AI / 拓扑 / 玩家 / 解谜 / 非光照场景；**未改** ambient / WorldEnvironment；**未再抬 energy**（≤0.55，本交付略降）。

---

## 0. 联检失败与再瞄准 rationale

`AUDIO-FIXTURE4-FILL` 在 feat tip `bc10edc` 后人眼联检 **FAIL**：energy/range 抬升后 **西墙洗亮多于大衣**，大衣仍糊进左墙（Wall10 @ X≈−3）。S02 几何：西廊中心线 X≈−1.75，西墙 X≈−3.0，服务核西面（Wall18）X≈−0.5；旧 Fixture4 `(-1.7, 3.15, 1.3)` 到大衣与西墙几乎等距（~2.9 m），Omni 无方向 → 抬 energy 同步抬墙。

本任务杠杆：**平移 Fixture4 到服务核大衣侧（+X / 略 −Z）+ 收紧 range / 提高 atten + 略降 energy**，让东侧 rim 近、西墙远，禁止再抬 energy。

---

## 1. 修改文件列表

| 路径 | 说明 |
|------|------|
| `EscapeExamGodot/scenes/chapters/chapter_01.tscn` | **仅** `Lighting/Fixture4` 父 `position` + `Light` Omni 参数 |
| `EscapeExamGodot/tools/capture_audio_fixture4_aim_s02_s03.gd` | S02/S03 锁定机位截图夹具（同 fill/spill pose） |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-aim/s02-before.png` | before：复制 fixture4-fill `s02-after.png`（feat tip 联检失败态） |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-aim/s03-before.png` | before：复制 fixture4-fill `s03-after.png` |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-aim/s02-after.png` | after：本分支 Fixture4 aim + 同 S02 pose |
| `EscapeExamGodot/evidence/screenshots/audio-fixture4-aim/s03-after.png` | after：本分支 Fixture4 aim + 同 S03 pose |
| `EscapeExamGodot/evidence/logs/audio-fixture4-aim-capture.log` | xvfb 捕获引擎日志 |
| `EscapeExamGodot/docs/AUDIO_FIXTURE4_AIM_20261027.md` | 本交付说明 |

**未改**：Fixture0–3/5–8 / FixtureS / Sun / ambient / teacher.gd / Alert UI / ambience / 地图拓扑。

---

## 2. Fixture4 before → after

| Param | before（feat `bc10edc` / FILL） | after（AIM） |
|-------|--------------------------------|--------------|
| fixture `position` | `(-1.7, 3.15, 1.3)` | **`(-1.05, 3.15, 0.45)`**（+0.65 X → 服务核大衣侧；−0.85 Z → 靠近教师） |
| Light local `position` | `(0, -0.25, 0)` | 未动 |
| `light_color` | `(0.48, 0.66, 0.88)` | 未动 |
| `light_energy` | `0.55` | **`0.48`**（略降，≤0.55） |
| `light_size` | `0.38` | 未动 |
| `omni_range` | `5.4` | **`3.9`**（收紧，减西墙洗） |
| `omni_attenuation` | `1.15` | **`1.4`**（更陡衰减） |
| `shadow_enabled` | `false` | **`false`**（不变） |

**几何意图（约）**：Light 世界位 `(-1.05, 2.9, 0.45)` → 大衣躯干 `(-1.75, 1.2, -1.0)` ≈2.4 m；西墙取样 `(-3.0, 1.5, -1.0)` ≈2.85 m（原先几乎等距）。

**Shadow Omni 计数**：仍 **4**（Fixture0 / 5 / 6 / 7）≤5。未新增阴影灯。离线校验：`python3 tools/check_shadow_omni_budget.py` → `PASS … measured=4`。捕获日志：`AUDIO_FIXTURE4_AIM_CAPTURE_OK … shadow=4 f4_shadow=false`。

**Ambient**：未改（仍 `ambient_light_energy = 0.045`）。

---

## 3. 截图路径与捕获

| 文件 | 含义 |
|------|------|
| `evidence/screenshots/audio-fixture4-aim/s02-before.png` | FILL after 基线（联检 FAIL · 墙洗） |
| `evidence/screenshots/audio-fixture4-aim/s02-after.png` | Fixture4 aim · S02 |
| `evidence/screenshots/audio-fixture4-aim/s03-before.png` | FILL after 基线 |
| `evidence/screenshots/audio-fixture4-aim/s03-after.png` | Fixture4 aim · S03 |

机位（与 `docs/SHOT_BLOCKS_CHAPTER01.md` / fill 夹具一致）：

- **S02**：player `(-1.75, 0.04, 1.5)` look `(-1.75, 1.55, -1.0)` FOV 72；teacher `(-1.75, 0.02, -1.0)` suspicious
- **S03**：player `(-3.0, 0.04, 0.2)` crouch Head.y=0.94 look `(-1.75, 1.35, -1.0)` FOV 70

```bash
cd /workspace/wt-audio-f4-aim/EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_audio_fixture4_aim_s02_s03.gd
# → AUDIO_FIXTURE4_AIM_CAPTURE_OK … shadow=4 f4_shadow=false
```

**渲染注记**：box 无 Vulkan → 实际 `gl_compatibility` + llvmpipe。目标机可用 `forward_plus` 复跑同夹具。

盒上粗测（S02 ROI 平均亮度 0–255，**仅供参考、非验收**）：

| ROI | before → after | 备注 |
|-----|----------------|------|
| coat body | 23.6 → 19.0 | 略降（energy/range 收） |
| coat east rim | 23.2 → 22.0 | 基本持平 |
| west wall | 40.7 → 32.2 | 墙洗明显下降 |
| rimR / west | 0.57 → **0.68** | 相对 rim 对比改善 |

---

## 4. 验收状态

| 项 | 状态 |
|----|------|
| 灯参/位姿 diff 仅 Fixture4 | 是 |
| Fixture4 `shadow_enabled=false` | 是 |
| energy ≤ 0.55（本交付 0.48） | 是 |
| Shadow Omni ≤5（实测 **4**） | 是（脚本 + 捕获日志） |
| S02/S03 before/after 已落盘 | 是 |
| **人眼联检（大衣轮廓可读 / 未洗西墙 / 教室暖不漂）** | **未验证**（待 @美术 / 联机 forward+ 复检） |
| 耳感 / mix | 本任务无关；未声称 |
| 帧率 / perf | **未测**；无虚构 FPS |

---

## 5. 给 @游戏开发 的合入提示

1. 独立分支，请 cherry-pick / merge **仅** `Lighting/Fixture4` 段进 `feat/chapter01-module-pass`，勿整文件覆盖非 Lighting 节点。
2. 合入后请 @美术 在 forward+ 下复看 S02/S03：大衣东侧 rim 是否可读、西墙是否仍糊。
3. 静态门禁：`python3 EscapeExamGodot/tools/check_shadow_omni_budget.py`。
4. **勿**再抬 Fixture4 energy；若仍不足，需新任务授权（仍勿加第 5 盏阴影 Omni）。

---

*AUDIO-FIXTURE4-AIM-20261027 · 光影音效交付 · 人眼联检未验证*
