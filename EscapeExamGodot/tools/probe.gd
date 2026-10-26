extends SceneTree
func _initialize() -> void:
	print("USER_DATA ", OS.get_user_data_dir())
	print("USER_DIR_ACCESS ", DirAccess.open("user://") != null)
	quit()
