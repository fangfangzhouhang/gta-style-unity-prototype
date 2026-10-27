extends SceneTree

const FailEvidence := preload("res://scripts/systems/fail_evidence.gd")
var game
var level
var passed := 0
var failed := 0
var lines: Array[String] = []

func _initialize() -> void:
	call_deferred("run")

func check(condition: bool, label: String) -> void:
	var message := ("PASS " if condition else "FAIL ") + label
	lines.append(message)
	print(message)
	if condition: passed += 1
	else: failed += 1

func frames(count: int) -> void:
	for i in count: await physics_frame

func run() -> void:
	root.size = Vector2i(1280, 800)
	game = root.get_node("Game")
	level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	await process_frame
	check(game.mode == "title" and paused, "Title pauses the world and releases mouse")
	game.start_new()
	var player = game.player
	var teacher = game.teacher
	teacher.frozen = true
	await frames(5)
	check(player.is_on_floor(), "Player starts on a physical classroom floor")
	var physics: PhysicsDirectSpaceState3D = player.get_world_3d().direct_space_state
	var ceiling := physics.intersect_ray(PhysicsRayQueryParameters3D.create(player.global_position + Vector3.UP * 1.6, player.global_position + Vector3.UP * 5, 1))
	check(not ceiling.is_empty(), "Classroom has a solid complete ceiling")
	player.teleport_to(Vector3(-9.4, 0.04, -5.4), PI/2)
	player.scripted_move = Vector2(0, -1)
	await frames(45)
	player.scripted_move = Vector2.ZERO
	check(player.global_position.x > -9.75, "Physical glass prevents escape through the classroom window")
	player.teleport_to(Vector3(2.7, 0.04, -4), 0)
	Input.action_press("crouch")
	check(player.set_crouch(true), "Player can crouch")
	var low := StaticBody3D.new()
	low.collision_layer = 1
	var shape := CollisionShape3D.new()
	var box := BoxShape3D.new()
	box.size = Vector3(1, 0.15, 1)
	shape.shape = box
	low.add_child(shape)
	level.add_child(low)
	low.global_position = Vector3(2.7, 1.19, -4)
	await frames(2)
	Input.action_release("crouch")
	await frames(2)
	check(not player.set_crouch(false), "Standing query rejects an obstructed full capsule")
	low.queue_free()
	await frames(2)
	check(player.set_crouch(false), "Standing is allowed after overhead obstruction is removed")
	player.teleport_to(Vector3(-1.7, 0.04, 1), 0)
	teacher.global_position = Vector3(-1.7, 0.04, -1)
	teacher.rotation.y = PI
	teacher.grace = 0
	await frames(2)
	check(teacher.can_see_player(), "Teacher detects unobstructed player in its actual field of view")
	player.teleport_to(Vector3(2.7, 0.04, 1), 0)
	teacher.rotation.y = -PI/2
	check(not teacher.can_see_player(), "Central service core blocks teacher vision through walls")
	teacher.state = "chase"
	teacher.last_seen = Vector3(-1.7,0,1)
	teacher.suspicion = 1
	teacher.frozen = false
	var remembered: Vector3 = teacher.last_seen
	await frames(20)
	check(teacher.last_seen.distance_to(remembered) < .01, "Occluded pursuit preserves last seen location instead of reading current player position")
	teacher.frozen = true
	teacher.state = "patrol"
	teacher.heard_id = ""
	teacher.frozen = false
	player.teleport_to(Vector3(-8.5,0.04,-5.4),0)
	game.acoustics.emit_sound(Vector3(-1.7,1,-4), "timer", 28, "测试铃声", "timer", "test_ring")
	check(teacher.state == "investigate", "Audible remote timer creates investigation")
	var initial: Vector3 = teacher.global_position
	await frames(90)
	check(teacher.global_position.distance_to(initial) > 0.5, "NavigationAgent moves teacher physically toward the investigated source")
	teacher.frozen = true
	teacher.state = "chase"
	teacher.frozen = false
	teacher.hear_noise(Vector3(3,0,-5), 20, "诱饵", "priority_test")
	check(teacher.state == "chase", "A lure does not cancel an active pursuit")
	teacher.frozen = true
	var door = level.get_node("Doors/Shortcut")
	var map: RID = player.get_world_3d().navigation_map
	var source := Vector3(-4.2,.1,-5.5)
	var destination := Vector3(-1.5,.1,-5.5)
	var before := NavigationServer3D.map_get_path(map, source, destination, true)
	check(before.size() > 1, "Initial route connects classroom and print room before shortcut unlock")
	player.teleport_to(source, -PI/2)
	door.interact(player)
	check(door.locked, "Shortcut cannot be unlocked from the classroom side")
	player.teleport_to(destination, PI/2)
	door.interact(player)
	await frames(35)
	check(not door.locked and door.is_open and game.progress.shortcut, "Back latch unlocks the physical door and persistent progress")
	var after := NavigationServer3D.map_get_path(map, source, destination, true)
	check(_length(after) < _length(before) - 3, "Shortcut changes navigation route length")
	check(game.checkpoint == "print", "Shortcut activates the next legal checkpoint")
	var closed_path = game.acoustics.audible_path(Vector3(-4,1,-5.5),Vector3(-2,1,-5.5))
	door.is_open = false
	var muffled_path = game.acoustics.audible_path(Vector3(-4,1,-5.5),Vector3(-2,1,-5.5))
	check(float(muffled_path.cost) > float(closed_path.cost) + 3 and muffled_path.muffled, "Closed door attenuates sound and hides exact source location")
	door.is_open = true
	game.timer.interact(player)
	game.timer.arm()
	check(game.timer.state == "countdown", "Timer begins its visible delayed countdown")
	game.timer.remaining = .01
	await frames(3)
	check(game.timer.state == "ringing", "Countdown transitions to a real ringing sound event")
	game.timer.reset_timer()
	var panel = level.get_node("Broadcast")
	panel.operate("mode")
	panel.operate("zone")
	panel.operate("stop")
	check(panel.output_mode == 1 and panel.zone == 1 and panel.stopped, "Broadcast controls independently change route, zone and repetition")
	game.gate.hold(2.05)
	await frames(35)
	check(game.gate.stage == 1 and game.progress.gate_stage == 1, "One completed ratchet stage is retained")
	check(absf(game.gate.get_node("GateBody").position.y - .25) < .01, "Gate collision body rises with its visible geometry")
	game.ui.open_reading("测试", "仅显示已发现的内容")
	check(paused and Input.mouse_mode == Input.MOUSE_MODE_VISIBLE, "Reading freezes the world and releases the pointer")
	var close := InputEventKey.new()
	close.physical_keycode = KEY_ESCAPE
	close.pressed = true
	root.push_input(close)
	await process_frame
	check(game.mode == "game", "Escape closes reading without immediately opening pause")
	game.set_mode("pause")
	var mouse := InputEventMouseButton.new()
	mouse.position = Vector2(250, 327)
	mouse.button_index = MOUSE_BUTTON_LEFT
	mouse.pressed = true
	root.push_input(mouse)
	await process_frame
	mouse = mouse.duplicate()
	mouse.pressed = false
	root.push_input(mouse)
	await process_frame
	check(game.mode == "game", "Actual viewport mouse click on Resume closes pause")
	game.test_mode = false
	check(game.save_progress(), "Save transaction writes isolated test profile")
	game.progress.gate_stage = 0
	game.facts.clear()
	check(game.load_progress() and game.gate.stage == 1 and game.progress.shortcut, "Load restores gate, shortcut and discovered facts at checkpoint")
	var save_path: String = game.SAVE_PATH
	var good_save := FileAccess.get_file_as_string(save_path)
	var good_data: Dictionary = JSON.parse_string(good_save)
	for entry in [["gate_stage", 1.5], ["broadcast_zone", 99], ["shortcut", "true"]]:
		var bad: Dictionary = good_data.duplicate(true)
		bad.progress[entry[0]] = entry[1]
		_write(save_path + ".invalid", JSON.stringify(bad))
		check(game._read_valid_save(save_path + ".invalid").is_empty(), "Malformed save rejects " + str(entry[0]))
	var bad_elapsed: Dictionary = good_data.duplicate(true)
	bad_elapsed.elapsed = "oops"
	_write(save_path + ".invalid", JSON.stringify(bad_elapsed))
	check(game._read_valid_save(save_path + ".invalid").is_empty(), "Malformed play time cannot crash load")
	_write(save_path + ".backup", good_save)
	_write(save_path, "{broken")
	check(game.load_progress() and game.gate.stage == 1 and game.progress.shortcut, "Corrupt primary save falls back to valid backup")
	_write(save_path + ".backup", "{also broken")
	check(not game.load_progress() and game.gate.stage == 1 and FileAccess.get_file_as_string(save_path) == "{broken", "Two corrupt saves leave files and running progress intact")
	_write(save_path, good_save)
	_write(save_path + ".backup", good_save)
	game.test_mode = true
	teacher.frozen = true
	game.caught("自动夹具测试")
	await create_timer(1.3, true).timeout
	check(game.mode == "game" and game.gate.stage == 1 and game.timer.owned, "Capture recovery retains critical progress and timer ownership")
	# --- Fail-evidence resolver (PLOT §2 / card §5 minimal slice) ---
	var fe_held := FailEvidence.resolve_primary({"timer_held": true, "timer_ringing": true, "chase_visible": true, "teacher_state": "chase"})
	check("手里" in fe_held, "FailEvidence: held ringing primary mentions hand")
	var fe_gate_rec := FailEvidence.resolve_recovery({"gate_stage": 1, "shortcut": false})
	check(fe_gate_rec == FailEvidence.RECOVERY_GATE and "档位" in fe_gate_rec, "FailEvidence: gate.stage>=1 recovery keeps stage line")
	var fe_no_shortcut := FailEvidence.resolve_recovery({"gate_stage": 0, "shortcut": false})
	check(fe_no_shortcut.is_empty(), "FailEvidence: shortcut false omits shortcut comfort")
	var fe_shortcut_only := FailEvidence.resolve_recovery({"gate_stage": 0, "shortcut": true})
	check(fe_shortcut_only == FailEvidence.RECOVERY_SHORTCUT, "FailEvidence: shortcut true alone shows near-path comfort")
	var fe_los := FailEvidence.resolve_primary({"teacher_state": "chase", "chase_visible": true})
	check(fe_los == FailEvidence.LINE_LOS_CORNER, "FailEvidence: chase catch maps to los_corner")
	var fe_fallback := FailEvidence.resolve_primary({})
	check(fe_fallback == FailEvidence.LINE_FALLBACK, "FailEvidence: empty context uses fallback")
	var fe_near_ring := FailEvidence.resolve_primary({"timer_held": false, "timer_ringing": true, "timer_near": true})
	check(fe_near_ring == FailEvidence.LINE_NEAR_RING, "FailEvidence: near ringing timer maps to 1.3")
	var fe_gate_chain := FailEvidence.resolve_primary({"gate_recent": true, "gate_near": true, "teacher_state": "chase"})
	check(fe_gate_chain == FailEvidence.LINE_GATE_CHAIN, "FailEvidence: recent gate work maps to 1.5")
	# Live catch: held ringing → recovery UI contains hand line; progress comfort uses gate stage
	teacher.frozen = true
	game.timer.owned = true
	game.timer.held = true
	game.timer.state = "ringing"
	game.timer.remaining = 4.0
	game.caught("held_ring_fixture")
	await create_timer(0.15, true).timeout
	var recovery_text: String = str(game.ui.get_node("Recovery/Label").text)
	check("手里" in recovery_text, "Live catch: held ringing recovery mentions hand")
	check("已升起的卷栅档位保留" in recovery_text, "Live catch: gate.stage>=1 second line is stage comfort")
	check("近路" not in recovery_text, "Live catch: with stage>=1 does not also print shortcut comfort")
	await create_timer(1.2, true).timeout
	check(game.mode == "game" and game.gate.stage == 1, "Held-ring catch recovery returns to game with stage kept")
	game.timer.state = "idle"
	game.timer.held = game.timer.owned
	player.global_position.y = -5
	await frames(3)
	check(player.global_position.y > -1, "Falling returns player to a legal checkpoint")
	# --- P1 greybox locks (before final gate exit; restore state so exit stays clean) ---
	var wedge = level.get_node("DoorWedge")
	var storage = level.get_node("Doors/Storage")
	var rack = level.get_node("PushableRack")
	var hatch_m = level.get_node("HatchM")
	var hatch_g = level.get_node("HatchG")
	check(is_instance_valid(wedge) and not wedge.owned and not wedge.held, "DoorWedge exists and was never picked up before exit path")
	# (2) Ajar Storage: capsule cannot squeeze through (DoorWedge API; stand on FloorS)
	wedge.held = true
	storage.apply_wedge(wedge)
	await frames(3)
	check(storage.wedged and absf(storage.hinge.rotation.y + storage.AJAR_ANGLE) < 0.02, "Storage door enters ajar via DoorWedge API")
	player.teleport_to(Vector3(-3.6, 0.04, 5.35), 0)
	await frames(2)
	var ajar_start_z: float = player.global_position.z
	player.scripted_move = Vector2(0, -1)
	await frames(70)
	player.scripted_move = Vector2.ZERO
	var ajar_z: float = player.global_position.z
	check(player.global_position.y > -0.5 and ajar_z > 4.25 and ajar_z < ajar_start_z + 0.05, "Ajar Storage door: player capsule cannot pass through")
	storage.clear_wedge(false)
	wedge.held = false
	wedge.restore_home()
	storage.reset_door()
	await frames(2)
	# (3) HatchM ↔ HatchG bidirectional after open (rack at limit required for HatchM)
	rack._set_travel(rack.travel_limit)
	await frames(2)
	check(rack.at_limit(), "PushableRack reaches travel limit exposing HatchM")
	hatch_m.set_open(true)
	await frames(40)
	check(hatch_m.is_open and hatch_g.is_open, "Opening HatchM also opens paired HatchG")
	player.teleport_to(Vector3(0.4, 0.04, -7.45), 0)
	await frames(2)
	player.scripted_move = Vector2(0, -1)
	var crossed_m_to_h: bool = false
	for _i in 45:
		await physics_frame
		if player.global_position.y < -0.5: break
		if player.global_position.z < -8.2:
			crossed_m_to_h = true
			break
	player.scripted_move = Vector2.ZERO
	check(crossed_m_to_h and player.global_position.y > -0.5, "HatchM open: player can pass M → H")
	player.teleport_to(Vector3(6.2, 0.04, -0.15), PI)
	await frames(2)
	player.scripted_move = Vector2(0, -1)
	var crossed_h_to_g: bool = false
	for _i in 45:
		await physics_frame
		if player.global_position.y < -0.5: break
		if player.global_position.z > 1.0:
			crossed_h_to_g = true
			break
	player.scripted_move = Vector2.ZERO
	check(crossed_h_to_g and player.global_position.y > -0.5, "HatchG open: player can pass H → G")
	player.teleport_to(Vector3(6.2, 0.04, 1.35), 0)
	await frames(2)
	player.scripted_move = Vector2(0, -1)
	var crossed_g_to_h: bool = false
	for _i in 45:
		await physics_frame
		if player.global_position.y < -0.5: break
		if player.global_position.z < 0.0:
			crossed_g_to_h = true
			break
	player.scripted_move = Vector2.ZERO
	check(crossed_g_to_h and player.global_position.y > -0.5, "HatchG open: player can pass G → H (no soft-lock)")
	player.teleport_to(Vector3(0.4, 0.04, -8.55), PI)
	await frames(2)
	player.scripted_move = Vector2(0, -1)
	var crossed_h_to_m: bool = false
	for _i in 45:
		await physics_frame
		if player.global_position.y < -0.5: break
		if player.global_position.z > -7.4:
			crossed_h_to_m = true
			break
	player.scripted_move = Vector2.ZERO
	check(crossed_h_to_m and player.global_position.y > -0.5, "HatchM open: player can pass H → M (bidirectional)")
	hatch_m.set_open(false)
	await frames(25)
	rack.reset_rack()
	player.teleport_to(Vector3(2.7, 0.04, -4), 0)
	await frames(2)
	check(not wedge.owned and not wedge.held, "P1 hatch/ajar path left DoorWedge unowned (not exit-required)")
	# (1) No-wedge exit: raise shutter and leave without ever calling wedge pick_up
	game.gate.hold(2.05)
	game.gate.hold(2.05)
	await frames(40)
	teacher.frozen = true
	player.teleport_to(Vector3(7,0.04,1.85),-PI/2)
	player.scripted_move = Vector2(0,-1)
	await frames(70)
	player.scripted_move = Vector2.ZERO
	check(game.mode == "ending" and not wedge.owned, "No-wedge exit: raise shutter and leave without picking up DoorWedge")
	print("RESULT passed=",passed," failed=",failed)
	lines.append("Fixture-based engine tests; not OS input or human play. passed=%d failed=%d" % [passed,failed])
	var report := FileAccess.open("res://evidence/logs/acceptance.txt",FileAccess.WRITE)
	report.store_string("\n".join(lines))
	report.close()
	quit(1 if failed else 0)

func _length(path: PackedVector3Array) -> float:
	var total := 0.0
	for i in range(1,path.size()): total += path[i].distance_to(path[i-1])
	return total

func _write(path: String, content: String) -> void:
	var file := FileAccess.open(path, FileAccess.WRITE)
	file.store_string(content)
	file.close()
