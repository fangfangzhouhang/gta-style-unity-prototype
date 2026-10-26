extends Node3D

@export var action := "test"

func prompt(_actor) -> String:
	return "E  " + {"stop": "切换循环播放", "mode": "切换监听 / 外放", "zone": "选择广播分区", "test": "播放终场铃"}[action]

func interact(_actor) -> void:
	get_parent().operate(action)
