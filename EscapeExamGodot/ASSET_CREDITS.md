# 参考与素材来源

记录日期：2026-10-25。**当前未导入任何第三方图片、模型、字体、声音或代码素材。** 新增文件为原创设计文档。以下网页用于研究，不授权将其中图片/音视频打包进游戏。

## 真实校园与视觉参考

| ID / 来源 | 作者或页面署名 | 用途与具体取舍 | 权利/修改/使用状态 |
| --- | --- | --- | --- |
| R01 [清华大学校史馆：1980—1985 我与三院的故事](https://xsg.tsinghua.edu.cn/info/1005/3928.htm) | 杨宜勇；清华大学校史馆 | 旧校园的半墙绿漆、上部白墙、木课桌、灯管与走廊材质记忆；仅参考感官细节，不把大学旧建筑当现代中学规范 | 已读页面；无素材导入或改图；未取得再分发许可 |
| R02 [南京师大附中晓庄校区](https://www.archiposition.com/items/4cbc3b5527) | 非常建筑提供文字，有方发布；摄影田方方 | 转折处的空间节点、彼此可见的入口、走廊与庭院的联系；缩成单层局部，不照搬大型校园或图书馆 | 已读页面与图注；图片未下载/再分发。页面注明非常建筑授权发布，不等于授权游戏使用 |
| R03 [西安市航天城第一中学](https://www.archdaily.com/zh/926059/xi-an-shi-hang-tian-cheng-di-zhong-xue-xi-an-jian-zhu-ke-ji-da-xue-jian-zhu-she-ji-yan-jiu-yuan) | 西安建筑科技大学建筑设计研究院；摄影张晓明；ArchDaily 发布 | 教学组团与庭院的清楚关系，窗外可识别的校园尺度。用于方向感参考，不扩做全校园 | 已检索图像并读取项目/庭院图页；无素材导入；未取得再分发许可 |
| R04 [Peking University Affiliated High School / Crossboundaries](https://www.archdaily.com/805981/affiliated-high-school-of-peking-university-crossboundaries) | Crossboundaries 提供项目文字；页面有 WANG Ziling / YANG Chao Ying 图像署名 | B 方案的可调整学习空间参考；A 只取清楚的开口与日常校园功能，不混入大量鲜艳现代家具 | 已读页面；未下载/修改素材，非游戏资产授权 |

这些是研究来源，不是复制清单。绿墙裙、水磨石、暖木与阴天光的组合为本项目自己的统一方案；不能声称某所参考学校使用了本项目全部材质或布局。本轮未生成概念效果图。

## 游戏体验与画面参考

| ID / 来源 | 已确认内容 | 具体采用的原则与排除项 | 使用状态 |
| --- | --- | --- | --- |
| G01 [Hello Neighbor 官方页](https://www.helloneighborgame.com/helloneighbor) | 官方描述潜行进入邻居住宅，并提及会响应玩家行为的 AI | 日常空间、观察追捕者与绕路；参考官方走廊图中的色块与人物轮廓分离。具体实施是本项目推论；不复制人物、地图、夸张建筑、谜题或自学习承诺 | 官方页面与图片检索；图片/商标/声音均未导入，权利属于原权利人 |
| G02 [Amnesia: The Bunker 开发商商店页](https://store.steampowered.com/app/1944430/Amnesia_The_Bunker/) | 开发/发行 Frictional Games；第一人称封闭空间中的持续威胁与工具求生 | 操作工具要同时考虑退路，区域有限但行动有压力；不引入武器、燃料经济或怪物传送方式 | 已读产品介绍；无代码/图片/音频复用；不以此页面推断内部 AI 算法 |

《小小邻居》尚未可靠对应到具体游戏；Hello Neighbor 是候选而不是核实结果。精确短语与带“游戏/steam”检索未提供确定作品证据；不把同名童书、成就页或小说当游戏参考。暂按用户描述的观察、探索、路线逃脱推进。

## 技术资料（只作 API/决策依据）

- [Godot 4.7.2 官方归档](https://godotengine.org/download/archive/4.7.2-stable/)。
- [4.7 CharacterBody3D](https://docs.godotengine.org/en/4.7/classes/class_characterbody3d.html)。
- [4.7 NavigationAgent3D](https://docs.godotengine.org/en/4.7/classes/class_navigationagent3d.html)。
- [4.7 NavigationObstacles](https://docs.godotengine.org/en/4.7/tutorials/navigation/navigation_using_navigationobstacles.html)。
- [4.7 渲染器](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html)。
- [4.7 存档](https://docs.godotengine.org/en/4.7/tutorials/io/saving_games.html)。
- [4.7 暂停](https://docs.godotengine.org/en/4.7/tutorials/scripting/pausing_games.html)。
- [4.7 AudioStreamPlayer3D](https://docs.godotengine.org/en/4.7/classes/class_audiostreamplayer3d.html)。

本轮未复制官方示例代码、分发引擎或制作导出包。实现期使用引擎与外部库时补齐交付所需许可证文件和鸣谢。

## 本地经验资料

旧 Unity/Godot 样板及旧截图的只读来源列在 [环境检查](docs/ENVIRONMENT_AUDIT.md)。不复制旧素材，不认为旧项目中存在文件就意味着新项目已获得使用权。

## 新资产登记规范

每个真正导入的资产登记：资产 ID、文件路径、作者、原始来源 URL、下载日期、具体许可证版本/证明路径、允许的使用范围、修改内容、AI 生成工具与输入来源（如适用）、审核状态。

状态只用：研究参考 / 待核许可 / 原型占位 / 已核可交付。未知许可的内容不进入交付包。原创模型也保存源文件与制作记录；第三方参考图不能直接成为贴图，游戏字体不得从系统字体目录随意打包。

当前可交付素材表：空。下一阶段先制作少量原创模块、文本和占位声音，随后逐项补许可与风格检查。


## A-M2.1 实际资产登记（2026-10-26）

用户已确认参考作品为《你好，邻居》Hello Neighbor，以上“身份未确认”为 D0 历史状态。参考页面图片没有导入。

| 路径 | 来源与状态 | 说明 |
| --- | --- | --- |
| scenes/、resources/chapter_navigation.tres | 本项目原创原型几何/场景与本机导航烘焙 | 没有复制旧 Unity 模型；模型/教师动画非最终美术 |
| assets/surface.gdshader | 本项目编写程序材质 | 无外部纹理 |
| assets/audio/*.wav | tools/author_scene.py 合成的八个占位音效 | 非录音/配音素材，无外部采样 |
| resources/chinese_font.tres | Godot SystemFont 在运行机器查找字体 | 不复制/分发系统字体文件；无中文字体机器可能显示异常 |
| evidence/screenshots/ | 本项目 Godot 实际运行渲染 | 机位夹具冻结老师，不是人工通关证据 |
| Godot 引擎 | 本机 4.7.2 标准版；MIT 及第三方组件许可证 | 引擎二进制不入源码 Git；本机开发包由 tools/write_licenses.gd 生成许可证文本 |

上述原型源码/自制内容未另行授予开源许可证；保持原仓库私有属性。本次不将第三方网页参考作为可再分发素材。

## 本轮自制音频（2026-10-27）

| 路径 | 性质 | 状态 |
| --- | --- | --- |
| `assets/audio/rain_bed.wav` | 程序合成雨声循环床 | 原创占位；可交付本机原型 |
| `assets/audio/tube_hum.wav` | 程序合成灯管 hum 循环 | 原创占位；可交付本机原型 |
| `assets/audio/catch_sting.wav` | 程序合成抓捕刺音 one-shot | 原创占位；可交付本机原型 |

上述 WAV 非第三方采样；后续若替换为录音/授权素材须在本表更新来源与许可。

## V2 自适应音频素材（2026-11-02 · claude/v2-audio）

生成工具：`tools/gen_v2_audio.py`（纯 Python stdlib 程序合成，固定种子可复现；参数注释在脚本内）。全部为本项目原创合成，**无第三方采样、无预训练模型权重**，状态：原型占位 / 可交付本机原型。

| 路径 | 性质 | 用途 |
| --- | --- | --- |
| `assets/audio/heartbeat.wav` | 心跳 lub-dub 两拍 one-shot（0.85 s，58/52 Hz 正弦衰减包络，峰值 0.55） | AdaptiveAudio 按 BPM 计时重播 |
| `assets/audio/keys.wav` | 钥匙串轻晃（0.38 s，5 次 3.1–6.2 kHz 金属 ping + 短噪声擦声，峰值 0.42） | 教师移动声音签名，kind="keys" |
| `assets/audio/pulse_loop.wav` | 追击脉冲循环（4.0 s 无缝，55 Hz 正弦脉冲 1 拍/s，PCM，峰值 0.50） | 自适应音乐 pulse 层（tension>0.25） |
| `assets/audio/tension_strings.wav` | 弦乐擦弦张力循环（8.0 s 无缝，带通噪声+0.25 Hz 颤音，交叉淡化成环，PCM，峰值 0.34） | 自适应音乐 strings 层（tension>0.5） |
| `assets/audio/drone_loop.wav` | 低频氛围嗡鸣（8.0 s 无缝，55/82.5/110/165 Hz 整周期叠加，PCM，峰值 0.40） | 自适应音乐 drone 常底层 |
| `assets/audio/chalk.wav` | 粉笔落地（0.22 s，宽带噪声脆响+190 Hz 软敲+70 ms 二次弹跳，硬钳 0.55；集成期补入 `tools/gen_v2_audio.py` 的 `chalk()`，种子 20261102，逐字节可复现，MD5 1fa90fe1…） | 粉笔投掷落地声，kind="chalk"（集成提交补登记，回应 QA 审计中级问题 1） |

`.import` 约定：三个循环素材 loop_mode=1 且 compress/mode=0（PCM，沿用 2026-10-28 床素材的 Windows 播放硬化结论）；heartbeat/keys 为短 one-shot。

