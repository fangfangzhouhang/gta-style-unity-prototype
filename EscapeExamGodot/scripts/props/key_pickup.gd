extends Node3D

## World key pickup. Progress-permanent; catch does not remove ownership.

@export var key_id := "k1"
@export var key_label := "钥匙"
@export var fact_id := ""
@export_multiline var fact_summary := ""
@export_multiline var empty_caption := "空抽屉"

func _ready() -> void:
	add_to_group("key_pickups")
	refresh_from_progress()

func refresh_from_progress() -> void:
	var owned := Game.has_key(key_id) if is_instance_valid(Game) else false
	visible = not owned
	if has_node("Target"):
		$Target.collision_layer = 0 if owned else 8
	if has_node("Caption"):
		$Caption.text = (empty_caption if owned else key_label) + ("\n\n已取走" if owned else "\n\nE  拿起")

func prompt(_actor) -> String:
	if Game.has_key(key_id):
		return "E  查看 · " + empty_caption
	return "E  拿起" + key_label

func interact(_actor) -> void:
	if Game.has_key(key_id):
		Game.ui.toast(empty_caption + "。钥匙已经在你身上。", 2.0)
		return
	Game.grant_key(key_id, key_label)
	if not fact_id.is_empty():
		Game.discover(fact_id, fact_summary if not fact_summary.is_empty() else ("拿到了" + key_label))
	refresh_from_progress()
	Game.trace("key_pickup", {"id": key_id})
