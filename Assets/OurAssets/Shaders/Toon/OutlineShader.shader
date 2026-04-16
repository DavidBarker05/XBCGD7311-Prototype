Shader "Toon/OutlineShader"
{
    Properties
    {
		_Scale("Scale", Integer) = 2
		_DepthThreshold("Depth Threshold", Float) = 1.5
		_NormalThreshold("Normal Threshold", Float) = 0.4
		_DepthNormalThreshold("Depth Normal Threshold", Float) = 4
		_DepthNormalThresholdScale("Depth Normal Threshold Scale", Float) = 7
		_OutlineColour("OutlineColour", Color) = (0, 0, 0, 1)
    }

	// Based on tutorial by Roystan: https://roystan.net/articles/outline-shader/
	// Used his techniques and ported over to URP

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "CustomPass"
            ZWrite Off
            Cull Off
			ZTest Always
            
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DynamicScaling.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex vert
            #pragma fragment frag

			struct Attributes
			{
				uint vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float2 texcoord   : TEXCOORD0;
				float3 viewSpaceDirection : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			#ifdef USE_FULL_PRECISION_BLIT_TEXTURE
			TEXTURE2D_X_FLOAT(_BlitTexture);
			#else
			TEXTURE2D_X(_BlitTexture);
			#endif
			TEXTURECUBE(_BlitCubeTexture);

			uniform float4 _BlitScaleBias;
			uniform float4 _BlitScaleBiasRt;
			uniform float4 _BlitTexture_TexelSize;
			uniform float _BlitMipLevel;
			uniform float2 _BlitTextureSize;
			uniform uint _BlitPaddingSize;
			uniform int _BlitTexArraySlice;
			uniform float4 _BlitDecodeInstructions;

			CBUFFER_START(UnityPerMaterial)
                int _Scale;
				float _DepthThreshold;
				float _NormalThreshold;
				float _DepthNormalThreshold;
				float _DepthNormalThresholdScale;
				float4 _OutlineColour;
            CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				float4 pos = GetFullScreenTriangleVertexPosition(IN.vertexID);
				float2 uv = GetFullScreenTriangleTexCoord(IN.vertexID);

				OUT.positionHCS = pos;
				OUT.texcoord = DYNAMIC_SCALING_APPLY_SCALEBIAS(uv);

				OUT.viewSpaceDirection = mul(unity_CameraInvProjection , OUT.positionHCS).xyz;

				return OUT;
			}

			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);
				return float4(color, alpha);
			}

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord;
				float halfScaleFloor = floor(_Scale * 0.5);
				float halfScaleCeil = ceil(_Scale * 0.5);

				float2 bottomLeftUV = uv - float2(_BlitTexture_TexelSize.x, _BlitTexture_TexelSize.y) * halfScaleFloor;
				float2 topRightUV = uv + float2(_BlitTexture_TexelSize.x, _BlitTexture_TexelSize.y) * halfScaleCeil;  
				float2 bottomRightUV = uv + float2(_BlitTexture_TexelSize.x * halfScaleCeil, -_BlitTexture_TexelSize.y * halfScaleFloor);
				float2 topLeftUV = uv + float2(-_BlitTexture_TexelSize.x * halfScaleFloor, _BlitTexture_TexelSize.y * halfScaleCeil);

				float depth0 = SampleSceneDepth(bottomLeftUV);
				float depth1 = SampleSceneDepth(topRightUV);
				float depth2 = SampleSceneDepth(bottomRightUV);
				float depth3 = SampleSceneDepth(topLeftUV);

				float3x3 worldNormalToViewMatrix = (float3x3)UNITY_MATRIX_MV;
				float3 normal0 = mul(worldNormalToViewMatrix, SampleSceneNormals(bottomLeftUV));
				float3 normal1 = mul(worldNormalToViewMatrix, SampleSceneNormals(topRightUV));
				float3 normal2 = mul(worldNormalToViewMatrix, SampleSceneNormals(bottomRightUV));
				float3 normal3 = mul(worldNormalToViewMatrix, SampleSceneNormals(topLeftUV));

				float3 normal = mul(worldNormalToViewMatrix, SampleSceneNormals(uv));
				float3 viewNormal = normal * 2 - 1;
				float NdotV = 1 - dot(viewNormal, -IN.viewSpaceDirection);

				float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
				float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;

				float depthFiniteDifference0 = depth1 - depth0;
				float depthFiniteDifference1 = depth3 - depth2;

				float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100;

				float depth = SampleSceneDepth(uv);
				float depthThreshold = _DepthThreshold * depth * normalThreshold;
				edgeDepth = edgeDepth > depthThreshold ? 1 : 0;

				float3 normalFiniteDifference0 = normal1 - normal0;
				float3 normalFiniteDifference1 = normal3 - normal2;

				float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
				edgeNormal = edgeNormal >_NormalThreshold ? 1 : 0;

				float edge = max(edgeDepth, edgeNormal);

				if (edge == 0) discard;

				return _OutlineColour;
            }
            ENDHLSL
        }
    }
}
