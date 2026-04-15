Shader "Custom/GridShader"
{
    Properties
    {
        [MainColor] _LineColour("Line Colour", Color) = (1, 1, 1, 1)
        _LineThickness("Line Thickness", Float) = 0.1
        _CellSize("Cell Size", Vector, 3) = (1, 1, 1, 0)
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
                float3 _CellSize;
                float3 _Tiling;
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

            float GridLine(float axis, float axisSize, float axisScale)
            {
                if (axisSize == 0 || axisScale == 0) return 1;
                float scaledLineThickness = _LineThickness / axisScale;
                float scaledAxisSize = axisSize / axisScale;
                float a = abs(axis);
                float a1 = abs(a + scaledLineThickness / 2);
                float a2 = a1 % scaledAxisSize;
                return smoothstep(a2, scaledLineThickness, 0);
            }

            float Grid(float4 positionOS, float3 scale)
            {
                float x = GridLine(positionOS.x, _CellSize.x, scale.x);
                float y = GridLine(positionOS.y, _CellSize.y, scale.y);
                float z = GridLine(positionOS.z, _CellSize.z, scale.z);
                return 1 - (x * y * z);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = _LineColour;
                colour.a *= Grid(IN.positionOS, IN.scale);
                return colour;
            }
            ENDHLSL
        }
    }
}
