extends Node3D

var output_mode := 0
var zone := 0
var stopped := false
var cooldown := 0.0
var activation := 0
var loop_time := 18.0

func reset_panel() -> void:
	output_mode = int(Game.progress.get("broadcast_mode", 0))
	zone = int(Game.progress.get("broadcast_zone", 0))
	stopped = Game.progress.get("loop_stopped", false)
	cooldown = 0
	if is_instance_valid(Ambience):
		Ambience.set_loop_quiet(stopped)
	update_labels()

func _process(delta: float) -> void:
	if Game.mode != "game": return
	cooldown = maxf(0, cooldown - delta)
	loop_time -= delta
	if loop_time <= 0 and not stopped:
		loop_time = 36
		Game.ui.sound_caption("广播：离结束还有一分钟。")
		# Narrative chime is not an AI stimulus: regular school announcements are routine.
		$LoopAudio.play()

func operate(action: String) -> void:
	match action:
		"stop":
			stopped = not stopped
			Game.progress.loop_stopped = stopped
			if stopped:
				$LoopAudio.stop()
				if is_instance_valid(Ambience):
					Ambience.set_loop_quiet(true)
				Game.discover("loop", "循环键被固定在重播。停止它，广播终于安静了。")
			else:
				if is_instance_valid(Ambience):
					Ambience.set_loop_quiet(false)
		"mode":
			output_mode = 1 - output_mode
			Game.progress.broadcast_mode = output_mode
		"zone":
			zone = (zone + 1) % 3
			Game.progress.broadcast_zone = zone
		"test":
			if cooldown > 0:
				Game.ui.toast("铃声正在播放，等这一遍结束。", 2)
				return
			cooldown = 8
			activation += 1
			var point: Vector3 = global_position + Vector3.UP
			if output_mode == 1:
				point = Game.world.get_node("Speakers/" + ["Classroom", "Print", "East"][zone]).global_position
			Game.acoustics.emit_sound(point, "broadcast", 30 if output_mode == 1 else 6, "终场铃", "broadcast")
			Game.discover("broadcast", "试听只在本机响；切到外放后，选择的区域才会响铃。教室和印务室都能作为远处声源。" if output_mode == 1 else "监听时只有本机小喇叭响。右侧的分区旋钮还没有送出声音。")
	update_labels()
	Game.save_progress()

func update_labels() -> void:
	$Stop/Label.text = "循环：已停" if stopped else "循环：重播"
	$Mode/Label.text = "输出：外放" if output_mode else "输出：监听"
	$Zone/Label.text = "分区：" + ["考场", "印务", "东廊"][zone]
	$Test/Label.text = "终场 / 测试"
