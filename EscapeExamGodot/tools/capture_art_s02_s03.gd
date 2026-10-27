extends SceneTree

func _initialize() -> void:
	call_deferred("go")

func go() -> void:
	root.size = Vector2i(1280, 800)
	var game = root.get_node("Game")
	var level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	game.start_new()
	await create_timer(1.0).timeout
	game.teacher.frozen = true
	game.player.input_locked = true
	game.ui.toast_time = 0
	# Teacher mid-corridor, facing camera
	game.teacher.global_position = Vector3(-1.75, 0.02, -1.0)
	game.teacher.rotation.y = 0
	game.teacher.suspicion = 0.55
	game.teacher.change_state("suspicious")
	# S02 corridor look — from refined capture pose toward teacher
	game.player.teleport_to(Vector3(-1.75, 0.04, 1.5), PI)
	game.player.look_at_point(Vector3(-1.75, 1.55, -1.0))
	await create_timer(0.7).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://evidence/screenshots/art-review/s02-door-corridor.png")
	# S03 lower angle near cabinet-ish
	game.player.teleport_to(Vector3(-3.0, 0.04, 0.2), PI * 0.65)
	game.player.look_at_point(Vector3(-1.75, 1.35, -1.0))
	await create_timer(0.6).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://evidence/screenshots/art-review/s03-cabinet-teacher.png")
	# Bonus: close silhouette
	game.player.teleport_to(Vector3(-1.75, 0.04, 0.4), PI)
	game.player.look_at_point(Vector3(-1.75, 1.6, -1.0))
	await create_timer(0.5).timeout
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://evidence/screenshots/art-review/s02b-teacher-close.png")
	print("ART_CAPTURE_OK")
	quit()
