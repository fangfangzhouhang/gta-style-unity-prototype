extends Node3D

## Single optional door wedge (peek / rack-brace). Not a key; never required to exit.

var held := false
var owned := false
var home := Vector3.ZERO
var home_rotation := Vector3.ZERO
var placed_door: Node = null
var placed_rack: Node = null

func _ready() -> void:
	home = position
	home_rotation = rotation
	add_to_group("door_wedge")

func prompt(_actor) -> String:
	return "E  拿起门楔"

func interact(_actor) -> void:
	pick_up()

func pick_up() -> void:
	if placed_door != null and is_instance_valid(placed_door):
		placed_door.clear_wedge(false)
		placed_door = null
	if placed_rack != null and is_instance_valid(placed_rack):
		placed_rack.clear_wedge(false)
		placed_rack = null
	owned = true
	held = true
	$Target.collision_layer = 0
	visible = true
	Game.discover("wedge", "门楔可以卡住门缝窥视，或固定已推开的卷纸架。卡住的门不能侧身挤过。")
	Game.note_wedge_owned()
	Game.trace("wedge_pickup")

func place_on_door(door: Node) -> void:
	if not held: return
	held = false
	placed_door = door
	placed_rack = null
	visible = false
	$Target.collision_layer = 0
	Game.acoustics.emit_sound(door.global_position + Vector3.UP, "door", 5.5, "门楔卡入", "wedge")
	Game.trace("wedge_door", {"door": door.name})

func place_on_rack(rack: Node) -> void:
	if not held: return
	held = false
	placed_rack = rack
	placed_door = null
	visible = false
	$Target.collision_layer = 0
	Game.acoustics.emit_sound(rack.global_position + Vector3.UP, "door", 5.5, "门楔卡住卷纸架", "wedge")
	Game.trace("wedge_rack")

func return_to_hand() -> void:
	placed_door = null
	placed_rack = null
	held = true
	owned = true
	visible = true
	$Target.collision_layer = 0
	Game.acoustics.emit_sound(Game.player.global_position + Vector3.UP, "door", 5.0, "取出门楔", "wedge")

func restore_home() -> void:
	## Catch / checkpoint: wedge never left unreachable.
	if placed_door != null and is_instance_valid(placed_door):
		placed_door.clear_wedge(false)
	if placed_rack != null and is_instance_valid(placed_rack):
		placed_rack.clear_wedge(false)
	placed_door = null
	placed_rack = null
	held = false
	position = home
	rotation = home_rotation
	scale = Vector3.ONE
	visible = true
	$Target.collision_layer = 8 if owned else 8
	# Always interactable at cabinet/tray after restore.
	$Target.collision_layer = 8

func reset_wedge() -> void:
	owned = Game.progress.get("wedge_owned", false)
	restore_home()
	if not owned:
		position = home
		$Target.collision_layer = 8

func _process(_delta: float) -> void:
	if not is_instance_valid(Game.player): return
	if held:
		global_transform = Game.player.camera.global_transform * Transform3D(Basis.IDENTITY, Vector3(-0.28, -0.22, -0.55))
		scale = Vector3.ONE * 0.9
