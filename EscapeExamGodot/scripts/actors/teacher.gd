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

# --- V2 three-layer brain (Design Bible §3) -------------------------------
# Body (this script) only ever acts on its own senses: the vision raycast,
# heard points delivered by acoustics, and its own memory (last_seen /
# noise_target). Belief turns those observations into a room distribution;
# Director schedules pressure. The ONLY place in the teacher stack that
# reads the player's real position is director.gd (scheduler contract:
# path-length measurement + room-granularity hints, never coordinates).
const BeliefScene := preload("res://scripts/systems/belief.gd")
const DirectorScene := preload("res://scripts/systems/director.gd")
const ProfileScene := preload("res://scripts/systems/player_profile.gd")
var belief: Node = null # belief.gd — believed room distribution
var director: Node = null # director.gd — menace/tension scheduler
var profile: Node = null # player_profile.gd — cross-run adaptation stats
var tension := 0.0 # mirrored from director every tick (audio/visual axis)
var visible_time := 0.0 # continuous seconds the player has been in sight
var _sighting_feed := 0.0 # throttle for belief sighting updates
var _absence_feed := 0.0 # throttle for belief absence erosion
var _encounter_recorded := false # one profile sighting per encounter
var search_queue: Array[Vector3] = [] # belief-planned search destinations
var search_rooms: Array[String] = [] # parallel room names for search_queue
var search_wait := 0.0 # dwell timer while checking a search destination
var search_item_time := 0.0 # per-destination budget; unreachable centres are skipped
var _countermeasure_used := false # profile countermeasure: once per search
var detour_point := Vector3.ZERO # Director hint detour (patrol only)
var has_detour := false
var retreat_active := false # Director retreat: head to farthest patrol point
var retreat_index := -1 # locked retreat target patrol index
var patrol_flavor := "" # current patrol flavour: papers / clock / mark / ""

func _ready() -> void:
	last_position = global_position
	nav.path_desired_distance = 0.25
	nav.target_desired_distance = 0.3
	# Self-registration (Bible §6): subsystems are code-created children so
	# no scene file or Autoload needs editing for the V2 brain.
	belief = BeliefScene.new()
	belief.name = "Belief"
	add_child(belief)
	director = DirectorScene.new()
	director.name = "Director"
	add_child(director)
	profile = ProfileScene.new()
	profile.name = "PlayerProfile"
	add_child(profile)
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
	# V2 brain reset: belief forgets, director pressure drains, and the
	# cross-run profile applies its (single) countermeasure for this run.
	visible_time = 0
	search_queue.clear()
	search_rooms.clear()
	search_wait = 0
	search_item_time = 0
	_countermeasure_used = false
	has_detour = false
	retreat_active = false
	retreat_index = -1
	patrol_flavor = ""
	_encounter_recorded = false
	if belief != null and belief.has_method("ensure_graph") and belief.ensure_graph():
		belief.reset()
	if director != null and director.has_method("reset_director"):
		director.reset_director()
	if profile != null and profile.has_method("note_reset"):
		profile.note_reset()
		var cm: String = profile.current_countermeasure()
		if not cm.is_empty():
			Game.trace("profile_applied", {"countermeasure_room": cm, "dwell_bias": snappedf(profile.dwell_bias(cm), 0.01), "timer_investigations": profile.timer_investigations})
	_update_alert_ui()

func can_see_player() -> bool:
	if not is_instance_valid(Game.player): return false
	var eye := global_position + Vector3.UP * 1.9
	var target_point: Vector3 = Game.player.global_position + Vector3.UP * (0.83 if Game.player.crouched else 1.36)
	# 集成（v2-integration）：探头时视线目标点随玩家头部横移（情节域 lean 接口）。
	if "lean_current" in Game.player:
		target_point += Game.player.global_basis.x * float(Game.player.get("lean_current")) * 0.35
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
	# Belief hears what the body heard: the delivered point (already the
	# muffled doorway point when the sound crossed a closed door) becomes a
	# room observation scaled by how much strength survived the path.
	if belief != null and is_instance_valid(Game.acoustics):
		belief.observe_noise(Game.acoustics.room_at(point), clampf(strength / 12.0, 0.25, 0.85))
	if profile != null and (description.contains("计时器") or id.begins_with("ring_")):
		profile.record_timer_investigation()
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
		_build_search_plan()
		Game.acoustics.emit_sound(global_position, "teacher_step", 10, "脚步放慢，开始寻找", "teacher")
	_update_alert_ui()

## Search is planned from Belief, not improvised: last_seen first, then up
## to 3 rooms ranked by information gain, with the profile countermeasure
## room (if any) checked first — once per search.
func _build_search_plan() -> void:
	search_queue.clear()
	search_rooms.clear()
	search_wait = 0
	search_item_time = 0
	search_queue.append(last_seen)
	search_rooms.append("")
	if belief == null or not is_instance_valid(Game.acoustics):
		return
	var cm := ""
	if profile != null:
		cm = profile.current_countermeasure()
	if not cm.is_empty() and not _countermeasure_used:
		_countermeasure_used = true
		search_queue.append(belief.room_center(cm))
		search_rooms.append(cm)
		Game.trace("countermeasure", {"room": cm, "use": "first_check_room"})
	var last_room: String = Game.acoustics.room_at(last_seen)
	for room in belief.best_search_rooms(3, global_position):
		if room == last_room or room == cm:
			continue
		search_queue.append(belief.room_center(room))
		search_rooms.append(room)

## Continuous exposure rate for the vision sense: the old binary model
## charged suspicion at a flat rate. Now the charge scales with how well
## the teacher can actually see the player — distance falloff, angle from
## the view axis, and the player's posture/speed (crouch 0.55, perfectly
## still lower still, sprinting 1.3). All inputs come from the teacher's
## own sighting geometry in this frame, not from a global query.
func _exposure_rate() -> float:
	if not is_instance_valid(Game.player):
		return 1.0
	var eye := global_position + Vector3.UP * 1.9
	var target_point: Vector3 = Game.player.global_position + Vector3.UP * (0.83 if Game.player.crouched else 1.36)
	var offset := target_point - eye
	var dist := offset.length()
	var distance_factor := clampf(3.4 / (dist + 1.4), 0.5, 1.6)
	var cos_angle := 1.0
	if dist > 1.2:
		cos_angle = -global_basis.z.dot(offset.normalized())
	var angle_factor := clampf(lerpf(0.55, 1.25, inverse_lerp(cos(deg_to_rad(half_fov)), 1.0, cos_angle)), 0.55, 1.25)
	var flat_speed := Vector2(Game.player.velocity.x, Game.player.velocity.z).length()
	var posture_factor := 1.0
	if Game.player.crouched:
		posture_factor = 0.55
	elif flat_speed > 3.4:
		posture_factor = 1.3
	elif flat_speed < 0.2:
		posture_factor = 0.45
	# 集成（v2-integration）：探头只露头，警觉积累按探出比例打折（满探=1.0，不探不打折）。
	if Game.player.has_method("lean_exposure"):
		var lean_exposure := float(Game.player.lean_exposure())
		if lean_exposure > 0.05:
			posture_factor *= 0.45 + 0.55 * lean_exposure
	return clampf(distance_factor * angle_factor * posture_factor, 0.15, 2.2)

func _physics_process(delta: float) -> void:
	if Game.mode != "game" or frozen: return
	grace = maxf(0, grace - delta)
	state_time += delta
	var visible := grace <= 0 and can_see_player()
	if visible:
		last_seen = Game.player.global_position
		lost_time = 0
		visible_time += delta
		suspicion = minf(1, suspicion + delta / notice_seconds * _exposure_rate())
		if state != "chase" and suspicion < 1: change_state("suspicious")
		if suspicion >= 1: change_state("chase")
		# Senses feed Belief (throttled): the sighting is the teacher's own
		# raycast result, converted to a room via its own position memory.
		_sighting_feed += delta
		if belief != null and _sighting_feed >= 0.5 and is_instance_valid(Game.acoustics):
			_sighting_feed = 0.0
			belief.observe_sighting(Game.acoustics.room_at(last_seen))
		if profile != null and not _encounter_recorded and state in ["suspicious", "chase"] and is_instance_valid(Game.acoustics):
			_encounter_recorded = true
			profile.record_sighting(Game.acoustics.room_at(last_seen))
	else:
		visible_time = 0.0
		if state not in ["suspicious", "chase"]:
			_encounter_recorded = false
		lost_time += delta
		suspicion = maxf(0, suspicion - delta * 0.7)
		if state == "suspicious" and suspicion <= 0: change_state("investigate")
		if state == "chase" and lost_time > 2:
			change_state("search")
		# Standing in a room without seeing the player is also evidence:
		# erode belief here so search planning moves on honestly.
		_absence_feed += delta
		if belief != null and _absence_feed >= 1.5 and is_instance_valid(Game.acoustics):
			_absence_feed = 0.0
			belief.observe_absence(Game.acoustics.room_at(global_position), 0.18)
	if state == "suspicious": noise_target = last_seen
	# --- V2 brain tick: belief diffusion + director scheduling -------------
	if belief != null:
		belief.diffuse(delta)
	if director != null:
		var orders: Dictionary = director.tick(delta, {"teacher_pos": global_position, "state": state, "visible": visible, "suspicion": suspicion})
		tension = float(director.tension)
		var hint_room := String(orders.get("hint_room", ""))
		if not hint_room.is_empty():
			# Director hint arrives as an uncertain room observation, and —
			# only while patrolling — as a single detour leg. Nothing more.
			if belief != null:
				belief.observe_noise(hint_room, 0.5)
			if state in ["patrol", "return"] and belief != null:
				detour_point = belief.room_center(hint_room)
				has_detour = true
		if bool(orders.get("retreat", false)) and state in ["search", "investigate", "suspicious"]:
			retreat_active = true
			# Lock the farthest patrol point NOW; recomputing "farthest"
			# every frame would flip the target as the teacher moves.
			retreat_index = -1
			var farthest := -1.0
			for i in patrol_points.size():
				var d := _flat_distance(patrol_points[i])
				if d > farthest:
					farthest = d
					retreat_index = i
			change_state("return")
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
			# Interception: while the player is *continuously* visible the
			# aim point leads the player's own velocity by up to 0.35 s.
			# The moment sight breaks, visible_time is 0 and the destination
			# falls back to the remembered last_seen — no tracking through
			# walls, ever (acceptance pins this).
			destination = last_seen
			if visible and visible_time > 0.0 and is_instance_valid(Game.player):
				var lead := minf(visible_time, 0.35)
				destination = last_seen + Vector3(Game.player.velocity.x, 0, Game.player.velocity.z) * lead
			speed = chase_speed
		"search":
			# Walk the belief-planned queue: last_seen, then ranked rooms.
			if search_queue.is_empty():
				change_state("return")
			else:
				destination = search_queue[0]
				search_item_time += delta
				if _flat_distance(destination) < 0.6:
					speed = 0
					search_wait += delta
					if search_wait > 1.2:
						var done_room: String = search_rooms[0]
						search_queue.pop_front()
						search_rooms.pop_front()
						search_wait = 0
						search_item_time = 0
						search_index += 1
						if belief != null and not done_room.is_empty():
							belief.mark_checked(done_room)
				elif search_item_time > 6.0:
					# Centre not reachable (nav island / blocked): skip it
					# instead of stalling the whole search.
					search_queue.pop_front()
					search_rooms.pop_front()
					search_item_time = 0
				else:
					speed = 1.7
			if state_time > 24: change_state("return")
		"return":
			var nearest_distance := INF
			for i in patrol_points.size():
				var distance := _flat_distance(patrol_points[i])
				if distance < nearest_distance:
					nearest_distance = distance
					patrol_index = i
			if retreat_active and retreat_index >= 0:
				# Director retreat order: rejoin patrol at the locked farthest
				# point instead of the nearest, buying the player real space.
				patrol_index = retreat_index
				nearest_distance = _flat_distance(patrol_points[patrol_index])
			destination = patrol_points[patrol_index]
			if nearest_distance < 0.4:
				retreat_active = false
				retreat_index = -1
				patrol_index = (patrol_index + 1) % patrol_points.size()
				change_state("patrol")
		"patrol":
			if has_detour:
				destination = detour_point
				if _flat_distance(destination) < 0.6:
					has_detour = false
					dwell = 0.8
			elif _flat_distance(destination) < 0.4:
				patrol_index = (patrol_index + 1) % patrol_points.size()
				_start_patrol_flavor(destination)
	if visible and state == "chase" and _flat_distance(Game.player.global_position) < 0.72:
		# The catch is witnessed by the teacher itself; the profile records
		# the room of its own last_seen memory, not the player's live data.
		if profile != null and is_instance_valid(Game.acoustics):
			profile.record_caught(Game.acoustics.room_at(last_seen))
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
		var needs_open: bool = (not door.is_open) or (door.get("wedged") == true)
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
	if state == "patrol" and dwell > 0.0 and patrol_flavor == "clock":
		# "Looking at the corridor clock": slow, wide head sweeps while dwelling.
		$Visual/Head.rotation.y = sin(state_time * 0.7) * 0.5
	elif state == "patrol" and dwell > 0.0 and patrol_flavor == "papers":
		# "Straightening the paper stack": head bowed, almost still.
		$Visual/Head.rotation.y = sin(state_time * 0.4) * 0.08
	_update_alert_ui()

## Patrol is a job, not a conveyor belt (Bible §3): on reaching a patrol
## point the teacher sometimes *does* something — straightens papers,
## checks the clock, marks the door frame with the red pen. Expressed with
## dwell time + head motion only (no new animation assets in this layer).
## Hot rooms from the cross-run profile stretch the dwell (capped), so a
## returning player literally sees him linger where they used to slip by.
func _start_patrol_flavor(point: Vector3) -> void:
	patrol_flavor = ""
	var room := ""
	if is_instance_valid(Game.acoustics):
		room = Game.acoustics.room_at(point)
	var bias := 1.0
	if profile != null and not room.is_empty():
		bias = profile.dwell_bias(room)
	var roll := randf()
	if roll < 0.22:
		patrol_flavor = "papers"
		dwell = 2.2 * bias
	elif roll < 0.40:
		patrol_flavor = "clock"
		dwell = 1.6 * bias
	elif roll < 0.55:
		patrol_flavor = "mark" # red-pen tick on the frame: the V2 signature
		dwell = 1.8 * bias
	else:
		dwell = 0.8 * bias
	if not patrol_flavor.is_empty():
		Game.trace("patrol_flavor", {"kind": patrol_flavor, "room": room, "dwell": snappedf(dwell, 0.1)})

func _update_alert_ui() -> void:
# Intentionally blank: alert billboards stay hidden; detection state machine unchanged.
	if has_node("AlertIcon"):
		$AlertIcon.visible = false
		$AlertIcon.text = ""
	if has_node("SuspicionBar"):
		$SuspicionBar.visible = false
		$SuspicionBar.text = ""

## Shared pressure axis for the audio/visual agents (mirrors Director).
func get_tension() -> float:
	return tension

func _flat_distance(point: Vector3) -> float:
	return Vector2(global_position.x - point.x, global_position.z - point.z).length()
