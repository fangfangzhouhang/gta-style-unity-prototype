extends Node3D
## 《还没响的铃》前情骨架：默认播 P01–P09；任意 Skip → 淡出进 B0。
## VO 原文见 docs/design/PROLOGUE_VO_FINAL_20261028.md（情节定稿 2026-10-28）。
## ART-PROLOGUE-DYNAMIC：镜内 Tween 缓推/横移 + 镜间短 crossfade（非静态幻灯）。
## 对齐 docs/design/CHAPTER01_PLAYTEST_PATCH_20261028.md §5 P1（tip 9b872df）。

signal finished(skipped: bool)

const FADE_SECONDS := 0.55
const XFADE_OUT := 0.16
const XFADE_IN := 0.22
## ART-PROLOGUE-CAMS 定稿机位为可读主框；eye_end/look_end 提供镜内轻运动。
## VO 九句勿改；Alert 保持 hidden；P07 非跳脸。
const SHOTS: Array[Dictionary] = [
	# P01 ★ 雨窗缓推穿窗（CAMS 起 → 略向雨幕推进）
	{"id": "P01", "duration": 3.5, "eye": Vector3(-8.75, 1.58, -5.35), "look": Vector3(-15.5, 1.05, -5.35),
		"eye_end": Vector3(-9.22, 1.54, -5.35), "look_end": Vector3(-15.5, 1.00, -5.35), "fov": 58.0, "fov_end": 56.0,
		"vo": "雨还没停。外面看起来还正常。"},
	# P02 同窗侧微横移 → 玻璃内课桌/灯管倒影
	{"id": "P02", "duration": 2.3, "eye": Vector3(-8.55, 1.56, -4.95), "look": Vector3(-9.85, 1.40, -3.80),
		"eye_end": Vector3(-8.42, 1.55, -4.72), "look_end": Vector3(-9.70, 1.38, -3.55), "fov": 63.0, "fov_end": 62.0,
		"vo": "灯还亮着。广播还没开始催。"},
	# P03 ★ 线索缓移对准校牌（略偏起 → CAMS 对准）
	{"id": "P03", "duration": 3.0, "eye": Vector3(-1.72, 1.62, -0.72), "look": Vector3(-2.88, 2.00, -0.62),
		"eye_end": Vector3(-1.95, 1.68, -0.55), "look_end": Vector3(-2.88, 2.08, -0.50), "fov": 50.0, "fov_end": 48.0,
		"vo": "校牌偏了一点。像有人刚碰过。"},
	# P04 ExamSheet 近读缓推
	{"id": "P04", "duration": 2.3, "eye": Vector3(-7.55, 1.35, -6.90), "look": Vector3(-7.80, 1.35, -7.73),
		"eye_end": Vector3(-7.62, 1.35, -7.12), "look_end": Vector3(-7.80, 1.35, -7.73), "fov": 44.0, "fov_end": 42.5,
		"vo": "规章边角多了一行手写。印刷体里不该有它。"},
	# P05 Desk00 卷面俯角轻推
	{"id": "P05", "duration": 2.3, "eye": Vector3(-8.30, 1.38, -2.30), "look": Vector3(-8.50, 0.78, -3.05),
		"eye_end": Vector3(-8.36, 1.28, -2.48), "look_end": Vector3(-8.50, 0.78, -3.05), "fov": 47.0, "fov_end": 45.5,
		"vo": "印章的印泥还没干。"},
	# P06 走道中景缓推向讲台剪影
	{"id": "P06", "duration": 2.8, "eye": Vector3(-6.50, 1.62, -1.35), "look": Vector3(-6.50, 1.55, -6.55),
		"eye_end": Vector3(-6.50, 1.60, -1.85), "look_end": Vector3(-6.50, 1.58, -6.55), "fov": 72.0, "fov_end": 70.0,
		"vo": "讲台那边站着人。他不坐。"},
	# P07 ★ 缓推近微笑眼镜高度（略远起 → CAMS 近景）；非跳脸
	{"id": "P07", "duration": 3.5, "eye": Vector3(-6.50, 1.72, -4.55), "look": Vector3(-6.50, 1.95, -6.34),
		"eye_end": Vector3(-6.50, 1.78, -5.05), "look_end": Vector3(-6.50, 2.02, -6.34), "fov": 40.0, "fov_end": 38.0,
		"vo": "他在笑。笑意不到眼睛。"},
	# P08 铃未响：轻收回落定（playtest §5.3 收回/落定；忌压迫急推）
	{"id": "P08", "duration": 3.0, "eye": Vector3(-6.12, 1.36, -5.55), "look": Vector3(-6.50, 0.95, -6.65),
		"eye_end": Vector3(-6.05, 1.40, -5.45), "look_end": Vector3(-6.50, 0.95, -6.65), "fov": 51.0, "fov_end": 52.0,
		"vo": "铃还没响。时间已经不对了。"},
	# P09 进 B0：缓落定到观察窗构图（Skip 仍立刻→B0）
	{"id": "P09", "duration": 2.0, "eye": Vector3(-4.40, 1.58, -0.72), "look": Vector3(-3.10, 1.42, 1.05),
		"eye_end": Vector3(-4.25, 1.55, -0.55), "look_end": Vector3(-3.05, 1.42, 1.10), "fov": 74.0, "fov_end": 74.0,
		"vo": "先别动。看清楚再说。"},
]

var _cam: Camera3D
var _index := -1
var _playing := false
var _finishing := false
var _advancing := false
var _shot_left := 0.0
var _pose_teacher := false
var _look_target := Vector3.ZERO
var _shot_tween: Tween
var _head_tween: Tween

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
	_advancing = false
	_index = -1
	_pose_teacher_for_shots(true)
	if is_instance_valid(Game.player) and is_instance_valid(Game.player.camera):
		Game.player.camera.current = false
	_cam.current = true
	set_process(true)
	_kick_advance()
	if is_instance_valid(Game.ui) and Game.ui.has_method("show_prologue"):
		Game.ui.show_prologue(true)
		Game.ui.set_prologue_fade(0.0)
		Game.ui.set_prologue_skip_hint(true)

func request_skip() -> void:
	if not _playing or _finishing:
		return
	_finish(true)

func _process(delta: float) -> void:
	if not _playing or _finishing or _advancing:
		return
	# Keep look_at stable while Tween moves eye (and optionally look_target).
	if is_instance_valid(_cam) and _cam.current:
		_cam.look_at(_look_target, Vector3.UP)
	_shot_left -= delta
	if _shot_left <= 0.0:
		_kick_advance()

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

func _kick_advance() -> void:
	if _advancing or _finishing or not _playing:
		return
	_advancing = true
	_advance_shot()

func _advance_shot() -> void:
	_index += 1
	if _index >= SHOTS.size():
		_advancing = false
		_finish(false)
		return
	var shot: Dictionary = SHOTS[_index]
	_kill_shot_tweens()

	# Mirror间短 crossfade：避免硬切幻灯片感（首镜不暗场）。
	if _index > 0 and is_instance_valid(Game.ui) and Game.ui.has_method("fade_prologue"):
		await Game.ui.fade_prologue(XFADE_OUT, true)
		if not _playing or _finishing:
			_advancing = false
			return

	var eye0: Vector3 = shot.eye as Vector3
	var look0: Vector3 = shot.look as Vector3
	var eye1: Vector3 = shot.get("eye_end", eye0) as Vector3
	var look1: Vector3 = shot.get("look_end", look0) as Vector3
	var fov0 := float(shot.fov)
	var fov1 := float(shot.get("fov_end", fov0))
	var dur := float(shot.duration)

	_cam.fov = fov0
	_cam.global_position = eye0
	_look_target = look0
	_cam.look_at(_look_target, Vector3.UP)
	_shot_left = dur

	if is_instance_valid(Game.ui) and Game.ui.has_method("set_prologue_vo"):
		Game.ui.set_prologue_vo(str(shot.vo))
	Game.trace("prologue_shot", {"id": str(shot.id), "vo": str(shot.vo), "dynamic": true})

	_start_shot_motion(eye0, eye1, look0, look1, fov0, fov1, dur)
	if str(shot.id) == "P07":
		_start_p07_teacher_micro(dur)

	if _index > 0 and is_instance_valid(Game.ui) and Game.ui.has_method("fade_prologue"):
		await Game.ui.fade_prologue(XFADE_IN, false)
		if not _playing or _finishing:
			_advancing = false
			return
	elif is_instance_valid(Game.ui) and Game.ui.has_method("set_prologue_fade"):
		Game.ui.set_prologue_fade(0.0)

	_advancing = false

func _start_shot_motion(eye0: Vector3, eye1: Vector3, look0: Vector3, look1: Vector3, fov0: float, fov1: float, dur: float) -> void:
	_shot_tween = create_tween()
	_shot_tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	_shot_tween.set_parallel(true)
	_shot_tween.set_ease(Tween.EASE_IN_OUT)
	_shot_tween.set_trans(Tween.TRANS_CUBIC)
	var motion_dur := maxf(0.35, dur - 0.05)
	_shot_tween.tween_property(_cam, "global_position", eye1, motion_dur).from(eye0)
	_shot_tween.tween_property(_cam, "fov", fov1, motion_dur).from(fov0)
	_shot_tween.tween_method(_set_look_target, look0, look1, motion_dur)

func _set_look_target(value: Vector3) -> void:
	_look_target = value

func _start_p07_teacher_micro(dur: float) -> void:
	## 纯 Visual：极轻头部微动；不动 AI / 不跳脸。
	if not is_instance_valid(Game.teacher):
		return
	var teacher = Game.teacher
	if not teacher.has_node("Visual/Head"):
		return
	var head: Node3D = teacher.get_node("Visual/Head")
	head.rotation = Vector3.ZERO
	_head_tween = create_tween()
	_head_tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	_head_tween.set_ease(Tween.EASE_IN_OUT)
	_head_tween.set_trans(Tween.TRANS_SINE)
	# ±~2° yaw, slow out-and-back — readable but never a lunge.
	var half := maxf(0.4, dur * 0.48)
	_head_tween.tween_property(head, "rotation:y", 0.032, half)
	_head_tween.tween_property(head, "rotation:y", -0.018, half)
	_head_tween.tween_property(head, "rotation:y", 0.0, maxf(0.25, dur * 0.2))

func _kill_shot_tweens() -> void:
	if _shot_tween != null and is_instance_valid(_shot_tween):
		_shot_tween.kill()
	_shot_tween = null
	if _head_tween != null and is_instance_valid(_head_tween):
		_head_tween.kill()
	_head_tween = null

func _finish(skipped: bool) -> void:
	if _finishing:
		return
	_finishing = true
	_advancing = false
	set_process(false)
	_kill_shot_tweens()
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
		if teacher.has_node("Visual/Head"):
			teacher.get_node("Visual/Head").rotation = Vector3.ZERO
		if teacher.has_method("_update_alert_ui"):
			teacher._update_alert_ui()
	elif _pose_teacher:
		_pose_teacher = false
		# Restore Phase0 inactive presence from progress (new run: not summoned).
		if Game.has_method("apply_teacher_presence_from_progress"):
			Game.apply_teacher_presence_from_progress()
