extends SceneTree

func _initialize() -> void:
	call_deferred("capture")

func capture() -> void:
	root.size = Vector2i(1280,800)
	var game = root.get_node("Game")
	var level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	game.start_new()
	game.teacher.frozen = true
	game.player.input_locked = true
	game.ui.toast_time = 0
	game.player.teleport_to(Vector3(-8.9,0.04,1.7),0)
	game.player.look_at_point(Vector3(-5.6,1.5,-5.8))
	await create_timer(1).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://evidence/screenshots/m2-classroom-refined.png")
	game.player.teleport_to(Vector3(-1.75,0.04,1.5),PI)
	await create_timer(.5).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://evidence/screenshots/m2-corridor-refined.png")
	game.player.teleport_to(Vector3(5.8,0.04,-2.6),-PI/2)
	await create_timer(.5).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://evidence/screenshots/m2-broadcast-refined.png")
	print("RENDER_CAPTURE_COMPLETE ", RenderingServer.get_current_rendering_method())
	quit()
