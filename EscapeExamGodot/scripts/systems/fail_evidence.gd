class_name FailEvidence
extends RefCounted
## Resolves PLOT fail-evidence primary + progress-aware recovery lines for catch UI.
## Source: docs/PLOT_P0_BEATS_AND_FAIL_LINES.md §2 · FAIL_EVIDENCE_IMPL_CARD.md §5

const LINE_HELD_RINGING := "铃在你手里响的时候，声源就是你自己。"
const LINE_NEAR_RING := "铃声把两边都叫来了——包括还站在铃边的你。"
const LINE_GATE_CHAIN := "链声把人引到了门口；你抬栅的时候退路还空着。"
const LINE_LOS_CORNER := "转角那边他还看得到你。下一次先断视线，再换路。"
const LINE_FALLBACK := "他确认你在这一带。先听脚步离开，再从另一条路走。"

const RECOVERY_GATE := "已升起的卷栅档位保留。"
const RECOVERY_SHORTCUT := "运卷近路仍开着。"

## Physics frames (~2s at 60 Hz) for "recent gate work" latch.
const RECENT_GATE_FRAMES := 120
const TIMER_NEAR_DISTANCE := 2.2
const GATE_NEAR_DISTANCE := 3.5

static func gather_from_game(game: Node) -> Dictionary:
	var ctx := {
		"kind": "fallback",
		"timer_held": false,
		"timer_ringing": false,
		"timer_near": false,
		"gate_recent": false,
		"gate_near": false,
		"gate_stage": 0,
		"shortcut": false,
		"teacher_state": "",
		"last_heard_kind": "",
		"heard_id": "",
		"chase_visible": false,
	}
	if game == null:
		return ctx
	var progress: Dictionary = game.get("progress") if game.get("progress") is Dictionary else {}
	ctx["shortcut"] = bool(progress.get("shortcut", false))
	var gate = game.get("gate")
	if is_instance_valid(gate):
		ctx["gate_stage"] = int(gate.get("stage"))
		var last_work: int = int(gate.get("last_work_frame"))
		var frames: int = Engine.get_physics_frames()
		ctx["gate_recent"] = last_work > 0 and (frames - last_work) <= RECENT_GATE_FRAMES
		var player = game.get("player")
		if is_instance_valid(player):
			ctx["gate_near"] = _flat_distance(player.global_position, gate.global_position) <= GATE_NEAR_DISTANCE
	var timer = game.get("timer")
	if is_instance_valid(timer):
		ctx["timer_held"] = bool(timer.get("held"))
		ctx["timer_ringing"] = str(timer.get("state")) == "ringing"
		var player2 = game.get("player")
		if is_instance_valid(player2):
			ctx["timer_near"] = _flat_distance(player2.global_position, timer.global_position) <= TIMER_NEAR_DISTANCE
	var teacher = game.get("teacher")
	if is_instance_valid(teacher):
		ctx["teacher_state"] = str(teacher.get("state"))
		ctx["last_heard_kind"] = str(teacher.get("last_heard_kind"))
		ctx["heard_id"] = str(teacher.get("heard_id"))
		ctx["chase_visible"] = ctx["teacher_state"] == "chase"
	return ctx


static func resolve_primary(ctx: Dictionary) -> String:
	var kind := classify(ctx)
	match kind:
		"held_ringing":
			return LINE_HELD_RINGING
		"near_ring":
			return LINE_NEAR_RING
		"gate_chain":
			return LINE_GATE_CHAIN
		"los_corner":
			return LINE_LOS_CORNER
		_:
			return LINE_FALLBACK


static func classify(ctx: Dictionary) -> String:
	## Priority: 2.1 → 1.3 → 1.5 → 1.1 → 1.7
	if bool(ctx.get("timer_held", false)) and bool(ctx.get("timer_ringing", false)):
		return "held_ringing"
	if bool(ctx.get("timer_ringing", false)) and bool(ctx.get("timer_near", false)):
		return "near_ring"
	if _looks_like_ring_evidence(ctx) and bool(ctx.get("timer_near", false)):
		return "near_ring"
	if bool(ctx.get("gate_recent", false)) and bool(ctx.get("gate_near", false)):
		return "gate_chain"
	if _looks_like_gate_evidence(ctx) and bool(ctx.get("gate_near", false)):
		return "gate_chain"
	if bool(ctx.get("chase_visible", false)) or str(ctx.get("teacher_state", "")) == "chase":
		return "los_corner"
	return "fallback"


static func resolve_recovery(ctx: Dictionary) -> String:
	## Pick one real progress line, or omit. Never fake shortcut/gate comfort.
	if int(ctx.get("gate_stage", 0)) >= 1:
		return RECOVERY_GATE
	if bool(ctx.get("shortcut", false)):
		return RECOVERY_SHORTCUT
	## Broadcast "settings kept" omitted this slice (no reliable "was changed" flag).
	return ""


static func format_recovery_ui(primary: String, recovery: String) -> String:
	if recovery.is_empty():
		return "被请回了座位。\n" + primary
	return "被请回了座位。\n" + primary + "\n" + recovery


static func build_message_from_game(game: Node) -> Dictionary:
	var ctx := gather_from_game(game)
	var kind := classify(ctx)
	ctx["kind"] = kind
	var primary := resolve_primary(ctx)
	var recovery := resolve_recovery(ctx)
	return {
		"ctx": ctx,
		"kind": kind,
		"primary": primary,
		"recovery": recovery,
		"message": format_recovery_ui(primary, recovery),
	}


static func _looks_like_ring_evidence(ctx: Dictionary) -> bool:
	var heard := str(ctx.get("heard_id", ""))
	var kind := str(ctx.get("last_heard_kind", ""))
	if heard.begins_with("ring_"):
		return true
	if "铃声" in kind or "计时器" in kind:
		return true
	return false


static func _looks_like_gate_evidence(ctx: Dictionary) -> bool:
	var kind := str(ctx.get("last_heard_kind", ""))
	return "链条" in kind or "卷栅" in kind


static func _flat_distance(a: Vector3, b: Vector3) -> float:
	return Vector2(a.x - b.x, a.z - b.z).length()
