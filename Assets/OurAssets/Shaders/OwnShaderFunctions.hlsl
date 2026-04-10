float3 ObjectScale()
{
    float scaleX = length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x));
    float scaleY = length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
    float scaleZ = length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z));
    return float3(scaleX, scaleY, scaleZ);
}

float Inverse(float value)
{
    return 1 - value;
}

float2 Inverse(float2 value)
{
    return 1 - value;
}

float3 Inverse(float3 value)
{
    return 1 - value;
}

float4 Inverse(float4 value)
{
    return 1 - value;
}