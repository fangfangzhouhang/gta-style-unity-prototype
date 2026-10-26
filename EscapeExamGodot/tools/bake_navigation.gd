extends SceneTree

func _initialize() -> void:
	call_deferred("bake")

func bake() -> void:
	var level = load("res://scenes/chapters/chapter_01.tscn").instantiate()
	root.add_child(level)
	paused = false
	await physics_frame
	await physics_frame
	var region: NavigationRegion3D = level.get_node("Architecture")
	region.bake_navigation_mesh(false)
	var result := ResourceSaver.save(region.navigation_mesh, "res://resources/chapter_navigation.tres")
	print("NAVIGATION_BAKED polygons=", region.navigation_mesh.get_polygon_count(), " result=", result)
	quit(0 if result == OK and region.navigation_mesh.get_polygon_count() > 0 else 1)
