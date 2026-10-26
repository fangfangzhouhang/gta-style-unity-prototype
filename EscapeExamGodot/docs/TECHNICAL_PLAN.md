# Godot 4.7.2 技术方案

状态：已核实引擎版本并查阅 4.7 文档；架构为待实现方案。采用标准版 GDScript，首个平台 Windows x64。

## 引擎与渲染决策

本机执行结果：`4.7.2.stable.official.ed1daf0bf`。锁定该版本开发，暂不安装新引擎。官方 [4.7.2 发布归档](https://godotengine.org/download/archive/4.7.2-stable/) 与本机版本相符。

初选 **Forward+** 供桌面 3D 样板使用；选择依据是 [4.7 渲染器文档](https://docs.godotengine.org/en/4.7/tutorials/rendering/renderers.html)，不是旧版性能比较。下一阶段第一个新项目场景做本机驱动/光照冒烟验证；如果不稳定，先比较 Mobile，再按实际结果确定基线。Compatibility 不保证与 Forward+ 同样画面，切换必须重新验收，不静默宣称同质。

当前 GPU 查询被系统拒绝；旧日志曾报告 RTX 5060 Laptop GPU，但不是本轮硬件实测。未确认目标最低硬件，不承诺性能。初始目标为本机 1080p 60fps，M3/M5 用帧时间记录验证；没有结果前不作达标声明。

引擎只通过路径调用。后续启动命令的 `--path` 和 `--log-file` 必须指向新项目；不启动旧项目以免改其导入缓存。导出模板的版本与位置需后续核实，不直接从旧小样复制。

## 最小的可复用模块

计划目录（尚未创建这些场景/脚本）：

```text
scenes/chapters/chapter_01.tscn   # 明确房间、门、声源、导航与检查点节点
scenes/prototypes/sound_loop.tscn
scenes/actors/player.tscn
scenes/actors/invigilator.tscn
scenes/props/door.tscn
scenes/props/exam_timer.tscn
scenes/props/broadcast_panel.tscn
scenes/props/ratchet_gate.tscn
scenes/ui/hud.tscn
scenes/ui/pause_menu.tscn
scripts/actors/                 # 移动、感知与有限状态行为
scripts/interaction/            # 一套交互契约，不按房间复制
scripts/systems/                # 章节进度、声音事件、存档、输入模式
resources/                     # 可在 Inspector 调整的参数 Resource
assets/                        # 原创或已核实授权的资产
tests/                         # 实现期所需引擎验收
```

只有章节进度/存档与输入模式确有跨场景需要时才使用 Autoload，不提前做通用任务编辑器、ECS 或插件框架。每个物件负责本地表现，通过信号报告稳定 ID 的状态变化；章节控制器组织开场/结束，不接管所有碰撞和 UI。

场景为显式 `.tscn`，门扇、碰撞、巡逻点、声区、灯和文本在编辑器可选可调。允许编辑器辅助生成重复桌椅后保存成场景；不把整章藏进启动时的巨大生成脚本。

## 玩家、交互与 UI

- [CharacterBody3D](https://docs.godotengine.org/en/4.7/classes/class_characterbody3d.html) 配 CapsuleShape3D，通过物理帧的 `move_and_slide()` 移动；相机/眼高单独枢轴。站起之前做完整站姿体积查询，不只向上打一根细线。
- 交互使用摄像机射线与最近可交互命中，世界遮挡先参与检测；碰撞层分世界实体、玩家、教师、交互代理。清玻璃挡身体但是否挡视线独立定义，不能误用同一掩码。
- `Interactable` 暴露提示与操作入口；门、计时器、面板、卷栅实现这个小契约。物件状态有稳定 ID，不依赖节点显示名称。
- 输入模式只允许 Gameplay / Pause / Journal / Reading 等互斥状态。参考 [暂停与 process mode](https://docs.godotengine.org/en/4.7/tutorials/scripting/pausing_games.html)：世界暂停，菜单继续处理并显示鼠标；Esc 同帧消费一次，按钮点击不能回流到摄像机捕获。
- 面板旋钮与卷栅是实体交互，世界照常运行；手册长文本暂停。失焦自动暂停；退出界面先确认输入释放再恢复视角，避免鼠标跳动。

## 教师导航与感知

教师也是 CharacterBody3D，子节点 [NavigationAgent3D](https://docs.godotengine.org/en/4.7/classes/class_navigationagent3d.html) 提供路径，实际移动仍经物理碰撞。Agent 不会自动移动角色；按文档在物理帧更新路径位置，并等待导航地图可用后开始寻路。

导航网格在编辑器烘焙，排除桌面/窗台，半径与教师胶囊一致。跨门连接用预设的小区域/NavigationLink3D 管理：解锁状态决定连接可用，关闭但可开的门需要教师先到门前操作再通过。边界要避免导航网格在门两边自动跨接绕过链接。

不把 NavigationObstacle3D 的避让当成动态寻路切断；[官方障碍文档](https://docs.godotengine.org/en/4.7/tutorials/navigation/navigation_using_navigationobstacles.html) 区分烘焙与避让作用。实际开关门必须联测连接、路径、门扇与角色碰撞。对本章几扇门优先显式连接状态，不每帧重烘整个关卡。

感知记录两类证据：最后可见位置/时间；听到的声源/门口及强度。有限状态机只有巡视、怀疑、调查、追逐、搜索返回，参数用 Resource 导出。丢视线不得继续查询玩家全局位置给追踪目标；抓捕需近距且无遮挡。

声音传播用小型房间—门口图，不模拟复杂声波：声源到开口路径有长度与门衰减，超过听阈就停止；封闭墙不是图边。布局变化同时改变门的物理、视觉与声区状态。[AudioStreamPlayer3D](https://docs.godotengine.org/en/4.7/classes/class_audiostreamplayer3d.html) 负责空间播放，它本身不是 AI 听觉或整套墙体遮声系统。一次事件驱动可听反馈和调查判断，避免两套不同半径。

## 进度与存档

第一版存档只存本章所需内容：schema_version、checkpoint_id、已发现事实 ID、运卷门解锁、广播设置/重播状态、出口棘轮档、计时器所有权与合法位置、章节完成。门扇瞬时角度、教师正在追逐的坐标不是必要持久进度。

参考 [官方保存教程](https://docs.godotengine.org/en/4.7/tutorials/io/saving_games.html)，使用 FileAccess + JSON；设置用 ConfigFile。额外的临时文件写入、校验、备份再替换与恢复策略是本项目设计，不能把文档示例当成已有完整事务保证。数值与 ID 校验后再应用；对象初始化与导航就绪后安置角色。

独立用户目录名计划为 `EscapeExamGodot`；测试目录通过测试启动参数隔离，不能写玩家正式存档或 Unity 存档。异常位置回退合法检查点。恢复时进度幂等应用，关键物件至少存在于玩家、合法放置点或恢复托盘之一。

## 工具依赖与验证

运行依赖只需 Godot 标准版；开发期可用 Blender 制作统一资产，但本轮未安装，也未确认 PATH 外安装状态。暂不引入第三方 AI/行为树插件、网络服务或资源包管理框架。现有能力足以完成 D0，无须为“可能有用”先安装 skill。

实现后先用引擎无窗口运行做解析/自动测试，再用真实窗口检查材质、导航、手感与 UI。自动路径脚本可使用夹具，但必须披露。真实输入与人工试玩各自留证；不能因为测试传入正确答案就认定谜题可推导。
