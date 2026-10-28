extends SceneTree
## S01 classroom corner sample capture (locked pose from SHOT_BLOCKS_CHAPTER01).

const OUT := "res://evidence/screenshots/art-review/"
const RES := Vector2i(1280, 800)

func _initialize() -> void:
	call_deferred("go")

func go() -> void:
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

	# S01 · seat → lectern/board + teacher silhouette; door toward +X
	game.teacher.global_position = Vector3(-6.35, 0.02, -5.55)
	game.teacher.rotation = Vector3(0, 0, 0)
	game.teacher.velocity = Vector3.ZERO
	game.player.teleport_to(Vector3(-6.8, 0.04, -0.85), 0.0)
	game.player.get_node("Head").position.y = 1.62
	game.player.camera.fov = 74.0
	game.player.look_at_point(Vector3(-5.4, 1.55, -5.3))
	await create_timer(0.75).timeout
	await RenderingServer.frame_post_draw
	RenderingServer.force_draw()
	await RenderingServer.frame_post_draw
	var img: Image = root.get_texture().get_image()
	var path := OUT + "art-s01-after.png"
	img.save_png(path)
	var omni := 0
	for n in level.find_children("*", "OmniLight3D", true, false):
		omni += 1
	var alert_hidden := true
	if game.teacher.has_node("AlertIcon"):
		alert_hidden = alert_hidden and (not game.teacher.get_node("AlertIcon").visible)
	if game.teacher.has_node("SuspicionBar"):
		alert_hidden = alert_hidden and (not game.teacher.get_node("SuspicionBar").visible)
	print("ART_S01_CORNER_OK method=", RenderingServer.get_current_rendering_method(),
		" size=", img.get_width(), "x", img.get_height(),
		" probe=", img.get_pixel(640, 400),
		" omni=", omni,
		" alert_hidden=", alert_hidden,
		" eye=", game.player.camera.global_position,
		" teacher=", game.teacher.global_position)
	quit(0)
