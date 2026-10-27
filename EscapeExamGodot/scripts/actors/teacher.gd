extends CharacterBody3D

@export var patrol_speed := 1.25
@export var chase_speed := 4.3
@export var sight_range := 9.0
@export var half_fov := 50.0
@export var notice_seconds := 1.05
@export var patrol_points: PackedVector3Array
@onready var nav: NavigationAgent3D = $NavigationAgent3D
var state := "patrol"
var suspicion := 0.0
var last_seen := Vector3.ZERO
var noise_target := Vector3.ZERO
var evidence := "门外的目击让老师确认了你的位置。"
var lost_time := 0.0
var state_time := 0.0
var dwell := 0.0
var patrol_index := 0
var grace := 4.0
var frozen := false
var heard_id := ""
var step_distance := 0.0
var repath := 0.0
var last_position := Vector3.ZERO
var anim_time := 0.0
var search_index := 0
var noise_priority := 0.0
var investigation_dwell := 0.0

func _ready() -> void:
	last_position = global_position
	nav.path_desired_distance = 0.25
	nav.target_desired_distance = 0.3
	# ark 2026-10-27: no floating alert icons; keep five-state suspicion logic only.
	if has_node("AlertIcon"):
		$AlertIcon.visible = false
	if has_node("SuspicionBar"):
		$SuspicionBar.visible = false

func reset_teacher() -> void:
	global_position = patrol_points[0]
	rotation.y = -PI / 2
	patrol_index = 1
	state = "patrol"
	suspicion = 0
	lost_time = 0
	state_time = 0
	grace = 4
	dwell = 1
	heard_id = ""
	noise_priority = 0
	investigation_dwell = 0
	velocity = Vector3.ZERO
	last_position = global_position
	repath = 0
	_update_alert_ui()

func can_see_player() -> bool:
	if not is_instance_valid(Game.player): return false
	var eye := global_position + Vector3.UP * 1.9
	var target_point: Vector3 = Game.player.global_position + Vector3.UP * (0.83 if Game.player.crouched else 1.36)
	var offset := target_point - eye
	if offset.length() > sight_range: return false
	if offset.length() > 1.2 and -global_basis.z.dot(offset.normalized()) < cos(deg_to_rad(half_fov)): return false
	var query := PhysicsRayQueryParameters3D.create(eye, target_point, 1 | 2 | 16)
	query.exclude = [get_rid()]
	var result := get_world_3d().direct_space_state.intersect_ray(query)
	return not result.is_empty() and result.collider == Game.player

func hear_noise(point: Vector3, strength: float, description: String, id: String) -> void:
	if frozen or grace > 0 or state == "chase" or strength < 0.5: return
	if heard_id == id:
		if state == "investigate": noise_target = point
		return
	if state == "suspicious" and suspicion > 0.3: return
	if state == "investigate" and strength < noise_priority * 0.7: return
	heard_id = id
	noise_priority = strength
	investigation_dwell = 0
	noise_target = point
	evidence = description + "让老师来到了这里。"
	change_state("investigate")
	dwell = 0.45
	Game.trace("heard", {"kind": description, "target": [point.x, point.z], "strength": snappedf(strength, 0.1)})

func change_state(next: String) -> void:
	if state == next: return
	Game.trace("teacher_state", {"from": state, "to": next})
	state = next
	state_time = 0
	repath = 0
	if next == "suspicious":
		Game.acoustics.emit_sound(global_position + Vector3.UP, "notice", 12, "老师停下了脚步", "teacher")
	elif next == "chase":
		evidence = "老师在无遮挡处确认了你的位置。转角和关门能切断视线。"
		Game.acoustics.emit_sound(global_position + Vector3.UP, "notice", 22, "急促的脚步追来", "teacher")
	elif next == "search":
		search_index = 0
		Game.acoustics.emit_sound(global_position, "teacher_step", 10, "脚步放慢，开始寻找", "teacher")
	_update_alert_ui()

func _physics_process(delta: float) -> void:
	if Game.mode != "game" or frozen: return
	grace = maxf(0, grace - delta)
	state_time += delta
	var visible := grace <= 0 and can_see_player()
	if visible:
		last_seen = Game.player.global_position
		lost_time = 0
		suspicion = minf(1, suspicion + delta / notice_seconds)
		if state != "chase" and suspicion < 1: change_state("suspicious")
		if suspicion >= 1: change_state("chase")
	else:
		lost_time += delta
		suspicion = maxf(0, suspicion - delta * 0.7)
		if state == "suspicious" and suspicion <= 0: change_state("investigate")
		if state == "chase" and lost_time > 2:
			change_state("search")
	if state == "suspicious": noise_target = last_seen
	var destination := patrol_points[patrol_index]
	var speed := patrol_speed
	match state:
		"suspicious":
			destination = last_seen
			speed = 0
		"investigate":
			destination = noise_target
			speed = 1.7
			if _flat_distance(destination) < 0.55:
				speed = 0
				investigation_dwell += delta
				if investigation_dwell > 4: change_state("return")
			if state_time > 24: change_state("return")
		"chase":
			destination = last_seen
			speed = chase_speed
		"search":
			destination = last_seen
			if _flat_distance(last_seen) < 0.6 or search_index > 0:
				search_index = 1
				var candidates := patrol_points
				var nearest := last_seen
				var distance := INF
				for candidate in candidates:
					var d := candidate.distance_to(last_seen)
					if d > 0.7 and d < distance:
						distance = d
						nearest = candidate
				destination = nearest
			if state_time > 8: change_state("return")
		"return":
			var nearest_distance := INF
			for i in patrol_points.size():
				var distance := _flat_distance(patrol_points[i])
				if distance < nearest_distance:
					nearest_distance = distance
					patrol_index = i
			destination = patrol_points[patrol_index]
			if nearest_distance < 0.4:
				patrol_index = (patrol_index + 1) % patrol_points.size()
				change_state("patrol")
		"patrol":
			if _flat_distance(destination) < 0.4:
				patrol_index = (patrol_index + 1) % patrol_points.size()
				dwell = 0.8
	if visible and state == "chase" and _flat_distance(Game.player.global_position) < 0.72:
		Game.caught(evidence if not evidence.is_empty() else "老师在无遮挡处看见了你。")
		return
	dwell = maxf(0, dwell - delta)
	if dwell > 0: speed = 0
	if NavigationServer3D.map_get_iteration_id(get_world_3d().navigation_map) == 0: return
	repath -= delta
	if repath <= 0:
		nav.target_position = destination
		repath = 0.25
	var next := nav.get_next_path_position()
	var direction := next - global_position
	direction.y = 0
	if state == "suspicious": direction = destination - global_position
	if direction.length() > 0.07:
		direction = direction.normalized()
		rotation.y = lerp_angle(rotation.y, atan2(-direction.x, -direction.z), minf(1, delta * 5))
	else:
		speed = 0
	for door in get_tree().get_nodes_in_group("doors"):
		var needs_open: bool = (not door.is_open) or bool(door.get("wedged"))
		if not door.locked and needs_open and global_position.distance_to(door.global_position) < 1.25:
			if door.request_teacher_open(delta): speed = 0
	velocity.x = direction.x * speed
	velocity.z = direction.z * speed
	velocity.y = -0.4 if is_on_floor() else velocity.y - 18 * delta
	move_and_slide()
	var travelled := Vector2(global_position.x - last_position.x, global_position.z - last_position.z).length()
	step_distance += travelled
	last_position = global_position
	# Audio-only cadence: chase steps a bit tighter; soft vs hard by state (no AI/speed change).
	var step_span := 0.85 if state == "chase" else 1.0
	if step_distance > step_span:
		step_distance = 0
		var step_kind := "teacher_step" if state == "chase" else "teacher_step_soft"
		var step_reach := 20.0 if state == "chase" else 15.0
		Game.acoustics.emit_sound(global_position, step_kind, step_reach, "老师的脚步", "teacher")
	anim_time += delta * (9 if state == "chase" else 4)
	var moving := travelled > 0.002
	$Visual/LeftLeg.rotation.x = sin(anim_time) * 0.30 if moving else 0
	$Visual/RightLeg.rotation.x = -$Visual/LeftLeg.rotation.x
	$Visual/LeftArm.rotation.x = -$Visual/LeftLeg.rotation.x * 0.8
	$Visual/RightArm.rotation.x = $Visual/LeftLeg.rotation.x * 0.6
	$Visual/Head.rotation.y = sin(state_time * 1.6) * 0.3 if state in ["search", "investigate"] and speed == 0 else 0
	_update_alert_ui()

func _update_alert_ui() -> void:
# Intentionally blank: alert billboards stay hidden; detection state machine unchanged.
	if has_node("AlertIcon"):
		$AlertIcon.visible = false
		$AlertIcon.text = ""
	if has_node("SuspicionBar"):
		$SuspicionBar.visible = false
		$SuspicionBar.text = ""

func _flat_distance(point: Vector3) -> float:
	return Vector2(global_position.x - point.x, global_position.z - point.z).length()
