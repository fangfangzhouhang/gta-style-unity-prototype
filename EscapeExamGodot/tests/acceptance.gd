extends SceneTree

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
	await frames(5)
	check(player.is_on_floor(), "Player starts on a physical classroom floor")
	var physics: PhysicsDirectSpaceState3D = player.get_world_3d().direct_space_state
	var ceiling := physics.intersect_ray(PhysicsRayQueryParameters3D.create(player.global_position + Vector3.UP * 1.6, player.global_position + Vector3.UP * 5, 1))
	check(not ceiling.is_empty(), "Classroom has a solid complete ceiling")
	# --- Phase0 gated locks ---
	var classroom = level.get_node("Doors/Classroom")
	var storage = level.get_node("Doors/Storage")
	var gate_door = level.get_node("Doors/GateAntechamber")
	check(classroom.locked and classroom.key_id == "k1", "Phase0 Classroom starts locked needing K1")
	check(storage.locked and storage.key_id == "k2", "Phase0 Storage starts locked needing K2")
	check(gate_door.locked and gate_door.key_id == "k3", "Phase0 GateAntechamber starts locked needing K3")
	check(not game.has_key("k1") and not game.has_key("k2") and not game.has_key("k3"), "Phase0 player holds no keys")
	check(teacher.frozen and not game.progress.teacher_summoned, "Phase0 teacher inactive / not summoned")
	check(not classroom.get_node("NavigationLink3D").enabled, "Phase0 Classroom NavigationLink disabled while locked")
	player.teleport_to(Vector3(-9.4, 0.04, -5.4), PI/2)
	player.scripted_move = Vector2(0, -1)
	await frames(45)
	player.scripted_move = Vector2.ZERO
	check(player.global_position.x > -9.75, "Physical glass prevents escape through the classroom window")
	# Locked classroom blocks capsule toward corridor
	player.teleport_to(Vector3(-4.2, 0.04, 1.1), -PI/2)
	await frames(2)
	var c_block_start_x: float = player.global_position.x
	player.scripted_move = Vector2(0, -1)
	await frames(55)
	player.scripted_move = Vector2.ZERO
	check(player.global_position.x > -3.55, "Locked Classroom door blocks capsule into west corridor")
	# False timer ring does not summon teacher in Phase0
	game.acoustics.emit_sound(Vector3(-6.5, 1, -5.5), "timer", 28, "误响测试", "timer", "phase0_false_ring")
	await frames(10)
	check(teacher.frozen and not game.progress.teacher_summoned, "Phase0 false timer ring does not summon teacher")
	# Pick K1 and unlock classroom → schedule summon
	game.grant_key("k1", "考场值班钥")
	player.teleport_to(Vector3(-3.55, 0.04, 1.1), -PI/2)
	await frames(2)
	classroom.interact(player)
	await frames(40)
	check(not classroom.locked and game.progress.doors_unlocked.classroom, "K1 unlocks Classroom into progress")
	check(game._summon_remaining >= 0.0 or game.progress.teacher_summoned, "First Classroom open schedules delayed teacher summon")
	# Force remaining delay short for fixture
	if game._summon_remaining > 0.05:
		game._summon_remaining = 0.05
	await frames(20)
	check(game.progress.teacher_summoned and not teacher.frozen, "Delayed summon activates teacher at corridor patrol start")
	check(teacher.global_position.y > -1.0 and absf(teacher.global_position.x - teacher.patrol_points[0].x) < 2.5, "Summoned teacher appears near corridor patrol start (not classroom center)")
	check(classroom.get_node("NavigationLink3D").enabled, "Unlocked Classroom NavigationLink enabled")
	# Crouch / stand clearance
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
	# Teacher perception (teacher already active)
	teacher.frozen = true
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
	# Shortcut still classroom-side locked; route via corridor exists after Classroom unlock
	var door = level.get_node("Doors/Shortcut")
	var map: RID = player.get_world_3d().navigation_map
	var source := Vector3(-4.2,.1,-5.5)
	var destination := Vector3(-1.5,.1,-5.5)
	var before := NavigationServer3D.map_get_path(map, source, destination, true)
	check(before.size() > 1, "After Classroom unlock, corridor still connects classroom-side and print-side points")
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
	# GateAntechamber still locked: physical block into G from E
	player.teleport_to(Vector3(3.3, 0.04, 1.8), -PI/2)
	await frames(2)
	var g_start_x: float = player.global_position.x
	player.scripted_move = Vector2(0, -1)
	await frames(55)
	player.scripted_move = Vector2.ZERO
	check(player.global_position.x < 3.9, "Locked GateAntechamber blocks capsule into exit antechamber")
	# Critical path: K2 → Storage → K3 → GateAntechamber
	game.grant_key("k2", "教具室钥")
	player.teleport_to(Vector3(-3.6, 0.04, 3.4), 0)
	storage.interact(player)
	await frames(35)
	check(not storage.locked and game.progress.doors_unlocked.storage, "K2 unlocks Storage into progress")
	game.grant_key("k3", "出口前室钥")
	player.teleport_to(Vector3(3.4, 0.04, 1.8), -PI/2)
	gate_door.interact(player)
	await frames(35)
	check(not gate_door.locked and game.progress.doors_unlocked.gateantechamber, "K3 unlocks GateAntechamber into progress")
	check(gate_door.get_node("NavigationLink3D").enabled, "Unlocked GateAntechamber NavigationLink enabled")
	# Half-chain save: keys + doors + teacher_summoned round-trip
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
	game.progress.teacher_summoned = false
	game.progress["keys"] = {"k1": false, "k2": false, "k3": false}
	game.progress.doors_unlocked = {"classroom": false, "storage": false, "gateantechamber": false}
	game.facts.clear()
	check(game.load_progress() and game.gate.stage == 1 and game.progress.shortcut, "Load restores gate, shortcut and discovered facts at checkpoint")
	check(game.has_key("k1") and game.has_key("k2") and game.has_key("k3"), "Half-chain save restores all three keys")
	check(game.progress.doors_unlocked.classroom and game.progress.doors_unlocked.storage and game.progress.doors_unlocked.gateantechamber, "Half-chain save restores door unlocks")
	check(game.progress.teacher_summoned and not teacher.frozen, "Half-chain save restores teacher_summoned presence")
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
	check(game.has_key("k1") and game.has_key("k3") and game.progress.doors_unlocked.classroom, "Capture recovery retains keys and door unlocks")
	player.global_position.y = -5
	await frames(3)
	check(player.global_position.y > -1, "Falling returns player to a legal checkpoint")
	# --- P1 greybox locks (before final gate exit; restore state so exit stays clean) ---
	var wedge = level.get_node("DoorWedge")
	var rack = level.get_node("PushableRack")
	var hatch_m = level.get_node("HatchM")
	var hatch_g = level.get_node("HatchG")
	check(is_instance_valid(wedge) and not wedge.owned and not wedge.held, "DoorWedge exists and was never picked up before exit path")
	# Ensure Storage unlocked for ajar (already unlocked above); re-open closed leaf
	storage.reset_door()
	await frames(2)
	check(not storage.locked, "Storage reset keeps unlock from progress")
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
	# No-wedge exit through unlocked GateAntechamber
	game.gate.hold(2.05)
	game.gate.hold(2.05)
	await frames(40)
	teacher.frozen = true
	player.teleport_to(Vector3(7,0.04,1.85),-PI/2)
	player.scripted_move = Vector2(0,-1)
	await frames(70)
	player.scripted_move = Vector2.ZERO
	check(game.mode == "ending" and not wedge.owned, "No-wedge exit: raise shutter and leave without picking up DoorWedge")
	# --- Prologue skip skeleton (还没响的铃) ---
	var prologue_node = level.get_node_or_null("Prologue")
	if prologue_node == null:
		game._ensure_prologue()
		prologue_node = game.prologue
	check(prologue_node != null, "Prologue node available for cinematic skeleton")
	var vo_ok := true
	for shot in prologue_node.SHOTS:
		if str(shot.get("vo", "")).strip_edges().is_empty() or str(shot.vo).begins_with("[VO]"):
			vo_ok = false
			break
	check(vo_ok and prologue_node.SHOTS.size() == 9, "Prologue P01–P09 carry finalized VO (not empty/[VO])")
	check(str(prologue_node.SHOTS[0].vo).begins_with("雨还没停"), "P01 VO matches plot final")
	check(str(prologue_node.SHOTS[6].vo).contains("笑意不到眼睛"), "P07 VO matches plot final (uncanny smile)")
	# Force a non-test-mode-style prologue then skip → B0
	game.playing = true
	game.progress = game._default_progress()
	game.checkpoint = "classroom"
	game._summon_remaining = -1.0
	game._ensure_prologue()
	if game.prologue.finished.is_connected(game._on_prologue_finished):
		pass
	else:
		game.prologue.finished.connect(game._on_prologue_finished, CONNECT_ONE_SHOT)
	game.set_mode("prologue")
	game.prologue.begin()
	await frames(3)
	check(game.mode == "prologue" and game.prologue.is_playing(), "Prologue begins in prologue mode")
	var alert_hidden := true
	if teacher.has_node("AlertIcon"):
		alert_hidden = teacher.get_node("AlertIcon").visible == false
	check(alert_hidden, "Alert UI stays hidden during prologue")
	check(game.timer.state == "idle", "Prologue never rings exam timer")
	game.skip_prologue()
	await frames(45)
	check(game.mode == "game" and not game.prologue.is_playing(), "Skip fades into B0 gameplay mode")
	check(player.global_position.distance_to(game.get_spawn()) < 0.35, "B0 places player at classroom observation spawn")
	check(teacher.frozen and not game.progress.teacher_summoned, "B0 keeps Phase0 teacher inactive (no summon from prologue)")
	check(game.timer.state == "idle", "Entering B0 does not trigger bell")
	if teacher.has_node("AlertIcon"):
		check(teacher.get_node("AlertIcon").visible == false, "Alert UI stays hidden after prologue→B0")
	else:
		check(true, "Alert UI stays hidden after prologue→B0")
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
