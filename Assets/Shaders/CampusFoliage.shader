Shader "Campus/Foliage"
{
    Properties { _Color ("Leaf Tint", Color) = (0.3,0.48,0.15,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0
        fixed4 _Color;
        struct Input { fixed4 color : COLOR; };
        void vert(inout appdata_full v)
        {
            float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
            v.vertex.x += sin(_Time.y * 1.6 + world.x * 0.8 + world.z) * 0.018 * saturate(v.vertex.y);
        }
        void surf(Input input, inout SurfaceOutputStandard output)
        {
            output.Albedo = _Color.rgb * input.color.rgb;
            output.Metallic = 0;
            output.Smoothness = 0.28;
            output.Occlusion = 0.9;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
