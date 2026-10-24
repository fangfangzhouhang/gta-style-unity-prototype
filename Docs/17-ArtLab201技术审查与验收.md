# 201 美术样板：技术审查与验收

2026-10-22；基线 main `907906b39a6d8c7b6358299a1776cba3aa3292dc`。已成功 fetch，GitHub 连接器也核对 main。功能分支 `feature/artlab-201`，独立目录 `D:/Unity/ArtLab201`。原 `feature/campus-v12-chapter-one` 的密码柜、203 等未合并玩法不包含在 main，本分支不替代该玩法分支。最终集成必须在合并后的基线上回归。

## 只读审查结论

已查阅 README、Docs/00、05、13–16、CampusWorldBuilder、CampusTeachingBuilding、LevelBuildUtil、PrimitiveTint、ThirdPersonController、ThirdPersonCamera、TeacherAgent、CampusNavigation、CampusWorld.unity 和第三方来源记录。

Unity 2022.3.49f1c1；Built-in；旧 Input Manager；Netcode 1.12.0；未安装 URP 或 Post Processing 包。场景为米制：201 为 9×12 米，楼面高 4 米，玩家高 1.8 米、眼高约 1.65 米。Camera、CharacterController 与 AI 不应重写。桌面约 .70×.50、高 .76 米；座高约 .455 米。

桌椅、门窗、灯具和老师主要为组合几何。可用临时模型有 Kenney 家具、城市模型，以及 Grab3D classroom-light / school-building-light。家具包带 CC0 License.txt；Grab3D 仅有 SOURCE.md 声明，未重新核验其原始授权，不作为本轮新引入资产。没有检出动画 clip、Animator controller 和可验收 Humanoid 教师。现有贴图以城市 colormap 为主，缺室内成套 PBR、皮肤、服装、细节法线和污渍遮罩。

CampusWorld 的 LightingDataAsset 为零；启用烘焙选项不等于已有烘焙。现有平坦环境光压缩空间层次，窗玻璃是不透明几何，程序化材质偏纯色。Campus/Foliage 使用 Built-in surface shader；CampusWorldText 需在迁移时检查透明排序与深度。

PrimitiveTint.ApplyAll 在 Bootstrap.Start 中重新创建材质，可能覆盖 PBR、丢贴图并产生大量材质实例；FindLitShader 按 Shader 是否存在选取，不检查激活管线。不可把“运行时不粉色”等同于 URP 完整兼容。新导入家具不用 PrimitiveTint，避免该覆盖路径。

## 实施分级与兼容风险

P0：保存原校园，独立场景与同机位截图，先验证移动、窗碰撞、通道净宽和现有 AI。仅替换可见桌椅；旧碰撞暂保留，必须检查新旧轮廓差异。窗口隐藏旧不透明玻璃可见面，保留碰撞；这只是临时透光处理，缺真实玻璃反射。不要声称最终美术。

P1：Blender 制作中国高中钢木桌椅、讲台、铝窗与荧光灯，材质与 UV 到位后局部迭代，补玻璃、墙裙、踢脚线、粉笔槽、纸张与可读板书。完成烘焙、曝光和角色动态探针测试后才能进入样板验收。

P2：教师蒙皮角色与动画、局部灰尘/污渍、性能优化。201 通过后才扩走廊、图书馆、配电间。不先批量替换校园。

高风险：生成器销毁 LevelRoot/Player，手工内容不能回写原生成树；渲染遮挡会影响 VisionConeDetector 射线；新碰撞可能堵住 TeacherAgent 的 .27 米半径导航；门锁 carving、谜题物件射线与第一人称点击仲裁需在 V1.2 集成时重测。原存档不得用于自动测试。材质实例化、透明玻璃排序、反射探针捕获整个校园也有性能成本。仅保持源文件不变不足以证明玩法通过。

## 资产制作规格（以下为目标预算，非实测面数）

| 关键物件 | 来源/制作 | PBR/UV | LOD0 / LOD1 / LOD2 三角面目标 | 碰撞/导航 |
| --- | --- | --- | --- | --- |
| 钢木桌椅 | 临时 Kenney CC0；最终 Blender 原创圆角、焊接钢管、木板包边 | 木纹 BaseColor、Normal、AO；钢件 Metallic/Smoothness；1–2K，统一纹素密度 | 每套 5000/2500/800 | 简单盒/胶囊组合，椅腿不做逐三角碰撞，通道保留 .9 米以上 |
| 讲台/讲桌/黑板 | Blender 原创；不能用未授权教材图案 | 2K 木纹/漆面/粉笔擦痕；板书单独图集 | 每件 6000/2500/800 | 盒碰撞，粉笔槽不阻挡导航 |
| 门/窗/墙模块 | Blender 原创，按实际门洞制作 | 2K plaster/dado，玻璃独立透明材质，门窗 trim sheet | 每模块 4000/1800/500 | 玻璃真实实体盒，门扇独立锁碰撞与 carving |
| 荧光灯 | Blender 原创灯壳、双灯管、悬挂件 | 1K 漆面、金属、Emission；发光材质不能替代实际灯光 | 1500/600/200 | 灯具不用导航碰撞 |
| 纸张/钟/考试标识 | 原创中文内容，避免教材扫描 | 1K 图集、局部透明与污渍遮罩 | 每件 500/200/平面 | 无碰撞；交互目标保留射线代理 |
| 窗外校园 | 复用现有校园 | 现有材质暂用，远景后续统一 | 由实际捕获决定 | 保持原物理边界，窗口不允许走出 |
| 教师 | 合法人体 + Blender 衣服/头发/皮肤；CC 可选、未购买 | 皮肤/眼球/衬衫/裤装分组 2K，近景可升4K | 45000/22000/8000 | 单一角色根胶囊，NavMeshAgent 负责位移，蒙皮无逐三角碰撞 |

FBX 通用：Blender 米制、应用 rotation/scale、导出选中对象，-Z Forward/Y Up，去相机灯光。Unity Scale Factor 1 并检查实际 bounds；Import Normals、Calculate Mikktspace Tangents，静态模型关闭动画、关闭自动 Collider、验证 UV2 无重叠并留足 padding；无 UV2 时生成 Lightmap UV。BaseColor 为 sRGB；Normal 设置 Normal Map；AO/Metallic/Smoothness 为线性数据。Built-in Standard 的 Smoothness 在 metallic alpha，不能直接塞 roughness；roughness 应反相转换。桌椅可共享 UV0，烘焙 UV2 必须唯一。避免非均匀缩放成为最终资产流程。

临时 Kenney 家具允许商业用途，保留许可证，来源：https://kenney.nl/assets/furniture-kit 。Blender 原创保留 .blend 和制作记录。AI 3D 仅可辅助原型，必须修拓扑/UV/法线/蒙皮，核对服务条款和输入权利，不能把生成成功当质量验收。第三方付费内容均未采购。

## 灯光与渲染闸门

独立编辑器工具复制 CampusWorld 为 ArtLab_201，固定相机 (-10.6,5.65,-2.8) 看向 (-7.4,5.3,4)，输出 before/day/dusk/suspense 1920×1080 的 Unity Camera.Render 图像。这些是编辑器实际渲染，不能当作 Play 截图或性能测试。保留原相机与跟随组件配置。

方向光软阴影、冷色室内点光、三色环境光作为初步灯光；36 个 Light Probe 采样位置和盒投影 Reflection Probe。当前探针为实时 OnAwake 128，尚未烘焙，不能宣称间接照明完成。后续采用 Progressive CPU 局部烘焙，先 20 texels/m、2 bounces、32 direct/256 indirect 起步，实查漏光、接缝与动态角色，再提高质量。每种光照必须有独立已验证烘焙结果，不能共用不匹配的光照贴图。

待补：成套 PBR、墙脚 AO、轻量贴花污渍、稀疏局部尘粒、玻璃 Fresnel、Built-in 后期曝光/色调映射。不能仅降低环境光制造微恐怖。核查黑板、试卷、钥匙暗部可读，窗外高亮保留轮廓，必要时调曝光而非无限增加点光。

## 人物方案与验收

优先合法基础人体 + Blender 原创老师服装，CC 是可选有成本路线。Reallusion 当前内容政策允许 Standard 导出，但同页 FAQ 存在旧 Extended 描述，采购时需确认具体商品与适用 EULA；iContent 不能替代外部引擎导出授权。未取得软件/资产许可，不下载、购买或提交相关人物。

Mixamo 官方允许角色/动作用于商业游戏，但其 FAQ 明示中国 country code 账户不可用；不能假定用户可以登录。可改用 Blender 原创 Idle/Walk/Run/Look 动作。AI 自动绑定只在骨骼映射、肩胯权重、脚底滑动检查通过后采用。来源：https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html 、https://www.reallusion.com/license/content.html 。

Humanoid 配置：T-pose，验证 Avatar 骨映射、脚底0与1.75–1.8米身高；动画 In Place，Animator.applyRootMotion=false，由原 TeacherAgent/NavMeshAgent 驱动。Animator speed 参数取实际速度，Idle/Walk/Run 混合树，转头用限角头颈 IK，严禁第二套追逐逻辑。

必测：原地站立60秒、巡视整圈、转头不折颈、触发追逐与丢失目标后回巡逻、门口绕障、桌椅碰撞和脚底稳定；每种动作近景与全身 Play 截图。当前无可用蒙皮角色，所有上述人物测试未执行，现有组合几何老师不算高质量人物原型。

## URP 决策与性能验收

当前明确建议：继续 Built-in 完成 201，暂不迁移工程。原因是资产、光照和人物不足尚未解决，迁移无法替代这些工作。尚无 URP 14 A/B 实测，所以不能宣称性能更好或迁移已验证。

如开展迁移：从本分支另开 feature/artlab-201-urp14，固定 Unity 版本，仅在隔离工程安装 URP14；比较 Standard 转换、Foliage surface shader 重写、WorldText 深度、PrimitiveTint 参数与活动管线判断、程序化材质/发光/玻璃、阴影和后期；同一相机与同机硬件实测后才作最终迁移决策。

1080p 性能协议：Development standalone，记录 CPU/GPU/驱动、质量设置、分辨率、VSync、构建 SHA；预热30秒，静止/巡视/追逐各采60秒，保存 Profiler .data。报告 Main Thread、Render Thread、GPU frame time、GC.Alloc、总内存/纹理/网格、批次与三角面，给中位数/P95；GPU 不支持则标 unavailable。编辑器 Camera.Render 不代替实际运行。本轮不填任何估算数字。

## 本轮执行记录

Unity batchmode 已完成编译与场景生成，首次运行日志含 ARTLAB201_BUILD_PASS，退出码0；生成20套临时家具、36个探针采样位置、反射探针和四张1920×1080实际编辑器渲染。首次视觉检查判定室内过暗，且202西墙在201侧窗外约1米处阻挡采光，因此在独立样板中关闭 Classroom_202，形成采光庭院，并提高室内填充光；这是样板的布局差异，不能直接复制回生产地图。原 CampusWorld 不变。此处需在生产整合时重新设计201外侧采光空间，保留202玩法入口与功能，不能用关闭邻室作为生产修复。

## 当前验收表

| 项目 | 状态 |
| --- | --- |
| 最新 main 与独立功能分支 | 已完成 |
| 原校园与运行时源码保持不变 | git diff 无已有文件修改；新增独立场景有202停用差异 |
| 独立场景/同机位渲染 | Unity生成通过，含20套临时家具；图为编辑器渲染 |
| 写实最终资产、PBR、灰尘污渍、色调映射 | 未完成 |
| 烘焙、有效 Light Probe 光照、玻璃材质 | 未完成 |
| 教师 Humanoid 与动作 | 缺资产，未执行 |
| Play 窗碰撞/解谜/导航回归 | 未执行 |
| CPU/GPU/内存 Profiler 实测 | 未执行，无性能数字 |
| URP14 独立分支 A/B | 未执行；当前不建议迁移 |
| 电影级视觉验收 | 未通过，当前为临时资产灯光研究 |

推广条件：201 先通过可读性、同机位对照、碰撞/解谜/AI 和1080p性能。随后提取经确认的门窗、墙裙、灯具为 prefabs；走廊先测长距离阴影与探针过渡；图书馆补书架 LOD、遮挡剔除和纸书材质；配电间补金属柜、受控应急灯与开关交互。每个区域单独验证，禁止一次重建全校。
