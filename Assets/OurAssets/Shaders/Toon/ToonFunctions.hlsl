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
        
float Shadow(Light light)
{
    return light.distanceAttenuation * light.shadowAttenuation;
}
        
float LightIntensity(Light light, float NdotL)
{
    float shadow = Shadow(light);
    return smoothstep(0, _ToonShadowSmoothness, NdotL * shadow);
}

float ToonSSAO(float ssao)
{
    return smoothstep(saturate(_SSAOStrength - _ToonShadowSmoothness), saturate(_SSAOStrength + _ToonShadowSmoothness), ssao);
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
        
float3 ToonTintSingle(Light light, InputData inputData)
{
    if (Float3Compare(light.color, 0)) return 0;
    float NdotL = dot(inputData.normalWS, normalize(light.direction));
    float lightIntensity = LightIntensity(light, NdotL);
    float3 lightTint = light.color * lightIntensity;
    float3 specularTint = SpecularTint(light, lightIntensity, inputData);
    float3 rimTint = RimTint(inputData, NdotL);
    return lightTint + specularTint + rimTint;
}
            
float3 AdditionalLightLoop(InputData inputData)
{
    float3 additionalTint = 0;
    uint meshRenderingLayers = GetMeshRenderingLayer();
    #if USE_CLUSTER_LIGHT_LOOP
        UNITY_LOOP for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); ++lightIndex)
        {
            Light light = GetAdditionalLight(lightIndex, inputData.positionWS, inputData.shadowMask);
            #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                additionalTint += ToonTintSingle(light, inputData);
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
            additionalTint += ToonTintSingle(light, inputData);
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
        tint += ToonTintSingle(mainLight, inputData);
    }
    #if defined(_ADDITIONAL_LIGHTS)
        tint += AdditionalLightLoop(inputData);
    #endif
    float ssao = SampleAmbientOcclusion(inputData.normalizedScreenSpaceUV);
    return _ToonShadowTint + tint * ToonSSAO(ssao);
}
#endif