extends Node3D

@export var seconds_per_stage := 2.0
var stage := 0
var work := 0.0
var noise_time := 0.0
var last_work_frame := 0
var tween: Tween

func reset_gate() -> void:
	stage = int(Game.progress.get("gate_stage", 0))
	work = 0
	$GateBody.position.y = [0.0, 0.25, 0.70, 2.1][stage]
	_update_label()

func prompt(_actor) -> String:
	if stage == 3: return "卷栅已经打开 · 可以离场"
	return "按住 E  摇动链条  %d/3  ·  松手可撤离  %d%%" % [stage, int(work / seconds_per_stage * 100)]

func interact(_actor) -> void:
	Game.discover("gate", "出口卷栅有三个棘轮档。每升一档都会卡住，松手或被抓也不会落回。")

func hold(delta: float) -> void:
	if stage >= 3: return
	last_work_frame = Engine.get_physics_frames()
	work += delta
	noise_time -= delta
	if noise_time <= 0:
		noise_time = 0.65
		Game.acoustics.emit_sound(global_position + Vector3(0, 1, 0), "gate", 17, "卷栅链条的响声", "gate")
	if work >= seconds_per_stage:
		stage += 1
		work = 0
		Game.progress.gate_stage = stage
		if is_instance_valid(tween): tween.kill()
		tween = create_tween().set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
		tween.tween_property($GateBody, "position:y", [0.0, 0.25, 0.70, 2.1][stage], 0.5)
		_update_label()
		Game.save_progress()
		Game.trace("gate_stage", {"stage": stage})
		Game.ui.toast("咔哒。第 %d 档卡住了。" % stage if stage < 3 else "卷栅打开了。外面没有铃声。", 2)

func _physics_process(_delta: float) -> void:
	if Engine.get_physics_frames() > last_work_frame + 2:
		work = 0

func _update_label() -> void:
	$Status.text = "已升起 %d / 3 档" % stage
