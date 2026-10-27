extends SceneTree

var scene: Node3D
var step := 0
var wait := 0
var out_dir := "res://evidence/screenshots/art-review"

func _initialize() -> void:
	print("CAPTURE_INIT")
	root.size = Vector2i(1280, 800)
	scene = load("res://scenes/actors/teacher_visual.tscn").instantiate()
	root.add_child(scene)
	_apply_pose(scene.get_node("Idle/Body"), "idle")
	_apply_pose(scene.get_node("Walk/Body"), "walk")
	_apply_pose(scene.get_node("Chase/Body"), "chase")
	scene.get_node("Walk").visible = false
	scene.get_node("Chase").visible = false
	_set_env("grey")
	scene.get_node("CamFront").current = true
	step = 1
	wait = 12
	print("CAPTURE_LOOP")

func _process(_dt: float) -> bool:
	if wait > 0:
		wait -= 1
		return true
	match step:
		1:
			_save("CamFront", "teacher-p0-turnaround-front.png")
			step = 2
			wait = 8
		2:
			_save("CamSide", "teacher-p0-turnaround-side.png")
			step = 3
			wait = 8
		3:
			_save("CamBack", "teacher-p0-turnaround-back.png")
			step = 4
			wait = 8
		4:
			_save("Cam8m", "teacher-p0-read-8m.png")
			step = 5
			wait = 8
		5:
			scene.get_node("Walk").position = Vector3(1.6, 0, 0)
			scene.get_node("Chase").position = Vector3(-1.6, 0, 0)
			scene.get_node("Walk").visible = true
			scene.get_node("Chase").visible = true
			_set_env("grey")
			var cam: Camera3D = scene.get_node("CamFront")
			cam.position = Vector3(0, 1.2, -5.5)
			cam.fov = 42.0
			cam.current = true
			step = 6
			wait = 10
		6:
			_save("CamFront", "teacher-p0-poses-engine.png")
			scene.get_node("Walk").visible = false
			scene.get_node("Chase").visible = false
			scene.get_node("Idle").position = Vector3.ZERO
			var cam2: Camera3D = scene.get_node("CamFront")
			cam2.position = Vector3(0, 1.15, -4.2)
			cam2.fov = 35.0
			_set_env("classroom")
			step = 7
			wait = 10
		7:
			_save("CamFront", "teacher-p0-classroom-warm.png")
			_set_env("corridor")
			step = 8
			wait = 10
		8:
			_save("CamFront", "teacher-p0-corridor-dim.png")
			step = 9
			wait = 8
		9:
			_save("Cam8m", "teacher-p0-corridor-8m.png")
			print("TEACHER_P0_CAPTURE_OK")
			quit(0)
	return true

func _save(cam_name: String, filename: String) -> void:
	var cam: Camera3D = scene.get_node(cam_name)
	cam.current = true
	var img: Image = root.get_texture().get_image()
	var path := out_dir + "/" + filename
	img.save_png(path)
	print("saved ", path)

func _apply_pose(body: Node, kind: String) -> void:
	if body == null or not body.has_node("Visual"):
		return
	var v: Node3D = body.get_node("Visual")
	var ll: Node3D = v.get_node("LeftLeg")
	var rl: Node3D = v.get_node("RightLeg")
	var la: Node3D = v.get_node("LeftArm")
	var ra: Node3D = v.get_node("RightArm")
	var head: Node3D = v.get_node("Head")
	match kind:
		"idle":
			ll.rotation = Vector3.ZERO
			rl.rotation = Vector3.ZERO
			la.rotation = Vector3(0.05, 0, 0.08)
			ra.rotation = Vector3(0.05, 0, -0.08)
			head.rotation = Vector3(0.02, 0, 0)
			v.rotation = Vector3.ZERO
		"walk":
			ll.rotation = Vector3(0.45, 0, 0)
			rl.rotation = Vector3(-0.40, 0, 0)
			la.rotation = Vector3(-0.35, 0, 0.05)
			ra.rotation = Vector3(0.30, 0, -0.05)
			head.rotation = Vector3(0.0, 0.08, 0)
			v.rotation = Vector3(0.04, 0, 0)
		"chase":
			ll.rotation = Vector3(0.75, 0, 0)
			rl.rotation = Vector3(-0.70, 0, 0)
			la.rotation = Vector3(-0.55, 0, 0.1)
			ra.rotation = Vector3(0.45, 0, -0.12)
			head.rotation = Vector3(0.12, 0, 0)
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
			key.light_energy = 1.15
			key.light_color = Color(1.0, 0.98, 0.94)
			fill.light_energy = 1.4
			rim.light_energy = 0.9
			warm.light_energy = 0.0
			cold.light_energy = 0.0
			we.environment.background_color = Color(0.55, 0.56, 0.58)
			we.environment.ambient_light_color = Color(0.72, 0.74, 0.76)
			we.environment.ambient_light_energy = 0.55
		"classroom":
			key.light_energy = 0.85
			key.light_color = Color(1.0, 0.95, 0.85)
			fill.light_energy = 0.6
			rim.light_energy = 0.35
			warm.light_energy = 2.2
			cold.light_energy = 0.0
			we.environment.background_color = Color(0.78, 0.74, 0.68)
			we.environment.ambient_light_color = Color(0.92, 0.88, 0.78)
			we.environment.ambient_light_energy = 0.5
		"corridor":
			key.light_energy = 0.25
			key.light_color = Color(0.65, 0.75, 0.90)
			fill.light_energy = 0.35
			rim.light_energy = 1.6
			warm.light_energy = 0.0
			cold.light_energy = 1.8
			we.environment.background_color = Color(0.14, 0.16, 0.20)
			we.environment.ambient_light_color = Color(0.30, 0.38, 0.48)
			we.environment.ambient_light_energy = 0.18
