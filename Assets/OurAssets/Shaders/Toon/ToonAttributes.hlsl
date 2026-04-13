#ifndef TOONATTRIBUTES_HLSL
#define TOONATTRIBUTES_HLSL

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
    float4 lightingData : TEXCOORD1;
};
#endif