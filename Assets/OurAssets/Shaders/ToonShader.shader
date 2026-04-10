Shader "Custom/ToonShader"
{
    Properties
    {
        [MainColor] _BaseColour("Base Colour", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseTexture("Base Texture", 2D) = "white" {}
        _LightColourInfluence("Light Colour Influence", Range(0, 1)) = 1
        _ToonShadowTint("Toon Shadow Tint", Color) = (0.4, 0.4, 0.4)
        _ToonShadowSmoothness("Toon Shadow Smoothness", Range(0, 1)) = 0.01
        _ToonSpecularTint("Toon Specular Tint", Color) = (0.9, 0.9, 0.9)
        _Glossiness("Glossiness", Range(0, 1)) = 0
        _ToonRimTint("Toon Rim Tint", Color) = (1, 1, 1)
        _ToonRimAmount("Toon Rim Amount", Range(0, 1)) = 0.716
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

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/OurAssets/Shaders/OwnShaderFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : NORMAL;
            };

            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColour;
                float4 _BaseTexture_ST;
                float _LightColourInfluence;
                float3 _ToonShadowTint;
                float _ToonShadowSmoothness;
                float3 _ToonSpecularTint;
                float _Glossiness;
                float3 _ToonRimTint;
                float _ToonRimAmount;
                float _ToonRimThreshold;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseTexture);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
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
                float gloss = 100 * _Glossiness;
                return gloss * gloss;
            }

            float3 LightColour(Light light)
            {
                return lerp(1, light.color, _LightColourInfluence);
            }

            float Shadow (Light light)
            {
                return light.distanceAttenuation * light.distanceAttenuation * light.shadowAttenuation;
            }

            float LightIntensity(Light light, float NdotL)
            {
                float shadow = Shadow(light);
                return smoothstep(0, _ToonShadowSmoothness, NdotL * shadow);
            }

            float3 LightTint(Light light, float lightIntensity)
            {
                return LightColour(light) * lightIntensity;
            }

            float3 SpecularTint(Light light, float lightIntensity, InputData inputData)
            {
                float3 halfVector = normalize(light.direction + inputData.viewDirectionWS);
                float NdotH = dot(inputData.normalWS, halfVector);
                float smoothSpecularIntensity = 0;
                if (_Glossiness > 0)
                {
                    float specularIntensity = pow(saturate(NdotH * lightIntensity), Glossiness());
                    smoothSpecularIntensity = smoothstep(0, 0.01, specularIntensity);
                }
                return _ToonSpecularTint * smoothSpecularIntensity;
            }

            float3 RimTint(InputData inputData, float NdotL)
            {
                if (_ToonRimAmount == 0) return 0;
                float rimDot = Inverse(dot(inputData.viewDirectionWS, inputData.normalWS));
                float invToonRimThreshold = Inverse(_ToonRimThreshold);
                float rimStep = rimDot * pow(saturate(NdotL), invToonRimThreshold);
                float invToonRimAmount = Inverse(_ToonRimAmount);
                float rimIntensity = smoothstep(invToonRimAmount - 0.01, invToonRimAmount + 0.01, rimStep);
                return _ToonRimTint * rimIntensity;
            }

            float3 ToonTintNoShadow(Light light)
            {
                return 0;
                float3 lightColour = LightColour(light);
                return lightColour * light.distanceAttenuation * light.distanceAttenuation;
            }

            float3 ToonTintShadow(Light light, InputData inputData)
            {
                float NdotL = dot(light.direction, inputData.normalWS);
                float lightIntensity = LightIntensity(light, NdotL);
                float3 lightTint = LightTint(light, lightIntensity);
                float3 specularTint = SpecularTint(light, lightIntensity, inputData);
                float3 rimTint = RimTint(inputData, NdotL);
                return _ToonShadowTint + lightTint + specularTint + rimTint;
            }

            void AccumulateTint(inout float3 tint, float3 tintToAdd)
            {
                tint += tintToAdd;
            }

            void AdditionalLight(inout float3 tint, uint index, InputData inputData)
            {
                half4 aoFactor = half4(1, 1, 1, 1);
                Light light = GetAdditionalLight(index, inputData.positionWS, aoFactor);
                float3 additionalTint;
                #if defined(_ADDITIONAL_LIGHT_SHADOWS)
                    additionalTint = ToonTintShadow(light, inputData);
                #else
                    additionalTint = ToonTintNoShadow(light);
                #endif
                AccumulateTint(tint, additionalTint);
            }
            
            void AdditionalLightLoop(inout float3 tint, InputData inputData)
            {
                #if USE_CLUSTER_LIGHT_LOOP
                    UNITY_LOOP for (uint i = 0; i < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); ++i)
                    {
                        AdditionalLight(tint, i, inputData);
                    }
                #endif
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    AdditionalLight(tint, lightIndex, inputData);
                LIGHT_LOOP_END
            }

            float3 ToonTint(float4 positionHCS, InputData inputData)
            {
                float3 tint = 0;
                Light mainLight = MainLight(positionHCS, inputData.positionWS);
                float3 mainTint = ToonTintShadow(mainLight, inputData);
                AccumulateTint(tint, mainTint);
                #if defined(_ADDITIONAL_LIGHTS)
                    AdditionalLightLoop(tint, inputData);
                #endif
                return tint;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, IN.uv) * _BaseColour;
                InputData inputData;
                ZERO_INITIALIZE(InputData, inputData);
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(IN.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionHCS);
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
            ZTest LEqual
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
