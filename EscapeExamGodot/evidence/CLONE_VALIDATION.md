# 干净源码交付验证 · 2026-10-26

基础源码本机提交 ce4530a；从 Git archive 导出，无 .godot、嵌套 .git、引擎、存档或运行缓存。Unity 基线为 d5192edf32518dd80017fb33d1d3c0d104e4a6f6，对应已存在的 PR #11。

- 在交付副本调用 tools/run.ps1 -Mode Check，指定本机 Godot 4.7.2 console 路径；先完成全新 headless editor import，再运行测试，退出码 0。
- 38 项夹具式引擎检查通过、0 失败。日志 clone-acceptance.log，摘要 clone-acceptance.txt。
- 广播路线活动 AI 脚本通关：22.39 秒、0 抓捕。纯计时器路线：19.38 秒、0 抓捕。均为熟练脚本的模拟时间，不是首次玩家时长。
- 没有本轮重新执行 Unity 测试；Unity 第五阶段使用原提交自带的验证记录，不混入当前 Unity 工作目录未提交实验。
- 引擎仍报告沙箱根证书读取失败；路线退出另有 2/4 个 ObjectDB 和 1/2 个资源释放警告。通关结果与退出清理问题分别记录，后者仍待修复。
- 没有人工试玩、实际 OS 输入验收、跨硬件或发行质量结论。

Git archive 的中文文件名在 Windows tar.exe 中出现解包问题，已使用 Python tarfile 按原始 Unicode 名称恢复两个 cmd，清除错误解码副本，并逐文件核对：114 个原始文件无缺失。首次导入可更新 .import 文件；测试会更新日志，其他游戏源码没有在此过程中被替换。
