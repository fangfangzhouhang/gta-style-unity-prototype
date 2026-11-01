extends Node
## Director layer (V2 §3, after Alien: Isolation's two-brain design).
## The Director is the game's *scheduler*, not the teacher's brain: it tracks
## pressure (menace) and pacing beats, and its only outputs are
##   1. a room-granularity hint when pressure has been low for too long,
##   2. a retreat order when pressure has peaked for too long,
##   3. the shared `tension` axis consumed by audio / lighting / post agents.
##
## FAIRNESS — the single sanctioned exception: to measure pressure the
## Director reads the player's position (path length) and room. That is the
## classic Director contract: it never hands coordinates to the teacher.
## Its hint is a room NAME only (same granularity as a heard sound's room),
## is rate-limited by a long cooldown, and is fed through Belief like any
## other uncertain observation. The teacher body must still find the player
## with its own senses.

var menace := 0.0 # smoothed pressure 0..1
var tension := 0.0 # shared axis for audio/visual systems (== menace, shaped by beat)
var beat := "calm" # calm -> build -> peak -> release -> calm
var time_since_encounter := 0.0
var path_length := -1.0 # last measured teacher->player navigation path length

const HINT_AFTER_QUIET := 40.0 # s without an encounter before a room hint
const HINT_COOLDOWN := 45.0
const PEAK_SUSTAIN := 5.0 # s at menace >= PEAK_LEVEL outside chase -> retreat
const PEAK_LEVEL := 0.85
const RELEASE_MIN := 4.0 # s minimum spent in release after a peak

var _hint_cooldown := 0.0
var _peak_time := 0.0
var _release_time := 0.0
var _trace_time := 0.0
var _retreat_latched := false

func _ready() -> void:
	name = "Director"

func reset_director() -> void:
	menace = 0.0
	tension = 0.0
	beat = "calm"
	time_since_encounter = 0.0
	_peak_time = 0.0
	_release_time = 0.0
	_retreat_latched = false
	# Hint cooldown deliberately survives resets: being caught must not
	# reset the scheduler into hinting immediately again.

## Called once per physics tick by the teacher while it is active.
## ctx: {teacher_pos: Vector3, state: String, visible: bool, suspicion: float}
## Returns {hint_room: String ("" = none), retreat: bool}
func tick(delta: float, ctx: Dictionary) -> Dictionary:
	var out := {"hint_room": "", "retreat": false}
	if delta <= 0.0:
		return out
	_hint_cooldown = maxf(0.0, _hint_cooldown - delta)

	# --- encounter bookkeeping (teacher senses reported in ctx) ---
	var encountering: bool = bool(ctx.get("visible", false)) or String(ctx.get("state", "")) in ["suspicious", "chase"]
	if encountering:
		time_since_encounter = 0.0
	else:
		time_since_encounter += delta

	# --- pressure measurement ---
	path_length = _navigation_path_length(ctx.teacher_pos)
	var proximity := 0.0
	if path_length >= 0.0:
		proximity = clampf(1.0 - path_length / 18.0, 0.0, 1.0)
	var state_factor := 0.15
	match String(ctx.get("state", "")):
		"chase": state_factor = 1.0
		"suspicious": state_factor = 0.7
		"search": state_factor = 0.55
		"investigate": state_factor = 0.5
		"return": state_factor = 0.2
	var raw := clampf(0.45 * proximity + 0.35 * state_factor + 0.20 * float(ctx.get("suspicion", 0.0)), 0.0, 1.0)
	# Long quiet also slowly rebuilds baseline pressure so the game breathes
	# back up instead of going permanently flat.
	if not encountering:
		raw = maxf(raw, clampf(time_since_encounter / 120.0, 0.0, 0.3))
	var rate := 1.6 if raw > menace else 0.45 # rises fast, drains slowly
	menace = move_toward(menace, raw, rate * delta)
	tension = menace

	# --- beats ---
	var next_beat := beat
	match beat:
		"calm":
			if menace >= 0.3 or encountering:
				next_beat = "build"
		"build":
			if menace >= PEAK_LEVEL or String(ctx.get("state", "")) == "chase":
				next_beat = "peak"
			elif menace < 0.15 and time_since_encounter > 6.0:
				next_beat = "calm"
		"peak":
			if String(ctx.get("state", "")) != "chase" and menace < 0.55:
				next_beat = "release"
				_release_time = 0.0
		"release":
			_release_time += delta
			if _release_time >= RELEASE_MIN and menace < 0.3:
				next_beat = "calm"
				_retreat_latched = false
	if next_beat != beat:
		beat = next_beat
		Game.trace("director", {"beat": beat, "menace": snappedf(menace, 0.01), "tension": snappedf(tension, 0.01)})

	# --- retreat order: sustained peak pressure outside an active chase ---
	if menace >= PEAK_LEVEL and String(ctx.get("state", "")) != "chase":
		_peak_time += delta
	else:
		_peak_time = 0.0
	if _peak_time >= PEAK_SUSTAIN and not _retreat_latched and String(ctx.get("state", "")) in ["search", "investigate", "suspicious"]:
		_retreat_latched = true
		_peak_time = 0.0
		beat = "release"
		_release_time = 0.0
		out.retreat = true
		Game.trace("director", {"order": "retreat", "menace": snappedf(menace, 0.01)})

	# --- room hint: quiet for too long -> nudge the hunt, room granularity ---
	if time_since_encounter >= HINT_AFTER_QUIET and _hint_cooldown <= 0.0 and beat in ["calm", "build"]:
		var room := _player_room()
		if not room.is_empty():
			out.hint_room = room
			_hint_cooldown = HINT_COOLDOWN
			time_since_encounter = 0.0
			Game.trace("director", {"order": "hint", "room": room})

	# --- throttled curve logging for playtest/balance review ---
	_trace_time += delta
	if _trace_time >= 2.0:
		_trace_time = 0.0
		Game.trace("director", {"menace": snappedf(menace, 0.01), "tension": snappedf(tension, 0.01), "beat": beat, "path": snappedf(path_length, 0.1), "quiet_s": snappedf(time_since_encounter, 0.1)})
	return out

## REAL player position is read ONLY inside this Director node, and only to
## (a) measure path length for pressure and (b) name the player's current
## room for a hint. Coordinates never leave this node.
func _player_room() -> String:
	if not is_instance_valid(Game.player) or not is_instance_valid(Game.acoustics):
		return ""
	return Game.acoustics.room_at(Game.player.global_position)

func _navigation_path_length(from_pos: Vector3) -> float:
	if not is_instance_valid(Game.player) or not is_instance_valid(Game.teacher):
		return -1.0
	var map: RID = Game.teacher.get_world_3d().navigation_map
	if NavigationServer3D.map_get_iteration_id(map) == 0:
		return -1.0
	var path := NavigationServer3D.map_get_path(map, from_pos, Game.player.global_position, true, 1)
	if path.size() < 2:
		return from_pos.distance_to(Game.player.global_position)
	var length := 0.0
	for i in range(1, path.size()):
		length += path[i].distance_to(path[i - 1])
	return length
