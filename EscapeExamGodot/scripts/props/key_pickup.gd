extends Node3D

## World key cabinet / drawer. Progress-permanent; catch does not remove ownership.
## open_first: E opens the cabinet, then a second E takes the key (true cabinet UX).

@export var key_id := "k1"
@export var key_label := "钥匙"
@export var fact_id := ""
@export_multiline var fact_summary := ""
@export_multiline var empty_caption := "空抽屉"
@export var open_first := true
@export var closed_prompt := "拉开抽屉"
@export var take_prompt := "取出钥匙"
## Optional CoverCabinet / furniture node whose Door* children swing open.
@export var cabinet_path: NodePath
@export var open_angle := -1.9

var is_open := false
var _cabinet_tween: Tween

func _ready() -> void:
	add_to_group("key_pickups")
	refresh_from_progress()

func refresh_from_progress() -> void:
	var owned := Game.has_key(key_id) if is_instance_valid(Game) else false
	if owned:
		is_open = true
	_apply_cabinet_visual(is_open, true)
	_set_key_meshes_visible(is_open and not owned)
	# Interact target stays on so empty cabinets remain readable.
	if has_node("Target"):
		$Target.collision_layer = 8
	if has_node("Caption"):
		$Caption.text = _caption_text(owned)

func prompt(_actor) -> String:
	if Game.has_key(key_id):
		return "E  查看 · " + empty_caption
	if open_first and not is_open:
		return "E  " + closed_prompt
	return "E  " + take_prompt

func interact(_actor) -> void:
	if Game.has_key(key_id):
		Game.ui.toast(empty_caption + "。钥匙已经在你身上。", 2.0)
		return
	if open_first and not is_open:
		_open_cabinet()
		return
	Game.grant_key(key_id, key_label)
	if not fact_id.is_empty():
		Game.discover(fact_id, fact_summary if not fact_summary.is_empty() else ("拿到了" + key_label))
	refresh_from_progress()
	Game.trace("key_pickup", {"id": key_id})

func _open_cabinet() -> void:
	is_open = true
	_apply_cabinet_visual(true, false)
	_set_key_meshes_visible(true)
	if has_node("Caption"):
		$Caption.text = _caption_text(false)
	Game.ui.toast(closed_prompt.replace("拉开", "拉开了").replace("打开", "打开了") + "。", 1.6)
	Game.trace("cabinet_open", {"id": key_id})

func _caption_text(owned: bool) -> String:
	if owned:
		return empty_caption + "\n\n已取走"
	if open_first and not is_open:
		return closed_prompt + "\n\nE  打开"
	return key_label + "\n\nE  拿起"

func _set_key_meshes_visible(show_key: bool) -> void:
	for child_name in ["Mesh", "Shaft", "Bit"]:
		if has_node(child_name):
			get_node(child_name).visible = show_key

func _apply_cabinet_visual(opened: bool, instant: bool) -> void:
	var cabinet = get_node_or_null(cabinet_path) if not cabinet_path.is_empty() else null
	if cabinet == null:
		return
	if is_instance_valid(_cabinet_tween):
		_cabinet_tween.kill()
	var doors: Array = []
	for child in cabinet.get_children():
		if String(child.name).begins_with("Door"):
			doors.append(child)
	if doors.is_empty():
		return
	if instant:
		for door in doors:
			door.rotation.y = open_angle if opened else 0.0
		return
	_cabinet_tween = create_tween().set_parallel(true).set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	for door in doors:
		_cabinet_tween.tween_property(door, "rotation:y", open_angle if opened else 0.0, 0.35)
