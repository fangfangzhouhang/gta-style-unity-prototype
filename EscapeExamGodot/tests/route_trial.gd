extends SceneTree

# Script-assisted play: real character physics, active teacher, viewport key events.
# No teleport after starting, no direct progress writes, no disabled perception.
var game
var level
var player
var failed := false
var strategy := "broadcast"

func _initialize() -> void:
	call_deferred("run")

func key(action: String, pressed: bool) -> void:
	var event := InputEventKey.new()
	event.physical_keycode = {"interact": KEY_E,"arm": KEY_Q}[action]
	event.pressed = pressed
	Input.parse_input_event(event)

func frames(n: int) -> void:
	for i in n: await physics_frame

func move_to(point: Vector3, max_frames := 900) -> bool:
	var map: RID = player.get_world_3d().navigation_map
	var path := NavigationServer3D.map_get_path(map, player.global_position, point, true)
	if path.is_empty():
		print("ROUTE_NO_PATH ",point)
		return false
	var index := 0
	for i in max_frames:
		if game.caught_count > 0 or game.mode not in ["game","ending"]:
			print("ROUTE_INTERRUPTED ",game.mode)
			player.scripted_move = Vector2.ZERO
			return false
		if game.mode == "ending": return true
		var target: Vector3 = path[index]
		var delta: Vector3 = target - player.global_position
		delta.y = 0
		if delta.length() < .18:
			index += 1
			if index >= path.size():
				player.scripted_move = Vector2.ZERO
				await frames(2)
				return true
			continue
		player.look_at_point(target + Vector3.UP*1.62)
		var local: Vector3 = player.global_basis.inverse()*delta.normalized()
		player.scripted_move = Vector2(local.x,local.z)
		await physics_frame
	player.scripted_move = Vector2.ZERO
	print("ROUTE_STUCK current=",player.global_position," wanted=",point," path=",path)
	return false

func interact_at(point: Vector3, expected: Node) -> bool:
	player.look_at_point(point)
	await frames(3)
	if player.target != expected:
		print("ROUTE_TARGET_MISMATCH wanted=",expected.name," actual=",player.target," position=",player.global_position)
		return false
	key("interact",true)
	await frames(2)
	key("interact",false)
	await frames(2)
	return true

func run() -> void:
	if "--timer-route" in OS.get_cmdline_user_args(): strategy = "timer"
	game = root.get_node("Game")
	level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	game.start_new()
	player = game.player
	await frames(5)
	failed = not await interact_at(game.timer.global_position + Vector3.UP*.04,game.timer)
	if not failed:
		player.look_at_point(Vector3(-6.5,.80,-6.65))
		await frames(3)
		failed = not game.timer.place_from_camera()
		key("arm",true)
		await frames(2)
		key("arm",false)
	if not failed: failed = not await move_to(Vector3(-4.0,.05,1.1))
	if not failed: failed = not await move_to(Vector3(-1.75,.05,1.1))
	if not failed: failed = not await move_to(Vector3(-1.75,.05,-3.0))
	if strategy == "timer" and not failed:
		# A route that skips the broadcast and latch; no hidden objective prerequisites.
		if not failed: failed = not await move_to(Vector3(2.7,.05,-3.0))
	else:
		if not failed: failed = not await move_to(Vector3(2.7,.05,-3.0))
		if not failed: failed = not await move_to(Vector3(5.3,.05,-3.6))
		if not failed: failed = not await move_to(Vector3(6.6,.05,-2.6))
		var panel = level.get_node("Broadcast")
		for name in ["Stop","Mode","Test"]:
			if not failed: failed = not await interact_at(panel.get_node(name).global_position,panel.get_node(name))
		if not failed: failed = not await move_to(Vector3(5.1,.05,-.7))
		if not failed: failed = not await move_to(Vector3(2.7,.05,-.7))
	if not failed: failed = not await move_to(Vector3(2.7,.05,1.8))
	if not failed: failed = not await move_to(Vector3(6.9,.05,2.8))
	if not failed:
		player.look_at_point(game.gate.get_node("Handle").global_position)
		await frames(3)
		if player.target != game.gate:
			print("ROUTE_GATE_TARGET_MISSING ",player.target)
			failed = true
		else:
			key("interact",true)
			for i in 410:
				await physics_frame
				if game.caught_count > 0:
					failed = true
					break
			key("interact",false)
			failed = failed or game.gate.stage != 3
	if not failed: failed = not await move_to(Vector3(7.3,.05,1.85))
	if not failed:
		player.look_at_point(Vector3(10,1.62,1.85))
		player.scripted_move = Vector2(0,-1)
		await frames(70)
		player.scripted_move = Vector2.ZERO
	var success: bool = not failed and game.mode == "ending"
	var result = {"strategy":strategy,"success":success,"seconds":game.elapsed,"captures":game.caught_count,"gate":game.gate.stage,"kind":"script_assisted_active_ai_no_teleport"}
	print("ROUTE_RESULT ",JSON.stringify(result))
	var file := FileAccess.open("res://evidence/logs/route-"+strategy+".json",FileAccess.WRITE)
	file.store_string(JSON.stringify({"result":result,"events":game.events},"\t"))
	file.close()
	if game.mode == "recover": await create_timer(1.3,true).timeout
	game.set_mode("title")
	for audio in game.acoustics.get_children():
		if audio is AudioStreamPlayer3D: audio.stop()
	level.queue_free()
	await process_frame
	await process_frame
	quit(0 if success else 1)
