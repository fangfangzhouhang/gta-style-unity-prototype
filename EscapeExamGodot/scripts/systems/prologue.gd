extends Node3D
## 《还没响的铃》前情骨架：默认播 P01–P09；任意 Skip → 淡出进 B0。
## VO 原文见 docs/design/PROLOGUE_VO_FINAL_20261028.md（情节定稿 2026-10-28）。

signal finished(skipped: bool)

const FADE_SECONDS := 0.55
const SHOTS: Array[Dictionary] = [
	{"id": "P01", "duration": 3.5, "eye": Vector3(-9.05, 1.60, -2.75), "look": Vector3(-14.0, 1.15, -2.75), "fov": 58.0,
		"vo": "雨还没停。外面看起来还正常。"},
	{"id": "P02", "duration": 2.3, "eye": Vector3(-8.85, 1.58, -2.55), "look": Vector3(-9.55, 1.35, -2.2), "fov": 62.0,
		"vo": "灯还亮着。广播还没开始催。"},
	{"id": "P03", "duration": 3.0, "eye": Vector3(-5.4, 1.55, 0.55), "look": Vector3(-4.1, 1.75, -0.7), "fov": 52.0,
		"vo": "校牌偏了一点。像有人刚碰过。"},
	{"id": "P04", "duration": 2.3, "eye": Vector3(-8.15, 1.25, -2.55), "look": Vector3(-8.5, 0.85, -3.05), "fov": 44.0,
		"vo": "规章边角多了一行手写。印刷体里不该有它。"},
	{"id": "P05", "duration": 2.3, "eye": Vector3(-8.35, 1.45, -2.55), "look": Vector3(-8.5, 0.78, -3.0), "fov": 47.0,
		"vo": "印章的印泥还没干。"},
	{"id": "P06", "duration": 2.8, "eye": Vector3(-6.5, 1.62, -1.2), "look": Vector3(-6.5, 1.55, -6.5), "fov": 72.0,
		"vo": "讲台那边站着人。他不坐。"},
	{"id": "P07", "duration": 3.5, "eye": Vector3(-6.5, 1.72, -5.15), "look": Vector3(-6.5, 1.78, -6.35), "fov": 38.0,
		"vo": "他在笑。笑意不到眼睛。"},
	{"id": "P08", "duration": 3.0, "eye": Vector3(-6.15, 1.45, -5.55), "look": Vector3(-6.5, 0.95, -6.65), "fov": 52.0,
		"vo": "铃还没响。时间已经不对了。"},
	{"id": "P09", "duration": 2.0, "eye": Vector3(-5.15, 1.55, -0.55), "look": Vector3(-3.7, 1.35, 1.0), "fov": 70.0,
		"vo": "先别动。看清楚再说。"},
]

var _cam: Camera3D
var _index := -1
var _playing := false
var _finishing := false
var _shot_left := 0.0
var _pose_teacher := false

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_cam = Camera3D.new()
	_cam.name = "PrologueCamera"
	_cam.current = false
	add_child(_cam)
	set_process(false)

func is_playing() -> bool:
	return _playing

func begin() -> void:
	if _playing:
		return
	_playing = true
	_finishing = false
	_index = -1
	_pose_teacher_for_shots(true)
	if is_instance_valid(Game.player) and is_instance_valid(Game.player.camera):
		Game.player.camera.current = false
	_cam.current = true
	set_process(true)
	_advance_shot()
	if is_instance_valid(Game.ui) and Game.ui.has_method("show_prologue"):
		Game.ui.show_prologue(true)
		Game.ui.set_prologue_fade(0.0)
		Game.ui.set_prologue_skip_hint(true)

func request_skip() -> void:
	if not _playing or _finishing:
		return
	_finish(true)

func _process(delta: float) -> void:
	if not _playing or _finishing:
		return
	_shot_left -= delta
	if _shot_left <= 0.0:
		_advance_shot()

func _unhandled_input(event: InputEvent) -> void:
	if not _playing or _finishing:
		return
	var skip := false
	if event.is_action_pressed("pause_game"):
		skip = true
	elif event is InputEventKey and event.pressed and not event.echo:
		if event.keycode in [KEY_SPACE, KEY_ENTER, KEY_KP_ENTER, KEY_ESCAPE]:
			skip = true
	elif event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		skip = true
	if skip:
		request_skip()
		get_viewport().set_input_as_handled()

func _advance_shot() -> void:
	_index += 1
	if _index >= SHOTS.size():
		_finish(false)
		return
	var shot: Dictionary = SHOTS[_index]
	_shot_left = float(shot.duration)
	_cam.fov = float(shot.fov)
	_cam.global_position = shot.eye as Vector3
	_cam.look_at(shot.look as Vector3, Vector3.UP)
	if is_instance_valid(Game.ui) and Game.ui.has_method("set_prologue_vo"):
		Game.ui.set_prologue_vo(str(shot.vo))
	Game.trace("prologue_shot", {"id": str(shot.id), "vo": str(shot.vo)})

func _finish(skipped: bool) -> void:
	if _finishing:
		return
	_finishing = true
	set_process(false)
	if is_instance_valid(Game.ui) and Game.ui.has_method("set_prologue_skip_hint"):
		Game.ui.set_prologue_skip_hint(false)
	# Fade out — never ring the bell; Alert stays hidden.
	if is_instance_valid(Game.ui) and Game.ui.has_method("fade_prologue"):
		await Game.ui.fade_prologue(FADE_SECONDS, true)
	_teardown_camera()
	_pose_teacher_for_shots(false)
	_playing = false
	_finishing = false
	if is_instance_valid(Game.ui) and Game.ui.has_method("show_prologue"):
		Game.ui.show_prologue(false)
		Game.ui.set_prologue_fade(0.0)
	Game.trace("prologue_finished", {"skipped": skipped})
	finished.emit(skipped)

func _teardown_camera() -> void:
	_cam.current = false
	if is_instance_valid(Game.player) and is_instance_valid(Game.player.camera):
		Game.player.camera.current = true
		Game.player.camera.fov = Game.view_fov

func _pose_teacher_for_shots(active: bool) -> void:
	## Visual-only pose for P06–P07; frozen, no summon, Alert stays forced-off.
	if not is_instance_valid(Game.teacher):
		return
	var teacher = Game.teacher
	if active:
		_pose_teacher = true
		teacher.visible = true
		teacher.frozen = true
		teacher.collision_layer = 0
		teacher.collision_mask = 0
		teacher.velocity = Vector3.ZERO
		teacher.global_position = Vector3(-6.5, 0.04, -6.45)
		teacher.rotation = Vector3(0, PI, 0)  # face +Z toward P07 cam
		teacher.state = "patrol"
		teacher.suspicion = 0.0
		if teacher.has_method("_update_alert_ui"):
			teacher._update_alert_ui()
	elif _pose_teacher:
		_pose_teacher = false
		# Restore Phase0 inactive presence from progress (new run: not summoned).
		if Game.has_method("apply_teacher_presence_from_progress"):
			Game.apply_teacher_presence_from_progress()
