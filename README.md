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
