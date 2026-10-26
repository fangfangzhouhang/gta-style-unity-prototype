extends Node3D

@export var door_name := "木门"
@export var is_shortcut := false
@export var initial_open := false
@export var unlock_side := 1.0
@onready var hinge: Node3D = $Hinge
var is_open := false
var locked := false
var cooldown := 0.0
var teacher_open_time := 0.0
var tween: Tween

func _ready() -> void:
	add_to_group("doors")
	reset_door()

func reset_door() -> void:
	locked = is_shortcut and not Game.progress.get("shortcut", false)
	is_open = initial_open
	if is_shortcut and not locked: is_open = true
	hinge.rotation.y = -PI / 2 if is_open else 0
	cooldown = 0
	teacher_open_time = 0
	if has_node("NavigationLink3D"): $NavigationLink3D.enabled = not locked

func _physics_process(delta: float) -> void:
	cooldown = maxf(0, cooldown - delta)

func prompt(actor) -> String:
	if locked:
		return "E  拉开背面的门闩" if to_local(actor.global_position).z * unlock_side > 0 else "E  查看运卷门 · 门闩在另一侧"
	return "E  " + ("关上" if is_open else "推开") + door_name

func interact(actor) -> void:
	if cooldown > 0: return
	if locked:
		if to_local(actor.global_position).z * unlock_side <= 0:
			Game.discover("latch_side", "运卷门的门闩在印务室那一侧。正门可以绕过去。")
			return
		locked = false
		if has_node("NavigationLink3D"): $NavigationLink3D.enabled = true
		Game.unlock_shortcut()
	set_open(not is_open)

func set_open(value: bool, teacher_action := false) -> bool:
	if locked or cooldown > 0: return false
	if not value:
		for actor in [Game.player, Game.teacher]:
			if is_instance_valid(actor) and global_position.distance_to(actor.global_position) < 1.0:
				if not teacher_action: Game.ui.toast("门口有人，先让开一点。", 1.5)
				return false
	if is_instance_valid(tween): tween.kill()
	is_open = value
	cooldown = 1.1
	tween = create_tween().set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	tween.tween_property(hinge, "rotation:y", -PI / 2 if is_open else 0.0, 0.45)
	Game.acoustics.emit_sound(global_position + Vector3.UP, "door", 9, "门轴与门闩", "teacher" if teacher_action else "door")
	Game.trace("door", {"name": name, "open": value})
	return true

func request_teacher_open(delta: float) -> bool:
	if locked or is_open: return false
	teacher_open_time += delta
	if teacher_open_time >= 1.2:
		set_open(true, true)
		teacher_open_time = 0
	return true
