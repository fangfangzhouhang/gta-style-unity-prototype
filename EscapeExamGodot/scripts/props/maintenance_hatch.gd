extends Node3D

## Maintenance hatch into corridor H. Open/close; weak door noise; optional rack clear check.

@export var hatch_name := "检修盖"
@export var pair_path: NodePath
@export var rack_path: NodePath
@export var open_angle := -1.35
@export var nav_link_path: NodePath

@onready var hinge: Node3D = $Hinge
var is_open := false
var locked := false  # doors-group compatibility for teacher loop
var cooldown := 0.0
var tween: Tween

func _ready() -> void:
	add_to_group("hatches")
	add_to_group("doors")  # acoustics portal meta/door can resolve is_open
	hinge.rotation.x = 0.0

func _physics_process(delta: float) -> void:
	cooldown = maxf(0.0, cooldown - delta)

func prompt(_actor) -> String:
	if not _rack_allows():
		return "E  查看检修盖 · 卷纸架挡住了"
	return "E  " + ("关上" if is_open else "打开") + hatch_name

func interact(_actor) -> void:
	if cooldown > 0.0: return
	if not _rack_allows() and not is_open:
		Game.ui.toast("先把卷纸架推开，才能打开检修盖。", 2.0)
		return
	set_open(not is_open)

func _rack_allows() -> bool:
	if rack_path.is_empty(): return true
	var rack = get_node_or_null(rack_path)
	if rack == null: return true
	return rack.at_limit()

func set_open(value: bool, silent := false) -> bool:
	if cooldown > 0.0 and not silent: return false
	if is_open == value: return true
	is_open = value
	cooldown = 0.8
	if is_instance_valid(tween): tween.kill()
	tween = create_tween().set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	tween.tween_property(hinge, "rotation:x", open_angle if is_open else 0.0, 0.4)
	# Open panel must not keep blocking the portal (pair orientations swing into the walk volume).
	if has_node("Hinge/Panel"):
		$Hinge/Panel.collision_layer = 0 if is_open else 16
	_set_nav(is_open)
	if not silent:
		Game.acoustics.emit_sound(global_position + Vector3.UP, "door", 10.0, "检修盖" + ("打开" if is_open else "关上"), "hatch")
		Game.discover("hatch", "检修夹道连到出口前室侧口。它不能代替卷栅离场。")
		Game.trace("hatch", {"name": name, "open": is_open})
	var pair = get_node_or_null(pair_path)
	if pair != null and pair.is_open != is_open:
		pair.set_open(is_open, true)
	return true

func _set_nav(enabled: bool) -> void:
	if nav_link_path.is_empty(): return
	var link = get_node_or_null(nav_link_path)
	if link != null:
		link.enabled = enabled

func reset_door() -> void:
	## Group "doors" reset from chapter.
	is_open = false
	cooldown = 0.0
	hinge.rotation.x = 0.0
	if has_node("Hinge/Panel"):
		$Hinge/Panel.collision_layer = 16
	_set_nav(false)

func request_teacher_open(delta: float) -> bool:
	if is_open: return false
	if not _rack_allows(): return false
	# Teacher spends same ~1.2s as a normal door.
	if not has_meta("teacher_t"): set_meta("teacher_t", 0.0)
	set_meta("teacher_t", float(get_meta("teacher_t")) + delta)
	if float(get_meta("teacher_t")) >= 1.2:
		set_open(true)
		set_meta("teacher_t", 0.0)
	return true
