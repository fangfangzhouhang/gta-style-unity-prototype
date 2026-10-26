extends Node3D

@export var delay_seconds := 3.0
@export var ringing_seconds := 8.0
var held := false
var owned := false
var state := "idle"
var remaining := 0.0
var pulse := 0.0
var activation := 0
var home := Vector3.ZERO

func _ready() -> void:
	home = position

func prompt(_actor) -> String:
	return "E  收回计时器 · 停止响铃" if state != "idle" else "E  拿起考试计时器"

func interact(_actor) -> void:
	owned = true
	held = true
	state = "idle"
	$Target.collision_layer = 0
	Game.discover("timer", "计时器上弦后等 3 秒响铃，持续 8 秒。放好后离开；响声会引来老师。")
	Game.save_progress()

func arm() -> void:
	if not owned: return
	if not held and Game.player.global_position.distance_to(global_position) > 2.2:
		Game.ui.toast("靠近计时器，才能再次上弦。", 2)
		return
	if state != "idle":
		state = "idle"
		Game.ui.toast("计时器已停。", 1.5)
		return
	activation += 1
	state = "countdown"
	remaining = delay_seconds
	pulse = 0
	Game.trace("timer_armed", {"held": held})

func place_from_camera() -> bool:
	if not held: return false
	var camera: Camera3D = Game.player.camera
	var from := camera.global_position
	var toward := -camera.global_basis.z
	var query := PhysicsRayQueryParameters3D.create(from, from + toward * 2.5, 1 | 16 | 32)
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	if hit.is_empty() or hit.normal.y < 0.85:
		Game.ui.toast("对准附近的桌面或地面放置。", 1.8)
		return false
	var placement: Vector3 = hit.position + Vector3.UP * 0.07
	var shape := SphereShape3D.new()
	shape.radius = 0.055
	var clearance := PhysicsShapeQueryParameters3D.new()
	clearance.shape = shape
	clearance.transform.origin = placement
	clearance.collision_mask = 1 | 16 | 32
	if not get_world_3d().direct_space_state.intersect_shape(clearance, 1).is_empty(): return false
	held = false
	global_position = placement
	global_rotation = Vector3(0, Game.player.rotation.y, 0)
	scale = Vector3.ONE
	$Target.collision_layer = 8
	Game.trace("timer_placed", {"position": [placement.x, placement.y, placement.z]})
	return true

func _process(delta: float) -> void:
	if not is_instance_valid(Game.player): return
	if held:
		global_transform = Game.player.camera.global_transform * Transform3D(Basis.IDENTITY, Vector3(0.34, -0.30, -0.65))
		scale = Vector3.ONE * 0.85
	if Game.mode != "game": return
	if state == "idle":
		$Display.text = "03"
		return
	remaining -= delta
	pulse -= delta
	$Display.text = str(ceili(maxf(remaining, 0))).pad_zeros(2)
	if state == "countdown":
		if pulse <= 0:
			pulse = 0.5
			Game.acoustics.emit_sound(global_position, "tick", 3, "计时器滴答", "timer")
		if remaining <= 0:
			state = "ringing"
			remaining = ringing_seconds
			pulse = 0
			Game.trace("timer_ringing")
	elif state == "ringing":
		if pulse <= 0:
			pulse = 1
			Game.acoustics.emit_sound(global_position, "timer", 28, "计时器铃声", "timer", "ring_" + str(activation))
		if remaining <= 0: state = "idle"

func reset_timer() -> void:
	state = "idle"
	remaining = 0
	held = owned
	if not owned:
		position = home
		rotation = Vector3.ZERO
		scale = Vector3.ONE
	$Target.collision_layer = 0 if held else 8
