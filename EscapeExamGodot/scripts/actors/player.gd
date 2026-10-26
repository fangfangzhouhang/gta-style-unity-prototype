extends CharacterBody3D

@export var walk_speed := 2.6
@export var run_speed := 4.5
@export var crouch_speed := 1.3
@export var standing_height := 1.75
@export var crouch_height := 1.05
@onready var camera: Camera3D = $Head/Camera3D
@onready var body_shape: CollisionShape3D = $CollisionShape3D
var crouched := false
var target
var step_distance := 0.0
var last_position := Vector3.ZERO
var scripted_move := Vector2.ZERO
var scripted_sprint := false
var input_locked := false

func _ready() -> void:
	body_shape.shape = body_shape.shape.duplicate()
	last_position = global_position

func _unhandled_input(event: InputEvent) -> void:
	if Game.mode != "game" or input_locked: return
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		rotation.y -= event.relative.x * Game.sensitivity
		$Head.rotation.x = clampf($Head.rotation.x - event.relative.y * Game.sensitivity, -1.35, 1.35)
	elif event.is_action_pressed("interact") and is_instance_valid(target):
		target.interact(self)
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("arm"):
		Game.timer.arm()
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("place") and Game.timer.held:
		Game.timer.place_from_camera()
		get_viewport().set_input_as_handled()

func _physics_process(delta: float) -> void:
	if Game.mode != "game" or input_locked: return
	set_crouch(Input.is_action_pressed("crouch"))
	var stick := Input.get_vector("left", "right", "forward", "back") + scripted_move
	stick = stick.limit_length()
	var direction := global_basis * Vector3(stick.x, 0, stick.y)
	var running := (Input.is_action_pressed("sprint") or scripted_sprint) and not crouched
	var speed := crouch_speed if crouched else (run_speed if running else walk_speed)
	velocity.x = move_toward(velocity.x, direction.x * speed, delta * 20)
	velocity.z = move_toward(velocity.z, direction.z * speed, delta * 20)
	velocity.y = -0.4 if is_on_floor() else velocity.y - 18 * delta
	move_and_slide()
	var travelled := Vector2(global_position.x - last_position.x, global_position.z - last_position.z).length()
	step_distance += travelled
	last_position = global_position
	if step_distance > (1.45 if running else 1.1):
		step_distance = 0
		Game.acoustics.emit_sound(global_position, "player_step", 2.2 if crouched else (11.0 if running else 4.5), "你的脚步", "player")
	$Head.position.y = move_toward($Head.position.y, 0.94 if crouched else 1.62, delta * 4)
	update_target()
	if global_position.y < -3: Game.fall_recover()
	if global_position.x > 8.8 and global_position.z > 0.5 and global_position.z < 3.4:
		Game.finish()

func set_crouch(wanted: bool) -> bool:
	if wanted == crouched: return true
	if not wanted:
		var query := PhysicsShapeQueryParameters3D.new()
		var standing := CapsuleShape3D.new()
		standing.radius = 0.26
		standing.height = standing_height
		query.shape = standing
		query.transform = Transform3D(Basis.IDENTITY, global_position + Vector3.UP * (standing_height / 2.0 + 0.02))
		query.collision_mask = 1 | 16 | 32
		query.exclude = [get_rid()]
		if not get_world_3d().direct_space_state.intersect_shape(query, 1).is_empty(): return false
	crouched = wanted
	var height := crouch_height if crouched else standing_height
	body_shape.shape.height = height
	body_shape.position.y = height / 2.0
	return true

func update_target() -> void:
	target = null
	var from := camera.global_position
	var query := PhysicsRayQueryParameters3D.create(from, from - camera.global_basis.z * 2.2, 1 | 8 | 16 | 32)
	query.exclude = [get_rid()]
	var hit := get_world_3d().direct_space_state.intersect_ray(query)
	if not hit.is_empty():
		var node = hit.collider
		while node != null and node != Game.world:
			if node.has_method("interact"):
				target = node
				break
			node = node.get_parent()
	Game.ui.set_prompt(target.prompt(self) if is_instance_valid(target) else "")
	if is_instance_valid(target) and target.has_method("hold") and Input.is_action_pressed("interact"):
		target.hold(get_physics_process_delta_time())

func teleport_to(point: Vector3, yaw: float) -> void:
	global_position = point
	rotation.y = yaw
	$Head.rotation.x = 0
	velocity = Vector3.ZERO
	last_position = point
	step_distance = 0
	set_crouch(false)

func look_at_point(point: Vector3) -> void:
	var direction := point - camera.global_position
	rotation.y = atan2(-direction.x, -direction.z)
	$Head.rotation.x = atan2(direction.y, Vector2(direction.x, direction.z).length())
