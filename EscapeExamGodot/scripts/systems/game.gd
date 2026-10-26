extends Node

const SCHEMA := 1
const SAVE_PATH := "user://chapter01.json"
var world
var player
var teacher
var timer
var gate
var acoustics
var ui
var mode := "title"
var facts: Dictionary = {}
var progress: Dictionary = {}
var checkpoint := "classroom"
var playing := false
var caught_count := 0
var elapsed := 0.0
var sensitivity := 0.0022
var view_fov := 78.0
var sound_captions := true
var test_mode := false
var events: Array[Dictionary] = []
var previous_mode := "pause"
var build_id := "A-M2.1"

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	test_mode = "--test" in OS.get_cmdline_user_args() or "--bake" in OS.get_cmdline_user_args()
	_register_inputs()
	load_settings()

func _register_inputs() -> void:
	var keys = {"forward": KEY_W, "back": KEY_S, "left": KEY_A, "right": KEY_D,
		"sprint": KEY_SHIFT, "crouch": KEY_CTRL, "interact": KEY_E, "arm": KEY_Q,
		"pause_game": KEY_ESCAPE, "journal": KEY_TAB, "screenshot": KEY_F8}
	for action in keys:
		if not InputMap.has_action(action):
			InputMap.add_action(action)
		var key := InputEventKey.new()
		key.physical_keycode = keys[action]
		InputMap.action_add_event(action, key)
	InputMap.add_action("place")
	var click := InputEventMouseButton.new()
	click.button_index = MOUSE_BUTTON_LEFT
	InputMap.action_add_event("place", click)

func boot(level: Node3D) -> void:
	world = level
	player = level.get_node("Player")
	teacher = level.get_node("Teacher")
	timer = level.get_node("ExamTimer")
	gate = level.get_node("Gate")
	acoustics = level.get_node("Acoustics")
	ui = level.get_node("HUD")
	player.camera.fov = view_fov
	set_mode("title")
	trace("boot", {"build": build_id, "renderer": RenderingServer.get_current_rendering_method()})

func _process(delta: float) -> void:
	if playing and mode == "game":
		elapsed += delta

func _unhandled_input(event: InputEvent) -> void:
	if not is_instance_valid(ui):
		return
	if event.is_action_pressed("screenshot"):
		capture()
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("pause_game"):
		if mode == "game": set_mode("pause")
		elif mode == "pause": set_mode("game")
		elif mode in ["journal", "reading", "settings"]:
			if mode == "settings": save_settings()
			set_mode(previous_mode)
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("journal") and playing and mode in ["game", "journal"]:
		if mode == "journal": set_mode("game")
		else:
			previous_mode = "game"
			set_mode("journal")
		get_viewport().set_input_as_handled()

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT and mode == "game" and not test_mode:
		set_mode("pause")

func set_mode(next: String) -> void:
	mode = next
	get_tree().paused = next != "game"
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED if next == "game" and not test_mode else Input.MOUSE_MODE_VISIBLE
	if is_instance_valid(ui): ui.show_mode(next)

func start_new() -> void:
	facts.clear()
	progress = {"shortcut": false, "gate_stage": 0, "broadcast_mode": 0, "broadcast_zone": 0, "loop_stopped": false, "wedge_owned": false}
	checkpoint = "classroom"
	caught_count = 0
	elapsed = 0
	playing = true
	world.reset_world()
	if is_instance_valid(Ambience):
		Ambience.reset_audio_state()
	set_mode("game")
	ui.toast("广播又说：离结束还有一分钟。\n试卷已经写完。门外的脚步还没有离开。", 7.0)
	save_progress()
	trace("new_run")

func get_spawn() -> Vector3:
	return world.get_node("Checkpoints/Print" if checkpoint == "print" else "Checkpoints/Classroom").global_position

func caught(reason: String) -> void:
	if mode != "game" or not playing: return
	caught_count += 1
	trace("caught", {"reason": reason, "checkpoint": checkpoint, "gate": gate.stage})
	if is_instance_valid(Ambience):
		Ambience.play_catch_sting()
	set_mode("recover")
	ui.show_recovery("被请回了座位。\n" + reason + "\n已打开的近路和卷栅档位保留。")
	await get_tree().create_timer(1.1, true).timeout
	world.restore_checkpoint()
	save_progress()
	set_mode("game")
	ui.toast("刚才的行动留下了进度。换一条路，再试一次。", 4)

func fall_recover() -> void:
	if mode != "game": return
	player.teleport_to(get_spawn(), 0)
	ui.toast("回到最近的合法落脚点。进度保留。", 3)
	trace("fall_recover")

func note_wedge_owned() -> void:
	progress.wedge_owned = true
	save_progress()

func unlock_shortcut() -> void:
	progress.shortcut = true
	checkpoint = "print"
	discover("shortcut", "运卷门：从印务侧拉开门闩后，两边都能通行。老师也能通过。")
	save_progress()

func discover(id: String, description: String) -> void:
	if facts.has(id): return
	facts[id] = description
	if is_instance_valid(ui): ui.toast("记下了：" + description, 4)
	trace("fact", {"id": id})
	save_progress()

func finish() -> void:
	if not playing or gate.stage < 3 or mode != "game": return
	playing = false
	progress.completed = true
	save_progress()
	trace("chapter_complete", {"seconds": elapsed, "captures": caught_count, "stopped_loop": progress.get("loop_stopped", false)})
	set_mode("ending")

func save_progress() -> bool:
	if test_mode or not playing and not progress.get("completed", false): return false
	var data = {"schema": SCHEMA, "progress": progress, "facts": facts, "checkpoint": checkpoint,
		"captures": caught_count, "elapsed": elapsed, "timer_owned": timer.owned}
	var file := FileAccess.open(SAVE_PATH + ".tmp", FileAccess.WRITE)
	if file == null: return false
	file.store_string(JSON.stringify(data))
	file.close()
	var temp = JSON.parse_string(FileAccess.get_file_as_string(SAVE_PATH + ".tmp"))
	if not temp is Dictionary: return false
	if FileAccess.file_exists(SAVE_PATH):
		DirAccess.copy_absolute(SAVE_PATH, SAVE_PATH + ".backup")
	return DirAccess.rename_absolute(SAVE_PATH + ".tmp", SAVE_PATH) == OK

func has_save() -> bool:
	return not test_mode and (FileAccess.file_exists(SAVE_PATH) or FileAccess.file_exists(SAVE_PATH + ".backup"))

func load_progress() -> bool:
	if test_mode: return false
	var data = _read_valid_save(SAVE_PATH)
	var used_backup := false
	if data.is_empty():
		data = _read_valid_save(SAVE_PATH + ".backup")
		used_backup = true
	if data.is_empty():
		ui.toast("存档无法读取，原文件仍保留。可以开始新的试走。", 5)
		return false
	progress = data.progress
	facts = data.get("facts", {})
	checkpoint = "print" if data.get("checkpoint", "classroom") == "print" and progress.get("shortcut", false) else "classroom"
	caught_count = int(data.get("captures", 0))
	elapsed = float(data.get("elapsed", 0))
	playing = true
	world.reset_world()
	if is_instance_valid(Ambience):
		Ambience.reset_audio_state()
	timer.owned = data.get("timer_owned", false)
	timer.reset_timer()
	set_mode("game")
	ui.toast("已从备份恢复。" if used_backup else "继续上次的进度。回到安全落脚点。", 4)
	return true

func _read_valid_save(path: String) -> Dictionary:
	if not FileAccess.file_exists(path): return {}
	var parser := JSON.new()
	if parser.parse(FileAccess.get_file_as_string(path)) != OK: return {}
	var data = parser.data
	if not data is Dictionary or data.get("schema", 0) != SCHEMA: return {}
	if not data.get("progress") is Dictionary or not data.get("facts", {}) is Dictionary: return {}
	var p: Dictionary = data.progress
	for key in ["gate_stage", "broadcast_mode", "broadcast_zone"]:
		var value = p.get(key, 0)
		if not (value is float or value is int): return {}
		var maximum := 3 if key == "gate_stage" else (1 if key == "broadcast_mode" else 2)
		if not is_finite(float(value)) or value != floorf(float(value)) or value < 0 or value > maximum: return {}
	for key in ["shortcut", "loop_stopped", "completed", "wedge_owned"]:
		if not p.get(key, false) is bool: return {}
	if not data.get("timer_owned", false) is bool: return {}
	for key in ["elapsed", "captures"]:
		var value = data.get(key, 0)
		if not (value is float or value is int): return {}
		if not is_finite(float(value)) or value < 0 or value > 100000000: return {}
	for key in data.get("facts", {}):
		if not key is String or not data.facts[key] is String: return {}
		if key.length() > 128 or data.facts[key].length() > 4096: return {}
	return data

func save_settings() -> void:
	if test_mode: return
	var config := ConfigFile.new()
	config.set_value("controls", "sensitivity", sensitivity)
	config.set_value("view", "fov", view_fov)
	config.set_value("audio", "captions", sound_captions)
	config.set_value("audio", "volume", AudioServer.get_bus_volume_db(0))
	config.save("user://settings.cfg")

func load_settings() -> void:
	var config := ConfigFile.new()
	if config.load("user://settings.cfg") != OK: return
	sensitivity = clampf(config.get_value("controls", "sensitivity", sensitivity), 0.0006, 0.005)
	view_fov = clampf(config.get_value("view", "fov", view_fov), 65, 95)
	sound_captions = config.get_value("audio", "captions", true)
	AudioServer.set_bus_volume_db(0, clampf(config.get_value("audio", "volume", 0.0), -30, 0))

func trace(event: String, data: Dictionary = {}) -> void:
	var entry := {"time": snappedf(elapsed, 0.01), "event": event, "data": data}
	events.append(entry)
	if events.size() > 2000: events.pop_front()
	print("EVENT ", JSON.stringify(entry))

func capture() -> void:
	await RenderingServer.frame_post_draw
	var directory := "user://screenshots" if OS.has_feature("playtest") else "res://evidence/screenshots"
	DirAccess.make_dir_recursive_absolute(directory)
	var path := directory + "/run-" + str(Time.get_unix_time_from_system()).replace(".", "-") + ".png"
	get_viewport().get_texture().get_image().save_png(path)
	print("SCREENSHOT ", ProjectSettings.globalize_path(path))
	if is_instance_valid(ui): ui.toast("截图已保存。", 2)
