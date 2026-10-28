extends SceneTree
## Prologue P01/P03/P07 camera evidence (gl_compat). Keep transforms synced with prologue.gd SHOTS.
## Does not claim visual PASS — box/llvmpipe evidence only.

const OUT_DIR := "res://evidence/art-prologue/"
const OUT_REVIEW := "res://evidence/screenshots/art-review/"
const RES := Vector2i(1280, 800)

# Mirrors scripts/systems/prologue.gd SHOTS (ART-PROLOGUE-CAMS). VO unused here.
const SHOTS := {
	"P01": {"eye": Vector3(-8.75, 1.58, -5.35), "look": Vector3(-15.5, 1.05, -5.35), "fov": 58.0, "file": "p01-rain-window.png"},
	"P03": {"eye": Vector3(-1.95, 1.68, -0.55), "look": Vector3(-2.88, 2.08, -0.50), "fov": 48.0, "file": "p03-plaque-clue.png"},
	"P07": {"eye": Vector3(-6.50, 1.78, -5.05), "look": Vector3(-6.50, 2.02, -6.34), "fov": 38.0, "file": "p07-teacher-smile.png"},
}

func _initialize() -> void:
	call_deferred("go")

func go() -> void:
	root.size = RES
	DisplayServer.window_set_size(RES)
	var game = root.get_node("Game")
	game.skip_prologue_once = true
	var level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	game.start_new()
	await create_timer(1.0).timeout
	game.player.input_locked = true
	game.ui.toast_time = 0.0
	if is_instance_valid(game.ui):
		game.ui.visible = false
	if game.teacher.has_node("AlertIcon"):
		game.teacher.get_node("AlertIcon").visible = false
	if game.teacher.has_node("SuspicionBar"):
		game.teacher.get_node("SuspicionBar").visible = false

	# Same frozen visual pose as prologue.gd _pose_teacher_for_shots(true)
	game.teacher.visible = true
	game.teacher.frozen = true
	game.teacher.collision_layer = 0
	game.teacher.collision_mask = 0
	game.teacher.velocity = Vector3.ZERO
	game.teacher.global_position = Vector3(-6.5, 0.04, -6.45)
	game.teacher.rotation = Vector3(0, PI, 0)
	game.teacher.state = "patrol"
	game.teacher.suspicion = 0.0
	if game.teacher.has_method("_update_alert_ui"):
		game.teacher._update_alert_ui()

	var cam := Camera3D.new()
	cam.name = "ArtPrologueCaptureCam"
	level.add_child(cam)
	if is_instance_valid(game.player) and is_instance_valid(game.player.camera):
		game.player.camera.current = false
	cam.current = true

	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUT_DIR))
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(OUT_REVIEW))

	var alert_hidden := true
	if game.teacher.has_node("AlertIcon"):
		alert_hidden = alert_hidden and (not game.teacher.get_node("AlertIcon").visible)
	if game.teacher.has_node("SuspicionBar"):
		alert_hidden = alert_hidden and (not game.teacher.get_node("SuspicionBar").visible)

	for id in ["P01", "P03", "P07"]:
		var shot: Dictionary = SHOTS[id]
		cam.fov = float(shot.fov)
		cam.global_position = shot.eye as Vector3
		cam.look_at(shot.look as Vector3, Vector3.UP)
		await create_timer(0.55).timeout
		await RenderingServer.frame_post_draw
		RenderingServer.force_draw()
		await RenderingServer.frame_post_draw
		var img: Image = root.get_texture().get_image()
		var fname: String = str(shot.file)
		var path_a := OUT_DIR + fname
		var path_b := OUT_REVIEW + "art-prologue-" + fname
		img.save_png(path_a)
		img.save_png(path_b)
		print("ART_PROLOGUE_CAM ", id,
			" method=", RenderingServer.get_current_rendering_method(),
			" size=", img.get_width(), "x", img.get_height(),
			" probe=", img.get_pixel(640, 400),
			" eye=", cam.global_position,
			" fov=", cam.fov,
			" alert_hidden=", alert_hidden,
			" -> ", path_a)

	print("ART_PROLOGUE_CAMS_OK alert_hidden=", alert_hidden,
		" method=", RenderingServer.get_current_rendering_method())
	quit(0)
