# 运行样板研究

本分支是未通过最终验收的美术研究，完整范围和资源缺口见 `17-ArtLab201技术审查与验收.md`。

使用 Unity 2022.3.49f1c1 打开本工程，打开 `Assets/Scenes/ArtLab_201.unity`。保留 main 的玩法基线，未包含尚未合并的 V1.2 密码柜与203流程。

需要重新生成时选择 `EscapeExam > ArtLab 201 > Create isolated study`。此操作从原校园重新复制样板，会覆盖 ArtLab_201 的手工修改，请先保存副本。不会重建或保存 CampusWorld；生成过程输出四张同机位编辑器实际渲染到 `Docs/ArtLab201`。

可在编辑器调用 `ArtLab201Builder.Lighting(0/1/2)` 切换白天/黄昏/悬疑起步参数。未提供按模式烘焙的 Lightmaps；切换参数不代表成品灯光。

命令行生成：

```powershell
& 'D:\Unity\Editors\2022.3.49f1c1\Editor\Unity.exe' -batchmode -quit -projectPath D:\Unity\ArtLab201 -executeMethod ArtLab201Builder.Build -logFile D:\Unity\artlab201-build.log
```

检查日志中的 `ARTLAB201_BUILD_PASS`，不能只根据进程启动成功判定生成成功。截图为 `Camera.Render`，不代表游戏实际运行帧率。没有新的 Play 回归或 Profiler 结果时，请保留未验证标签。
