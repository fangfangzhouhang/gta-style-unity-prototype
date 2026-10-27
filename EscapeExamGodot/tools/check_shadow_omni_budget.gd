extends SceneTree
## Static-ish budget check: OmniLight3D under Lighting with shadow_enabled must be ≤5 (expect 4).

func _initialize() -> void:
	call_deferred("go")

func go() -> void:
	var level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	await process_frame
	var lighting: Node = level.get_node_or_null("Lighting")
	if lighting == null:
		printerr("FAIL Lighting node missing")
		quit(1)
		return
	var n := 0
	var names: Array[String] = []
	for child in lighting.get_children():
		var light: Node = child.get_node_or_null("Light")
		if light is OmniLight3D and (light as OmniLight3D).shadow_enabled:
			n += 1
			names.append(str(child.name))
	print("SHADOW_OMNI_COUNT ", n, " fixtures=", ",".join(names))
	if n > 5:
		printerr("FAIL shadow Omni count %d exceeds budget ≤5" % n)
		quit(1)
		return
	if n != 4:
		print("WARN expected 4 shadow Omnis (Fixture0/5/6/7); got ", n)
	print("PASS shadow Omni budget ≤5 (measured=%d)" % n)
	quit(0)
