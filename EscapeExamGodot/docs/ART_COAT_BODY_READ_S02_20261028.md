# 【美术交付】ART-COAT-BODY-READ-S02 · 西廊大衣躯干可读性

- **任务 ID**：`ART-COAT-BODY-READ-S02`
- **分支**：`art/chapter01-module-pass`（worktree `/workspace/wt-art`）
- **基线 tip（rebase 起点）**：`2b95d84`（`origin/feat/chapter01-module-pass`）
- **本交付 Commit**：5dcd17e3ea6a95522b9599da59f7cf588805093d（5dcd17e）
- **日期**：2026-10-28（Asia/Shanghai）
- **Godot**：4.7.2 stable · box 实跑 `gl_compatibility` + llvmpipe
- **范围**：仅大衣材质 albedo + Visual 肩/袖/领块面比例；**未改**灯参 / Omni / AudioBus / post、`teacher.gd` AI、NavMesh、chapter 拓扑、Alert UI。
- **状态**：材质刀修完成 · 盒上同机位计量完成 · **视觉 PASS 待 @游戏审核 forward+ 复看**

## 0. 背景

QA @ `1af8e06` / `2b95d84`：Fixture4 右肩 rim 已改善，但大衣**躯干**仍暗/糊进西墙 → 西廊视觉 **NOT PASS**。光影已停刀；**禁止**加第 5 盏阴影 Omni；**禁止**改灯。本交付只抬大衣主体 mid albedo + 微肩剪影块面。

## 1. Albedo before → after

| 资源 | 角色 | Before | After | sRGB lum | 说明 |
| --- | --- | --- | --- | --- | --- |
| `assets/materials/teacher/mat_coat.tres` | 大衣主体 / 袖 | `Color(0.40, 0.48, 0.60)` · lum≈0.472 · B−G=+0.12 | **`Color(0.48, 0.54, 0.62)`** · lum≈**0.533** · B−G=+0.08 | +0.061 | 抬 mid；略去饱和蓝，仍蓝灰大衣族（非亮青、非灰泥） |
| `assets/materials/teacher/mat_coat_flare.tres` | CoatFlare / 翻领 | `Color(0.46, 0.54, 0.66)` · lum≈0.532 | **`Color(0.54, 0.60, 0.68)`** · lum≈**0.593** | +0.061 | 保持比主体亮一档 |
| chapter `r2` 下墙（只读） | 西廊 dado | `Color(0.32, 0.43, 0.37)` · lum≈0.402 | **未改** | — | 对照墙 |

## 2. 肩剪影微刀（保持 slim tall 躯干）

| 项 | Before | After | 说明 |
| --- | --- | --- | --- |
| `mesh_coat` 躯干 | `0.48 × 1.02 × 0.24` | **同左** | **未增肥** |
| `mesh_sleeve` | `0.12 × 0.98 × 0.14` | **`0.14 × 0.98 × 0.15`** | 袖平面略宽，肩/臂外轮廓 |
| `mesh_collar` | `0.30 × 0.08 × 0.20` | **`0.34 × 0.09 × 0.22`** | 领平面略宽 |
| `mesh_coat_flare` | `0.52 × 0.20 × 0.26` | **`0.56 × 0.20 × 0.27`** | 下摆 flare 略宽 |
| `LeftArm` / `RightArm` pos.x | ±0.33 | **±0.34** | 袖加宽后外移 1 cm，避免并入躯干 |

改动文件：`scenes/actors/teacher.tscn` · `scenes/actors/teacher_visual_body.tscn`。

## 3. 同机位截图

目录：`EscapeExamGodot/evidence/screenshots/art-coat-body-read/`

| 文件 | 角色 |
| --- | --- |
| `s02-before.png` / `s03-before.png` | tip `2b95d84` + 旧 albedo（= rim 合入后躯干仍糊基线） |
| `s02-after.png` / `s03-after.png` | 本刀 after |
| `s02-baseline-rim-after.png` / `s03-baseline-rim-after.png` | 拷贝自 `audio-fixture4-coat-rim/*-after.png` 作 rim 对照 |
| `s02-roi-metrics.json` | ROI 计量 |

复跑：

```bash
cd EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_coat_body_read_s02_s03.gd -- --tag=after
```

（box 无 Vulkan → `gl_compatibility`；目标机可用 `forward_plus`。勿 `--headless`。）

## 4. 像素探针（S02 · ROI 均值 0–255）

ROI 锁定 AUDIO-FIXTURE4-COAT-RIM 公布框；BT.709 luma。

| ROI (x0,y0)-(x1,y1) | before | after |
| --- | --- | --- |
| 大衣宽框 (666,340)-(806,458) | 24.68 | **27.82** |
| 躯干紧框 (688,430)-(770,504) | 20.26 | **22.84** |
| 左墙 (68,306)-(180,482) | 13.91 | **13.91**（不变） |
| **大衣−左墙 Δ** | **+10.77** | **+13.91** |
| 东侧 rim (760,360)-(820,460) | 25.67 | 25.67 |
| 近廊墙 (200,320)-(320,500) | 12.27 | 12.27 |

中心 probe `(640,400)`：before `(0.0275, 0.0902, 0.1961)` → after `(0.0471, 0.1098, 0.2039)`（更亮、仍偏蓝）。

**读数说明**：墙 luma 不变（未动灯）；大衣宽框 / 躯干紧框绝对 luma 上抬，Δ 再扩约 +3.1。盒上 `gl_compatibility`/llvmpipe 与目标 `forward_plus` 色调有差——**计量 ≠ 视觉 PASS**。

## 5. 自检 / 约束

- [x] 未加第 5 盏阴影 Omni；未改任何灯 / AudioBus / post
- [x] 未改 `teacher.gd` / NavMesh / chapter 拓扑 / Alert UI
- [x] 大衣躯干 slim tall（`mesh_coat` 0.48 宽保持）
- [x] 蓝灰大衣族；无写真贴图、无血污、无过强描边
- [x] AlertIcon / SuspicionBar capture 时 force-hide
- [x] Shadow Omni 捕获日志 = 4

## 6. 请 @游戏开发 / @游戏审核

1. **@游戏开发**：请将 `art/chapter01-module-pass` **merge / cherry-pick** 进 `feat/chapter01-module-pass`（材质 + Visual 块面 + 本交付文档/证据/捕获脚本）。勿整文件覆盖非美术节点。美术 **不自行合** feat/main。
2. **@游戏审核**：合入后在 **forward+** 复看西廊 S02（及 S03）：大衣**躯干体积**是否相对冷暗墙可读、右肩 rim 是否仍在、教室暖区是否漂、是否出现过亮青绿或灰泥。
3. 若仍不足：**勿**擅自加阴影 Omni；开新任务授权。

---

*ART-COAT-BODY-READ-S02 · 美术交付 · 盒计量完成 · 视觉 PASS 待 forward+ 复看*
