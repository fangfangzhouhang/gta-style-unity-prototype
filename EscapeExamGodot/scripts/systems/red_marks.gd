extends Node3D
## 红笔标记系统（V2 招牌意象 · 情节/机制域自注册，Game.boot 挂到 world 下）。
## 教师巡查后在门框上留下的红笔勾/圈：随进程推进逐步增多，玩家读红笔即读教师的心智。
## 进程量 severity = 卷栅档位 + 教师已上场 + 近路已开 + 停播 + min(被抓次数,2)，
## 需要标记的门数 = clamp(severity, 0, 门总数)。标记不入存档：按 progress 随时可重建，
## reset / 读档后由轮询自动对齐（多删少补）。锚点 = 门节点上方门框右柱外侧。

const STAMP_RED := Color(0.616, 0.314, 0.282)  # 印章红 #9D5048，与 ART_DIRECTION 一致
const POLL_SECONDS := 0.25

var _material: StandardMaterial3D
var _doors: Array = []
var _marks: Dictionary = {}  # door node -> mark node
var _poll := 0.0
var _last_severity := -1

func _ready() -> void:
	_material = StandardMaterial3D.new()
	_material.albedo_color = STAMP_RED
	_material.roughness = 0.55
	_doors = get_tree().get_nodes_in_group("doors")
	_doors.sort_custom(func(a, b): return String(a.name) < String(b.name))
	set_process(true)
	_sync_marks(true)

func _process(delta: float) -> void:
	_poll -= delta
	if _poll > 0.0:
		return
	_poll = POLL_SECONDS
	if _doors.is_empty():
		_doors = get_tree().get_nodes_in_group("doors")
		_doors.sort_custom(func(a, b): return String(a.name) < String(b.name))
	_sync_marks(false)

func severity() -> int:
	var p: Dictionary = Game.progress
	var value := int(p.get("gate_stage", 0))
	if bool(p.get("teacher_summoned", false)):
		value += 1
	if bool(p.get("shortcut", false)):
		value += 1
	if bool(p.get("loop_stopped", false)):
		value += 1
	value += mini(Game.caught_count, 2)
	return value

func _sync_marks(force: bool) -> void:
	var level := severity()
	if not force and level == _last_severity:
		return
	_last_severity = level
	var wanted := clampi(level, 0, _doors.size())
	# 多删：进程回退（新一局/读档）时移除超出数量的标记，按生成顺序从后往前删。
	var present: Array = _marks.keys()
	for i in range(present.size() - 1, wanted - 1, -1):
		if i < 0:
			break
		var door = present[i]
		if is_instance_valid(_marks[door]):
			_marks[door].queue_free()
		_marks.erase(door)
	# 少补：按门名字母序逐个补齐，保证同一进程下标记落点稳定可复现。
	for i in wanted:
		var door = _doors[i]
		if _marks.has(door) and is_instance_valid(_marks[door]):
			continue
		_add_mark(door, i)

func _add_mark(door: Node3D, index: int) -> void:
	var mark := Node3D.new()
	mark.name = "RedMark_" + String(door.name)
	# 锚点：门框右柱（局部 x=+0.48）上部 y≈2.02，略出门框面 z=+0.13，避免与门框共面闪烁。
	var anchor_local := Vector3(0.48, 2.02, 0.13)
	mark.global_transform = Transform3D(door.global_basis, door.global_position + door.global_basis * anchor_local)
	add_child(mark)
	if index % 2 == 0:
		_build_check(mark)
	else:
		_build_circle(mark)
	_marks[door] = mark
	Game.trace("red_mark_added", {"door": String(door.name), "stage": severity()})

func _box(parent: Node3D, size: Vector3, pos: Vector3, rot_z: float) -> void:
	var mesh := BoxMesh.new()
	mesh.size = size
	mesh.material = _material
	var instance := MeshInstance3D.new()
	instance.mesh = mesh
	instance.position = pos
	instance.rotation.z = rot_z
	parent.add_child(instance)

func _build_check(parent: Node3D) -> void:
	# 一道勾：短臂 + 长臂两小盒拼成 ✓ 的抽象形。
	_box(parent, Vector3(0.05, 0.16, 0.02), Vector3(-0.055, -0.02, 0), 0.7)
	_box(parent, Vector3(0.05, 0.30, 0.02), Vector3(0.075, 0.045, 0), -0.6)

func _build_circle(parent: Node3D) -> void:
	# 一道圈：四小盒拼成方形圈注（红笔圈读的抽象形）。
	_box(parent, Vector3(0.30, 0.045, 0.02), Vector3(0, 0.13, 0), 0.0)
	_box(parent, Vector3(0.30, 0.045, 0.02), Vector3(0, -0.13, 0), 0.0)
	_box(parent, Vector3(0.045, 0.30, 0.02), Vector3(-0.13, 0, 0), 0.0)
	_box(parent, Vector3(0.045, 0.30, 0.02), Vector3(0.13, 0, 0), 0.0)
