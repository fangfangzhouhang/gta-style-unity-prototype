## 《逃离考场》演进说明

本项目正演进为中文叙事向 Unity 游戏 **《逃离考场》**：愿景是高考压力下的潜行逃出考场 → 逃出学校 → 乘高铁 / 飞机 / 地铁 / 车去各地旅行做任务；并探讨多人分考场逃脱后汇合。

**当前可玩（Built-in + 旧 Input Manager）**

| 切片 | 内容 | 入口 |
| --- | --- | --- |
| **v0.1 开世界** | 第三人称 WASD、F 上下车、开车至黄圈「任务完成」 | `OpenWorld` 场景 / 菜单 **OpenWorld → 生成占位场景** |
| **v0.2 考场潜行** | 监考视野锥、蹲走、课桌/柜子掩体、被发现回检查点、逃出考场 | 菜单 **EscapeExam → 生成考场场景** → Play |
| **v0.2 学校逃离** | 走廊、1–2 巡逻（复用视野组件）、校门「逃出学校」 | 考场成功自动进入，或 **EscapeExam → 生成学校场景** |

交通枢纽、OSM 真地图、联机与精美美术等**尚未合入**。代理 / 开发者请从 **`Docs/00-任务交接.md`** 读起。重型工作默认不做，须先与仓库维护者确认。

# 开放世界小原型

一个能在 Unity 里直接 Play 的第三人称小切片：走路，靠近红车按 **F** 上车，沿路开到黄色光柱，屏幕出现 **「任务完成」**。

场景全是方块和圆柱，不需要 Asset Store，也不需要联网、警察、枪战或大地图流式加载。

## 推荐版本

- **Unity 2022.3 LTS** 或更新版本（含 Unity 6）
- 工程按 **内置渲染管线（Built-in）** 保存
- 输入使用 **旧版 Input Manager**（`UnityEngine.Input`），没有安装 Input System 包

如果 Hub 提示工程版本较旧、需要升级，确认升级即可。升级不会改玩法。

用 URP / HDRP 打开时，编辑器里材质可能是粉色。进入 Play 后，脚本会按当前管线重新上色。验收以 Play 为准。

## 用 Unity Hub 打开

1. 安装 Unity 2022.3 LTS 或更新版本。
2. Unity Hub → **Add** → **Add project from disk**。
3. 选中本目录（里面要能看到 `Assets`、`Packages`、`ProjectSettings`）。
4. 打开工程。首次导入会生成 `Library`，等它结束。
5. 若没有自动打开场景，双击 `Assets/Scenes/OpenWorld.unity`。
6. 点编辑器上方的 **Play**。

场景里已经放好：地面、道路、方块楼、蓝色玩家、红色车、黄色光柱。直接 Play 就能验收，不用再摆物体。

## v0.2 考场 / 学校怎么玩

1. 退出 Play。菜单 **EscapeExam → 生成考场场景**（首次建议再点一次 **将场景写入 Build Settings**）。
2. 点 **Play**。点击 Game 视图锁定鼠标。
3. **WASD** 移动，**Ctrl** 或 **C** 蹲走；用课桌 / 柜子挡住监考视线。
4. 走进监考前方扇形且无遮挡会被发现，屏幕显示「被发现了」并回到出生点。
5. 绕到左侧绿色出口 →「逃出考场」→ 自动进入学校走廊（需 Build Settings 含 `SchoolEscape`）。
6. 学校关：避开紫色巡逻，走到蓝色校门 →「逃出学校」。

也可单独：**EscapeExam → 生成学校场景** 后直接验收学校段。  
开世界切片不变：仍用 **OpenWorld → 生成占位场景** / `OpenWorld.unity`。

## 怎么玩（验收）

1. 点击 **Game** 视图（否则键盘鼠标进不了游戏）。
2. 屏幕提示「点击游戏画面，开始控制镜头」。点一下，鼠标被锁定。
3. **WASD** 朝镜头方向走，鼠标转动镜头。玩家在红车正后方。
4. 走到车尾，底部出现 **「按 F 上车」**，按 **F**。
5. **W** 前进、**A / D** 转向、**S** 倒车、**空格** 刹车。沿虚线开到路尽头的黄色光柱。
6. 车头开进黄圈后，画面中央出现 **「任务完成」**。

开车时再按 **F** 可以下车。**Esc** 释放鼠标。

| 操作 | 步行 | 开车 |
| --- | --- | --- |
| W A S D | 相对镜头移动 | 油门 / 转向 / 倒车 |
| 鼠标 | 转动镜头 | 转动镜头 |
| Shift | 冲刺 | — |
| 空格 | 跳跃 | 刹车 |
| F | 靠近红车时上车 | 下车 |
| Esc | 释放鼠标 | 释放鼠标 |

请用英文输入法，避免 **F** 被输入法吃掉。

## 如果场景空了

菜单栏 **OpenWorld → 生成占位场景**。

它会按脚本重新摆地面、建筑、玩家、车和任务点，并保存 `OpenWorld.unity`。然后再 Play。

**OpenWorld → 打开原型场景** 用来切回这个场景。

## 脚本

| 文件 | 作用 |
| --- | --- |
| `Assets/Scripts/Player/ThirdPersonController.cs` | 第三人称走路、冲刺、跳跃 |
| `Assets/Scripts/Player/ThirdPersonCamera.cs` | 鼠标轨道镜头，跟着人或车 |
| `Assets/Scripts/Vehicle/VehicleController.cs` | 前进、倒车、转向、刹车 |
| `Assets/Scripts/Vehicle/PlayerVehicleSwitch.cs` | F 上下车，切换控制和镜头 |
| `Assets/Scripts/Mission/MissionCheckpoint.cs` | 黄圈触发，标记完成 |
| `Assets/Scripts/Mission/MissionHUD.cs` | 屏幕提示，包括「任务完成」 |
| `Assets/Scripts/World/PrototypeWorldBuilder.cs` | 生成占位街区 |
| `Assets/Editor/PrototypeSceneMenu.cs` | 编辑器菜单 |

界面中文使用 `Assets/Resources/UIFont.ttf`（Droid Sans Fallback，Apache 2.0）。许可证在 `Assets/Fonts/LICENSE-DroidSansFallback.txt`。

## 输入说明

工程的 `ProjectSettings` 里 `activeInputHandler` 为 **Input Manager (Old)**。

如果以后装了 Input System，并且弹出选择窗口，请保持 **Input Manager** 或 **Both**。不要改成只使用新输入系统，否则 WASD 会失效。

## Agent Skills

本仓库已经带好给 Cursor / 云端代理用的 skills。文件在 `.agents/skills/<名称>/SKILL.md`。`.cursor/skills` 是指向该目录的链接，两边看到的是同一套。

`npx skills add -a cursor` 会把 skill 装进 `.agents/skills`，并写进根目录的 `skills-lock.json`。玩法脚本和场景没有因此改动。

### Unity 官方（34 个，整套）

来源：[Unity-Technologies/skills](https://github.com/Unity-Technologies/skills)（Unity Companion License）。

`2d-pixel-perfect`、`asset-transformer-toolkit`、`audio-setup-mixers`、`build-gtk`、`build-live-game`、`create-2d-physics`、`create-tile-palette-or-rule-tiles`、`generate-editor-search-query`、`implement-in-app-purchases`、`initialize-ai-navigation`、`levelplay-unity-integration`、`localization`、`manage-sprite-atlas`、`migrate-birp-to-urp`、`new-unity-project`、`optimize-audio`、`optimize-text-mesh-pro`、`optimize-web`、`physics-3d-collision`、`project-auditor-fixes`、`setup-multiplayer-services`、`setup-vivox-voice-chat`、`shader-graph-create-custom-node`、`sprite-editor`、`sprite-segment-3x3grid`、`ui`、`ui-imgui`、`ui-ugui`、`ui-uitk`、`unity-cli`、`unity-package-management`、`unity-pipeline`、`urp-postprocessing`、`validate-urp-render-graph-renderer-feature`。

### 社区游戏向（15 个，只留和本原型相关的）

来源：[gamedev-skills/awesome-gamedev-agent-skills](https://github.com/gamedev-skills/awesome-gamedev-agent-skills)（Apache 2.0）。没有装 Godot、Unreal、Roblox 或其他引擎整包。

- 路由：`router`
- Unity：`unity-animation`、`unity-build-pipeline`、`unity-csharp-scripting`、`unity-input-system`、`unity-navmesh`、`unity-physics`、`unity-scriptableobjects`、`unity-tilemap-2d`
- 工作流：`prototype-fast`
- 通用：`camera-systems`、`input-systems`、`game-feel`、`level-design`、`physics-tuning`

上游 router 里写的路径 `skills/unity/<名称>/SKILL.md`，在本仓库对应 `.agents/skills/<名称>/SKILL.md`。

### 更新

在仓库根目录执行。已安装的全部按锁文件更新：

```bash
npx skills update -p -y
```

重新装官方整套：

```bash
npx skills add Unity-Technologies/skills -a cursor -s '*' -y --copy
```

只补社区里这 15 个（`--full-depth` 才会扫到子目录）：

```bash
npx skills add gamedev-skills/awesome-gamedev-agent-skills -a cursor --full-depth -y --copy \
  -s router \
  -s unity-animation -s unity-build-pipeline -s unity-csharp-scripting \
  -s unity-input-system -s unity-navmesh -s unity-physics \
  -s unity-scriptableobjects -s unity-tilemap-2d \
  -s prototype-fast \
  -s camera-systems -s input-systems -s game-feel -s level-design -s physics-tuning
```
