extends Node
## Belief layer (V2 §3): the teacher's *believed* player distribution over the
## authored room graph (Acoustics Zones/Portals). This is a discrete Bayes /
## histogram filter: observations sharpen it, diffusion spreads it along room
## edges, and absence (teacher present, player not seen) erodes it.
##
## FAIRNESS: this node never reads Game.player. It only receives observations
## the teacher's own senses produced (sightings / heard points, converted to
## rooms by the teacher) plus the Director's room-granularity hint.

var rooms: Array[String] = []
var adjacency: Dictionary = {} # room -> Array[String]
var centers: Dictionary = {} # room -> Vector3
var belief: Dictionary = {} # room -> float (sums to 1)
var prior: Dictionary = {} # room -> float
var last_checked: Dictionary = {} # room -> float seconds (local clock)
var clock := 0.0
var _graph_ready := false

const DIFFUSION_RATE := 0.10 # per second, probability flow to neighbours
const PRIOR_PULL := 0.02 # per second, relaxation back to the patrol prior

func _ready() -> void:
	name = "Belief"

## Build the graph lazily: Acoustics' zones/portals exist at scene load, but
## Game.acoustics is only wired in Game.boot() (chapter _ready), which runs
## after this node's _ready. Retry until available.
func ensure_graph() -> bool:
	if _graph_ready:
		return true
	if not is_instance_valid(Game.acoustics):
		return false
	var ac = Game.acoustics
	if ac.zones.is_empty() or ac.portals.is_empty():
		ac._ready() # idempotent re-read of children metadata containers
	if ac.zones.is_empty():
		return false
	rooms.clear()
	adjacency.clear()
	centers.clear()
	for zone in ac.zones:
		var room: String = zone.name
		rooms.append(room)
		adjacency[room] = []
		# Zone markers carry no transform in the authored scene (all at the
		# Acoustics origin), so the room centre is derived from the bounds
		# metadata the acoustics system itself uses for room_at().
		if zone.has_meta("bounds"):
			var b: Rect2 = zone.get_meta("bounds")
			centers[room] = Vector3(b.get_center().x, 0.05, b.get_center().y)
		else:
			centers[room] = (zone as Node3D).global_position
	for portal in ac.portals:
		if not (portal.has_meta("a") and portal.has_meta("b")):
			continue
		var a: String = portal.get_meta("a")
		var b: String = portal.get_meta("b")
		if adjacency.has(a) and not (adjacency[a] as Array).has(b):
			(adjacency[a] as Array).append(b)
		if adjacency.has(b) and not (adjacency[b] as Array).has(a):
			(adjacency[b] as Array).append(a)
	if rooms.is_empty():
		return false
	reset()
	_graph_ready = true
	return true

func reset() -> void:
	belief.clear()
	prior.clear()
	last_checked.clear()
	var uniform := 1.0 / maxf(1.0, float(rooms.size()))
	for room in rooms:
		belief[room] = uniform
		prior[room] = uniform
		last_checked[room] = -1000.0

func room_center(room: String) -> Vector3:
	return centers.get(room, Vector3.ZERO)

func belief_of(room: String) -> float:
	return float(belief.get(room, 0.0))

## A confirmed sighting: belief collapses onto that room.
func observe_sighting(room: String) -> void:
	if not ensure_graph() or not belief.has(room):
		return
	for r in rooms:
		belief[r] = float(belief[r]) * 0.04
	belief[room] = float(belief[room]) + 0.96
	_normalize()
	mark_checked(room)

## A heard sound (or a Director room hint). certainty 0..1 scales how much
## mass moves to the room; neighbours get a small share (the player moves).
func observe_noise(room: String, certainty: float) -> void:
	if not ensure_graph() or not belief.has(room):
		return
	var w := clampf(certainty, 0.0, 1.0)
	belief[room] = float(belief[room]) + 0.55 * w
	for nb in adjacency.get(room, []):
		belief[nb] = float(belief[nb]) + 0.12 * w
	_normalize()

## Teacher is physically in `room` and does not see the player: erode belief
## there and hand the removed mass to the neighbours.
func observe_absence(room: String, strength: float = 0.25) -> void:
	if not ensure_graph() or not belief.has(room):
		return
	var removed := float(belief[room]) * clampf(strength, 0.0, 0.9)
	if removed <= 0.0001:
		return
	belief[room] = float(belief[room]) - removed
	var nbs: Array = adjacency.get(room, [])
	if nbs.is_empty():
		_normalize()
		return
	for nb in nbs:
		belief[nb] = float(belief[nb]) + removed / float(nbs.size())
	mark_checked(room)

func mark_checked(room: String) -> void:
	if last_checked.has(room):
		last_checked[room] = clock

## Per-second diffusion along graph edges + relaxation to the patrol prior.
func diffuse(delta: float) -> void:
	if not ensure_graph() or delta <= 0.0:
		return
	clock += delta
	var next: Dictionary = {}
	for room in rooms:
		var value := float(belief[room])
		var nbs: Array = adjacency.get(room, [])
		if not nbs.is_empty():
			var neighbour_mean := 0.0
			for nb in nbs:
				neighbour_mean += float(belief[nb])
			neighbour_mean /= float(nbs.size())
			value += DIFFUSION_RATE * delta * (neighbour_mean - value)
		value = lerpf(value, float(prior[room]), PRIOR_PULL * delta)
		next[room] = maxf(0.0001, value)
	belief = next
	_normalize()

## Search planning = information gain: utility is belief mass discounted by
## travel distance and boosted by how long the room went unchecked.
## from_pos is the TEACHER's position (its own body), never the player's.
func best_search_rooms(n: int, from_pos: Vector3) -> Array[String]:
	var scored: Array = []
	if not ensure_graph():
		return []
	for room in rooms:
		var dist := Vector2(from_pos.x, from_pos.z).distance_to(Vector2(centers[room].x, centers[room].z))
		var distance_factor := 1.0 / (1.0 + dist * 0.35)
		var unchecked_for := clock - float(last_checked.get(room, -1000.0))
		var novelty := clampf(unchecked_for / 30.0, 0.3, 1.5)
		scored.append({"room": room, "score": float(belief[room]) * distance_factor * novelty})
	scored.sort_custom(func(a: Dictionary, b: Dictionary) -> bool: return float(a.score) > float(b.score))
	var out: Array[String] = []
	for i in mini(n, scored.size()):
		out.append(String(scored[i].room))
	return out

func _normalize() -> void:
	var total := 0.0
	for room in rooms:
		total += float(belief[room])
	if total <= 0.0:
		reset()
		return
	for room in rooms:
		belief[room] = float(belief[room]) / total
