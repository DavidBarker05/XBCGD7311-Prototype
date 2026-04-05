Shader "Custom/GridShader"
{
    Properties
    {
        [MainColor] _LineColour("Line Colour", Color) = (1, 1, 1, 1)
        _LineThickness("Line Thickness", Float) = 0.1
        _CellSize("Cell Size", Vector, 3) = (1, 1, 1, 0)
        _Tiling("Tiling", Vector, 3) = (1, 1, 1, 0)
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

        Pass
        {
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
                float4 positionOS : TEXCOORD0;
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
                return OUT;
            }

            float GridLine(float axis, float axisSize)
            {
                if (axisSize == 0) return 1;
                float a = abs(axis);
                float a1 = abs(a + _LineThickness / 2);
                float a2 = a1 % axisSize;
                return smoothstep(a2, _LineThickness, 0);
            }

            float Grid(float3 positionOS)
            {
                float x = GridLine(positionOS.x, _CellSize.x / _Tiling.x);
                float y = GridLine(positionOS.y, _CellSize.y / _Tiling.y);
                float z = GridLine(positionOS.z, _CellSize.z / _Tiling.z);
                return 1 - (x * y * z);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = _LineColour;
                colour.a *= Grid(IN.positionOS.xyz);
                return colour;
            }
            ENDHLSL
        }
    }
}
