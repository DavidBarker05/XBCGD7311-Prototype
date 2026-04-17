Shader "Custom/HoleShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
		_HoleSize("Hole Size", Float) = 0.675
    }

    SubShader
    {
        Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Assets/OurAssets/Shaders/OwnShaderFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
				float4 positionSS : TEXCOORD0;
				float4 positionOS : TEXCOORD1;
				float3 size : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
				float _HoleSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = EMPTY(Varyings);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.positionSS = ComputeScreenPos(OUT.positionHCS);
				OUT.positionOS = IN.positionOS;
				OUT.size = ObjectScale();
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
				float2 uv = IN.positionSS.xy / IN.positionSS.w;
				float rawDepth = SampleSceneDepth(uv);
				float radius = _HoleSize / IN.size.x;
				float circle = CircleMask(IN.positionOS.xz, radius);
				if (rawDepth == 0 || circle == 0) discard;
                return _BaseColor;
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

			ColorMask R

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Assets/OurAssets/Shaders/OwnShaderFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
				float4 positionOS : TEXCOORD1;
				float3 size : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
				float _HoleSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = EMPTY(Varyings);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.positionOS = IN.positionOS;
				OUT.size = ObjectScale();
                return OUT;
            }

            float frag(Varyings IN) : SV_Target
            {
				float radius = _HoleSize / IN.size.x;
				float circle = CircleMask(IN.positionOS.xz, radius);
				if (circle == 0) discard;
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

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Assets/OurAssets/Shaders/OwnShaderFunctions.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
				float3 normalWS : NORMAL;
				float4 positionOS : TEXCOORD1;
				float3 size : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
				float _HoleSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = EMPTY(Varyings);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
				OUT.positionOS = IN.positionOS;
				OUT.size = ObjectScale();
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
				float radius = _HoleSize / IN.size.x;
				float circle = CircleMask(IN.positionOS.xz, radius);
				if (circle == 0) discard;
				float3 normalWS = NormalizeNormalPerPixel(IN.normalWS);
                return float4(normalWS, 1);
            }
            ENDHLSL
        }
    }
}
