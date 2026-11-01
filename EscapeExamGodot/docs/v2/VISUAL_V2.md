# VISUAL V2 · 画面与人物形象方案

> 域：画面与人物形象（claude/v2-visual，基线 73d72b3 设计圣经）
> 纲领：圣经 §4——质感 = 强剪影 + 分区戏剧光 + 受控后期 + 动态氛围；自有风格「终场秩序」。
> 状态声明：**自动验收 95/95 通过；本机截图为 xvfb + gl_compatibility，仅验管线与构图，不报视觉 PASS。**
> **本文所有数值都是初值，待 Forward+ 真机同机位（S01–S05）校准后才许定版。**

---

## 1. 改动清单（严格限于本域所有权）

| 文件 | 改动 |
|---|---|
| `scenes/actors/teacher.tscn` | 教师形象重构：本地 V2 材质覆盖（不改外部 mat_*.tres)、大衣加长+梯形下摆、眼窝加深、红笔印章红、新增左臂卷纸筒 + 腰侧钥匙串。**$Visual 节点名/层级、碰撞、巡逻点、Alert 节点全部未动** |
| `scripts/systems/visual_mood.gd`（新建） | VisualMood 运行时系统：环境调校 / 灯光闪烁调度 / 后期链挂载 / 浮尘+雨幕 |
| `scripts/systems/chapter.gd` | 仅 `_ready` 末尾加 VisualMood 自注册 4 行；reset_world / restore_checkpoint 未动 |
| `assets/post_process.gdshader`（新建） | 全屏后期：暗角 + 胶片颗粒 + 径向色差，tension 驱动 |
| `assets/surface.gdshader` | 新增 `stain_strength/stain_scale` 两个 uniform 与 surface_kind==0 的极轻微纵向水渍（最大暗化 0.045×0.35)，原有逻辑未动 |
| `assets/post_process.gdshader.uid`、`scripts/systems/visual_mood.gd.uid` | 引擎生成的 uid，随文件入库 |

未改：teacher.gd / player.gd / game.gd / acoustics / ambience / tests / chapter_01.tscn（环境与灯光全部由 VisualMood 运行时调整）。

## 2. 教师形象（剪影三件套 + 秩序签名）

设计读法：**先读剪影（大衣梯形）、再读脸（苍白+礼貌空笑）、最后读签名（红笔/钥匙串/卷纸）**。

| 部位 | 改动 | 数值 |
|---|---|---|
| 大衣主体 Coat | 加长并压暗：BoxMesh 0.48×1.02×0.24 → 0.46×**1.16**×0.23，中心 y 1.38→1.44；材质覆盖 V2Coat | `#2E3336`(0.18,0.20,0.212)，roughness 0.86 |
| 大衣下摆 CoatFlare | 加宽 0.56→0.60、加厚 0.20→0.24，y 0.82→0.88；覆盖 V2CoatDark | `#232729`(0.137,0.153,0.161)，roughness 0.88 |
| **梯形裙 CoatSkirt（新增）** | 4 边圆柱台（CylinderMesh top_r 0.30 / bottom_r 0.44 / h 0.58，旋转 45° 成方台，z 压扁 0.55)，y=0.55 | 与下摆同 V2CoatDark；形成上窄下宽的梯形剪影，下摆底边约在 y=0.26 |
| 翻领 LapelL/R、袖子 Sleeve | 覆盖 V2CoatDark / V2Coat | 与大衣同族 |
| 领口 Collar | 覆盖试卷米白（衬衫领口的唯一亮色横带） | `#E4D8B8` 系 V2ExamPaper(0.898,0.847,0.722) |
| 面部 Face + Nose | 覆盖 V2SkinPale（低饱和苍白） | `#CBB9A6`(0.796,0.725,0.651)，roughness 0.62 |
| 眼窝 EyeL/R | 块加大 0.040×0.012 → 0.046×0.020，覆盖深色 V2DarkSocket | `#121212`(0.07)，roughness 0.5 / metallic 0.2 |
| 眼镜 GlassL/R/Bridge、微笑 Smile* | 保留 P0 的细框眼镜与礼貌空笑几何，统一覆盖 V2DarkSocket——远看并入眼窝、近看才读出"礼貌" | 同上 |
| 头发 Hair | 覆盖 V2DarkSocket（近黑） | 同上 |
| 手 Hand | 覆盖 V2SkinPale | 同面部 |
| **红笔 RedPen（右手）** | 笔身 0.17→0.19，覆盖 V2SealRedPen——全游戏唯一高饱和色 | **`#9D5048`(0.616,0.314,0.282)**，roughness 0.42 |
| 卷纸夹 PaperFolder（右臂，原有） | 覆盖 V2ExamPaper | 米白 |
| **卷纸筒 PaperRoll（新增，左臂下）** | CylinderMesh r 0.055 × h 0.42，横抱（rot x≈77°)，位置 LeftArm 局部 (0.05,-0.60,-0.03) | V2ExamPaper |
| **钥匙串 KeyChain（新增，腰右侧）** | Visual 子节点 (0.27,1.04,-0.05)：金属环 Torus + 3 把钥匙片 BoxMesh(0.035×0.095×0.012) 错角悬挂 | V2KeyMetal：(0.72,0.62,0.38)，**metallic 0.9 / roughness 0.28**——全队唯一高光材质，动起来会有闪点（与钥匙声签名呼应） |
| 裤/鞋 | 覆盖 V2Trousers / V2HardShoe | (0.11,0.125,0.14) / (0.05,0.05,0.055)，鞋 roughness 0.38（硬底鞋的弱反光） |

契约核对：teacher.gd 引用的 `$Visual/LeftLeg|RightLeg|LeftArm|RightArm|Head` 及其旋转逻辑**零改动**；新增节点（CoatSkirt/PaperRoll/KeyChain）均为静态装饰子节点，挂在 Visual 或 LeftArm 下随摆臂/身体自然运动，不参与碰撞与逻辑。AlertIcon/SuspicionBar 保持 hidden 原状（不重开警戒 UI，遵 P0 产品决定）。

## 3. VisualMood 参数表（`scripts/systems/visual_mood.gd`）

自注册：chapter.gd `_ready` 在 `Game.boot(self)` 之后 `add_child`——保证 Game.teacher/player 已就绪。

### 3.1 环境（复制 Environment 后改，不动场景资源本体）

| 参数 | 场景原值 | V2 初值 | 意图 |
|---|---|---|---|
| fog_enabled | false | **true** | 走廊纵深与远景压暗 |
| fog 模式/密度 | — | 指数雾 density **0.016** | 约 30m 外显著衰减，本图最长视线 ~20m 内只做层次不做遮挡 |
| fog_light_color | — | (0.36,0.47,0.53) 冷青 | 窗外/雨夜的色温 |
| fog_light_energy | — | 0.4 | 雾本身不发光 |
| volumetric_fog | — | false（明确关） | 性能与可控性，真机再议 |
| tonemap_exposure | 0.88 | **0.72** | 整体压暗约 0.3 档，去"白天感" |
| adjustment_brightness | 1.0（未启用） | 0.93 | 与曝光叠加再沉一档 |
| adjustment_contrast | 1.0 | 1.07 | 灯下亮、角落沉 |
| adjustment_saturation | 1.0 | **0.84** | 抽艳色，保留绿白/米白/印章红的秩序色板 |
| ambient_light_energy | 0.045 | 0.032 | 暗部下沉但不死黑 |
| glow | 关 | 开，intensity 0.22 / strength 0.55 / bloom 0.04 | 只晕灯管与出口绿字 |

### 3.2 灯光闪烁

- 对象：`Lighting/Fixture*` 下全部 OmniLight3D（运行时按路径含 `/Lighting/` 收集，基线共 10 盏 Fixture0–8 + FixtureS)，Sun（DirectionalLight）不参与。
- 算法：FastNoiseLite Perlin，`energy = base × (1 + noise(t×freq, i×23.7) × amp)`；freq = 1.4 + tension×2.6；amp = 0.03 + tension×0.05（基础 ±3%、满 tension ±8%——守 ART_DIRECTION 可读底线，任何时刻不许黑到看不见路）。
- 开关：`flicker_enabled` export（光敏考虑可单独关）。

### 3.3 tension 自算（与音频域同口径，不依赖其文件）

```
prox = clamp(1 - (dist(teacher, player) - 1.5) / 10.5, 0, 1)
base = 0.08 + prox × 0.55
state: suspicious +0.15 | investigate/search +0.22 | chase = 1.0
平滑：向目标逼近，升速率 2.2/s、降速率 0.55/s（升快降慢 = 余悸）
```
教师未上场（visible=false）时回落基线 0.08。`debug_force_tension >= 0` 可强制（真机截图调参用）。

### 3.4 后期链（post_process.gdshader，CanvasLayer layer=64）

| 效果 | 公式要点 | 初值 |
|---|---|---|
| 暗角 | `1 - r² × 0.34 × (1 + tension×0.55)`，下限钳 0.55 | vignette_strength 0.34 |
| 色差 | 径向偏移 `ab = 0.0011 × (0.55 + tension×1.5) × (0.4 + r²×2.2)`（SCREEN_UV 比例） | aberration_strength 0.0011 |
| 颗粒 | hash(SCREEN_UV×(1831,1013) + fract(TIME)×17.31) − 0.5，恒定低幅（不随 tension，免得高压时画面"吵") | grain_strength 0.032 |

开关：VisualMood 的 `post_enabled` export 一键全关（无障碍）。layer=64 会覆盖 HUD/菜单做统一胶片感——若真机发现菜单可读性受损，把 layer 改到 HUD 之下或整体关，属一行调整。

### 3.5 动态氛围

| 系统 | 参数 | 定位依据 |
|---|---|---|
| WindowDust 浮尘 | GPUParticles3D 70 粒、lifetime 7s、盒 (0.55,1.6,3.6)、速度 0.015–0.05 m/s 近漂浮、gravity −0.006、Quad 0.016m、米白 alpha **0.22** 无光照 | 教室西窗内侧 (-9.25, 1.75, -4.2)；Wall03 玻璃在 x=−10、庭院在窗外，窗光从西来 |
| CourtyardRain 雨幕 | 420 粒、lifetime 0.62s、盒 (2.2,0.3,5.2)、初速 6.5–8.5 m/s + gravity −2.5、Quad 0.008×0.3 竖丝、冷青 alpha 0.38 | 窗外 (-12.6, 4.7, -2.6)，落在庭院地面 y≈−0.2 附近消亡；**只在窗外，室内不可见穿墙**（本机截图已从教室内机位核对：雨丝只出现在玻璃外） |

两系统均带自定义 visibility_aabb 防剔除闪烁；`particles_enabled` 可关。

## 4. surface.gdshader 水渍

surface_kind==0（涂料墙）新增：以 (x,z) 列噪声选列（step 0.82，约 18% 列）、沿 y 正弦拉长的水渍暗带，最大暗化 0.045 × stain_strength(0.35) ≈ 0.016——**只在近看时存在**，不改变原有颗粒/石材/木纹分支。关法：材质上把 stain_strength 调 0。

## 5. 验证记录（如实分栏）

| 类型 | 结果 |
|---|---|
| headless 验收 | `RESULT passed=95 failed=0`（--import 后与最终版各跑一次，含 grep 脚本/shader 错误为空） |
| headless 普通启动冒烟 | 主场景运行 15s 零脚本错误（超时终止为预期） |
| headless 截图 | **失败**：headless 虚拟渲染器下 frame_post_draw 不触发，90s 超时无图——不作为证据 |
| xvfb + gl_compatibility 截图 | **成功**（/tmp 留存，不入库）：①前情雨窗——雨丝只在玻璃外、密度成立；②教室机位——雾/压暗/暗角/颗粒生效，浮尘在窗光区可见（初版过亮如雪点，已调 alpha 0.32→0.22、粒径 0.02→0.016、数量 90→70 后复拍确认收敛）；③教师近景——梯形大衣剪影成立、苍白脸+眼镜空笑可读、领口米白横带、钥匙串腰侧有金属闪点、纸夹/卷筒在臂下可辨；红笔在背光侧不够醒目（见 §6) |
| Forward+ 真机 | **未验**——本机无真机渲染条件，所有数值待真机 S01–S05 同机位校准，不报视觉 PASS |
| 人工试玩 | 未执行（归 ark） |

## 6. 已知局限与待真机清单

1. **红笔可读性**：笔身小且常处背光侧，gl_compat 截图中不够醒目。真机若仍弱，候选（按序）：笔尖加一截更亮的印章红小块 / Fixture 补光对齐时再议——不要先加自发光，会破色彩纪律。
2. 大衣在逆光下近纯黑是**有意的剪影优先**，但真机需确认 8m 外朝向可读（ART_DIRECTION 推广门槛），必要时把 V2Coat 提亮到 0.22 一线或给 FixtureS 走廊灯留教师 rim。
3. 指数雾 density 0.016 在 Forward+ 与 gl_compat 下观感可能不同，真机重点看走廊尽头（东廊雨窗）是否被雾吃掉方向感。
4. 后期 layer=64 覆盖菜单，暗角下限已钳 0.55，真机看暂停菜单四角文字可读性。
5. 雨幕为 billboard 竖丝，侧面机位（沿 x 方向看）雨丝会转正对相机变宽点感——真机从东廊窗位复核，必要时换固定朝向的十字片。
6. 浮尘只做了教室一盒；播音间/印务室如需同系统，复制节点改 position 即可（参数已在代码内成组）。

## 7. 给集成者的注意事项

- 合入时若 chapter.gd 与其他域冲突：本域只加了 `_ready` 里 4 行自注册，手动保留即可；visual_mood.gd 对 Game 的依赖只有 `Game.teacher / Game.player` 两个既有字段与 teacher 的 `state` 字符串（用 `get("state")` 弱引用，教师 AI 域改状态名时请同步知会本域——状态名是跨域契约）。
- VisualMood 在每次 chapter 实例化时 duplicate 一份 Environment，场景资源本体不被污染；测试/多实例安全（验收已证）。
- teacher.tscn 的 load_steps 已按新增 13 个子资源同步（42→55)；后续用编辑器保存会自动重算。
- 本域新增素材全部为程序化（材质/网格/粒子/shader)，**无外部资产、无许可登记需求**；ASSET_CREDITS 无需新增条目。
