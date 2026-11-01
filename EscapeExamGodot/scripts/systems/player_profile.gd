extends Node
## Cross-run player profile (V2 §3, Hello-Neighbor-style adaptation done with
## statistics, not a neural net). Persisted to user://player_profile.json in
## the same transactional style as game.gd's chapter save (tmp -> verify ->
## backup -> rename). Stores only *events the teacher itself witnessed*:
## rooms where it sighted the player, rooms where it caught the player, and
## how often it investigated the exam timer.
##
## Adaptation contract (fairness): exactly ONE countermeasure is active —
## the hottest room becomes the teacher's first-check room when it starts a
## search. Patrol dwell in hot rooms is biased by a capped multiplier.
## Nothing here speeds the teacher up, widens its senses, or reveals the
## player's position.

const SAVE_PATH := "user://player_profile.json"
const SCHEMA := 1
const DECAY := 0.5 # every new run, all counts are halved
const MAX_DWELL_BIAS := 2.0 # patrol dwell in the hottest room at most doubles

var sightings_by_room: Dictionary = {} # room -> float
var caught_by_room: Dictionary = {} # room -> float
var timer_investigations := 0.0
var runs := 0

var _countermeasure_room := ""
var _loaded := false
var _last_elapsed := -1.0
var _last_caught := -1

func _ready() -> void:
	name = "PlayerProfile"
	load_profile()

## The room the single active countermeasure targets ("" = none yet).
func current_countermeasure() -> String:
	return _countermeasure_room

## Heat of one room: sightings count once, catches count double.
func heat_of(room: String) -> float:
	return float(sightings_by_room.get(room, 0.0)) + 2.0 * float(caught_by_room.get(room, 0.0))

## Capped dwell multiplier for patrol points inside a room.
func dwell_bias(room: String) -> float:
	if room.is_empty():
		return 1.0
	return clampf(1.0 + 0.34 * heat_of(room), 1.0, MAX_DWELL_BIAS)

# --- recording (called by the teacher from its own sensory events only) ---

func record_sighting(room: String) -> void:
	if room.is_empty():
		return
	sightings_by_room[room] = float(sightings_by_room.get(room, 0.0)) + 1.0
	_recompute_countermeasure()
	save_profile()

func record_caught(room: String) -> void:
	if room.is_empty():
		return
	caught_by_room[room] = float(caught_by_room.get(room, 0.0)) + 1.0
	_recompute_countermeasure()
	save_profile()

func record_timer_investigation() -> void:
	timer_investigations += 1.0
	save_profile()

# --- run lifecycle ---

## Called from teacher.reset_teacher(). Detects a genuinely new run (the
## chapter clock restarted) and only then applies decay, so the checkpoint
## restores that also funnel through reset_teacher do not decay repeatedly.
func note_reset() -> void:
	if not _loaded:
		load_profile()
	var elapsed := 0.0
	var caught := 0
	if is_instance_valid(Game):
		elapsed = float(Game.elapsed)
		caught = int(Game.caught_count)
	if _last_elapsed >= 0.0 and elapsed < _last_elapsed - 0.5:
		_apply_decay()
		runs += 1
		save_profile()
		Game.trace("profile_applied", {"reason": "new_run_decay", "countermeasure_room": _countermeasure_room, "runs": runs})
	_last_elapsed = elapsed
	_last_caught = caught
	_recompute_countermeasure()

func _apply_decay() -> void:
	for room in sightings_by_room.keys():
		sightings_by_room[room] = float(sightings_by_room[room]) * DECAY
	for room in caught_by_room.keys():
		caught_by_room[room] = float(caught_by_room[room]) * DECAY
	timer_investigations *= DECAY

func _recompute_countermeasure() -> void:
	var best := ""
	var best_heat := 0.99 # a room must earn it: at least one full sighting-equivalent
	for room in sightings_by_room.keys() + caught_by_room.keys():
		var h := heat_of(room)
		if h > best_heat:
			best_heat = h
			best = room
	_countermeasure_room = best

# --- persistence (mirrors game.gd: tmp file, parse-verify, backup, rename) ---

func load_profile() -> void:
	_loaded = true
	if Game.test_mode:
		return # tests run on an in-memory profile; never touch the player's file
	var data := _read_valid(SAVE_PATH)
	if data.is_empty():
		data = _read_valid(SAVE_PATH + ".backup")
	if data.is_empty():
		return
	sightings_by_room = _sanitize_counts(data.get("sightings_by_room", {}))
	caught_by_room = _sanitize_counts(data.get("caught_by_room", {}))
	timer_investigations = clampf(float(data.get("timer_investigations", 0.0)), 0.0, 100000.0)
	runs = int(clampf(float(data.get("runs", 0)), 0.0, 100000.0))
	_recompute_countermeasure()

func save_profile() -> bool:
	if Game.test_mode:
		return false
	var data := {"schema": SCHEMA, "sightings_by_room": sightings_by_room, "caught_by_room": caught_by_room,
		"timer_investigations": timer_investigations, "runs": runs}
	var file := FileAccess.open(SAVE_PATH + ".tmp", FileAccess.WRITE)
	if file == null:
		return false
	file.store_string(JSON.stringify(data))
	file.close()
	var temp = JSON.parse_string(FileAccess.get_file_as_string(SAVE_PATH + ".tmp"))
	if not temp is Dictionary:
		return false
	if FileAccess.file_exists(SAVE_PATH):
		DirAccess.copy_absolute(SAVE_PATH, SAVE_PATH + ".backup")
	return DirAccess.rename_absolute(SAVE_PATH + ".tmp", SAVE_PATH) == OK

func _read_valid(path: String) -> Dictionary:
	if not FileAccess.file_exists(path):
		return {}
	var parser := JSON.new()
	if parser.parse(FileAccess.get_file_as_string(path)) != OK:
		return {}
	var data = parser.data
	if not data is Dictionary or int(data.get("schema", 0)) != SCHEMA:
		return {}
	return data

func _sanitize_counts(raw) -> Dictionary:
	var out: Dictionary = {}
	if not raw is Dictionary:
		return out
	for key in raw:
		if key is String and (raw[key] is float or raw[key] is int):
			var v := float(raw[key])
			if is_finite(v) and v > 0.0 and v <= 100000.0 and String(key).length() <= 8:
				out[key] = v
	return out
