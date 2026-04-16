#ifndef TOONINPUT_HLSL
#define TOONINPUT_HLSL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
        
CBUFFER_START(UnityPerMaterial)
    bool _AlphaClipping;
    float _AlphaClippingThreshold;
            
    float4 _BaseColor;
    float4 _BaseMap_ST;
            
    float3 _ToonShadowTint;
    float _ToonShadowSmoothness;
    float _SSAOStrength;
            
    float3 _ToonSpecularTint;
    float _ToonGlossiness;
            
    float3 _ToonRimTint;
    float _ToonRimAmount;
    float _ToonRimThreshold;
CBUFFER_END
#endif