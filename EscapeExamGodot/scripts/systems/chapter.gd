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
	if has_node("DoorWedge"):
		$DoorWedge.reset_wedge()
	if has_node("PushableRack"):
		$PushableRack.reset_rack()
	if has_node("AnswerStack"):
		$AnswerStack.reset_stack()
	for key in get_tree().get_nodes_in_group("key_pickups"):
		if key.has_method("refresh_from_progress"):
			key.refresh_from_progress()
	restore_checkpoint()

func restore_checkpoint() -> void:
	$Player.teleport_to(Game.get_spawn(), 0)
	Game.apply_teacher_presence_from_progress()
	$ExamTimer.reset_timer()
	if has_node("DoorWedge"):
		$DoorWedge.restore_home()
