#ifndef UNIVERSAL_BASE_CORE_INCLUDED
#define UNIVERSAL_BASE_CORE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// 深度纹理
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_ScreenTextures_linear_clamp);

// 获取片段世界坐标与深度纹理
inline half4 ConvertScreenPosToWorldSpace(half4 ScreenPosition)
{
    half2 position = ScreenPosition.xy / ScreenPosition.w;
    half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_ScreenTextures_linear_clamp, position);

    half4 positionCS = half4(position * 2 - 1, depth, 1);
    half4 positionHWS = mul(UNITY_MATRIX_I_VP, positionCS);
    half3 positionWS = positionHWS.xyz / positionHWS.w;

    return half4(positionWS, depth);
}

// 片段深度与深度纹理深度的渐近值
inline half FragmentAsymptotic(half4 ScreenPosition)
{
    half2 position = ScreenPosition.xy / ScreenPosition.w;

    half depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_ScreenTextures_linear_clamp, position);
    half Out = LinearEyeDepth(depth, _ZBufferParams);

    return Out - ScreenPosition.w;
}

// 光照与阴影
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

// 内置变量，由引擎自动赋值
half3 _LightDirection;

float4 WorldToShadowCoord(float3 positionWS)
{
    return TransformWorldToShadowCoord(positionWS);
}

// 环境光采样
inline half3 LightmapOrSh(half3 normalWS, half2 lightmapUV)
{
#ifdef LIGHTMAP_ON
    return half3(lightmapUV.xy * unity_LightmapST.xy + unity_LightmapST.zw, 1);
#else
    return SampleSHVertex(normalWS);
#endif
}

inline half3 BakedGI(half3 lightmap_or_sh, half3 normalWS)
{
    // 环境光颜色
#ifdef LIGHTMAP_ON
    return SampleLightmap(lightmap_or_sh.xy, normalWS);
#else
    return SampleSHPixel(lightmap_or_sh, normalWS);
#endif
}

#endif // UNIVERSAL_BASE_CORE_INCLUDED
