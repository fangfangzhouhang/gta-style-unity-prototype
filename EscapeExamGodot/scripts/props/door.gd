extends Node3D

@export var door_name := "木门"
@export var is_shortcut := false
@export var initial_open := false
@export var unlock_side := 1.0
@export var allow_wedge := true
@onready var hinge: Node3D = $Hinge
var is_open := false
var wedged := false
var locked := false
var cooldown := 0.0
var teacher_open_time := 0.0
var tween: Tween
const AJAR_ANGLE := 0.30  # ~17° — silhouette/acoustics, capsule cannot squeeze through

func _ready() -> void:
	add_to_group("doors")
	reset_door()

func reset_door() -> void:
	locked = is_shortcut and not Game.progress.get("shortcut", false)
	wedged = false
	is_open = initial_open
	if is_shortcut and not locked: is_open = true
	hinge.rotation.y = -PI / 2 if is_open else 0
	cooldown = 0
	teacher_open_time = 0
	if has_node("NavigationLink3D"): $NavigationLink3D.enabled = not locked

func is_fully_open() -> bool:
	return is_open and not wedged

func _physics_process(delta: float) -> void:
	cooldown = maxf(0, cooldown - delta)

func prompt(actor) -> String:
	if locked:
		return "E  拉开背面的门闩" if to_local(actor.global_position).z * unlock_side > 0 else "E  查看运卷门 · 门闩在另一侧"
	var wedge = _wedge()
	if allow_wedge and wedge != null and wedge.held and not wedged and not is_fully_open():
		return "E  卡门楔 · 保持门缝窥视"
	if wedged:
		return "E  取出门楔 · " + door_name
	return "E  " + ("关上" if is_fully_open() else "推开") + door_name

func interact(actor) -> void:
	if cooldown > 0: return
	if locked:
		if to_local(actor.global_position).z * unlock_side <= 0:
			Game.discover("latch_side", "运卷门的门闩在印务室那一侧。正门可以绕过去。")
			return
		locked = false
		if has_node("NavigationLink3D"): $NavigationLink3D.enabled = true
		Game.unlock_shortcut()
	var wedge = _wedge()
	if allow_wedge and wedge != null and wedge.held and not wedged and not is_fully_open():
		apply_wedge(wedge)
		return
	if wedged:
		clear_wedge(true)
		return
	set_open(not is_open)

func apply_wedge(wedge: Node) -> void:
	if is_instance_valid(tween): tween.kill()
	wedged = true
	is_open = true
	hinge.rotation.y = -AJAR_ANGLE
	cooldown = 0.6
	wedge.place_on_door(self)
	Game.ui.toast("门缝卡住了。能看见剪影，但挤不过去。", 2.5)
	Game.trace("door_ajar", {"name": name})

func clear_wedge(return_to_hand: bool) -> void:
	if not wedged: return
	wedged = false
	is_open = false
	if is_instance_valid(tween): tween.kill()
	hinge.rotation.y = 0.0
	cooldown = 0.6
	var wedge = _wedge()
	if return_to_hand and wedge != null:
		wedge.return_to_hand()
	elif wedge != null:
		wedge.placed_door = null

func set_open(value: bool, teacher_action := false) -> bool:
	if locked or cooldown > 0: return false
	if wedged:
		clear_wedge(false)
		var wedge = _wedge()
		if wedge != null:
			wedge.restore_home()
	if not value:
		for actor in [Game.player, Game.teacher]:
			if is_instance_valid(actor) and global_position.distance_to(actor.global_position) < 1.0:
				if not teacher_action: Game.ui.toast("门口有人，先让开一点。", 1.5)
				return false
	if is_instance_valid(tween): tween.kill()
	is_open = value
	wedged = false
	cooldown = 1.1
	tween = create_tween().set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	tween.tween_property(hinge, "rotation:y", -PI / 2 if is_open else 0.0, 0.45)
	Game.acoustics.emit_sound(global_position + Vector3.UP, "door", 9, "门轴与门闩", "teacher" if teacher_action else "door")
	Game.trace("door", {"name": name, "open": value})
	return true

func request_teacher_open(delta: float) -> bool:
	if locked: return false
	if is_fully_open(): return false
	if wedged:
		# Dislodge wedge and open fully so AI is never soft-locked at an ajar leaf.
		clear_wedge(false)
		var wedge = _wedge()
		if wedge != null: wedge.restore_home()
		set_open(true, true)
		teacher_open_time = 0
		return true
	if is_open: return false
	teacher_open_time += delta
	if teacher_open_time >= 1.2:
		set_open(true, true)
		teacher_open_time = 0
	return true

func _wedge() -> Node:
	if not is_instance_valid(Game.world): return null
	return Game.world.get_node_or_null("DoorWedge")
