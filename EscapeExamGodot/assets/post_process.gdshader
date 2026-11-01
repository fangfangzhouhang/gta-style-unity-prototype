shader_type canvas_item;

// V2 post chain: restrained vignette + film grain + radial chromatic aberration.
// All intensities are initial values pending Forward+ on-device calibration.
// Driven per-frame by VisualMood via the `tension` uniform.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;

uniform float vignette_strength : hint_range(0.0, 1.0) = 0.34;
uniform float grain_strength : hint_range(0.0, 0.2) = 0.032;
uniform float aberration_strength : hint_range(0.0, 0.01) = 0.0011;
uniform float tension : hint_range(0.0, 1.0) = 0.0;
uniform float effect_enabled = 1.0;

float hash12(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

void fragment() {
	vec2 centered = SCREEN_UV - 0.5;
	float r2 = dot(centered, centered);
	float on = effect_enabled;

	// Chromatic aberration grows radially and slightly with tension.
	float ab = aberration_strength * (0.55 + tension * 1.5) * on;
	vec2 dir = centered * ab * (0.4 + r2 * 2.2);
	float r = texture(screen_texture, SCREEN_UV + dir).r;
	float g = texture(screen_texture, SCREEN_UV).g;
	float b = texture(screen_texture, SCREEN_UV - dir).b;
	vec3 col = vec3(r, g, b);

	// Vignette: constant base, deepens modestly as tension rises.
	float vig = 1.0 - r2 * vignette_strength * (1.0 + tension * 0.55) * on;
	col *= clamp(vig, 0.55, 1.0);

	// Film grain: low, constant amplitude (not tension-driven, avoids noise panic).
	float grain = hash12(SCREEN_UV * vec2(1831.0, 1013.0) + fract(TIME) * 17.31) - 0.5;
	col += grain * grain_strength * on;

	COLOR = vec4(col, 1.0);
}
