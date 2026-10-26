extends Node3D

@export var fact_id := ""
@export var title := ""
@export_multiline var body := ""
@export_multiline var summary := ""

func prompt(_actor) -> String:
	return "E  查看" + title

func interact(_actor) -> void:
	Game.discover(fact_id, summary if not summary.is_empty() else body)
	Game.ui.open_reading(title, body)
