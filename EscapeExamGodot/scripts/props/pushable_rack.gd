extends Node3D

## Pushable paper rack in M. Travel limit ~0.9 m; blocks LoS via collision; optional wedge brace.

@export var travel_limit := 0.9
@export var push_dir := Vector3(1, 0, 0)
@export var linked_paths: Array[NodePath] = []

var travel := 0.0
var wedged := false
var home := Vector3.ZERO
var linked_homes: Array[Vector3] = []
var linked: Array[Node3D] = []

func _ready() -> void:
	home = position
	add_to_group("pushable_racks")
	for path in linked_paths:
		var node := get_node_or_null(path)
		if node is Node3D:
			linked.append(node)
			linked_homes.append(node.position)

func at_limit() -> bool:
	return travel >= travel_limit - 0.04

func prompt(_actor) -> String:
	var wedge = Game.world.get_node_or_null("DoorWedge")
	if wedge != null and wedge.held and at_limit() and not wedged:
		return "E  用门楔固定卷纸架"
	if wedged:
		return "E  取下卷纸架上的门楔"
	if at_limit():
		return "E  拉回卷纸架"
	return "E  推移卷纸架"

func interact(_actor) -> void:
	var wedge = Game.world.get_node_or_null("DoorWedge")
	if wedge != null and wedge.held and at_limit() and not wedged:
		apply_wedge(wedge)
		return
	if wedged:
		clear_wedge(true)
		return
	if at_limit():
		_set_travel(0.0)
		Game.acoustics.emit_sound(global_position + Vector3.UP, "door", 6.0, "卷纸架挪回", "rack")
	else:
		_set_travel(travel_limit)
		Game.acoustics.emit_sound(global_position + Vector3.UP, "door", 7.0, "卷纸架刮过地面", "rack")
		Game.discover("rack", "印务室卷纸架可以推移。架后露出检修盖。")
	Game.trace("rack_push", {"travel": snappedf(travel, 0.01)})

func apply_wedge(wedge: Node) -> void:
	wedged = true
	wedge.place_on_rack(self)
	Game.ui.toast("门楔卡住了卷纸架。", 2.0)

func clear_wedge(return_to_hand: bool) -> void:
	if not wedged: return
	wedged = false
	var wedge = Game.world.get_node_or_null("DoorWedge")
	if return_to_hand and wedge != null:
		wedge.return_to_hand()

func _set_travel(value: float) -> void:
	if wedged and value < travel:
		Game.ui.toast("先取下门楔，才能拉回卷纸架。", 1.8)
		return
	travel = clampf(value, 0.0, travel_limit)
	var offset := push_dir.normalized() * travel
	position = home + offset
	for i in linked.size():
		if is_instance_valid(linked[i]):
			linked[i].position = linked_homes[i] + offset

func reset_rack() -> void:
	wedged = false
	_set_travel(0.0)
