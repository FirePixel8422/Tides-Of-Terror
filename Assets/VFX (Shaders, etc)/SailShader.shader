Shader "Custom/SailLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _WindStrength ("Wind Strength", Float) = 0.3
        _WindSpeed ("Wind Speed", Float) = 1.5
        _WaveScale ("Wave Scale", Float) = 3.0
        _WindDirection ("Wind Direction (X,Z)", Vector) = (1,0,1,0)
        _LightStrength ("Light Strength", Range(0,5)) = 1.0
        _AmbientStrength ("Ambient Strength", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Cull Off // render both sides

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _WindStrength;
                float _WindSpeed;
                float _WaveScale;
                float4 _WindDirection;
                float _LightStrength;
                float _AmbientStrength;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 pos = IN.positionOS.xyz;
                float2 windDir = normalize(_WindDirection.xz);
                float t = _Time.y * _WindSpeed;

                float wavePhase = dot(pos.xz, windDir) * _WaveScale + t;
                float wave = sin(wavePhase);
                pos.y += wave * _WindStrength;

                OUT.positionCS = TransformObjectToHClip(pos);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(pos);

                return OUT;
            }

            float3 ApplyAdditionalLights(float3 positionWS, float3 normalWS, float3 baseColor)
            {
                float3 lighting = 0;

                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint i = 0; i < pixelLightCount; ++i)
                {
                    Light light = GetAdditionalLight(i, positionWS);
                    float3 L = normalize(light.direction);
                    float NdotL = saturate(dot(normalWS, L));

                    lighting += baseColor * (light.color.rgb * NdotL * light.distanceAttenuation * light.shadowAttenuation);
                }

                return lighting;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float3 baseColor = texColor.rgb * _BaseColor.rgb;

                Light mainLight = GetMainLight();

                float3 N = normalize(IN.normalWS);
                if (dot(N, IN.positionWS - _WorldSpaceCameraPos) > 0)
                {
                    N = -N;
                }

                float3 L = normalize(mainLight.direction);
                float NdotL = saturate(dot(N, L));

                float3 litColor = baseColor * (mainLight.color.rgb * NdotL * _LightStrength);

                // Additional lights
                litColor += ApplyAdditionalLights(IN.positionWS, N, baseColor);

                // Ambient fill
                litColor += baseColor * _AmbientStrength;

                return float4(litColor, 1.0);
            }

            ENDHLSL
        }
    }
}
