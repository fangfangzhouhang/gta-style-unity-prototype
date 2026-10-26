extends CanvasLayer

var toast_time := 0.0
var sound_time := 0.0
var last_sound := ""
var hint_level := 0

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	$Menu/Box/Resume.pressed.connect(_resume)
	$Menu/Box/New.pressed.connect(_new)
	$Menu/Box/Journal.pressed.connect(func(): Game.previous_mode = Game.mode; Game.set_mode("journal"))
	$Menu/Box/Settings.pressed.connect(func(): Game.previous_mode = Game.mode; Game.set_mode("settings"))
	$Menu/Box/Quit.pressed.connect(func(): Game.save_progress(); get_tree().quit())
	$Reading/Box/Close.pressed.connect(func(): Game.set_mode(Game.previous_mode))
	$Reading/Box/Hint.pressed.connect(_hint)
	$Settings/Box/Close.pressed.connect(func(): Game.save_settings(); Game.set_mode(Game.previous_mode))
	$Settings/Box/Sensitivity.value_changed.connect(func(v): Game.sensitivity = v / 10000.0)
	$Settings/Box/Fov.value_changed.connect(func(v): Game.view_fov = v; Game.player.camera.fov = v)
	$Settings/Box/Volume.value_changed.connect(func(v): AudioServer.set_bus_volume_db(0, v))
	$Settings/Box/Captions.toggled.connect(func(v): Game.sound_captions = v)
	$Confirm/Box/Yes.pressed.connect(func(): $Confirm.hide(); Game.start_new())
	$Confirm/Box/No.pressed.connect(func(): $Confirm.hide())
	$Ending/Box/TitleButton.pressed.connect(func(): Game.set_mode("title"))

func _process(delta: float) -> void:
	toast_time = maxf(0, toast_time - delta)
	sound_time = maxf(0, sound_time - delta)
	$Toast.visible = toast_time > 0 and Game.mode == "game"
	$Sound.visible = sound_time > 0 and Game.mode == "game" and Game.sound_captions
	if is_instance_valid(Game.timer):
		var clock = Game.timer
		$Inventory.text = ("计时器在手  ·  Q 上弦/停止  ·  左键放下" if clock.held else ("计时器已放置  ·  靠近按 Q 上弦" if clock.owned else "WASD 移动  ·  鼠标观察  ·  E 调查"))
		if clock.state != "idle": $Inventory.text += "  |  " + ("响铃 " if clock.state == "ringing" else "预备 ") + str(ceili(maxf(0, clock.remaining)))

func show_mode(mode: String) -> void:
	for path in ["Menu", "Reading", "Settings", "Recovery", "Ending", "Confirm"]:
		get_node(path).hide()
	$Prompt.visible = mode == "game"
	$Crosshair.visible = mode == "game"
	$Inventory.visible = mode == "game"
	$Help.visible = mode == "game"
	$Chapter.visible = mode == "game"
	match mode:
		"title", "pause":
			$Menu.show()
			$Menu/Box/Title.text = "逃离考场" if mode == "title" else "暂时停笔"
			$Menu/Box/Subtitle.text = "第一章  /  最后一遍铃\n声音会暴露你，也能替你开路。"
			$Menu/Box/Resume.text = "继续上次的进度" if mode == "title" else "回到考场"
			$Menu/Box/Resume.visible = Game.has_save() if mode == "title" else true
			$Menu/Box/New.text = "开始新的试走"
			$Menu/Box/Journal.visible = Game.playing
			$Menu/Box/Footer.text = "可玩开发版 " + Game.build_id + "\n本章保存近路、卷栅和已发现的事实。"
		"journal":
			$Reading.show()
			$Reading/Box/Title.text = "已经知道的事"
			var text := ""
			for key in Game.facts: text += "• " + str(Game.facts[key]) + "\n\n"
			if text.is_empty(): text = "还没有记下什么。观察门、纸张与发声的物件。\n"
			text += "\n尚未确定：下一次应该从哪里行动？\n手册只记录亲自发现的事。"
			$Reading/Box/Body.text = text
			$Reading/Box/Hint.show()
		"settings":
			$Settings.show()
			$Settings/Box/Sensitivity.value = Game.sensitivity * 10000
			$Settings/Box/Fov.value = Game.view_fov
			$Settings/Box/Volume.value = AudioServer.get_bus_volume_db(0)
			$Settings/Box/Captions.button_pressed = Game.sound_captions
		"ending":
			$Ending.show()
			$Ending/Box/Body.text = "第一章 · 已离场\n\n你没有等来许可。\n你给自己留出了一条路。\n\n本次用时 %d 分 %02d 秒   ·   重试 %d 次\n开发版时长，不代表首玩目标已验证。" % [int(Game.elapsed) / 60, int(Game.elapsed) % 60, Game.caught_count]

func _resume() -> void:
	if Game.mode == "title": Game.load_progress()
	else: Game.set_mode("game")

func _new() -> void:
	if Game.has_save() or Game.playing: $Confirm.show()
	else: Game.start_new()

func set_prompt(text: String) -> void:
	$Prompt.text = text

func toast(text: String, duration := 3.0) -> void:
	$Toast.text = text
	toast_time = duration

func sound_caption(text: String) -> void:
	if text == last_sound and sound_time > 0.8: return
	last_sound = text
	$Sound.text = "〔 " + text + " 〕"
	sound_time = 2.4

func open_reading(title: String, body: String) -> void:
	Game.previous_mode = "game"
	Game.set_mode("reading")
	$Reading.show()
	$Reading/Box/Title.text = title
	$Reading/Box/Body.text = body
	$Reading/Box/Hint.hide()

func show_recovery(text: String) -> void:
	$Recovery.show()
	$Recovery/Label.text = text

func _hint() -> void:
	hint_level = mini(hint_level + 1, 3)
	var hints := ["", "先看哪里能绕行。老师不是一直朝着同一个方向。", "如果发声的位置与你不同，你可以从另一个出口离开。", "把计时器留在考场并上弦，经印务室绕到东廊；卷栅可分三次摇开，随时撤退。"]
	$Reading/Box/Body.text += "\n\n主动提示 %d / 3：%s" % [hint_level, hints[hint_level]]
