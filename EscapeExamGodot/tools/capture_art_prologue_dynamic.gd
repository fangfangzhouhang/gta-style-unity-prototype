extends SceneTree
## ART-PROLOGUE-DYNAMIC evidence: multi-frame start/mid/end for P01/P03/P07.
## Mirrors scripts/systems/prologue.gd SHOTS eye→eye_end motion (gl_compat / llvmpipe).
## Does not claim visual PASS.

const OUT_DIR := "res://evidence/art-prologue/"
const OUT_REVIEW := "res://evidence/screenshots/art-review/"
const RES := Vector2i(1280, 800)

# Keep in sync with prologue.gd SHOTS (dynamic ends).
const SHOTS := {
	"P01": {
		"eye": Vector3(-8.75, 1.58, -5.35), "look": Vector3(-15.5, 1.05, -5.35),
		"eye_end": Vector3(-9.22, 1.54, -5.35), "look_end": Vector3(-15.5, 1.00, -5.35),
		"fov": 58.0, "fov_end": 56.0, "stem": "p01-rain-window",
	},
	"P03": {
		"eye": Vector3(-1.72, 1.62, -0.72), "look": Vector3(-2.88, 2.00, -0.62),
		"eye_end": Vector3(-1.95, 1.68, -0.55), "look_end": Vector3(-2.88, 2.08, -0.50),
		"fov": 50.0, "fov_end": 48.0, "stem": "p03-plaque-clue",
	},
	"P07": {
		"eye": Vector3(-6.50, 1.72, -4.55), "look": Vector3(-6.50, 1.95, -6.34),
		"eye_end": Vector3(-6.50, 1.78, -5.05), "look_end": Vector3(-6.50, 2.02, -6.34),
		"fov": 40.0, "fov_end": 38.0, "stem": "p07-teacher-smile",
	},
}

func _initialize() -> void:
	call_deferred("go")

func _lerp_shot(shot: Dictionary, t: float) -> Dictionary:
	var eye0: Vector3 = shot.eye
	var eye1: Vector3 = shot.eye_end
	var look0: Vector3 = shot.look
	var look1: Vector3 = shot.look_end
	var fov0 := float(shot.fov)
	var fov1 := float(shot.fov_end)
	return {
		"eye": eye0.lerp(eye1, t),
		"look": look0.lerp(look1, t),
		"fov": lerpf(fov0, fov1, t),
	}

func _snap(cam: Camera3D, pose: Dictionary) -> void:
	cam.fov = float(pose.fov)
	cam.global_position = pose.eye as Vector3
	cam.look_at(pose.look as Vector3, Vector3.UP)

func _capture(cam: Camera3D, path_a: String, path_b: String) -> Image:
	await create_timer(0.35).timeout
	await RenderingServer.frame_post_draw
	RenderingServer.force_draw()
	await RenderingServer.frame_post_draw
	var img: Image = root.get_texture().get_image()
	img.save_png(path_a)
	img.save_png(path_b)
	return img

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

	game.teacher.visible = true
	game.teacher.frozen = true
	game.teacher.collision_layer = 0
	game.teacher.collision_mask = 0
	game.teacher.velocity = Vector3.ZERO
	game.teacher.global_position = Vector3(-6.5, 0.04, -6.45)
	game.teacher.rotation = Vector3(0, PI, 0)
	game.teacher.state = "patrol"
	game.teacher.suspicion = 0.0
	if game.teacher.has_node("Visual/Head"):
		game.teacher.get_node("Visual/Head").rotation = Vector3.ZERO
	if game.teacher.has_method("_update_alert_ui"):
		game.teacher._update_alert_ui()

	var cam := Camera3D.new()
	cam.name = "ArtPrologueDynamicCaptureCam"
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
		var stem: String = str(shot.stem)
		for phase in [
			{"t": 0.0, "tag": "start"},
			{"t": 0.5, "tag": "mid"},
			{"t": 1.0, "tag": "end"},
		]:
			var pose := _lerp_shot(shot, float(phase.t))
			_snap(cam, pose)
			# P07 end: tiny head yaw like runtime micro (visual only)
			if id == "P07" and game.teacher.has_node("Visual/Head"):
				var head: Node3D = game.teacher.get_node("Visual/Head")
				head.rotation.y = 0.032 if str(phase.tag) == "mid" else ( -0.018 if str(phase.tag) == "end" else 0.0 )
			var fname := "%s-dyn-%s.png" % [stem, str(phase.tag)]
			var path_a := OUT_DIR + fname
			var path_b := OUT_REVIEW + "art-prologue-" + fname
			var img: Image = await _capture(cam, path_a, path_b)
			var eye: Vector3 = pose.eye
			var delta := (shot.eye_end as Vector3) - (shot.eye as Vector3)
			print("ART_PROLOGUE_DYN ", id, " ", phase.tag,
				" method=", RenderingServer.get_current_rendering_method(),
				" size=", img.get_width(), "x", img.get_height(),
				" eye=", eye,
				" delta_end=", delta,
				" fov=", pose.fov,
				" alert_hidden=", alert_hidden,
				" -> ", path_a)

	print("ART_PROLOGUE_DYNAMIC_OK alert_hidden=", alert_hidden,
		" method=", RenderingServer.get_current_rendering_method())
	quit(0)
