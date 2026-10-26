extends Node3D

func _ready() -> void:
	Game.boot(self)

func reset_world() -> void:
	for door in get_tree().get_nodes_in_group("doors"):
		door.reset_door()
	$Gate.reset_gate()
	$Broadcast.reset_panel()
	$ExamTimer.owned = false
	$ExamTimer.reset_timer()
	restore_checkpoint()

func restore_checkpoint() -> void:
	$Player.teleport_to(Game.get_spawn(), 0)
	$Teacher.reset_teacher()
	$ExamTimer.reset_timer()
