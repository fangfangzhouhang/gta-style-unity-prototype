extends Node
## AdaptiveAudio — V2 自适应张力音频层（由 Ambience 在 _ready 自注册创建）。
##
## 自己算 tension，不依赖其他 agent 的 Director（集成期可改喂外部 tension，
## 只需外部调用 set_external_tension(v) 并置 _external = true，本层即停自算）。
##
## 输入：教师↔玩家欧氏距离 + 同房间加权（Game.acoustics.room_at）、
##       Game.teacher.state（chase/suspicious 权重最高）、玩家奔跑/速度。
## 平滑：上升快（attack ≈ 1.6/s）、下降慢（release ≈ 0.10/s）——脱离追击后
##       tension 从 1 衰到 0.25 约需 7–8 s，抄第五人格追击音乐"长延迟衰减"：
##       音乐骤停等于告诉玩家安全了，会杀掉余悸。
## 驱动：drone 常底（Music 总线）→ pulse 层（tension > 0.25 起）→
##       strings 层（tension > 0.5 起）；心跳独立非空间播放器（Ambient 总线），
##       BPM = lerp(50, 125, proximity)，proximity 由距离 12 m→0、1.5 m→1 归一，
##       tension 低且距离远时停。
## 钥匙声签名：教师移动速度 > 0.3 m/s 时，按与其速度/状态匹配的间隔在教师
##       位置 emit "keys"（source_id = "teacher" —— 已核对 acoustics.gd
##       emit_sound 的契约：source_id == "teacher" 时不调用教师 hear_noise）。
## quiet（广播停播的"真安静"）：本层全部静默，与 Ambience 床保持一致；
##       由 Ambience.set_loop_quiet 转发 set_quiet()，同时每帧兜底轮询
##       Ambience 的 _quiet 状态（同 Autoload，读其成员仅作静默判断）。

const PULSE := preload("res://assets/audio/pulse_loop.wav")
const STRINGS := preload("res://assets/audio/tension_strings.wav")
const DRONE := preload("res://assets/audio/drone_loop.wav")
const HEART := preload("res://assets/audio/heartbeat.wav")

const MUSIC_BUS := "Music"
const AMBIENT_BUS := "Ambient"
const SILENT_DB := -40.0
const DRONE_DB := -20.0
const PULSE_MAX_DB := -13.0
const STRINGS_MAX_DB := -15.0
const HEART_MAX_DB := -7.0

var tension := 0.0
var proximity := 0.0

var _pulse: AudioStreamPlayer
var _strings: AudioStreamPlayer
var _drone: AudioStreamPlayer
var _heart: AudioStreamPlayer
var _quiet := false
var _external := false
var _heart_timer := 0.0
var _keys_timer := 0.0
var _teacher_last_pos := Vector3.ZERO
var _teacher_speed := 0.0
var _started := false

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_pulse = _make_layer("AdaptivePulse", PULSE, MUSIC_BUS)
	_strings = _make_layer("AdaptiveStrings", STRINGS, MUSIC_BUS)
	_drone = _make_layer("AdaptiveDrone", DRONE, MUSIC_BUS)
	_heart = AudioStreamPlayer.new()
	_heart.name = "AdaptiveHeartbeat"
	_heart.stream = HEART
	_heart.bus = AMBIENT_BUS
	_heart.volume_db = HEART_MAX_DB
	_heart.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(_heart)
	if is_instance_valid(Game) and is_instance_valid(Game.teacher):
		_teacher_last_pos = Game.teacher.global_position

func _make_layer(p_name: String, stream: AudioStream, bus: String) -> AudioStreamPlayer:
	var p := AudioStreamPlayer.new()
	p.name = p_name
	p.stream = stream
	p.bus = bus
	p.volume_db = SILENT_DB
	p.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(p)
	return p

func set_quiet(quiet: bool) -> void:
	_quiet = quiet
	if quiet:
		_stop_all()

func set_external_tension(v: float) -> void:
	_external = true
	tension = clampf(v, 0.0, 1.0)

func clear_external_tension() -> void:
	_external = false

func _stop_all() -> void:
	for p in [_pulse, _strings, _drone, _heart]:
		if p and p.playing:
			p.stop()

func _process(delta: float) -> void:
	if delta <= 0.0:
		return
	# quiet 兜底轮询：Ambience 已转发的 set_quiet 为主，这里防转发遗漏。
	if is_instance_valid(Ambience) and bool(Ambience.get("_quiet")):
		_quiet = true
	if _quiet:
		_stop_all()
		tension = move_toward(tension, 0.0, delta * 0.5)
		return
	if not _started:
		_started = true
		for p in [_pulse, _strings, _drone]:
			if not p.playing:
				p.play()
	if not _external:
		_update_tension(delta)
	_update_layers(delta)
	_update_heartbeat(delta)
	_update_keys(delta)

func _update_tension(delta: float) -> void:
	var target := 0.0
	if is_instance_valid(Game) and is_instance_valid(Game.teacher) and is_instance_valid(Game.player):
		var tp: Vector3 = Game.teacher.global_position
		var pp: Vector3 = Game.player.global_position
		var dist := tp.distance_to(pp)
		proximity = clampf(1.0 - (dist - 1.5) / 10.5, 0.0, 1.0)  # 12 m→0，1.5 m→1
		target = proximity * 0.55
		# 同房间加权：在一间屋里，距离的压迫被放大。
		if is_instance_valid(Game.acoustics):
			if Game.acoustics.room_at(tp) == Game.acoustics.room_at(pp):
				target += 0.18
		match String(Game.teacher.get("state")):
			"chase":
				target = maxf(target, 0.92)
			"suspicious":
				target = maxf(target, 0.62)
			"search":
				target = maxf(target, 0.55)
			"investigate":
				target = maxf(target, 0.45)
		# 玩家奔跑：自己的噪音与暴露抬高张力下限。
		var pv: Vector3 = Game.player.velocity
		pv.y = 0.0
		if pv.length() > 3.6:
			target = maxf(target, target + 0.10)
	else:
		proximity = 0.0
	target = clampf(target, 0.0, 1.0)
	if target > tension:
		tension = move_toward(tension, target, delta * 1.6)   # 上升快
	else:
		tension = move_toward(tension, target, delta * 0.10)  # 下降慢（6–10 s 衰减）

func _layer_db(t: float, lo: float, hi: float, max_db: float) -> float:
	if t <= lo:
		return SILENT_DB
	var k := clampf((t - lo) / (hi - lo), 0.0, 1.0)
	return lerpf(SILENT_DB, max_db, k)

func _update_layers(_delta: float) -> void:
	# drone 常底：只要不在 quiet 就有极低的秩序嗡鸣垫底。
	_drone.volume_db = DRONE_DB if tension > 0.02 else lerpf(SILENT_DB, DRONE_DB, 0.5)
	_pulse.volume_db = _layer_db(tension, 0.25, 0.65, PULSE_MAX_DB)
	_strings.volume_db = _layer_db(tension, 0.50, 0.85, STRINGS_MAX_DB)
	# 追击时脉冲略升调，制造加速感（不改素材本身）。
	_pulse.pitch_scale = lerpf(1.0, 1.12, clampf((tension - 0.6) / 0.4, 0.0, 1.0))

func _update_heartbeat(delta: float) -> void:
	# 距离远且 tension 低：心跳停。否则按 BPM 计时重播 lub-dub。
	if proximity <= 0.03 and tension < 0.30:
		return
	_heart_timer -= delta
	if _heart_timer > 0.0:
		return
	var bpm := lerpf(50.0, 125.0, proximity)
	_heart_timer = 60.0 / bpm
	_heart.volume_db = lerpf(-16.0, HEART_MAX_DB, proximity)
	if not _heart.playing:
		_heart.play()

func _update_keys(delta: float) -> void:
	if not is_instance_valid(Game) or not is_instance_valid(Game.teacher) or not is_instance_valid(Game.acoustics):
		return
	var tp: Vector3 = Game.teacher.global_position
	if delta > 0.0:
		_teacher_speed = _teacher_last_pos.distance_to(tp) / delta
	_teacher_last_pos = tp
	if _teacher_speed <= 0.3:
		_keys_timer = 0.0
		return
	_keys_timer -= delta
	if _keys_timer > 0.0:
		return
	# 间隔随速度/状态：走得越快、越警觉，钥匙响得越密。
	var state := String(Game.teacher.get("state"))
	var base := clampf(2.2 / maxf(_teacher_speed, 0.4), 0.45, 1.6)
	if state == "chase":
		base *= 0.55
	elif state == "suspicious" or state == "search":
		base *= 0.8
	_keys_timer = base
	# reach 只决定玩家听距；source_id="teacher" 保证不喂教师听觉。
	Game.acoustics.emit_sound(tp + Vector3.UP * 0.9, "keys", 14.0, "钥匙串轻晃", "teacher")
