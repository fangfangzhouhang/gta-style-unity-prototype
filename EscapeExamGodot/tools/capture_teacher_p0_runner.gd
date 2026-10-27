extends Node

var scene: Node3D
var step := 0
var wait := 0

func _ready() -> void:
	print("RUNNER_READY")
	get_window().size = Vector2i(1280, 800)
	scene = load("res://scenes/actors/teacher_visual.tscn").instantiate()
	add_child(scene)
	_apply_pose(scene.get_node("Idle/Body"), "idle")
	_apply_pose(scene.get_node("Walk/Body"), "walk")
	_apply_pose(scene.get_node("Chase/Body"), "chase")
	scene.get_node("Walk").visible = false
	scene.get_node("Chase").visible = false
	scene.get_node("Backdrop").visible = false
	_set_env("grey")
	_activate("CamFront")
	step = 1
	wait = 20

func _process(_dt: float) -> void:
	if wait > 0:
		wait -= 1
		return
	match step:
		1:
			_save("CamFront", "teacher-p0-turnaround-front.png"); step = 2; wait = 12
		2:
			_save("CamSide", "teacher-p0-turnaround-side.png"); step = 3; wait = 12
		3:
			_save("CamBack", "teacher-p0-turnaround-back.png"); step = 4; wait = 12
		4:
			_save("Cam8m", "teacher-p0-read-8m.png"); step = 5; wait = 12
		5:
			scene.get_node("Walk").position = Vector3(1.6, 0, 0)
			scene.get_node("Chase").position = Vector3(-1.6, 0, 0)
			scene.get_node("Walk").visible = true
			scene.get_node("Chase").visible = true
			_set_env("grey")
			var cam: Camera3D = scene.get_node("CamFront")
			cam.position = Vector3(0, 1.25, -5.8)
			cam.rotation = Vector3(0, 3.14159, 0)
			cam.fov = 42.0
			_activate("CamFront")
			step = 6; wait = 15
		6:
			_save("CamFront", "teacher-p0-poses-engine.png")
			scene.get_node("Walk").visible = false
			scene.get_node("Chase").visible = false
			scene.get_node("Idle").position = Vector3.ZERO
			var cam2: Camera3D = scene.get_node("CamFront")
			cam2.position = Vector3(0, 1.15, -4.2)
			cam2.rotation = Vector3(0, 3.14159, 0)
			cam2.fov = 35.0
			_set_env("classroom")
			_activate("CamFront")
			step = 7; wait = 15
		7:
			_save("CamFront", "teacher-p0-classroom-warm.png")
			_set_env("corridor")
			_activate("CamFront")
			step = 8; wait = 15
		8:
			_save("CamFront", "teacher-p0-corridor-dim.png"); step = 9; wait = 12
		9:
			_save("Cam8m", "teacher-p0-corridor-8m.png")
			print("TEACHER_P0_CAPTURE_OK")
			get_tree().quit(0)

func _activate(cam_name: String) -> void:
	var cam: Camera3D = scene.get_node(cam_name)
	cam.current = true
	RenderingServer.force_draw()

func _save(cam_name: String, filename: String) -> void:
	_activate(cam_name)
	RenderingServer.force_draw()
	var img: Image = get_viewport().get_texture().get_image()
	var path := "res://evidence/screenshots/art-review/" + filename
	img.save_png(path)
	print("saved ", path, " size=", img.get_width(), "x", img.get_height(), " uniq_probe=", img.get_pixel(640, 400))

func _apply_pose(body: Node, kind: String) -> void:
	var v: Node3D = body.get_node("Visual")
	match kind:
		"idle":
			v.get_node("LeftLeg").rotation = Vector3.ZERO
			v.get_node("RightLeg").rotation = Vector3.ZERO
			v.get_node("LeftArm").rotation = Vector3(0.05, 0, 0.08)
			v.get_node("RightArm").rotation = Vector3(0.05, 0, -0.08)
			v.get_node("Head").rotation = Vector3(0.02, 0, 0)
			v.rotation = Vector3.ZERO
		"walk":
			v.get_node("LeftLeg").rotation = Vector3(0.45, 0, 0)
			v.get_node("RightLeg").rotation = Vector3(-0.40, 0, 0)
			v.get_node("LeftArm").rotation = Vector3(-0.35, 0, 0.05)
			v.get_node("RightArm").rotation = Vector3(0.30, 0, -0.05)
			v.get_node("Head").rotation = Vector3(0.0, 0.08, 0)
			v.rotation = Vector3(0.04, 0, 0)
		"chase":
			v.get_node("LeftLeg").rotation = Vector3(0.75, 0, 0)
			v.get_node("RightLeg").rotation = Vector3(-0.70, 0, 0)
			v.get_node("LeftArm").rotation = Vector3(-0.55, 0, 0.1)
			v.get_node("RightArm").rotation = Vector3(0.45, 0, -0.12)
			v.get_node("Head").rotation = Vector3(0.12, 0, 0)
			v.rotation = Vector3(0.18, 0, 0)

func _set_env(mode: String) -> void:
	var we: WorldEnvironment = scene.get_node("WorldEnvironment")
	var key: DirectionalLight3D = scene.get_node("KeyLight")
	var fill: OmniLight3D = scene.get_node("FillLight")
	var rim: OmniLight3D = scene.get_node("RimLight")
	var warm: OmniLight3D = scene.get_node("ClassroomWarm")
	var cold: OmniLight3D = scene.get_node("CorridorCold")
	match mode:
		"grey":
			key.light_energy = 1.15; fill.light_energy = 1.4; rim.light_energy = 0.9
			warm.light_energy = 0.0; cold.light_energy = 0.0
			key.light_color = Color(1, 0.98, 0.94)
			we.environment.background_color = Color(0.55, 0.56, 0.58)
			we.environment.ambient_light_color = Color(0.72, 0.74, 0.76)
			we.environment.ambient_light_energy = 0.55
		"classroom":
			key.light_energy = 0.85; fill.light_energy = 0.6; rim.light_energy = 0.35
			warm.light_energy = 2.2; cold.light_energy = 0.0
			key.light_color = Color(1, 0.95, 0.85)
			we.environment.background_color = Color(0.78, 0.74, 0.68)
			we.environment.ambient_light_color = Color(0.92, 0.88, 0.78)
			we.environment.ambient_light_energy = 0.5
		"corridor":
			key.light_energy = 0.25; fill.light_energy = 0.35; rim.light_energy = 1.6
			warm.light_energy = 0.0; cold.light_energy = 1.8
			key.light_color = Color(0.65, 0.75, 0.90)
			we.environment.background_color = Color(0.14, 0.16, 0.20)
			we.environment.ambient_light_color = Color(0.30, 0.38, 0.48)
			we.environment.ambient_light_energy = 0.18
