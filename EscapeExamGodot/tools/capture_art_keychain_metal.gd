extends SceneTree
## ART-KEYCHAIN-METAL-READ — KeyK* + GateAntechamber interact-metal evidence.

const OUT := "res://evidence/screenshots/art-keychain-metal/"
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
	game.player.visible = false

	var cam := Camera3D.new()
	cam.fov = 45.0
	cam.near = 0.04
	cam.far = 80.0
	level.add_child(cam)
	cam.current = true

	# Soft fill Omni for albedo proof only (NOT committed scene lighting).
	var fill := OmniLight3D.new()
	fill.light_color = Color(0.95, 0.93, 0.88)
	fill.light_energy = 1.6
	fill.omni_range = 3.5
	fill.shadow_enabled = false
	level.add_child(fill)

	for id in ["KeyK1", "KeyK2", "KeyK3"]:
		var k: Node3D = level.get_node_or_null(id)
		if k:
			k.visible = true

	# --- albedo-fill close-ups ---
	fill.global_position = Vector3(-4.05, 1.4, -0.2)
	await _shot_at(cam, Vector3(-4.05, 1.12, -0.12), Vector3(-4.05, 1.05, -0.55), "key-k1-albedo.png", "K1A")
	fill.global_position = Vector3(-2.55, 0.9, 4.0)
	await _shot_at(cam, Vector3(-2.55, 0.62, 3.95), Vector3(-2.55, 0.55, 4.35), "key-k2-albedo.png", "K2A")
	fill.global_position = Vector3(-3.35, 1.4, 6.2)
	await _shot_at(cam, Vector3(-3.35, 1.22, 6.15), Vector3(-3.35, 1.15, 6.55), "key-k3-albedo.png", "K3A")

	var gate: Node3D = level.get_node("Doors/GateAntechamber")
	var handle: Node3D = gate.get_node("Hinge/Panel/Handle0_065")
	var plate: Node3D = gate.get_node_or_null("Hinge/Panel/LockPlate")
	print("HANDLE ", handle.global_position, " PLATE ", plate.global_position if plate else null)
	fill.global_position = handle.global_position + Vector3(-0.4, 0.3, 0.2)
	await _shot_at(cam, handle.global_position + Vector3(-0.5, 0.08, 0.2), handle.global_position, "gate-handle-albedo.png", "GATEA")

	# --- in-situ (fill off) ---
	fill.light_energy = 0.0
	game.player.visible = true
	cam.current = false
	# Player-view GateAntechamber (restores earlier good framing)
	game.player.teleport_to(Vector3(2.6, 0.04, 1.8), -PI * 0.5)
	game.player.get_node("Head").position.y = 1.55
	game.player.camera.fov = 55.0
	game.player.look_at_point(Vector3(4.0, 1.05, 1.8))
	await _snap_root("gate-antechamber-insitu.png", "GATEI")

	# Player near KeyK2 (warm caption + mesh if any light)
	game.player.teleport_to(Vector3(-2.55, 0.04, 3.6), 0.0)
	game.player.get_node("Head").position.y = 1.1
	game.player.camera.fov = 60.0
	game.player.look_at_point(Vector3(-2.55, 0.55, 4.35))
	await _snap_root("key-k2-insitu.png", "K2I")

	print("ART_KEYCHAIN_METAL_OK method=", RenderingServer.get_current_rendering_method())
	quit(0)

func _shot_at(cam: Camera3D, pos: Vector3, look: Vector3, filename: String, tag: String) -> void:
	cam.current = true
	cam.global_position = pos
	cam.look_at(look, Vector3.UP)
	await create_timer(0.45).timeout
	await RenderingServer.frame_post_draw
	RenderingServer.force_draw()
	await RenderingServer.frame_post_draw
	var img: Image = root.get_texture().get_image()
	img.save_png(OUT + filename)
	print("saved ", tag, " ", OUT + filename, " center=", img.get_pixel(640, 400))

func _snap_root(filename: String, tag: String) -> void:
	await create_timer(0.55).timeout
	await RenderingServer.frame_post_draw
	RenderingServer.force_draw()
	await RenderingServer.frame_post_draw
	var img: Image = root.get_texture().get_image()
	img.save_png(OUT + filename)
	print("saved ", tag, " ", OUT + filename, " center=", img.get_pixel(640, 400))
