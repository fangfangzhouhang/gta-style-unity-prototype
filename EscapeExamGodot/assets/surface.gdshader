shader_type spatial;
render_mode diffuse_burley;
uniform vec4 base_color : source_color = vec4(0.5,0.5,0.4,1.0);
uniform int surface_kind = 0;
varying vec3 object_position;
float hash(vec3 p) { return fract(sin(dot(p,vec3(127.1,311.7,74.7)))*43758.5453); }
void vertex() { object_position = VERTEX; }
void fragment() {
    vec3 p = object_position;
    float grain = hash(floor(p*180.0));
    float variation = (grain-0.5)*0.008;
    if (surface_kind == 1) {
        float stone = hash(floor(p*35.0));
        variation += stone > 0.92 ? 0.025 : (stone < 0.04 ? -0.025 : 0.0);
    } else if (surface_kind == 2) {
        variation += sin(p.z*120.0 + sin(p.x*13.0)*2.0)*0.008;
        variation += (hash(floor(p*9.0)) - 0.5)*0.008;
    }
    ALBEDO = clamp(base_color.rgb + variation, vec3(0.02), vec3(0.92));
    ROUGHNESS = surface_kind == 2 ? 0.66 : 0.84;
    SPECULAR = 0.22;
}
