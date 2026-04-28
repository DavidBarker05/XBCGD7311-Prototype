#ifndef TOONFUNCTIONS_HLSL
#define TOONFUNCTIONS_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/OurAssets/Shaders/OwnShaderFunctions.hlsl"

#ifndef SAMPLE_BASE
#define SAMPLE_BASE() SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor
#endif

#ifndef ALPHA_CLIP
#define ALPHA_CLIP(alpha, threshold) if(_AlphaClipping) clip(alpha - threshold)
#endif

Light MainLight(float4 positionHCS, float3 positionWS)
{
    Light mainLight;
    #if defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS)
        float4 shadowCoord;
        #if SHADOWS_SCREEN
            shadowCoord = ComputeScreenPos(positionHCS.xyz);
        #else
            shadowCoord = TransformWorldToShadowCoord(positionWS);
        #endif
        mainLight = GetMainLight(shadowCoord);
    #else
        mainLight = GetMainLight();
    #endif
    return mainLight;
}
        
float Glossiness()
{
    return exp2(10 * _ToonGlossiness + 1);
}
        
float3 SpecularTint(Light light, float lightIntensity, InputData inputData)
{
    float3 halfVector = normalize(light.direction + inputData.viewDirectionWS);
    float NdotH = dot(inputData.normalWS, halfVector);
    float specularIntensity = pow(saturate(NdotH * lightIntensity), Glossiness());
    float smoothSpecularIntensity = smoothstep(0, 0.01, specularIntensity);
    return _ToonSpecularTint * smoothSpecularIntensity;
}
        
float3 RimTint(InputData inputData, float NdotL)
{
    if (_ToonRimAmount == 0) return 0;
    float rimDot = 1 - dot(inputData.viewDirectionWS, inputData.normalWS);
    float invToonRimThreshold = 1 - _ToonRimThreshold;
    float rimStep = rimDot * pow(saturate(NdotL), invToonRimThreshold);
    float invToonRimAmount = 1 - _ToonRimAmount;
    float rimIntensity = smoothstep(invToonRimAmount - 0.01, invToonRimAmount + 0.01, rimStep);
    return _ToonRimTint * rimIntensity;
}
        
//float3 ToonTintSingle(Light light, InputData inputData)
//{
//    return saturate(1.0 - floor(length(light.direction) * 3) / 3);
//    if (Float3Compare(light.color, 0)) return 0;
//    float NdotL = dot(inputData.normalWS, normalize(light.direction));
//    float lightIntensity = LightIntensity(light, NdotL);
//    float3 lightTint = light.color * lightIntensity;
//    float3 specularTint = SpecularTint(light, lightIntensity, inputData);
//    float3 rimTint = RimTint(inputData, NdotL);
//    return lightTint + specularTint + rimTint;
//}

float3 ToonTintMain(Light light, InputData inputData)
{
    if (Float3Compare(light.color, 0)) return 0;
    float NdotL = dot(inputData.normalWS, normalize(light.direction));
#if defined(_MAIN_LIGHT_SHADOWS_CASCADE) || defined(_MAIN_LIGHT_SHADOWS)
        float shadow = light.shadowAttenuation;
#else
    float shadow = 1;
#endif
    float lightIntensity = smoothstep(0, _ToonShadowSmoothness, NdotL * shadow);
    float3 lightTint = light.color * lightIntensity;
    float3 specularTint = SpecularTint(light, lightIntensity, inputData);
    float3 rimTint = RimTint(inputData, NdotL);
    return lightTint + specularTint + rimTint;
}

float AdditionalLightAttenuation(uint lightIndex, float3 positionWS, float lightBands)
{
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
	    float4 lightPositionWS = _AdditionalLightsBuffer[lightIndex].position;
	    half4 spotDirection = _AdditionalLightsBuffer[lightIndex].spotDirection;
	    half4 lightAttenuation = _AdditionalLightsBuffer[lightIndex].attenuation;
#else
    float4 lightPositionWS = _AdditionalLightsPosition[lightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[lightIndex];
    half4 lightAttenuation = _AdditionalLightsAttenuation[lightIndex];
#endif
    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), REAL_MIN);
    float range = rsqrt(lightAttenuation.x);
    float dist = sqrt(distanceSqr) / range;
    bool bSpot = lightAttenuation.z > 0;
    if (!bSpot) return saturate(1.0 - floor(dist * lightBands) / lightBands);
    float3 lightDirection = float3(lightVector * rsqrt(distanceSqr));
    float SdotL = dot(spotDirection.xyz, lightDirection);
    float spotAttenuation = Sqr(saturate(SdotL * lightAttenuation.z + lightAttenuation.w));
    float spotBands = lightBands + 1; // Always have 1 extra band because it is the centre part of light, so have central beam + bands
    return (floor(spotAttenuation * (spotBands)) / (spotBands)) * step(dist, 1);
}

float3 ToonTintAdditional(uint lightIndex, InputData inputData)
{
#if !USE_CLUSTER_LIGHT_LOOP
    lightIndex = GetPerObjectLightIndex(lightIndex);
#endif
    Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
    float NdotL = dot(inputData.normalWS, normalize(light.direction));
    float attenuation = AdditionalLightAttenuation(lightIndex, inputData.positionWS, _AdditionalLightBands);
#if defined(_ADDITIONAL_LIGHT_SHADOWS_CASCADE) || defined(_ADDITIONAL_LIGHT_SHADOWS)
        float shadow = light.shadowAttenuation;
#else
    float shadow = 1;
#endif
    float lightIntensity = smoothstep(0, _ToonShadowSmoothness, NdotL * shadow) * attenuation;
    float3 lightTint = light.color * lightIntensity;
    float3 specularTint = SpecularTint(light, lightIntensity, inputData);
    float3 rimTint = RimTint(inputData, NdotL);
    return lightTint + specularTint + rimTint;
}
            
float3 AdditionalLightLoop(InputData inputData)
{
    float3 additionalTint = 0;
    uint meshRenderingLayers = GetMeshRenderingLayer();
    float lightBands = 3;
    #if USE_CLUSTER_LIGHT_LOOP
        UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); ++lightIndex)
        {
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                additionalTint += ToonTintAdditional(lightIndex, inputData);
            }
        }
    #endif
    uint pixelLightCount = GetAdditionalLightsCount();
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
        #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
        #endif
        {
            additionalTint += ToonTintAdditional(lightIndex, inputData);
        }
    LIGHT_LOOP_END
    return additionalTint;
}
        
float3 ToonTint(float4 positionHCS, InputData inputData)
{
    float3 tint = 0;
    Light mainLight = MainLight(positionHCS, inputData.positionWS);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    #endif
    {
        tint += ToonTintMain(mainLight, inputData);
    }
    #if defined(_ADDITIONAL_LIGHTS)
        tint += AdditionalLightLoop(inputData);
    #endif
    return _ToonShadowTint + tint;
}
#endif