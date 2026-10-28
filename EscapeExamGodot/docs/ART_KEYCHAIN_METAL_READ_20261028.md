# 【美术交付】ART-KEYCHAIN-METAL-READ · 交互金属族钥匙 / 前室门可读性

- **任务 ID**：`ART-KEYCHAIN-METAL-READ`
- **分支**：`art/chapter01-module-pass`（worktree `/workspace/wt-art`）
- **基线 tip（FF sync）**：`9121693209ffa42a67c576b47b87ebedb7559978`（`feat/chapter01-module-pass` · gated min keychain）
- **本交付 Commit**：477a8f3f346f825e8b2c2ac887a071fa8f0ab8ab（477a8f3）
- **日期**：2026-10-28（Asia/Shanghai）
- **Godot**：4.7.2 stable · box 实跑 `gl_compatibility` + llvmpipe
- **范围**：共享交互金属材质 + KeyK1/K2/K3 可视 mesh/材质 + GateAntechamber 把手/锁牌材质覆盖；**未改** `.gd` 钥匙/门逻辑、chapter 拓扑、NavMesh、教师 AI、灯/AudioBus、Alert UI。
- **状态**：材质刀修完成 · 盒上 albedo-fill + in-situ 证据已拍 · **视觉 PASS 待 @游戏审核 forward+ 复看**

## 0. 背景

feat tip `9121693` 已挂灰盒 KeyK1/KeyK2/KeyK3 与 `Doors/GateAntechamber`。钥匙原为单盒 mesh + 内联偏黄金属；前室门把手走 `door.tscn` 内联铜绿。美术需按 `ART_DIRECTION` **交互机械件 `#B78C48`** 统一可读性，使钥匙相对灰墙/木柜可辨、前室门握位同族。

## 1. Albedo before → after

| 资源 | 角色 | Before | After | 说明 |
| --- | --- | --- | --- | --- |
| （新）`assets/materials/mat_interact_metal.tres` | 交互金属族 | — | **`Color(0.717647, 0.54902, 0.282353)`** = `#B78C48` · roughness **0.42** · metallic **0.72** · specular 0.55 | 共享 StandardMaterial3D |
| chapter `rKeyMat`（已删） | KeyK* 内联 | `Color(0.72, 0.62, 0.28)` · rough 0.35 · metal 0.55 | **改引用** `mat_interact_metal` | 去饱和偏绿黄 → 规范黄铜 |
| `door.tscn` Handle `r3`（未改资源） | 其他门把手 | `Color(0.57, 0.52, 0.35)` | **未改** | 仅 GateAntechamber 实例覆盖 |
| GateAntechamber Handle* override | 前室门把手 | 继承上表 | **`mat_interact_metal`** | `surface_material_override/0` |
| GateAntechamber `LockPlate`（新） | 锁牌/握位板 | — | 同族金属 · 随 `Hinge/Panel` 开合 | 不改动画节点/脚本 |
| KeyK* Caption `modulate` | 近距标签 | `Color(0.85, 0.8, 0.55)` | **`Color(0.78, 0.64, 0.36)`** | 贴近金属族暖读 |

## 2. 钥匙低模剪影（仍灰盒预算）

| 部件 | Mesh | 尺寸 / 位移 | 材质 |
| --- | --- | --- | --- |
| `Mesh`（环/Bow） | `rKeyMesh` Box | `0.07 × 0.07 × 0.018` | `mat_interact_metal` |
| `Shaft`（新） | `rKeyShaft` Box | `0.11 × 0.018 × 0.018` @ `(0.09,0,0)` | 同左 |
| `Bit`（新） | `rKeyBit` Box | `0.035 × 0.042 × 0.014` @ `(0.155,-0.012,0)` | 同左 |

碰撞 `rKeyShape` / `key_pickup.gd` / `key_id` / progress **未动**。

## 3. GateAntechamber

- 实例仍为 `scenes/props/door.tscn`；**未改** `door.tscn` / `door.gd`。
- 仅在 chapter 上覆盖：
  - `Hinge/Panel/Handle-0_065` · `Handle0_065` → `mat_interact_metal`
  - 新增 `Hinge/Panel/LockPlate`（Box `0.12 × 0.2 × 0.02` @ leaf 面）→ 同材质；挂在 Panel 下随开门动画。
- 门叶/框/Inset 仍木漆中性色。

## 4. 证据截图

目录：`EscapeExamGodot/evidence/screenshots/art-keychain-metal/`

| 文件 | 角色 |
| --- | --- |
| `key-k1-albedo.png` / `key-k2-albedo.png` / `key-k3-albedo.png` | 临时 fill Omni 近拍 · 证明 `#B78C48` 族 albedo（**非**场景灯改） |
| `gate-handle-albedo.png` | 把手/锁牌 fill 近拍 |
| `gate-antechamber-insitu.png` | 玩家机位 in-situ · 前室门握位相对冷色门叶对比 |

复跑：

```bash
cd EscapeExamGodot
xvfb-run -a godot --path . \
  --rendering-driver opengl3 --rendering-method gl_compatibility \
  -s res://tools/capture_art_keychain_metal.gd
```

（box 无 Vulkan → `gl_compatibility`；目标机可用 `forward_plus`。fill Omni 仅 capture 脚本临时节点，不进场景。）

盒上 center probe（albedo-fill）：K1≈`(0.77,0.61,0.29)` · Gate≈`(0.91,0.82,0.50)`（受 fill 抬亮；规范 albedo 仍为 `#B78C48`）。

## 5. 自检 / 约束

- [x] 未改任何 `.gd` 钥匙/门逻辑、NavMesh、教师 AI、灯、AudioBus、Alert UI
- [x] 未改 chapter 房间拓扑 / 门进度字段
- [x] 未自行 merge feat/main
- [x] 交互金属仅小范围（钥匙 + 前室门握位/锁牌）
- [x] 无血腥、无过强描边；轻漫画风格化写实校园微恐怖

## 6. 请 @游戏开发 / @游戏审核

1. **@游戏开发**：请将 `art/chapter01-module-pass` **merge / cherry-pick** 进 `feat/chapter01-module-pass`（材质 + Key 可视子节点 + GateAntechamber 覆盖 + 本交付文档/证据/捕获脚本）。勿整文件覆盖非美术逻辑。美术 **不自行合** feat/main。
2. **@游戏审核**：合入后在 **forward+** 复看：三把钥匙相对灰墙/柜体是否可读为拾取物；出口前室门把手/锁牌是否同族黄铜、开门动画是否正常；教室暖区/走廊冷区是否被金属漂色。
3. 若仍不足：开新任务授权（可考虑计时器/卷栅链同族，**本交付未扩**）。

---

*ART-KEYCHAIN-METAL-READ · 美术交付 · 盒证据完成 · 视觉 PASS 待 forward+ 复看*
