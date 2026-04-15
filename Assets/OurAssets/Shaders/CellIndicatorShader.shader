Shader "Custom/CellIndicatorShader"
{
    Properties
    {
        [MainColor] _LineColour("Line Colour", Color) = (1, 1, 1, 1)
        _LineThickness("Line Thickness", Float) = 0.2
        _DefaultSize("Default Size", Vector, 3) = (10, 0, 10)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

        Pass
        {
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
                float3 scale : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColour;
                float _LineThickness;
                float3 _DefaultSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                ZERO_INITIALIZE(Varyings, OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionOS = IN.positionOS;
                OUT.scale = ObjectScale();
                return OUT;
            }

			float Square(float2 xy, float2 sideLengths, float2 scale, float lineThickness)
			{
				float2 scaledXY = xy * scale;
				float2 scaledSideLengths = sideLengths * scale;
				if (abs(scaledXY.x) >= scaledSideLengths.x / 2 - lineThickness) return 1;
				if (abs(scaledXY.y) >= scaledSideLengths.y / 2 - lineThickness) return 1;
				return 0;
			}

            float4 frag(Varyings IN) : SV_Target
            {
				float square = Square(IN.positionOS.xz, _DefaultSize.xz, IN.scale.xz, _LineThickness);
                float4 colour = _LineColour * square;
                return colour;
            }
            ENDHLSL
        }
    }
}
