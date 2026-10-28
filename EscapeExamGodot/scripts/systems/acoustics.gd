extends Node3D

const CLIPS := {
	"player_step": preload("res://assets/audio/step_soft.wav"),
	"teacher_step": preload("res://assets/audio/step_hard.wav"),
	"teacher_step_soft": preload("res://assets/audio/step_soft.wav"),
	"timer": preload("res://assets/audio/bell.wav"),
	"tick": preload("res://assets/audio/tick.wav"),
	"door": preload("res://assets/audio/door.wav"),
	"gate": preload("res://assets/audio/chain.wav"),
	"notice": preload("res://assets/audio/notice.wav"),
	"broadcast": preload("res://assets/audio/chime.wav")
}
var serial := 0
var portals: Array[Node] = []
var zones: Array[Node] = []

func _ready() -> void:
	portals = $Portals.get_children()
	zones = $Zones.get_children()

func room_at(point: Vector3) -> String:
	for zone in zones:
		var bounds: Rect2 = zone.get_meta("bounds")
		if bounds.has_point(Vector2(point.x, point.z)): return zone.name
	return "O"

# Small authored portal graph. Closed doors add cost and obscure exact origin.
func audible_path(source: Vector3, listener: Vector3) -> Dictionary:
	var start := room_at(source)
	var finish := room_at(listener)
	if start == finish: return {"cost": source.distance_to(listener), "origin": source, "muffled": false}
	var queue: Array[Dictionary] = [{"room": start, "point": source, "cost": 0.0, "route": [], "muffled": false, "visited": [start]}]
	var best := {"cost": INF, "origin": source, "muffled": true}
	while not queue.is_empty():
		var entry: Dictionary = queue.pop_front()
		if float(entry.cost) >= float(best.cost): continue
		for portal in portals:
			var a: String = portal.get_meta("a")
			var b: String = portal.get_meta("b")
			if entry.room != a and entry.room != b: continue
			var next: String = b if entry.room == a else a
			if next in entry.visited: continue
			var closed := false
			if portal.has_meta("door"):
				var door = Game.world.get_node_or_null(NodePath(portal.get_meta("door")))
				closed = door != null and not door.is_open
			var cost: float = float(entry.cost) + entry.point.distance_to(portal.global_position) + (7.0 if closed else 0.0)
			var muffled: bool = entry.muffled or closed
			if next == finish:
				cost += portal.global_position.distance_to(listener)
				if cost < float(best.cost):
					best = {"cost": cost, "origin": portal.global_position if muffled else source, "muffled": muffled,
						"audio_origin": portal.global_position}
			else:
				var visited: Array = entry.visited.duplicate()
				visited.append(next)
				queue.append({"room": next, "point": portal.global_position, "cost": cost, "muffled": muffled, "visited": visited})
	return best

func emit_sound(point: Vector3, kind: String, reach: float, caption: String, source_id: String = "", event_id: String = "") -> void:
	if not is_instance_valid(Game.player): return
	serial += 1
	var id := event_id if not event_id.is_empty() else source_id + str(serial)
	var teacher_path := audible_path(point, Game.teacher.global_position + Vector3.UP)
	if source_id != "teacher" and float(teacher_path.cost) < reach:
		Game.teacher.hear_noise(teacher_path.origin, reach - float(teacher_path.cost), caption, id)
	var listener: Vector3 = Game.player.camera.global_position
	var path := audible_path(point, listener)
	if float(path.cost) >= reach * 1.4: return
	var sound := AudioStreamPlayer3D.new()
	add_child(sound)
	sound.global_position = path.get("audio_origin", point)
	sound.stream = CLIPS.get(kind, CLIPS.door)
	sound.bus = "SFX"
	sound.unit_size = 5
	sound.max_distance = 45
	sound.volume_db = clampf(-float(path.cost) * 0.55 - (5.0 if path.muffled else 0.0), -28, -2)
	if source_id == "player": sound.volume_db -= 11  # ark: footsteps too loud
	# Door open/close: less piercing — extra atten + slight pitch down (longer feel).
	if kind == "door":
		sound.volume_db = clampf(sound.volume_db - 4.0, -32.0, -6.0)
		sound.pitch_scale = 0.94
	# Teacher footsteps: louder as distance to player decreases (horror proximity cue).
	elif kind == "teacher_step" or kind == "teacher_step_soft":
		var dist := listener.distance_to(sound.global_position)
		# near 0.75 m → -2 dB; far 18 m → -26 dB; closed-door muffling −6.
		var t := clampf((dist - 0.75) / 17.25, 0.0, 1.0)
		sound.volume_db = lerpf(-2.0, -26.0, t)
		if path.muffled:
			sound.volume_db -= 6.0
		if kind == "teacher_step":
			sound.volume_db += 1.5  # chase/hard presence
		sound.volume_db = clampf(sound.volume_db, -30.0, -1.0)
		# Slightly longer hear radius for 3D falloff; bus stays SFX.
		sound.unit_size = 6.5
		sound.max_distance = 40.0
	sound.play()
	sound.finished.connect(sound.queue_free)
	if Game.sound_captions and source_id != "player" and kind != "tick":
		var side: float = Game.player.camera.global_basis.x.dot(sound.global_position - listener)
		Game.ui.sound_caption(caption + (" · 左侧" if side < -1 else (" · 右侧" if side > 1 else " · 附近")))

