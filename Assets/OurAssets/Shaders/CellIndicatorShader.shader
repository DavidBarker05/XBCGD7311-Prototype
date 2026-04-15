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

            float Edge(float axis, float axisSize, float axisScale)
            {
                if (axisSize == 0 || axisScale == 0) return 1;
                float halfSize = axisSize / 2;
                float scaledHalfSize = halfSize * axisScale;
                float scaledLineThickness = _LineThickness / scaledHalfSize;
                float a = abs(axis);
                float a1 = a / halfSize;
                float a2 = 1 - a1;
                return smoothstep(a2, scaledLineThickness, 0);
            }

            float Edges(float4 positionOS, float3 scale)
            {
                float x = Edge(positionOS.x, _DefaultSize.x, scale.x);
                float y = Edge(positionOS.y, _DefaultSize.y, scale.y);
                float z = Edge(positionOS.z, _DefaultSize.z, scale.z);
                return 1 - (x * y * z);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 colour = _LineColour;
                colour.a *= Edges(IN.positionOS, IN.scale);
                return colour;
            }
            ENDHLSL
        }
    }
}
