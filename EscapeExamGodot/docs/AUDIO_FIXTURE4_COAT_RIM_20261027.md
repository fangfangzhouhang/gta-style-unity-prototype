# 【光影音效交付】西廊 Fixture4 大衣 rim 刀修 · AUDIO-FIXTURE4-COAT-RIM-20261027

- **任务 ID**：`AUDIO-FIXTURE4-COAT-RIM-20261027`
- **分支**：`audio/fixture4-coat-rim`（worktree `/workspace/wt-audio-joint`；**未**合并进 `feat/chapter01-module-pass`）
- **本交付 Commit**：（ · ）
- **基线 tip**：`373da02d15b21a6cb5a85bbf19159724542f26f9`（F4 aim `6b05191` ∪ art coat albedo `614e536`）
- **日期**：2026-10-27（Asia/Shanghai）
- **Godot**：4.7.2 stable（`/home/box/.local/bin/godot`）· box 实跑 `gl_compatibility` + llvmpipe
- **范围**：仅 `Lighting/Fixture4` 父 `position` + Omni `energy` / `size` / `range` / `attenuation`；`shadow_enabled=false` 保持
- **未改**：`mat_coat` / 墙材质 / teacher AI / NavMesh / 拓扑 / 玩家 / 解谜 / Alert UI / AudioBus / ambient / WorldEnvironment / 非 Fixture4 灯
- **状态**：计量完成 · **不报视觉 PASS** · 人眼与风格仍待 @游戏审核 / @美术

## 0. 背景（为何刀修）

联检 tip `373da02` 西廊 S02：美术+QA 已判大衣仍糊进左墙，coat−wall 仍为负（QA 约 −6.5；本盒同机位 art ROI 复测 tip before **−2.86**）。S03 较好但不构成 PASS。本交付在光影自有范围内对 **无阴影填充 Fixture4** 做微刀：更贴大衣肩/东侧 rim、收紧 falloff，继续压西墙洗亮。

## 1. Fixture4 before → after

| 参数 | tip `373da02` (before) | 本分支 (after) |
|------|------------------------|----------------|
| fixture `position` | `(-1.05, 3.15, 0.45)` | **`(-0.82, 3.15, 0.05)`**（+0.23 X 靠服务核大衣侧；−0.40 Z 靠教师肩） |
| `light_energy` | `0.48` | **`0.52`**（≤0.55；收 range 后微补 rim） |
| `light_size` | `0.38` | **`0.32`** |
| `omni_range` | `3.9` | **`3.25`** |
| `omni_attenuation` | `1.4` | **`1.75`** |
| `light_color` | `(0.48, 0.66, 0.88)` | 同左 |
| `shadow_enabled` | `false` | **`false`** |

几何粗算（Light 世界位 y=pos.y−0.25）：大衣躯干≈2.20 m、东肩≈1.86 m、西墙取样≈2.80 m；墙−大衣距离优势 ≈0.46→**0.59 m**。

## 2. 同机位截图

目录：`EscapeExamGodot/evidence/screenshots/audio-fixture4-coat-rim/`

| 文件 | 角色 |
|------|------|
| `s02-before.png` | tip `373da02` 同机位（joint recheck 捕获） |
| `s02-after.png` | 本刀 Fixture4 after · S02 |
| `s03-before.png` | tip before · S03 |
| `s03-after.png` | after · S03 |
| `s02-roi-metrics.json` | ROI 计量落盘 |

复跑：

```bash
cd EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_audio_fixture4_coat_rim_s02_s03.gd
```

（box 无 Vulkan → `gl_compatibility`；目标机可用 `forward_plus` 复跑。勿 `--headless`。）

## 3. 像素探针（S02 · ROI 均值 0–255）

方法：BT.709 luma `0.2126 R + 0.7152 G + 0.0722 B`；ROI 锁定为复现美术 ART-COAT-CONTRAST-S02 公布值（大衣宽框 / 左墙 → before +1.2 / after +3.8）的像素框。

| ROI (x0,y0)-(x1,y1) | tip before | knife after |
|---------------------|------------|-------------|
| 大衣宽框 (666,340)-(806,458) | 28.10 | 24.68 |
| 躯干紧框 (688,430)-(770,504) | 24.77 | 20.26 |
| 左墙 (68,306)-(180,482) | 30.96 | **13.91** |
| **大衣−左墙 Δ** | **−2.86** | **+10.77** |
| 东侧 rim 参考 (760,360)-(820,460) | 28.65 | 25.67 |
| 近廊墙参考 (200,320)-(320,500) | 27.04 | 12.27 |
| rim−近廊墙 | +1.61 | +13.40 |

对照（同 ROI，非本交付改动）：

| 源 | 大衣−左墙 Δ |
|----|-------------|
| art coat after（无本 tip F4 合入态） | +3.80 |
| audio F4-aim after（无 coat albedo） | −6.29 |
| tip `373da02` before（本 before） | −2.86 |
| **本刀 after** | **+10.77** |

中心 probe `(640,400)`：before `(0.039, 0.110, 0.231)` → after `(0.028, 0.090, 0.196)`。

**读数说明**：墙洗明显下降是主贡献；大衣绝对 luma 也随 falloff 略降，相对 Δ 由负转正。盒上 `gl_compatibility`/llvmpipe 与目标 `forward_plus` 色调有差——**计量完成 ≠ 视觉 PASS**。

## 4. Omni 阴影预算 / Alert

| 项 | 结果 |
|----|------|
| `python3 tools/check_shadow_omni_budget.py` | **measured=4**（Fixture0/5/6/7）≤5 |
| 捕获日志 `SHADOW_OMNI_COUNT` | **4**；`f4_shadow=false` |
| AlertIcon / SuspicionBar | 场景 `visible=false`；`teacher.gd` `_ready`/更新路径强制 `visible=false`；捕获夹具再 force-hide。**未重开** |

## 5. 验收状态

| 项 | 状态 |
|----|------|
| diff 仅 Fixture4 灯参/位姿 | 是 |
| Fixture4 `shadow_enabled=false` | 是 |
| energy ≤ 0.55（本交付 0.52） | 是 |
| Shadow Omni ≤5（实测 4） | 是 |
| S02/S03 before/after 已落盘 | 是 |
| S02 coat−wall Δ 计量（−2.86 → +10.77） | 是（盒 metric） |
| **人眼联检（大衣轮廓可读 / 未洗西墙 / 教室暖不漂）** | **未验证**（待 @游戏审核 / @美术 · forward+） |
| 耳感 / mix | 本任务无关；未声称 |
| 帧率 / perf | **未测** |

## 6. 给 @游戏开发 的合入提示

1. 独立分支：请 cherry-pick / merge **仅** `Lighting/Fixture4` 段进 `feat/chapter01-module-pass`，勿整文件覆盖非 Lighting 节点。
2. 合入后请 @美术 / @游戏审核 在 **forward+** 下复看 S02/S03：大衣东侧 rim 是否可读、西墙是否仍糊、教室暖区是否漂。
3. 静态门禁：`python3 EscapeExamGodot/tools/check_shadow_omni_budget.py`。
4. **勿**为剪影加第 5 盏阴影 Omni；若仍不足，需新任务授权。

---

*AUDIO-FIXTURE4-COAT-RIM-20261027 · 光影音效交付 · 计量完成 · 不报 PASS*
