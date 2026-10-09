Shader "MathPirates/Low Poly Water"
{
    Properties
    {
        _DeepColor("Deep water", Color) = (0.035, 0.37, 0.49, 1)
        _ShallowColor("Shallow water", Color) = (0.15, 0.78, 0.69, 1)
        _Opacity("Opacity (0.4 = 60% transparent)", Range(0,1)) = 0.4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "Waves"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half _Opacity;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float2 positionOS : TEXCOORD1; half facetTint : TEXCOORD2; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 position = input.positionOS.xyz;
                float t = _Time.y;
                position.y += sin(t * 1.1 + position.x * 1.7 + position.z * .8) * .08
                    + sin(t * .8 + position.z * 2.3 - position.x * .4) * .035;
                output.positionWS = TransformObjectToWorld(position);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.positionOS = position.xz;
                output.facetTint = input.color.r;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Derivatives produce one flat normal per moving triangle, without CPU mesh updates.
                float3 normal = normalize(cross(ddy(input.positionWS), ddx(input.positionWS)));
                normal *= normal.y < 0 ? -1 : 1;
                float3 light = normalize(float3(-.4, .8, -.3));
                float3 view = normalize(GetWorldSpaceViewDir(input.positionWS));
                half shallow = 1 - smoothstep(.7, 2.7, length(input.positionOS));
                half3 water = lerp(_DeepColor.rgb, _ShallowColor.rgb, shallow);
                half facets = saturate(dot(normal, light));
                half glint = pow(saturate(dot(normal, normalize(light + view))), 72);
                half fresnel = pow(1 - saturate(dot(normal, view)), 3);
                water *= (.45 + facets * .85) * input.facetTint;
                water += glint * .40 + fresnel * half3(.10, .19, .20);
                return half4(water, _Opacity);
            }
            ENDHLSL
        }
    }
}
