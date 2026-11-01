extends SceneTree
## V2 新机制专项验收（与 acceptance.gd 的 95 项基线分开计数，回应设计圣经 §6
## 「新增机制要同步加验收项」与 QA_AUDIT_V2 中级问题 2）。
## 运行：godot --headless --path EscapeExamGodot -s res://tests/v2_acceptance.gd -- --test
## 只测 V2 新增：教师三层脑、统一 tension 消费、探头、粉笔、红笔、广播异变、声音 kind 注册。

var game
var level
var passed := 0
var failed := 0

func _initialize() -> void:
	call_deferred("run")

func check(condition: bool, label: String) -> void:
	print(("PASS " if condition else "FAIL ") + label)
	if condition: passed += 1
	else: failed += 1

func frames(count: int) -> void:
	for i in count: await physics_frame

func run() -> void:
	root.size = Vector2i(1280, 800)
	game = root.get_node("Game")
	game.test_mode = true
	level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	await process_frame
	game.start_new()
	var player = game.player
	var teacher = game.teacher
	await frames(10)

	# --- 教师三层脑自注册 ---
	check(teacher.has_node("Belief"), "V2 Belief node self-registered under teacher")
	check(teacher.has_node("Director"), "V2 Director node self-registered under teacher")
	check(teacher.has_node("PlayerProfile"), "V2 PlayerProfile node self-registered under teacher")
	var belief = teacher.get_node("Belief")
	var director = teacher.get_node("Director")
	var profile = teacher.get_node("PlayerProfile")

	# --- Belief 行为 ---
	belief.reset()
	belief.observe_sighting("E")
	check(belief.belief_of("E") > 0.5, "V2 Belief sighting collapses distribution to room E")
	check(not belief.best_search_rooms(3, Vector3.ZERO).is_empty(), "V2 Belief yields search rooms")
	belief.observe_noise("C", 0.8)
	check(belief.belief_of("C") > 0.05, "V2 Belief noise observation raises room C")

	# --- Director / 统一 tension ---
	check(teacher.has_method("get_tension"), "V2 Teacher exposes get_tension()")
	var t := float(teacher.get_tension())
	check(t >= 0.0 and t <= 1.0, "V2 Director tension within [0,1]")
	check(is_instance_valid(root.get_node_or_null("Ambience/AdaptiveAudio")), "V2 AdaptiveAudio self-registered under Ambience")
	check(level.has_node("VisualMood") or _find(level, "VisualMood"), "V2 VisualMood self-registered in chapter")

	# --- Profile 接口 ---
	check(profile.has_method("current_countermeasure"), "V2 Profile exposes current_countermeasure()")
	profile.record_caught("E")
	profile.record_caught("E")
	check(profile.heat_of("E") > 0.0, "V2 Profile heat accumulates for caught room")

	# --- 声音 kind 注册 ---
	var acoustics = game.acoustics
	check(acoustics != null and acoustics.CLIPS.has("chalk"), "V2 acoustics CLIPS registers chalk")
	check(acoustics != null and acoustics.CLIPS.has("keys"), "V2 acoustics CLIPS registers keys")

	# --- 探头 Lean ---
	Input.action_press("lean_right")
	await frames(30)
	check(player.lean_current > 0.2, "V2 Lean right offsets head (lean_current > 0.2)")
	Input.action_release("lean_right")
	await frames(30)
	check(absf(player.lean_current) < 0.1, "V2 Lean releases back to centre")

	# --- 粉笔：拾取与投掷 ---
	check(level.has_node("ChalkSpawner"), "V2 ChalkSpawner registered in world")
	check(game.chalk_count == 0, "V2 Chalk count starts at 0")
	var pickup = _find_chalk_pickup(level)
	check(pickup != null, "V2 Chalk pickup exists near podium")
	if pickup != null:
		pickup.interact(player)
		await frames(3)
		check(game.chalk_count == 1, "V2 Chalk pickup grants one chalk")
		player.throw_chalk()
		await frames(3)
		check(game.chalk_count == 0, "V2 Chalk throw consumes one chalk")

	# --- 红笔标记随进程出现 ---
	check(level.has_node("RedMarks"), "V2 RedMarks registered in world")
	game.progress.gate_stage = 1
	await frames(40)  # red_marks polls on a timer
	var marks := 0
	for child in level.get_node("RedMarks").get_children():
		marks += 1
	check(marks >= 1, "V2 Red marks appear after first gate stage")
	game.progress.gate_stage = 0

	# --- 广播异变恰好一次 ---
	game.progress.loop_stopped = true
	game.progress.gate_stage = 2
	var broadcast = level.get_node("Broadcast")
	player.teleport_to(broadcast.global_position + Vector3(0, 0, 1.5), 0)
	await frames(80)
	check(game.facts.has("broadcast_glitched"), "V2 Broadcast glitch triggers near panel after loop stopped")
	game.progress.loop_stopped = false
	game.progress.gate_stage = 0

	print("RESULT passed=", passed, " failed=", failed)
	quit(1 if failed > 0 else 0)

func _find(node: Node, node_name: String) -> bool:
	if node.name == node_name: return true
	for child in node.get_children():
		if _find(child, node_name): return true
	return false

func _find_chalk_pickup(node: Node) -> Node:
	if str(node.get("role")) == "pickup":
		return node
	for child in node.get_children():
		var found := _find_chalk_pickup(child)
		if found != null: return found
	return null
