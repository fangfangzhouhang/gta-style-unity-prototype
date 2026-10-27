extends SceneTree
## S02/S03 capture for Fixture4 west-corridor no-shadow fill bump.
## Poses locked to docs/SHOT_BLOCKS_CHAPTER01.md / capture_art_s01_s05.gd / capture_audio_cold_spill_s02_s03.gd.

const OUT := "res://evidence/screenshots/audio-fixture4-fill/"
const RES := Vector2i(1280, 800)

func _initialize() -> void:
	call_deferred("go")

func go() -> void:
	DirAccess.make_dir_recursive_absolute(OUT)
	root.size = RES
	DisplayServer.window_set_size(RES)
	var game = root.get_node("Game")
	var level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	game.start_new()
	await create_timer(1.0).timeout
	game.teacher.frozen = true
	game.player.input_locked = true
	game.ui.toast_time = 0.0
	if game.teacher.has_node("AlertIcon"):
		game.teacher.get_node("AlertIcon").visible = false
	if game.teacher.has_node("SuspicionBar"):
		game.teacher.get_node("SuspicionBar").visible = false

	var shadow_n := 0
	var f4_energy := -1.0
	var f4_range := -1.0
	var f4_shadow := true
	var lighting: Node = level.get_node_or_null("Lighting")
	if lighting:
		for child in lighting.get_children():
			var light: Node = child.get_node_or_null("Light")
			if light is OmniLight3D and light.shadow_enabled:
				shadow_n += 1
				print("SHADOW_ON ", child.name)
			if child.name == "Fixture4" and light is OmniLight3D:
				f4_energy = light.light_energy
				f4_range = light.omni_range
				f4_shadow = light.shadow_enabled
				print("FIXTURE4 color=", light.light_color, " energy=", f4_energy, " range=", f4_range, " att=", light.omni_attenuation, " size=", light.light_size, " shadow=", f4_shadow, " pos=", child.global_position)
	print("SHADOW_OMNI_COUNT ", shadow_n)

	# --- S02 · door → corridor teacher ---
	_place_teacher(game, Vector3(-1.75, 0.02, -1.0), 0.0)
	game.teacher.suspicion = 0.55
	if game.teacher.has_method("change_state"):
		game.teacher.change_state("suspicious")
	game.player.teleport_to(Vector3(-1.75, 0.04, 1.5), PI)
	game.player.get_node("Head").position.y = 1.62
	game.player.camera.fov = 72.0
	game.player.look_at_point(Vector3(-1.75, 1.55, -1.0))
	await _snap("s02-after.png", "S02")

	# --- S03 · crouch near cabinet ---
	game.player.teleport_to(Vector3(-3.0, 0.04, 0.2), PI * 0.65)
	game.player.set_crouch(true)
	game.player.get_node("Head").position.y = 0.94
	game.player.camera.fov = 70.0
	game.player.look_at_point(Vector3(-1.75, 1.35, -1.0))
	await _snap("s03-after.png", "S03")

	print("AUDIO_FIXTURE4_FILL_CAPTURE_OK method=", RenderingServer.get_current_rendering_method(), " shadow=", shadow_n, " f4_energy=", f4_energy, " f4_range=", f4_range, " f4_shadow=", f4_shadow)
	quit(0)

func _place_teacher(game, pos: Vector3, yaw: float) -> void:
	game.teacher.global_position = pos
	game.teacher.rotation = Vector3(0, yaw, 0)
	game.teacher.velocity = Vector3.ZERO

func _snap(filename: String, tag: String) -> void:
	await create_timer(0.75).timeout
	await RenderingServer.frame_post_draw
	RenderingServer.force_draw()
	await RenderingServer.frame_post_draw
	var img: Image = root.get_texture().get_image()
	var path := OUT + filename
	img.save_png(path)
	var p = root.get_node("Game").player
	print("saved ", tag, " ", path, " ", img.get_width(), "x", img.get_height(),
		" probe=", img.get_pixel(640, 400),
		" fov=", p.camera.fov,
		" eye=", p.camera.global_position,
		" teacher=", root.get_node("Game").teacher.global_position)
