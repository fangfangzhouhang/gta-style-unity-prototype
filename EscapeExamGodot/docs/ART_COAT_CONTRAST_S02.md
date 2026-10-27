# 【美术交付】ART-COAT-CONTRAST-S02

- **任务 ID**：`ART-COAT-CONTRAST-S02`
- **分支**：`art/chapter01-module-pass` @ `bf5b49e` (`bf5b49e11404af5f7e93ae78ac0b11906fdc8154`)
- **日期**：2026-10-27（Asia/Shanghai）
- **范围**：仅美术材质 / 块面明度；**未改**灯参、`teacher.gd`、NavMesh、chapter 拓扑、AudioBus。Alert 保持 hidden。

## 问题

西廊 S02：QA 反馈墙 ROI 被抬高多于大衣，大衣主体与冷暗墙色差不足，肉眼难分大衣块。

## 对照色值（albedo）

| 资源 | 角色 | Before | After | 说明 |
| --- | --- | --- | --- | --- |
| `assets/materials/teacher/mat_coat.tres` | 大衣主体 / 袖 | `Color(0.32, 0.40, 0.48)` · sRGB lum≈0.389 | `Color(0.40, 0.48, 0.60)` · sRGB lum≈0.471 | 略提明度 + 偏蓝灰，与下墙拉开 |
| `assets/materials/teacher/mat_coat_flare.tres` | **新建** · CoatFlare / 翻领 | —（此前共用 mat_coat） | `Color(0.46, 0.54, 0.66)` · sRGB lum≈0.532 | 肩/下摆 flare 与翻领块面略亮一档 |
| chapter `r2` 下墙（西廊 dado） | 墙（只读，未改） | `Color(0.32, 0.43, 0.37)` · lum≈0.403 | **同左** | 保持冷暗绿灰 |
| chapter `r1` 上墙 | 墙（只读，未改） | `Color(0.66, 0.65, 0.57)` · lum≈0.647 | **同左** | 未动 |

场景接线：`scenes/actors/teacher.tscn` · `scenes/actors/teacher_visual_body.tscn` — `mesh_coat_flare` / 翻领改挂 `m_coat_flare`。

## 引擎截图（同机位 SHOT_BLOCKS S02 / S03）

目录：`EscapeExamGodot/evidence/screenshots/art-review/`

| 文件 | 角色 |
| --- | --- |
| `art-s02-coat-contrast-before.png` | before（合入前 `art-s02-after.png` 拷贝 · tip `88e91f6` 一带） |
| `art-s02-coat-contrast-after.png` | after · 本 pass |
| `art-s03-coat-contrast-before.png` | optional S03 before |
| `art-s03-coat-contrast-after.png` | optional S03 after |

复跑：

```bash
godot --path EscapeExamGodot --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_coat_contrast_s02.gd
```

（box：需 DISPLAY；勿 `--headless`，否则 `get_texture` 易挂。）

## 像素探针（S02 · ROI 均值 0–255）

| ROI | Before lum | After lum |
| --- | --- | --- |
| 大衣宽框 | 8.9 | 11.5 |
| 躯干紧框 | 5.8 | 9.9 |
| 左墙 | 7.7 | 7.7（不变） |
| 大衣−左墙 Δ | +1.2 | **+3.8** |

中心 probe：before `(0, 0.020, 0.071)` → after `(0, 0.039, 0.106)`（更蓝、略亮）。

## 自检

- [x] S02 after 大衣主体比墙更可读（蓝灰块 vs 冷暗墙）
- [x] 未改灯参 / AudioBus / NavMesh / chapter 拓扑 / teacher.gd
- [x] Alert / SuspicionBar capture 时强制 hidden
- [x] 无写真贴图堆砌、无血污、无过强描边；轻漫画风格化写实 + 校园微恐一致

## 剩余风险

- 仍依赖冷廊侧溢灯（audio/lighting Fixture 旁支）；极暗无 spill 时色差仍会收窄，但材质基线已拉开。
- `gl_compatibility`/llvmpipe 与目标机 `forward_plus` 色调略差；合入 feat 后建议在目标渲染器复看一眼。
- 未合 audio 分支；勿用本 commit 强推 feat。
