Shader "Toon/ToonShader"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Culling", Integer) = 2 // 0 = Render Front and Back, 1 = Render Back Only, 2 = Render Front Only

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
            Cull[_Cull]
        
            HLSLPROGRAM
        
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS _ADDITIONAL_LIGHT_SHADOWS_CASCADE _ADDITIONAL_LIGHT_SHADOWS_SCREEN
        
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonAttributes.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonInput.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonFunctions.hlsl"
        
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2);
            };
        
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUTPUT_LIGHTMAP_UV(IN.lightingData, unity_LightmapST, OUT.lightmapUV);
                OUTPUT_SH(OUT.normalWS, OUT.vertexSH);
                return OUT;
            }
        
            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = SAMPLE_BASE();
                ALPHA_CLIP(colour.a, _AlphaClippingThreshold);
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
            #include "Assets/OurAssets/Shaders/Toon/ToonAttributes.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonInput.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonFunctions.hlsl"

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
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
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = SAMPLE_BASE();
                ALPHA_CLIP(colour.a, _AlphaClippingThreshold);
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
            #include "Assets/OurAssets/Shaders/Toon/ToonAttributes.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonInput.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonFunctions.hlsl"

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
                float4 colour = SAMPLE_BASE();
                ALPHA_CLIP(colour.a, _AlphaClippingThreshold);
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
            #include "Assets/OurAssets/Shaders/Toon/ToonAttributes.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonInput.hlsl"
            #include "Assets/OurAssets/Shaders/Toon/ToonFunctions.hlsl"

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = SAMPLE_BASE();
                ALPHA_CLIP(colour.a, _AlphaClippingThreshold);
                float3 normalWS = NormalizeNormalPerPixel(IN.normalWS);
                return float4(normalWS, 1);
            }
            ENDHLSL
        }
    }
}
