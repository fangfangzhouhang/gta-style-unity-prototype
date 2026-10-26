extends SceneTree
func _initialize() -> void:
	var folder := "res://builds/Playtest-A-M2.1"
	DirAccess.make_dir_recursive_absolute(folder)
	var file := FileAccess.open(folder + "/GODOT_LICENSES.txt",FileAccess.WRITE)
	file.store_string(Engine.get_license_text()+"\n\nThird party components:\n"+JSON.stringify(Engine.get_copyright_info(),"\t")+"\n\nLicense texts:\n"+JSON.stringify(Engine.get_license_info(),"\t"))
	file.close()
	print("ENGINE_LICENSES_WRITTEN")
	quit()
