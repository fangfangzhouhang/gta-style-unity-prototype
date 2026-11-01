extends Node
## VisualMood · V2 画面域运行时系统（自注册，见 chapter.gd _ready）。
##
## 职责（全部运行时完成，不改 chapter_01.tscn，避免多 agent 场景冲突）：
## 1. 环境调校：复制 WorldEnvironment 的 Environment 并改雾/曝光/调整层，去"白天感"。
## 2. 灯光调度：遍历 Lighting/Fixture* 下的 OmniLight3D，用平滑噪声做极轻微荧光闪烁，
##    幅度守可读底线（基础 ±3%，tension 满时 ±8%），频率随 tension 略升。
## 3. 后期链：CanvasLayer + 全屏 ColorRect（post_process.gdshader），暗角/颗粒/色差由 tension 驱动。
## 4. 动态氛围：教室西窗光柱浮尘 + 窗外（庭院方向）雨幕，GPUParticles3D 代码创建。
##
## tension 自算口径与音频域一致：教师与玩家的直线距离 + 教师状态（state 字符串）。
## 不依赖其他 agent 的文件；教师未上场（不可见）时 tension 回落到基线。
## 所有数值均为初值，待 Forward+ 真机同机位校准（见 docs/v2/VISUAL_V2.md）。

@export var post_enabled := true ## 后期链总开关（无障碍：可关暗角/色差/颗粒）
@export var flicker_enabled := true ## 灯管闪烁开关（光敏考虑，可单独关）
@export var particles_enabled := true ## 浮尘/雨幕开关
@export var debug_force_tension := -1.0 ## >=0 时强制 tension（截图调参用），-1 为自算

const POST_SHADER: Shader = preload("res://assets/post_process.gdshader")

# 教室西窗（Wall03 玻璃在 x=-10）内侧的光柱浮尘盒；庭院在 x≈-15，雨幕放在窗外 x≈-12.6。
const DUST_POS := Vector3(-9.25, 1.75, -4.2)
const RAIN_POS := Vector3(-12.6, 4.7, -2.6)

var _env: Environment
var _lights: Array[OmniLight3D] = []
var _base_energy: Array[float] = []
var _noise := FastNoiseLite.new()
var _time := 0.0
var tension := 0.0
var _post_mat: ShaderMaterial

func _ready() -> void:
	_noise.noise_type = FastNoiseLite.TYPE_PERLIN
	_noise.frequency = 0.9
	_noise.fractal_octaves = 2
	_setup_environment()
	_collect_lights()
	_setup_post()
	if particles_enabled:
		_setup_particles()

func _process(delta: float) -> void:
	_time += delta
	_update_tension(delta)
	if flicker_enabled:
		var freq := 1.4 + tension * 2.6
		var amp := 0.03 + tension * 0.05
		for i in _lights.size():
			var n := _noise.get_noise_2d(_time * freq, float(i) * 23.7)
			_lights[i].light_energy = _base_energy[i] * (1.0 + n * amp)
	if _post_mat != null:
		_post_mat.set_shader_parameter("tension", tension)

# ---------------------------------------------------------------- tension 自算
func _update_tension(delta: float) -> void:
	if debug_force_tension >= 0.0:
		tension = clampf(debug_force_tension, 0.0, 1.0)
		return
	# 集成（v2-integration）：Director 在场时用全游戏统一的 tension 轴，本域自算只作兜底。
	if is_instance_valid(Game.teacher) and Game.teacher.has_method("get_tension") and Game.mode == "game":
		var ext := clampf(float(Game.teacher.get_tension()), 0.0, 1.0)
		var ext_rate := 2.2 if ext > tension else 0.55
		tension = clampf(lerpf(tension, ext, clampf(delta * ext_rate, 0.0, 1.0)), 0.0, 1.0)
		return
	var target := 0.08
	var teacher: Node3D = Game.teacher
	var player: Node3D = Game.player
	if is_instance_valid(teacher) and is_instance_valid(player) and teacher.visible:
		var d := teacher.global_position.distance_to(player.global_position)
		var prox := clampf(1.0 - (d - 1.5) / 10.5, 0.0, 1.0)
		target = 0.08 + prox * 0.55
		match str(teacher.get("state")):
			"suspicious":
				target += 0.15
			"investigate", "search":
				target += 0.22
			"chase":
				target = 1.0
	# 升得快（危险逼近要立刻压上）、落得慢（余悸不许骤停，与追击乐延迟衰减同理）。
	var rate := 2.2 if target > tension else 0.55
	tension = clampf(lerpf(tension, target, clampf(delta * rate, 0.0, 1.0)), 0.0, 1.0)

# ---------------------------------------------------------------- 环境调校
func _setup_environment() -> void:
	var we := get_parent().get_node_or_null("WorldEnvironment") as WorldEnvironment
	if we == null or we.environment == null:
		return
	_env = we.environment.duplicate() as Environment
	we.environment = _env
	# 雾：低密度指数雾，冷青色——压远景、给走廊纵深，不许糊住可读性。
	_env.fog_enabled = true
	_env.fog_light_color = Color(0.36, 0.47, 0.53)
	_env.fog_light_energy = 0.4
	_env.fog_sun_scatter = 0.08
	_env.fog_density = 0.016
	_env.volumetric_fog_enabled = false
	# 曝光：原 0.88 → 0.72（约压暗 0.3 档），tonemap 模式保持场景原有 Filmic。
	_env.tonemap_exposure = 0.72
	# 调整层：整体再压一档亮度、轻提对比、降饱和——"终场秩序"的绿白米色保留，艳色抽走。
	_env.adjustment_enabled = true
	_env.adjustment_brightness = 0.93
	_env.adjustment_contrast = 1.07
	_env.adjustment_saturation = 0.84
	# 环境光：原 0.045 → 0.032，暗部沉下去但不死黑。
	_env.ambient_light_energy = 0.032
	# 辉光：只给灯管/出口绿字一点点晕，不做梦幻滤镜。
	_env.glow_enabled = true
	_env.glow_intensity = 0.22
	_env.glow_strength = 0.55
	_env.glow_bloom = 0.04

# ---------------------------------------------------------------- 灯光收集
func _collect_lights() -> void:
	for node in get_parent().find_children("*", "OmniLight3D", true, false):
		var light := node as OmniLight3D
		if light != null and str(light.get_path()).contains("/Lighting/"):
			_lights.append(light)
			_base_energy.append(light.light_energy)

# ---------------------------------------------------------------- 后期链
func _setup_post() -> void:
	if not post_enabled:
		return
	var layer := CanvasLayer.new()
	layer.name = "VisualMoodPost"
	layer.layer = 64 # 在 HUD/菜单之上做统一胶片感；菜单可读性靠低强度保住（见 VISUAL_V2.md）
	add_child(layer)
	var rect := ColorRect.new()
	rect.name = "PostRect"
	rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_post_mat = ShaderMaterial.new()
	_post_mat.shader = POST_SHADER
	_post_mat.set_shader_parameter("tension", tension)
	rect.material = _post_mat
	layer.add_child(rect)

# ---------------------------------------------------------------- 动态氛围
func _particle_material(color: Color) -> StandardMaterial3D:
	var mat := StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	mat.albedo_color = color
	mat.billboard_mode = BaseMaterial3D.BILLBOARD_ENABLED
	return mat

func _setup_particles() -> void:
	# ① 光柱浮尘：教室西窗内侧，慢速漂浮的微粒（几乎只靠灯光方向感存在，不许像雪）。
	var dust := GPUParticles3D.new()
	dust.name = "WindowDust"
	dust.amount = 70
	dust.lifetime = 7.0
	dust.preprocess = 7.0
	dust.position = DUST_POS
	dust.visibility_aabb = AABB(Vector3(-2, -2.5, -5), Vector3(4, 5, 10))
	var dm := ParticleProcessMaterial.new()
	dm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	dm.emission_box_extents = Vector3(0.55, 1.6, 3.6)
	dm.direction = Vector3(1, 0, 0)
	dm.spread = 180.0
	dm.gravity = Vector3(0, -0.006, 0)
	dm.initial_velocity_min = 0.015
	dm.initial_velocity_max = 0.05
	dm.scale_min = 0.6
	dm.scale_max = 1.4
	dust.process_material = dm
	var quad := QuadMesh.new()
	quad.size = Vector2(0.016, 0.016)
	quad.material = _particle_material(Color(0.92, 0.9, 0.82, 0.22))
	dust.draw_pass_1 = quad
	add_child(dust)

	# ② 雨幕：只在窗外庭院方向（x≈-12.6，地面/建筑之外），室内任何位置都不应看到雨丝穿墙。
	var rain := GPUParticles3D.new()
	rain.name = "CourtyardRain"
	rain.amount = 420
	rain.lifetime = 0.62 # 以 ~7.5m/s 落约 4.9m，正好在庭院地面（y≈-0.2）附近消亡，不穿地
	rain.preprocess = 0.62
	rain.position = RAIN_POS
	rain.visibility_aabb = AABB(Vector3(-4, -6, -7), Vector3(8, 12, 14))
	var rm := ParticleProcessMaterial.new()
	rm.emission_shape = ParticleProcessMaterial.EMISSION_SHAPE_BOX
	rm.emission_box_extents = Vector3(2.2, 0.3, 5.2)
	rm.direction = Vector3(0, -1, 0)
	rm.spread = 3.5
	rm.gravity = Vector3(0, -2.5, 0)
	rm.initial_velocity_min = 6.5
	rm.initial_velocity_max = 8.5
	rain.process_material = rm
	var streak := QuadMesh.new()
	streak.size = Vector2(0.008, 0.3)
	streak.material = _particle_material(Color(0.58, 0.68, 0.78, 0.38))
	rain.draw_pass_1 = streak
	add_child(rain)
