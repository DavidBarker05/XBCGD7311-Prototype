Shader "Custom/CircleMarkerShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
		_MaxLineThickness("Max Line Thickness", Range(0.001, 1)) = 0.05
		_SquareSize("Base Square Size", Float) = 10
		_NumCircles("Number of Circles", Integer) = 5
    }

    SubShader
    {
        Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
			"Queue" = "Transparent"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

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
				float4 positionOS : TEXCOORD0;
				float3 size : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
				float _MaxLineThickness;
				float _SquareSize;
				int _NumCircles;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = EMPTY(Varyings);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.positionOS = IN.positionOS;
				OUT.size = ObjectScale();
                return OUT;
            }

			float Circle(float2 xy, float radius, float lineThickness)
			{
				return Sqr(xy.x) + Sqr(xy.y) >= Sqr(radius - lineThickness) && Sqr(xy.x) + Sqr(xy.y) <= Sqr(radius + lineThickness) ? 1 : 0;
			}

            float4 frag(Varyings IN) : SV_Target
            {
				float baseHalfExtents = abs(_SquareSize) / 2 - _MaxLineThickness;
				float size = IN.size.x;
				float radius = (baseHalfExtents / float(_NumCircles));
				float lineThickness = _MaxLineThickness * min(abs(size), 1 / abs(size)) * min((baseHalfExtents / float(_NumCircles)), 1);
				float circle = 0;
				for (int i = 1; i <= _NumCircles && circle == 0; ++i)
				{
					circle = Circle(IN.positionOS.xz, radius * i, lineThickness);
				}
                float4 color = _BaseColor * circle;
                return color;
            }
            ENDHLSL
        }
    }
}
