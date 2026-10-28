extends Node
## 雨/灯管环境床 + Ambient duck + 抓捕刺音。床不进 acoustics AI 事件。

const RAIN := preload("res://assets/audio/rain_bed.wav")
const HUM := preload("res://assets/audio/tube_hum.wav")
const STING := preload("res://assets/audio/catch_sting.wav")

const AMBIENT_BUS := "Ambient"
const STING_BUS := "Sting"
const BED_RAIN_DB := -9.0
const BED_HUM_DB := -13.0
const AMBIENT_NORMAL_DB := 0.0
const AMBIENT_DUCK_DB := -22.0
const BED_FADE_OUT_DB := -40.0

var _rain: AudioStreamPlayer
var _hum: AudioStreamPlayer
var _sting: AudioStreamPlayer
var _duck_tw: Tween
var _bed_tw: Tween
var _quiet := false
var _catching := false
## Bumped to invalidate pending sting-restore timers on stop/reset.
var _sting_gen := 0

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_ensure_buses()
	_rain = AudioStreamPlayer.new()
	_rain.name = "RainBed"
	_rain.stream = RAIN
	_rain.bus = AMBIENT_BUS
	_rain.volume_db = BED_RAIN_DB
	_rain.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(_rain)
	_hum = AudioStreamPlayer.new()
	_hum.name = "TubeHum"
	_hum.stream = HUM
	_hum.bus = AMBIENT_BUS
	_hum.volume_db = BED_HUM_DB
	_hum.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(_hum)
	_sting = AudioStreamPlayer.new()
	_sting.name = "CatchSting"
	_sting.stream = STING
	_sting.bus = STING_BUS
	_sting.volume_db = 2.0
	_sting.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(_sting)
	_clear_bus_mute_flags()
	# 仅玩法/标题后开床；测试模式也开以便总线存在，但不依赖 AI。
	if not _quiet:
		if not _rain.playing:
			_rain.play()
		if not _hum.playing:
			_hum.play()

func _ensure_buses() -> void:
	# 若项目未挂 layout，运行时补 Music / Ambient / SFX / Sting。
	for bus_name in ["Music", AMBIENT_BUS, "SFX", STING_BUS]:
		if AudioServer.get_bus_index(bus_name) == -1:
			AudioServer.add_bus()
			var idx := AudioServer.bus_count - 1
			AudioServer.set_bus_name(idx, bus_name)
			AudioServer.set_bus_send(idx, "Master")
	_clear_bus_mute_flags()

func _clear_bus_mute_flags() -> void:
	## Harden: Ambient/SFX must not stay muted/solo after duck or bad local_data.
	for bus_name in [AMBIENT_BUS, "SFX", STING_BUS, "Master"]:
		var i := AudioServer.get_bus_index(bus_name)
		if i < 0:
			continue
		AudioServer.set_bus_mute(i, false)
		AudioServer.set_bus_solo(i, false)

func set_loop_quiet(quiet: bool) -> void:
	## Broadcast 停播 → 床真正安静（淡出后 stop）；恢复则重开循环床。
	_quiet = quiet
	if _catching:
		return
	if quiet:
		_fade_beds_then_stop(0.4)
	else:
		_ensure_beds_playing()
		_tween_beds(BED_RAIN_DB, BED_HUM_DB, 0.55)

func play_catch_sting() -> void:
	_catching = true
	_sting_gen += 1
	var gen := _sting_gen
	_duck_ambient(AMBIENT_DUCK_DB, 0.08)
	_sting.stop()
	_sting.pitch_scale = randf_range(0.97, 1.03)
	_sting.play()
	# 不 await finished：caught() 里另有 recover 计时；刺音结束后再抬床。
	var restore_after := 0.85
	get_tree().create_timer(restore_after, true).timeout.connect(
		func() -> void:
			if gen == _sting_gen:
				_on_sting_done()
	, CONNECT_ONE_SHOT)

func _on_sting_done() -> void:
	_catching = false
	_duck_ambient(AMBIENT_NORMAL_DB, 0.45)
	set_loop_quiet(_quiet)

func _duck_ambient(target_db: float, secs: float) -> void:
	var i := AudioServer.get_bus_index(AMBIENT_BUS)
	if i < 0:
		return
	if _duck_tw:
		_duck_tw.kill()
	_duck_tw = create_tween()
	_duck_tw.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	var from := AudioServer.get_bus_volume_db(i)
	_duck_tw.tween_method(func(v: float): AudioServer.set_bus_volume_db(i, v), from, target_db, secs)

func _tween_beds(rain_db: float, hum_db: float, secs: float) -> void:
	if _bed_tw:
		_bed_tw.kill()
	_bed_tw = create_tween()
	_bed_tw.set_parallel(true)
	_bed_tw.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	_bed_tw.tween_property(_rain, "volume_db", rain_db, secs)
	_bed_tw.tween_property(_hum, "volume_db", hum_db, secs)

func _fade_beds_then_stop(secs: float) -> void:
	if _bed_tw:
		_bed_tw.kill()
	_bed_tw = create_tween()
	_bed_tw.set_parallel(true)
	_bed_tw.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
	_bed_tw.tween_property(_rain, "volume_db", BED_FADE_OUT_DB, secs)
	_bed_tw.tween_property(_hum, "volume_db", BED_FADE_OUT_DB, secs)
	_bed_tw.set_parallel(false)
	_bed_tw.tween_callback(func() -> void:
		if _quiet and not _catching:
			_rain.stop()
			_hum.stop()
	)

func _ensure_beds_playing() -> void:
	if not _rain.playing:
		_rain.volume_db = BED_FADE_OUT_DB
		_rain.play()
	if not _hum.playing:
		_hum.volume_db = BED_FADE_OUT_DB
		_hum.play()

func reset_audio_state() -> void:
	## 关卡重开 / stop / enter_b0：杀掉 duck/bed Tween，作废待恢复的刺音计时；清 mute；重开床。
	_sting_gen += 1
	if _duck_tw:
		_duck_tw.kill()
		_duck_tw = null
	if _bed_tw:
		_bed_tw.kill()
		_bed_tw = null
	_catching = false
	if _sting and _sting.playing:
		_sting.stop()
	_clear_bus_mute_flags()
	var i := AudioServer.get_bus_index(AMBIENT_BUS)
	if i >= 0:
		AudioServer.set_bus_volume_db(i, AMBIENT_NORMAL_DB)
	_quiet = Game.progress.get("loop_stopped", false) if is_instance_valid(Game) else false
	if _quiet:
		_rain.stop()
		_hum.stop()
		_rain.volume_db = BED_FADE_OUT_DB
		_hum.volume_db = BED_FADE_OUT_DB
	else:
		_rain.volume_db = BED_RAIN_DB
		_hum.volume_db = BED_HUM_DB
		if not _rain.playing:
			_rain.play()
		if not _hum.playing:
			_hum.play()
