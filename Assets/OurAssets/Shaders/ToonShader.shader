Shader "Custom/ToonShader"
{
    Properties
    {
        [Toggle] _AlphaClipping("Alpha Clipping", Integer) = 0
        _AlphaClippingThreshold("Alpha Clipping Threshold", Range(0, 1)) = 0
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _ToonShadowTint("Toon Shadow Tint", Color) = (0.4, 0.4, 0.4)
        _ToonShadowSmoothness("Toon Shadow Smoothness", Range(0, 1)) = 0.01
        _ToonSpecularTint("Toon Specular Tint", Color) = (0.9, 0.9, 0.9)
        _ToonGlossiness("Toon Glossiness", Range(0, 1)) = 0
        _ToonRimTint("Toon Rim Tint", Color) = (1, 1, 1)
        _ToonRimAmount("Toon Rim Amount", Range(0, 1)) = 0
        _ToonRimThreshold("Toon Rim Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardPass"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS _ADDITIONAL_LIGHT_SHADOWS_CASCADE _ADDITIONAL_LIGHT_SHADOWS_SCREEN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/OurAssets/Shaders/OwnShaderFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 texcoord1: TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2);
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                bool _AlphaClipping;
                float _AlphaClippingThreshold;
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float3 _ToonShadowTint;
                float _ToonShadowSmoothness;
                float3 _ToonSpecularTint;
                float _ToonGlossiness;
                float3 _ToonRimTint;
                float _ToonRimAmount;
                float _ToonRimThreshold;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUTPUT_LIGHTMAP_UV(IN.texcoord1, unity_LightmapST, OUT.lightmapUV);
                OUTPUT_SH(OUT.normalWS, OUT.vertexSH);
                return OUT;
            }

            Light MainLight(float4 positionHCS, float3 positionWS)
            {
                Light mainLight;
                #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS)
                    float4 shadowCoord;
                    #if SHADOWS_SCREEN
                        shadowCoord = ComputeScreenPos(positionHCS.xyz);
                    #else
                        shadowCoord = TransformWorldToShadowCoord(positionWS);
                    #endif
                    mainLight = GetMainLight(shadowCoord);
                #else
                    mainLight = GetMainLight();
                #endif
                return mainLight;
            }

            float Glossiness()
            {
                return exp2(10 * _ToonGlossiness + 1);
            }

            float Shadow (Light light)
            {
                return light.distanceAttenuation * light.shadowAttenuation;
            }

            float LightIntensity(Light light, float NdotL)
            {
                float shadow = Shadow(light);
                return smoothstep(0, _ToonShadowSmoothness, NdotL * shadow);
            }

            float3 SpecularTint(Light light, float lightIntensity, InputData inputData)
            {
                float3 halfVector = normalize(light.direction + inputData.viewDirectionWS);
                float NdotH = dot(inputData.normalWS, halfVector);
                float specularIntensity = pow(saturate(NdotH * lightIntensity), Glossiness());
                float smoothSpecularIntensity = smoothstep(0, 0.01, specularIntensity);
                return _ToonSpecularTint * smoothSpecularIntensity;
            }

            float3 RimTint(InputData inputData, float NdotL)
            {
                if (_ToonRimAmount == 0) return 0;
                float rimDot = 1 - dot(inputData.viewDirectionWS, inputData.normalWS);
                float invToonRimThreshold = 1 - _ToonRimThreshold;
                float rimStep = rimDot * pow(saturate(NdotL), invToonRimThreshold);
                float invToonRimAmount = 1 - _ToonRimAmount;
                float rimIntensity = smoothstep(invToonRimAmount - 0.01, invToonRimAmount + 0.01, rimStep);
                return _ToonRimTint * rimIntensity;
            }

            float3 ToonTintNoShadow(Light light)
            {
                return light.color * light.distanceAttenuation;
            }

            float3 ToonTintShadow(Light light, InputData inputData)
            {
                if (Float3Compare(light.color, 0)) return 0;
                float NdotL = dot(normalize(light.direction), inputData.normalWS);
                float lightIntensity = LightIntensity(light, NdotL);
                float3 lightTint = light.color * lightIntensity;
                float3 specularTint = SpecularTint(light, lightIntensity, inputData);
                float3 rimTint = RimTint(inputData, NdotL);
                return lightTint + specularTint + rimTint;
            }

            float3 AdditionalLight(Light light, InputData inputData)
            {
                #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    return ToonTintShadow(light, inputData);
                #else
                    return ToonTintNoShadow(light);
                #endif
            }
            
            float3 AdditionalLightLoop(InputData inputData)
            {
                float3 additionalTint = 0;
                #if USE_CLUSTER_LIGHT_LOOP
                    UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                        additionalTint += AdditionalLight(light, inputData);
                    }
                #endif
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
                    additionalTint += AdditionalLight(light, inputData);
                LIGHT_LOOP_END
                return additionalTint;
            }

            float3 ToonTint(float4 positionHCS, InputData inputData)
            {
                float3 tint = _ToonShadowTint;
                Light mainLight = MainLight(positionHCS, inputData.positionWS);
                float3 mainTint;
                #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS)
                    mainTint = ToonTintShadow(mainLight, inputData);
                #else
                    mainTint = ToonTintNoShadow(mainLight);
                #endif
                tint += mainTint;
                #if defined(_ADDITIONAL_LIGHTS)
                    tint += AdditionalLightLoop(inputData);
                #endif
                return tint;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                if (_AlphaClipping) clip(colour.a - _AlphaClippingThreshold);
                InputData inputData;
                ZERO_INITIALIZE(InputData, inputData);
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(IN.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
                inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUV);
                float3 toonTint = ToonTint(IN.positionHCS, inputData);
                float4 tint = float4(toonTint, 1);
                return colour * tint;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float3 _LightDirection;

            float4 GetShadowPositionHClip(Attributes IN)
            {
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = normalize(TransformObjectToWorldNormal(IN.normalOS));
                float3 shadowPositionWS = ApplyShadowBias(positionWS, normalWS, _LightDirection);
                float4 shadowPositionHCS = ApplyShadowClamping(TransformWorldToHClip(shadowPositionWS));
                return shadowPositionHCS;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = GetShadowPositionHClip(IN);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask R

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
                return IN.positionHCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }

            ZWrite On

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 normalWS = NormalizeNormalPerPixel(IN.normalWS);
                return float4(normalWS, 1);
            }
            ENDHLSL
        }
    }
}
