extends SceneTree
## Runtime evidence probe: Alert UI forced-off + Ambience true-quiet / sting duck / reset.
## Headless/xvfb: bus/player state assertions are code evidence; human ear = 未验证.

var game
var level
var passed := 0
var failed := 0
var lines: Array[String] = []
var screenshot_ok := 0
var screenshot_fail := 0

func ambience_node() -> Node:
	# -s SceneTree scripts may not see Autoload globals at compile time; use tree path.
	return root.get_node_or_null("Ambience")

func _initialize() -> void:
	call_deferred("run")

func check(condition: bool, label: String) -> void:
	var message := ("PASS " if condition else "FAIL ") + label
	lines.append(message)
	print(message)
	if condition:
		passed += 1
	else:
		failed += 1

func note(message: String) -> void:
	lines.append("NOTE " + message)
	print("NOTE " + message)

func frames(count: int) -> void:
	for i in count:
		await physics_frame

func ambient_bus_db() -> float:
	var i := AudioServer.get_bus_index("Ambient")
	if i < 0:
		return -999.0
	return AudioServer.get_bus_volume_db(i)

func rain_player() -> AudioStreamPlayer:
	var a := ambience_node()
	return null if a == null else a.get_node_or_null("RainBed") as AudioStreamPlayer

func hum_player() -> AudioStreamPlayer:
	var a := ambience_node()
	return null if a == null else a.get_node_or_null("TubeHum") as AudioStreamPlayer

func sting_player() -> AudioStreamPlayer:
	var a := ambience_node()
	return null if a == null else a.get_node_or_null("CatchSting") as AudioStreamPlayer

func assert_alert_hidden(teacher: Node, phase: String) -> void:
	var alert_ok := true
	var bar_ok := true
	if teacher.has_node("AlertIcon"):
		alert_ok = teacher.get_node("AlertIcon").visible == false
	else:
		note("AlertIcon node missing in %s (treated as hidden)" % phase)
	if teacher.has_node("SuspicionBar"):
		bar_ok = teacher.get_node("SuspicionBar").visible == false
	else:
		note("SuspicionBar node missing in %s (treated as hidden)" % phase)
	check(alert_ok, "AlertIcon.visible==false during %s" % phase)
	check(bar_ok, "SuspicionBar.visible==false during %s" % phase)

func call_update_alert(teacher: Node) -> void:
	if teacher.has_method("_update_alert_ui"):
		teacher._update_alert_ui()

func save_shot(path: String, label: String) -> void:
	DirAccess.make_dir_recursive_absolute("res://evidence/screenshots/audio-alert-off")
	await RenderingServer.frame_post_draw
	await process_frame
	await RenderingServer.frame_post_draw
	var img: Image = root.get_texture().get_image()
	if img == null:
		screenshot_fail += 1
		note("screenshot FAILED (null image): %s (%s)" % [label, path])
		return
	# Headless without a real surface can yield empty/black images; still write if pixels exist.
	var err := img.save_png(path)
	if err != OK:
		screenshot_fail += 1
		note("screenshot FAILED save_png err=%s: %s (%s)" % [str(err), label, path])
		return
	var abs_path := ProjectSettings.globalize_path(path)
	if not FileAccess.file_exists(path):
		screenshot_fail += 1
		note("screenshot FAILED missing file after save: %s" % abs_path)
		return
	var size := FileAccess.get_file_as_bytes(path).size()
	if size < 200:
		screenshot_fail += 1
		note("screenshot FAILED tiny file (%d bytes): %s" % [size, abs_path])
		return
	screenshot_ok += 1
	note("screenshot OK %s bytes=%d path=%s" % [label, size, abs_path])

func count_shadow_omnis(level_root: Node) -> int:
	var lighting := level_root.get_node_or_null("Lighting")
	if lighting == null:
		return -1
	var n := 0
	for child in lighting.get_children():
		var light := child.get_node_or_null("Light")
		if light is OmniLight3D and (light as OmniLight3D).shadow_enabled:
			n += 1
	return n

func run() -> void:
	root.size = Vector2i(1280, 800)
	game = root.get_node("Game")
	level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	await process_frame
	game.start_new()
	var player = game.player
	var teacher = game.teacher
	teacher.frozen = true
	if "input_locked" in player:
		player.input_locked = true
	if is_instance_valid(game.ui) and "toast_time" in game.ui:
		game.ui.toast_time = 0
	await frames(5)

	var amb := ambience_node()
	check(amb != null, "Ambience autoload node present at /root/Ambience")
	if amb == null:
		note("Ambience missing — aborting remaining audio checks")
		print("RESULT passed=%d failed=%d" % [passed, failed])
		lines.append("RESULT passed=%d failed=%d" % [passed, failed])
		DirAccess.make_dir_recursive_absolute("res://evidence/logs")
		var early := FileAccess.open("res://evidence/logs/audio-alert-off-evidence.log", FileAccess.WRITE)
		if early:
			early.store_string("\n".join(lines) + "\n")
			early.close()
		quit(1)
		return

	# --- Alert UI forced off across danger states ---
	teacher.state = "patrol"
	teacher.suspicion = 0.0
	call_update_alert(teacher)
	await frames(2)
	assert_alert_hidden(teacher, "patrol")

	teacher.state = "investigate"
	teacher.suspicion = 0.35
	call_update_alert(teacher)
	await frames(2)
	assert_alert_hidden(teacher, "investigate")

	teacher.state = "suspicious"
	teacher.suspicion = 0.7
	call_update_alert(teacher)
	await frames(2)
	assert_alert_hidden(teacher, "suspicious")

	teacher.state = "chase"
	teacher.suspicion = 1.0
	call_update_alert(teacher)
	await frames(2)
	assert_alert_hidden(teacher, "chase")

	# --- Ambience beds after start ---
	var rain := rain_player()
	var hum := hum_player()
	var sting := sting_player()
	check(rain != null and hum != null, "RainBed and TubeHum players exist under Ambience")
	check(sting != null, "CatchSting player exists under Ambience")
	check(rain != null and rain.playing, "RainBed.playing==true after start (unless quiet)")
	check(hum != null and hum.playing, "TubeHum.playing==true after start (unless quiet)")
	check(AudioServer.get_bus_index("Ambient") >= 0, "Ambient bus exists")
	check(AudioServer.get_bus_index("Sting") >= 0, "Sting bus exists")
	var shadow_n := count_shadow_omnis(level)
	note("shadow OmniLight3D count under Lighting = %d (budget ≤5; expect 4)" % shadow_n)
	if shadow_n >= 0:
		check(shadow_n <= 5, "shadow Omni count ≤5 (measured=%d)" % shadow_n)

	# --- Broadcast true-quiet (stop players, not volume-only duck) ---
	ambience_node().call("set_loop_quiet", true)
	# fade 0.4s then stop callback
	await create_timer(0.7, true).timeout
	await frames(3)
	check(rain != null and not rain.playing, "true quiet: RainBed not playing after set_loop_quiet(true)")
	check(hum != null and not hum.playing, "true quiet: TubeHum not playing after set_loop_quiet(true)")

	ambience_node().call("set_loop_quiet", false)
	await create_timer(0.7, true).timeout
	await frames(3)
	check(rain != null and rain.playing, "beds restart: RainBed.playing after set_loop_quiet(false)")
	check(hum != null and hum.playing, "beds restart: TubeHum.playing after set_loop_quiet(false)")

	# --- Catch sting duck toward ~-22 then restore toward 0 ---
	var before_db := ambient_bus_db()
	note("Ambient bus before sting = %.2f dB" % before_db)
	ambience_node().call("play_catch_sting")
	await create_timer(0.15, true).timeout
	await frames(2)
	var ducked_db := ambient_bus_db()
	note("Ambient bus shortly after sting = %.2f dB (target ~-22)" % ducked_db)
	check(ducked_db <= -12.0, "Ambient bus ducked (≤ -12 dB shortly after sting; got %.2f)" % ducked_db)
	var sting_was_playing := sting != null and (sting.playing or sting.get_playback_position() > 0.001)
	check(sting_was_playing, "catch sting played (CatchSting.playing or position>0 shortly after play_catch_sting)")
	# Sample restore after ~1.0s (restore timer 0.85 + tween 0.45)
	await create_timer(1.05, true).timeout
	await frames(4)
	# Allow restore tween to settle a bit more
	await create_timer(0.5, true).timeout
	var restored_db := ambient_bus_db()
	note("Ambient bus after restore window = %.2f dB (target ~0)" % restored_db)
	check(restored_db >= -6.0, "Ambient bus restored toward 0 (≥ -6 dB; got %.2f)" % restored_db)

	# --- reset_audio_state clears stuck duck ---
	ambience_node().call("play_catch_sting")
	await create_timer(0.12, true).timeout
	ambience_node().call("reset_audio_state")
	await frames(2)
	var reset_db := ambient_bus_db()
	note("Ambient bus after reset_audio_state = %.2f dB" % reset_db)
	check(absf(reset_db - 0.0) < 0.5, "reset_audio_state: Ambient bus ~0 (got %.2f)" % reset_db)
	check(sting == null or not sting.playing, "reset_audio_state: CatchSting not stuck playing")

	# --- Static: ambience must not call acoustics.emit_sound ---
	var ambience_src := FileAccess.get_file_as_string("res://scripts/systems/ambience.gd")
	check(ambience_src.find("emit_sound") < 0, "ambience.gd does not call acoustics.emit_sound (static)")
	check(ambience_src.find("acoustics") >= 0, "ambience.gd documents acoustics AI exclusion (comment OK)")

	# --- Mood screenshots (best-effort under xvfb/headless) ---
	note("attempting mood viewport PNGs (may fail under pure headless — do not fake)")
	# classroom-warm near Fixture0 (-7.8,-5.8)
	player.teleport_to(Vector3(-8.9, 0.04, 1.7), 0)
	if player.has_method("look_at_point"):
		player.look_at_point(Vector3(-5.6, 1.5, -5.8))
	await create_timer(0.6, true).timeout
	await save_shot("res://evidence/screenshots/audio-alert-off/classroom-warm.png", "classroom-warm")

	# corridor-cold west-ish / Fixture5 area (2.7, 1.2) — use west corridor-ish then cold exit approach
	player.teleport_to(Vector3(-1.75, 0.04, 1.5), PI)
	if player.has_method("look_at_point"):
		player.look_at_point(Vector3(2.7, 1.4, 1.2))
	await create_timer(0.5, true).timeout
	await save_shot("res://evidence/screenshots/audio-alert-off/corridor-cold.png", "corridor-cold")

	# exit-pressure near Fixture7 / gate (6.4, -2.5) / exit
	player.teleport_to(Vector3(5.8, 0.04, -2.6), -PI / 2.0)
	if player.has_method("look_at_point"):
		player.look_at_point(Vector3(7.0, 1.4, 1.5))
	await create_timer(0.5, true).timeout
	await save_shot("res://evidence/screenshots/audio-alert-off/exit-pressure.png", "exit-pressure")

	note("screenshots ok=%d fail=%d" % [screenshot_ok, screenshot_fail])
	note("human ear / playfeel: 未验证 (box headless/xvfb; no audio recording claimed)")

	print("RESULT passed=%d failed=%d" % [passed, failed])
	lines.append("RESULT passed=%d failed=%d" % [passed, failed])
	lines.append("Fixture/bus/player state probe; human ear 未验证. screenshots_ok=%d screenshots_fail=%d" % [screenshot_ok, screenshot_fail])

	DirAccess.make_dir_recursive_absolute("res://evidence/logs")
	var report := FileAccess.open("res://evidence/logs/audio-alert-off-evidence.log", FileAccess.WRITE)
	if report:
		report.store_string("\n".join(lines) + "\n")
		report.close()
		print("LOG written res://evidence/logs/audio-alert-off-evidence.log")
	else:
		print("LOG write FAILED")
		failed += 1

	quit(1 if failed else 0)
