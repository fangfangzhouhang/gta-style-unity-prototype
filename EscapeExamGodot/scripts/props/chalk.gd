extends RigidBody3D
## 粉笔（V2 情节/机制域）：同一场景 chalk.tscn 三种角色——
## role="spawner"：由 Game.boot 在讲台处自注册生成，负责在讲台上放 2 支可拾取粉笔，
##   并在新一局（Game.elapsed 回退）时补回已消耗的拾取物。
## role="pickup"：冻结的拾取物，玩家准星 E 拾取 → Game.chalk_count += 1（上限 Game.CHALK_MAX）。
## role="thrown"：投掷刚体，首次撞击发声（acoustics kind="chalk"），6 秒后自动消失。
## 拾取与投掷共用本场景：拾取态 freeze=true 且碰撞层置 8（交互层，供玩家射线命中）；
## 投掷态解冻并切到普通刚体层。

const PICKUP_OFFSETS: Array[Vector3] = [Vector3(-0.30, 0.03, 0.06), Vector3(0.26, 0.03, -0.04)]
const THROW_SPEED := 8.0
const THROW_LIFT := 2.0
const THROWN_LIFETIME := 6.0

var role := "pickup"
var _sounded := false
var _age := 0.0
var _last_elapsed := 0.0
var _pickups: Array = []

func _ready() -> void:
	if role == "spawner":
		# Spawner 本体无形无碰撞，只负责生成/补回拾取物。
		freeze = true
		collision_layer = 0
		collision_mask = 0
		if has_node("Visual"):
			$Visual.visible = false
		# 延迟到 boot 完成（讲台坐标已写入）后再生成拾取物。
		call_deferred("_spawn_pickups")
		set_process(true)
		return
	contact_monitor = true
	max_contacts_reported = 4
	if role == "pickup":
		freeze = true
		collision_layer = 8
		collision_mask = 0
	else:
		# 投掷态必须与世界/门/玻璃碰撞（场景默认 mask=0 是拾取态的，投掷态在此覆盖）。
		collision_layer = 64
		collision_mask = 1 | 16 | 32
		body_entered.connect(_on_body_entered)

func _process(delta: float) -> void:
	if role == "spawner":
		# 新一局会把 Game.elapsed 清零：据此补回拾取物（被抓不补，粉笔随身保留）。
		if _last_elapsed > 1.0 and Game.elapsed < 0.5 and Game.playing:
			_spawn_pickups()
		_last_elapsed = Game.elapsed
		return
	if role == "thrown":
		_age += delta
		if _age >= THROWN_LIFETIME:
			queue_free()

func _spawn_pickups() -> void:
	# 拾取物挂在 world 下（不作 spawner 的刚体子节点，避免刚体嵌套）。
	var host: Node = get_parent()
	if host == null:
		return
	for old in _pickups:
		if is_instance_valid(old):
			old.queue_free()
	_pickups.clear()
	var scene: PackedScene = load("res://scenes/props/chalk.tscn")
	for offset in PICKUP_OFFSETS:
		var pickup = scene.instantiate()
		pickup.set("role", "pickup")
		host.add_child(pickup)
		pickup.global_position = global_position + offset
		pickup.rotation.y = randf() * PI
		_pickups.append(pickup)

static func spawn_thrown(parent: Node, from: Vector3, direction: Vector3) -> Node:
	var scene: PackedScene = load("res://scenes/props/chalk.tscn")
	var body = scene.instantiate()
	body.set("role", "thrown")
	parent.add_child(body)
	body.global_position = from
	body.linear_velocity = direction * THROW_SPEED + Vector3.UP * THROW_LIFT
	body.angular_velocity = Vector3(randf_range(-6.0, 6.0), randf_range(-6.0, 6.0), 0.0)
	return body

func prompt(_actor) -> String:
	if role != "pickup":
		return ""
	if Game.chalk_count >= Game.CHALK_MAX:
		return "粉笔 · 身上已经有两支了"
	return "E  捡起粉笔（右键扔出声响）"

func interact(_actor) -> void:
	if role != "pickup":
		return
	if Game.chalk_count >= Game.CHALK_MAX:
		Game.ui.toast("粉笔够用了。右键扔出去，能把人引开。", 2.0)
		return
	Game.chalk_count += 1
	Game.trace("chalk_pickup", {"count": Game.chalk_count})
	Game.ui.toast("捡起一支粉笔（%d/%d）。右键扔向别处，落地声能把老师引过去。" % [Game.chalk_count, Game.CHALK_MAX], 2.5)
	queue_free()

func _on_body_entered(_body: Node) -> void:
	if role != "thrown" or _sounded:
		return
	_sounded = true
	if is_instance_valid(Game.acoustics):
		Game.acoustics.emit_sound(global_position, "chalk", 9.0, "粉笔落在地上", "chalk")
	Game.trace("chalk_landed", {"position": [global_position.x, global_position.z]})
