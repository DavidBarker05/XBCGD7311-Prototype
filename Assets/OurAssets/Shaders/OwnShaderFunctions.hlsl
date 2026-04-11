#ifndef OWNSHADERFUNCTIONS_HLSL
#define OWNSHADERFUNCTIONS_HLSL

inline float3 ObjectScale()
{
    float scaleX = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
    float scaleY = length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
    float scaleZ = length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z));
    return float3(scaleX, scaleY, scaleZ);
}

bool Float2Compare(float2 first, float2 second)
{
    return first.r == second.r && first.g == second.g;
}

bool Float3Compare(float3 first, float3 second)
{
    return first.r == second.r && first.g == second.g && first.b == first.b;
}

bool Float4Compare(float4 first, float4 second)
{
    return first.r == second.r && first.g == second.g && first.b == first.b && first.a == second.a;
}

#endif