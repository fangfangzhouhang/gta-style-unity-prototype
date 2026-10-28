# 《逃离考场》第一章 · Agent 交接上下文（2026-10-28）

- **所有者**：ark（方舟航）产品拍板；合 `main` 仅 ark 明确批准
- **总分支（集成）**：`feat/chapter01-module-pass`（**不要用 `main` 验收/推进**）
- **仓库**：`fangfangzhouhang/gta-style-unity-prototype`
- **时区**：Asia/Shanghai
- **协作房间**：游戏（情节设计 / 开发 / 人物形象画面 / 光影音效 / 游戏审核）
- **本机试玩**：`D:\game\play.cmd`，尽量 **Forward+**；钉 tip，勿默默跟到无关清理提交

---

## 1. 当前 tip（运行时真源）

| Tip（短） | 全长 SHA | 内容 |
| --- | --- | --- |
| **`2344404`** | `234440437190c4612ddd80cdc63c0f5e2d06ff4c` | **当前推荐钉点**：玩家脚步 `volume_db -= 16`；叠动镜 + P0 |
| `5e82703` | （祖先） | ART-PROLOGUE-DYNAMIC：镜内 Tween + 镜间 crossfade（内容 commit `16daf3e`） |
| `aa09ce8` | `aa09ce88054891e4fd2cd10022af3c9cca0f6e8e` | 真机 P0：无 K3 不可开 G；柜 `open_first`；acceptance **95/95** |
| `9588e50` | `9588e50e494d05cf82956bda399bc65c2a383bdd` | 前情 Skip→B0 骨架 + 裸跑强制 `test_mode` |
| `d6fec5e` | `d6fec5ed3225cf5f3c54ccd636f3ec1d487dd707` | ART-PROLOGUE-CAMS 定稿机位 |

设计销钉文档（可 cherry）：`docs/design/CHAPTER01_PLAYTEST_PATCH_20261028.md`（docs tip `9b872df`）。

> 注意：曾出现 tip `e0a2cbd`（清 Unity 杂项）。**真机验收不要钉它**；以 `9588e50` 线 / 现 `2344404` 为准。

---

## 2. 流程与口径（强制）

```
设计 → 开发集成 → 自动测试 → 游戏审核 → 人工验收 → ark 批准 → 才合 main
```

- **文档完成 ≠ 代码完成 ≠ 测试通过 ≠ 体验通过**
- **TECH PASS ≠ EXPERIENCE PASS ≠ 可发布**
- 审核结论必须绑 **Commit SHA**；无人工试玩证据标「体验未验证」
- 审核**不自改**被审核心玩法/美术；不合 `main`
- Alert UI **保持关闭**；时长口径：熟练 10–15 / 首玩目标 15–25
- Omni 阴影预算 **≤4**；钥匙链产品向：`C → K1 → 出廊 → K2 → S → K3 → G`；G·K3 = `Doors/GateAntechamber`

---

## 3. 已完成（技术侧摘要）

### 3.1 机制 / 门禁（TECH PASS @ `aa09ce8`）

- 无 **K3** 不可开 `Doors/GateAntechamber`；HatchM↔G 需门已解锁；jamb 补碰撞防 clip
- 讲台第三格 / 洗手池 / 教具柜：`open_first` 先开柜再取钥（非纯世界拾取假装是柜）
- acceptance：`-- --test` 与裸跑均为 **95/95**（裸跑缺 flag 时 WARN + 强制 `test_mode`，自 `9588e50`）
- Skip→B0 / 不响铃 / Alert 关 / VO 九句定稿：在 `aa09ce8` 仍绿

### 3.2 前情

- VO 真源：`docs/design/PROLOGUE_VO_FINAL_20261028.md`（九句；P07 非跳脸）
- 机位定稿：`d6fec5e`；动态：`5e82703`（缓推 + 淡入淡出）；Skip 仍可用
- **盒端 gl_compat 不报视觉 PASS**；动镜是否还像幻灯片 → ark 真机 Forward+

### 3.3 音频（部分）

- 脚步：`aa09ce8` −7→−11；`2344404` → **≈−16 dB**（`acoustics.gd`）
- Ambient 床 / 开门软化 / 老师脚步距衰等已在较早 feat；**整体 SFX「听不见」仍未关**（光影音效在查）

### 3.4 美术其他

- ART-S01-CORNER、钥点金属方向、西廊大衣等已有 tip；视觉真机 Forward+ 前不报 PASS

---

## 4. ark 真机反馈（体验 **NOT PASS**）

时间：2026-10-28。分支 `feat/chapter01-module-pass`（请确认 tip ≥ `aa09ce8`，优先 `2344404`）。

| 严重度 | 问题 | 状态 |
| --- | --- | --- |
| P0 | 可直达大门开门 / 无钥绕过 | 代码修于 `aa09ce8` → **待 ark 复测** |
| P0 | 钥匙柜「打不开」 | 改 `open_first` @ `aa09ce8` → **待 ark 复测** |
| P1 | 听不到音效 | **仍开**（光影音效） |
| P1 | 脚步过大 | 已压至 ≈−16 @ `2344404` → **待耳听确认** |
| P2 | 前情全静态像幻灯片 | 动镜已上 `5e82703` → **待真机观感** |
| 产品 | 过松、须强制走完每个解密点 | 销钉文档已写；硬门禁已部分落地 → **待体验确认** |

**结论**：体验仍 **NOT PASS**；**不合 main**。TECH PASS ≠ 体验过。

---

## 5. 审核待办（给下一个 游戏审核 / Agent）

1. **独立复验 tip `2344404`（或更新 tip）**  
   - acceptance 95/95（`-- --test` + 裸跑）  
   - 无 K3 开不了 G；柜 `open_first`；Skip→B0 / 不响铃 / Alert / VO  
   - Omni=4；`acoustics` 玩家脚步 `-= 16`；prologue 含 `eye_end` Tween + crossfade  
   - 注：曾对 `2344404` 启动复验但因交接打包中断，**必须重跑**，勿采信「已验」口头。
2. SFX 听不见旁支合入后：再绑 tip 验（技术夹具 + 请 ark 耳听）
3. ark 盲测结论进群后：体验 PASS/FAIL 写死；仅 ark 批 `main` 后才允许合主干
4. 已知 P2（不挡当前）：雨丝精修、前情 VO 音频、`route_trial.gd` 过时、Classroom nav 跨锁洞

### Acceptance 命令

```bash
godot --headless --path EscapeExamGodot -s res://tests/acceptance.gd -- --test
```

裸跑亦应 95/95（WARN 强制 test_mode）。

证据目录惯例：`EscapeExamGodot/evidence/logs/qa-<topic>-<shortsha>/`

---

## 6. 未来工作（按优先级）

1. **光影音效**：根治 SFX 听不见（总线/Ambient/非脚步）；合进 feat；耳听销钉  
2. **审核**：完成 `2344404+` TECH 复验报告（SHA 绑定）  
3. **ark**：真机 Forward+ 复测柜/大门/Skip/脚步/动镜/SFX；群里给体验结论  
4. **美术**：真机确认动镜 + P01/P03/P07 + S01/大衣/金属钥点；盒端继续不报视觉 PASS  
5. **开发**：按 ark 新反馈开旁支；保持不合 main 直至批准  
6. **合 main**：仅 ark 明确「批准合主干」之后由开发执行  

暂缓：第二敌人、大扩建、大改写（第一章可玩质量优先）。

---

## 7. 关键文档索引

- `docs/design/CHAPTER01_PLAYTEST_PATCH_20261028.md` — 真机反馈设计销钉  
- `docs/design/PROLOGUE_VO_FINAL_20261028.md` — 前情九句 VO  
- `docs/ART_PROLOGUE_CAMS_20261028.md` / `ART_PROLOGUE_DYNAMIC_20261028.md` — 机位与动镜  
- `docs/ART_KEYCHAIN_METAL_READ_20261028.md` / `ART_S01_CORNER_SAMPLE_20261028.md`  
- `docs/AUDIO_HORROR_BED_DOOR_SOFT_20261028.md`  
- 历史 QA：`docs/QA_CHAPTER01_MODULE_PASS_20261027.md`

---

## 8. 本交接写入说明

- 由 **游戏审核** 在额度紧张时写入并 **push 到 `feat/chapter01-module-pass`**，**未合 `main`**
- 目的：给新 Agent / 下一会话提供可检索真源，避免进度只留本地
