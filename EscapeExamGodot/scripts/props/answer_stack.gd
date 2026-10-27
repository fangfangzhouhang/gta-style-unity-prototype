extends Node3D

## Optional M answer-stack slide revealing Label3D zone tags. No listen lock.

@export var slide_distance := 0.35
@export var labels_path: NodePath

var slid := false
var home := Vector3.ZERO

func _ready() -> void:
	home = position
	_set_labels(false)

func prompt(_actor) -> String:
	return "E  " + ("推回答卷堆" if slid else "挪开答卷堆")

func interact(_actor) -> void:
	slid = not slid
	var offset := Vector3(slide_distance if slid else 0.0, 0.0, 0.0)
	position = home + offset
	_set_labels(slid)
	Game.acoustics.emit_sound(global_position, "door", 4.0, "纸堆摩擦", "papers")
	if slid:
		Game.discover("stack_tags", "答卷堆下的分区名与播音面板一致。试听不必先读到这里。")
	Game.trace("answer_stack", {"slid": slid})

func _set_labels(show: bool) -> void:
	if labels_path.is_empty(): return
	var node = get_node_or_null(labels_path)
	if node != null:
		node.visible = show

func reset_stack() -> void:
	slid = false
	position = home
	_set_labels(false)
