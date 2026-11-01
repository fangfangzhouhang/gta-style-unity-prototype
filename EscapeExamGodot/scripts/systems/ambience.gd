extends Node
## 雨/灯管环境床 + Ambient duck + 抓捕刺音。床不进 acoustics AI 事件。

const RAIN := preload("res://assets/audio/rain_bed.wav")
const HUM := preload("res://assets/audio/tube_hum.wav")
const STING := preload("res://assets/audio/catch_sting.wav")

const AMBIENT_BUS := "Ambient"
const STING_BUS := "Sting"
const OCCLUDED_BUS := "SFXOccluded"
const BED_RAIN_DB := -9.0
const BED_HUM_DB := -13.0
const AMBIENT_NORMAL_DB := 0.0
const AMBIENT_DUCK_DB := -22.0
const BED_FADE_OUT_DB := -40.0

var _rain: AudioStreamPlayer
var _hum: AudioStreamPlayer
var _sting: AudioStreamPlayer
## V2 自适应张力层（adaptive_audio.gd），_ready 自注册为本节点子节点。
var _adaptive: Node
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
	# V2：自适应张力音频层自注册（不改 project.godot / 场景）。quiet 状态
	# 由 set_loop_quiet 转发 + 它自身轮询 _quiet 兜底，两者缺一不可。
	_adaptive = preload("res://scripts/systems/adaptive_audio.gd").new()
	_adaptive.name = "AdaptiveAudio"
	add_child(_adaptive)
	if _quiet and _adaptive.has_method("set_quiet"):
		_adaptive.set_quiet(true)
	# 仅玩法/标题后开床；测试模式也开以便总线存在，但不依赖 AI。
	if not _quiet:
		if not _rain.playing:
			_rain.play()
		if not _hum.playing:
			_hum.play()

func _ensure_buses() -> void:
	# 若项目未挂 layout，运行时补 Music / Ambient / SFX / Sting / SFXOccluded。
	for bus_name in ["Music", AMBIENT_BUS, "SFX", STING_BUS, OCCLUDED_BUS]:
		if AudioServer.get_bus_index(bus_name) == -1:
			AudioServer.add_bus()
			var idx := AudioServer.bus_count - 1
			AudioServer.set_bus_name(idx, bus_name)
			AudioServer.set_bus_send(idx, "Master")
	_ensure_occluded_effects()
	_clear_bus_mute_flags()

func _ensure_occluded_effects() -> void:
	## SFXOccluded：隔墙/隔门声的统一去向——低通 ~800 Hz 砍掉高频细节，
	## 再叠一层轻混响（wet 克制，只是"隔了一层"的空间感，不是大厅回声）。
	## 幂等：已有同类效果不重复添加（layout 将来若内置同名总线与效果，
	## 这里按类型查重后跳过）。
	var idx := AudioServer.get_bus_index(OCCLUDED_BUS)
	if idx < 0:
		return
	var has_lp := false
	var has_rev := false
	for e in AudioServer.get_bus_effect_count(idx):
		var eff := AudioServer.get_bus_effect(idx, e)
		if eff is AudioEffectLowPassFilter:
			has_lp = true
		elif eff is AudioEffectReverb:
			has_rev = true
	if not has_lp:
		var lp := AudioEffectLowPassFilter.new()
		lp.cutoff_hz = 800.0
		lp.resonance = 0.4
		AudioServer.add_bus_effect(idx, lp)
	if not has_rev:
		var rev := AudioEffectReverb.new()
		rev.room_size = 0.55
		rev.damping = 0.65
		rev.wet = 0.18
		rev.dry = 1.0
		AudioServer.add_bus_effect(idx, rev)

func _clear_bus_mute_flags() -> void:
	## Harden: Ambient/SFX must not stay muted/solo after duck or bad local_data.
	for bus_name in [AMBIENT_BUS, "SFX", STING_BUS, OCCLUDED_BUS, "Music", "Master"]:
		var i := AudioServer.get_bus_index(bus_name)
		if i < 0:
			continue
		AudioServer.set_bus_mute(i, false)
		AudioServer.set_bus_solo(i, false)

func set_loop_quiet(quiet: bool) -> void:
	## Broadcast 停播 → 床真正安静（淡出后 stop）；恢复则重开循环床。
	## V2：自适应层（脉冲/擦弦/心跳/drone）同样必须静默——"真安静"不许打折。
	_quiet = quiet
	if is_instance_valid(_adaptive) and _adaptive.has_method("set_quiet"):
		_adaptive.set_quiet(quiet)
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
	if is_instance_valid(_adaptive) and _adaptive.has_method("set_quiet"):
		_adaptive.set_quiet(_quiet)
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
