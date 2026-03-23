#ifndef VAT_DOTS_COMMON_INCLUDED
#define VAT_DOTS_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_VATPositionTex);
SAMPLER(sampler_VATPositionTex);
TEXTURE2D(_VATNormalTex);
SAMPLER(sampler_VATNormalTex);
TEXTURE2D(_VATMetaTex);
SAMPLER(sampler_VATMetaTex);

float4 _VATPositionTex_TexelSize;
float4 _VATNormalTex_TexelSize;
float4 _VATMetaTex_TexelSize;
float4 _VATBoundsMin;
float4 _VATBoundsExtent;

struct VATAnimStateData
{
    float animIndex;
    float animTime;
    float prevAnimIndex;
    float prevAnimTime;
    float blend;
    float pad0;
    float pad1;
    float pad2;
};

StructuredBuffer<VATAnimStateData> _VATAnimStateBuffer;


struct VATClipMeta
{
    uint rowStart;
    uint frameCount;
    float clipLength;
    uint meshIndex;
};

float2 VATPixelCenter(uint x, uint y, float4 texelSize)
{
    return float2((x + 0.5) * texelSize.x, (y + 0.5) * texelSize.y);
}

VATAnimStateData VATLoadAnimState()
{
    return _VATAnimStateBuffer[unity_RendererUserValue];
}

VATClipMeta VATLoadClipMeta(float clipIndex)
{
    VATClipMeta meta;
    float safeIndex = max(clipIndex, 0.0);
    float2 uv = float2((safeIndex + 0.5) * _VATMetaTex_TexelSize.x, 0.5 * _VATMetaTex_TexelSize.y);
    float4 data = SAMPLE_TEXTURE2D_LOD(_VATMetaTex, sampler_VATMetaTex, uv, 0);

    meta.rowStart = (uint)max(data.x, 0.0);
    meta.frameCount = max((uint)max(data.y, 1.0), 1u);
    meta.clipLength = max(data.z, 1e-5);
    meta.meshIndex = (uint)max(data.w, 0.0);
    return meta;
}

float3 VATDecodePosition(float4 encoded)
{
    return _VATBoundsMin.xyz + encoded.xyz * _VATBoundsExtent.xyz;
}

float3 VATDecodeNormal(float4 encoded)
{
    return normalize(encoded.xyz * 2.0 - 1.0);
}

void VATGetRows(VATClipMeta meta, float time01, out uint rowA, out uint rowB, out float frameT)
{
    float wrapped = frac(max(time01, 0.0));
    float frame = wrapped * meta.frameCount;
    uint frameIndex = (uint)floor(frame);
    frameT = frac(frame);

    rowA = meta.rowStart + (frameIndex % meta.frameCount);
    rowB = meta.rowStart + ((frameIndex + 1u) % meta.frameCount);
}

void VATSampleFrame(uint vertexID, uint row, out float3 positionOS, out float3 normalOS)
{
    float2 uv = VATPixelCenter(vertexID, row, _VATPositionTex_TexelSize);
    float4 encodedPosition = SAMPLE_TEXTURE2D_LOD(_VATPositionTex, sampler_VATPositionTex, uv, 0);
    float4 encodedNormal = SAMPLE_TEXTURE2D_LOD(_VATNormalTex, sampler_VATNormalTex, uv, 0);

    positionOS = VATDecodePosition(encodedPosition);
    normalOS = VATDecodeNormal(encodedNormal);
}

void VATSampleClip(uint vertexID, float clipIndex, float time01, out float3 positionOS, out float3 normalOS)
{
    VATClipMeta meta = VATLoadClipMeta(clipIndex);

    uint rowA;
    uint rowB;
    float frameT;
    VATGetRows(meta, time01, rowA, rowB, frameT);

    float3 posA;
    float3 posB;
    float3 nrmA;
    float3 nrmB;
    VATSampleFrame(vertexID, rowA, posA, nrmA);
    VATSampleFrame(vertexID, rowB, posB, nrmB);

    positionOS = lerp(posA, posB, frameT);
    normalOS = normalize(lerp(nrmA, nrmB, frameT));
}

float3 VATFallbackPerpendicular(float3 normalOS)
{
    float3 axis = abs(normalOS.y) < 0.99 ? float3(0.0, 1.0, 0.0) : float3(1.0, 0.0, 0.0);
    return normalize(cross(axis, normalOS));
}

float4 VATRebuildTangent(float4 tangentOS, float3 normalOS)
{
    float3 tangent = tangentOS.xyz - normalOS * dot(normalOS, tangentOS.xyz);
    float tangentLenSq = dot(tangent, tangent);
    tangent = tangentLenSq > 1e-6 ? tangent * rsqrt(tangentLenSq) : VATFallbackPerpendicular(normalOS);
    return float4(tangent, tangentOS.w);
}

void VATSampleBlended(uint vertexID, out float3 positionOS, out float3 normalOS)
{
    VATAnimStateData state = VATLoadAnimState();

    float3 currentPos;
    float3 currentNrm;
    VATSampleClip(vertexID, state.animIndex, state.animTime, currentPos, currentNrm);

    if (state.prevAnimIndex >= 0.0 && state.blend < 0.9999)
    {
        float3 previousPos;
        float3 previousNrm;
        VATSampleClip(vertexID, state.prevAnimIndex, state.prevAnimTime, previousPos, previousNrm);
        positionOS = lerp(previousPos, currentPos, saturate(state.blend));
        normalOS = normalize(lerp(previousNrm, currentNrm, saturate(state.blend)));
    }
    else
    {
        positionOS = currentPos;
        normalOS = currentNrm;
    }
}

#endif
