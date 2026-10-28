# 美术 Agent 交接上下文 · 2026-10-28

面向下一个 **@人物形象和画面组块设计** agent。额度近耗尽时的完整现场快照；**不要只留本地**——后续改动继续 commit 并 push 到总分支。

- **日期**：2026-10-28（Asia/Shanghai）
- **总分支（唯一合入目标）**：`feat/chapter01-module-pass`
- **远程**：`fangfangzhouhang/gta-style-unity-prototype` · 子树 `EscapeExamGodot/`
- **禁止**：checkout / merge / push `main`；禁止开 PR 合 main（除非 ark 书面宣布可合主干）
- **本交接写入时的远程 tip（fetch 后）**：见文末「真机验证钉 tip」；交接 commit 会叠在该 tip 之上

---

## 1. 角色边界与协作 V2

协作真源：`docs/COLLAB_CHAPTER01_IMPROVEMENT.md`、根目录 `AGENTS.md`。

### 1.1 循环（必须遵守）

```text
art（本角色 worktree 提交）
  → @游戏开发 合并/快进到 feat/chapter01-module-pass
  → @游戏审核 QA（机制/光影/分辨率；真机 Forward+）
  → 打回则开发改策划/重派；OK 后才由 ark 考虑合 main
```

- **不合 main**，除非 ark 明确授权。
- 成员提交前缀：`art:` / `docs:`（美术交付文档也可用 `docs:`）。
- 旁支（如历史 `art/chapter01-module-pass`、`art/prologue-dynamic`）仅作本地隔离；**最终必须出现在 `origin/feat/chapter01-module-pass`**。

### 1.2 本角色范围（可做）

人物 / 场景样板材质 / 交互金属族外观 / 镜头组块（静态机位 + 前情动镜 Visual）：

- `scenes/actors/teacher.tscn` 的 **Visual/** 节点与材质
- `assets/models/`、`assets/materials/`（含 `teacher/`、墙裙/柜/窗、`mat_interact_metal.tres`）
- `scripts/systems/prologue.gd` 中**镜头运动 / crossfade / P07 Head 微动**等纯 Visual 层（不改 Skip→B0 语义、不改钥/门禁）
- `tools/capture_art_*.gd`、`evidence/screenshots/art-*`、`evidence/art-prologue/`
- `docs/ART_*.md`、`docs/TEACHER_*.md`、`docs/SHOT_BLOCKS_CHAPTER01.md`、本交接文档

### 1.3 禁止改路径 / 禁止事项

| 禁止 | 说明 |
| --- | --- |
| `scripts/actors/teacher.gd` AI / 巡逻 / 侦测数值 | 冲突时**丢弃美术侧对该文件的改动**，保开发版 |
| 钥链逻辑、门禁、Cabinet UX、NavMesh | 归开发；美术只做钥匙/门闩**外观材质** |
| AudioBus / ambience / 脚步 / 刺音 | 归光影音效 |
| 存档 schema、通关条件、acceptance 夹具语义 | 归开发 |
| 重开 Alert `?` / `!` / 嫌疑条 | **产品已取消**，可读性靠姿态/音效/剪影 |
| `tools/author_scene.py` 随意重跑 | 会盖光照与场景 |
| 合 `main` / 开合 main 的 PR | 仅 ark |

冲突 rebase 原则：**只保美术文件**（材质、mesh、Visual、capture、art docs/evidence）；勿改 teacher AI / 音频 / 钥逻辑。

---

## 2. 视觉方向（摘要）

真源：`docs/ART_DIRECTION.md`。

- **风格**：轻漫画风格化写实 + 校园微恐；气质可参考《你好，邻居》的停-听-开门压迫感，**不抄资产、不抄关**。
- **一致性 > 单模**：共用材质族（灰白涂料 / 绿墙裙 / 木 / 磨石 / 漆金属 / 玻璃 + 交互金属 `#B78C48`）。
- **不要**：写真扫描拼装、血腥医院脏污、过强描边卡通、全黑手电筒路线。
- **教师**：高瘦监考感（头身约 1:7–1:8）；大衣剪影 + 眼镜/微笑双层识别；预算约 15k–30k tris。
- **警戒 UI**：Alert `?`/`!`/条 **已取消**，不得重开。
- **验收**：同机位复拍（S01–S05 / 前情 P01–P09）；盒端 `gl_compatibility` ≠ 真机 Forward+，**盒图不报视觉 PASS**。

---

## 3. 已完成（带 tip SHA）

下列 tip **均已是** `origin/feat/chapter01-module-pass` 的祖先（2026-10-28 fetch 时 tip 为 `2344404`）。旁支 `art/*` 无额外未合入 commit。

| 交付 | tip（短） | 全长 | 要点 / 文档·证据 |
| --- | --- | --- | --- |
| 教师高瘦剪影 P0 | `91204e2` | `91204e2e3ef847ea2f928983b448408c37e6dfc6` | `art: slim tall teacher silhouette`；`docs/TEACHER_STYLE_SAMPLE_P0.md`、`TEACHER_VISUAL_BRIEF.md`；`assets/models/teacher/teacher_p0_blockout.glb`；`evidence/screenshots/art-review/teacher-p0-*` |
| 大衣–墙对比 S02 | `bf5b49e` / stamp `614e536` | `bf5b49e11404af5f7e93ae78ac0b11906fdc8154` | `docs/ART_COAT_CONTRAST_S02.md`；`mat_coat*.tres`；`art-review/art-s02-coat-contrast-*` |
| 大衣躯干/肩线可读 | `5dcd17e` / stamp `0066f6d` | `5dcd17e3ea6a95522b9599da59f7cf588805093d` | `docs/ART_COAT_BODY_READ_S02_20261028.md`；`evidence/screenshots/art-coat-body-read/` |
| 交互钥金属 `#B78C48` | `477a8f3` / stamp `d4422a2` | `477a8f3f346f825e8b2c2ac887a071fa8f0ab8ab` | `docs/ART_KEYCHAIN_METAL_READ_20261028.md`；`assets/materials/mat_interact_metal.tres`；`evidence/screenshots/art-keychain-metal/` |
| S01 角落样板 | `ea2d952` / stamp `367012f` | `ea2d95296c1aa3683d2708ba776e782a50d45393` / `367012f4f1877c9612404346b92f23c32376b8c9` | 墙裙/低柜/窗边；`docs/ART_S01_CORNER_SAMPLE_20261028.md`；`tools/capture_art_s01_corner.gd`；`art-review/art-s01-*` |
| 前情镜头表 V2 + 静态机位 P01–P09 | `d73f56e` / stamp `d6fec5e` | `d73f56ef0f6936b5dc5d647645b8fa2d4f6da7e0` | `docs/ART_PROLOGUE_CAMS_20261028.md`、`ART_PROLOGUE_CINEMATIC_20261028.md`；`tools/capture_art_prologue_cams.gd` |
| 动态缓推 + crossfade | `16daf3e` / stamp `5e82703` | `16daf3e2799dd9a5c6a165870b403d3bcafbeeaa` / `5e8270350cb008bad95380a6649cfdce77ddfe82` | `prologue.gd` Tween + fade；`docs/ART_PROLOGUE_DYNAMIC_20261028.md`；`tools/capture_art_prologue_dynamic.gd`；`evidence/art-prologue/*-dyn-*` 与 `art-review/art-prologue-*-dyn-*` |

**相关路径速查**

- 方向：`docs/ART_DIRECTION.md`
- 镜头验收机位：`docs/SHOT_BLOCKS_CHAPTER01.md`
- 材质：`assets/materials/**`、`assets/materials/teacher/**`、`mat_interact_metal.tres`
- 模型：`assets/models/teacher/`
- Capture：`tools/capture_art_*.gd`（prologue cams/dynamic、s01 corner、s01–s05、coat、keychain）

---

## 4. 进行中 / 卡点

| 项 | 状态 | 说明 |
| --- | --- | --- |
| 视觉 PASS | **阻塞** | 等真机 **Forward+** 复看；盒端仅 `gl_compatibility` / llvmpipe，**不得**据此报 PASS |
| 体验/惊吓节奏 | **等 ark 盲测** | 动镜与大衣可读已交证据，玩法体感不由美术单方判完 |
| 盒 vs 真机渲染差 | 已知 | 暗部、金属高光、大衣边缘在 gl_compat 与 Forward+ 不一致；以真机同机位为准 |
| Alert 可视化 | 已关 | 勿再做 `?`/`!`/条；若审核提「危险不可读」，用姿态/剪影/光差，不重开 UI |

---

## 5. 未来工作（给下一任美术）

1. **P2 机关视觉**：计时器、门楔、检修盖、卷栅链/棘轮——沿用交互金属族 `#B78C48` + 形状区分；先出同机位 before/after，再交【美术交付】。
2. **Art Bible / `ART_DIRECTION.md` 维护**：样板推广门槛（8m 教师朝向、门窗可读、无大面积曝光）未全过前，继续修样板而非扩全楼。
3. **S01–S05 同机位真机对照**：用 `SHOT_BLOCKS_CHAPTER01.md` 钉死坐标/FOV；真机 Forward+ 复拍入库。
4. **教师绑骨 / 姿态**：Idle · Walk · Chase（及 Turn/Investigate 若排期）；临时 blockout 不算完成角色。
5. **刀口补光**：真机定稿前**暂不加**刀光/补灯；若审核点名肩线洗白，先与 @光影音效 对齐 Fixture 边界，勿单方面加 Omni。
6. **与开发/审核交接口径**：每完成一块必须写 `docs/ART_<TASK>_….md`，标题以 **`【美术交付】`** 开头，至少含：任务 ID、分支、tip SHA（短+全长）、改动路径、未改路径、复跑命令、请协作方（开发合并 / 审核真机 / 不报 PASS 声明）。

---

## 6. 真机验证钉 tip

- **本交接 commit（已 push 的 `origin/feat/chapter01-module-pass` tip）**：  
  - 短：`5401c56`  
  - 全长：`5401c56318b45562ac0912ccad24328b3c7ffb93`  
  - message：`docs(art): ART agent handoff 20261028 + residual imports/evidence`
- **美术内容已齐时的功能 tip（脚步等非美术叠层）**：`2344404` / `234440437190c4612ddd80cdc63c0f5e2d06ff4c`（本交接祖先；含动镜 `5e82703`）。
- **动镜验收祖先**：看 `5e82703`（`5e8270350cb008bad95380a6649cfdce77ddfe82`）即可；内容 tip 为 `16daf3e`。
- **静态前情机位祖先**：`d6fec5e` / 内容 `d73f56e`。
- **S01 角落**：`367012f` / 内容 `ea2d952`。
- 之后每次再 push，以 **`git fetch` + `git rev-parse origin/feat/chapter01-module-pass`** 更新钉 tip。

同日其它角色交接（已在 feat 祖先链上，供协作）：审核 `CHAPTER01_AGENT_HANDOFF_20261028.md`、情节 `docs/design/AGENT_HANDOFF_PLOT_MECHANICS_20261028.md`、光影 `AUDIO_LIGHTING_AGENT_CONTEXT_20261028.md`。

---

## 7. 复跑命令（盒端）

Godot **4.7.2**；盒上常用 `opengl3` + `gl_compatibility`（**≠ Forward+**）。

```bash
cd EscapeExamGodot

# 前情动态 P01/P03/P07 start/mid/end
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_prologue_dynamic.gd

# 前情静态 P01–P09
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_prologue_cams.gd

# S01 角落样板
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_s01_corner.gd

# S01–S05 / 大衣 / 钥金属（按需）
# res://tools/capture_art_s01_s05.gd
# res://tools/capture_art_coat_body_read_s02_s03.gd
# res://tools/capture_art_coat_contrast_s02.gd
# res://tools/capture_art_keychain_metal.gd
```

自动验收（勿当视觉 PASS）：

```bash
godot --path EscapeExamGodot --headless -s res://tests/acceptance.gd -- --test
```

---

## 8. Worktree / 推送备忘

```bash
git fetch origin
# 推荐干净 worktree 跟总分支
git worktree add ../wt-art origin/feat/chapter01-module-pass
cd ../wt-art
# …只改美术路径…
git add EscapeExamGodot/docs EscapeExamGodot/assets EscapeExamGodot/tools/capture_art_* \
  EscapeExamGodot/evidence/screenshots/art-* EscapeExamGodot/evidence/art-prologue \
  EscapeExamGodot/scenes/actors/teacher.tscn EscapeExamGodot/scripts/systems/prologue.gd
git commit -m "art: …"
git pull --rebase origin feat/chapter01-module-pass   # 冲突只保美术文件
git push origin HEAD:feat/chapter01-module-pass
git ls-remote origin refs/heads/feat/chapter01-module-pass
```

**绝对不要**：`git push origin main`、向 main 开 PR、把未推送进度只留在盒本地。

---

## 9. 本交接打包说明（2026-10-28）

- 已确认远程 feat 含 `5e82703`（动镜）、`367012f`（S01）、大衣/钥金属等祖先。
- 旁支 `art/chapter01-module-pass`（远端 tip `d6fec5e`）、本地 `art/prologue-dynamic`（`5e82703`）**无领先 feat 的未推 commit**。
- 本交接 tip `5401c56` 额外入库：缺失的 `teacher-p0-*.png.import`、若干 `capture_*.gd.uid`、QA 留存的 `art-coat-body-read/s0*-qa-0066f6d.png`、`art-keychain-metal/key-k2-insitu.png`，以及本文档（随后 stamp 本 tip SHA）。
