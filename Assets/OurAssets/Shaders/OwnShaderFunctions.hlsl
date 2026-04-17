#ifndef OWNSHADERFUNCTIONS_HLSL
#define OWNSHADERFUNCTIONS_HLSL

#ifndef EMPTY
#define EMPTY(type) (type)0
#endif

inline float3 ObjectScale()
{
    float scaleX = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
    float scaleY = length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
    float scaleZ = length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z));
    return float3(scaleX, scaleY, scaleZ);
}

inline float Sqr(float x)
{
    return x * x;
}

inline float2 Sqr(float2 xy)
{
    return xy * xy;
}

inline float3 Sqr(float3 xyz)
{
    return xyz * xyz;
}

inline float4 Sqr(float4 xyzw)
{
    return xyzw * xyzw;
}

inline float CircleMask(float2 xy, float radius)
{
    return Sqr(xy.x) + Sqr(xy.y) <= Sqr(radius) ? 1 : 0;
}

inline float CircleMask(float2 xy, float radius, float lineThickness)
{
    return Sqr(xy.x) + Sqr(xy.y) >= Sqr(radius - lineThickness) && Sqr(xy.x) + Sqr(xy.y) <= Sqr(radius + lineThickness) ? 1 : 0;
}

inline bool Float2Compare(float2 first, float2 second)
{
    return first.r == second.r && first.g == second.g;
}

inline bool Float3Compare(float3 first, float3 second)
{
    return first.r == second.r && first.g == second.g && first.b == first.b;
}

inline bool Float4Compare(float4 first, float4 second)
{
    return first.r == second.r && first.g == second.g && first.b == first.b && first.a == second.a;
}

inline float3 ProjectVector(float3 a, float3 b)
{
    return b * (dot(a, b) / Sqr(length(b)));
}

inline float3 ProjectVectorOnPlane(float3 v, float3 n)
{
    n = normalize(n);
    return v - n * dot(v, n);
}

#endif