#include "Constants.fxh"

struct Material
{
    float4 Diffuse;
    float4 Ambient;
    float4 Specular;
    float4 Emissive;
    float Power;
};

struct Lighting
{
    float4 Diffuse      : COLOR0;
    float4 Specular     : COLOR1;
};

struct DirectionalLightInfo
{
    int Count;
    int SunIndex;
    float4 Ambient;
    float4 Diffuse[MAX_DIRECTIONAL_LIGHTS];
    float4 Specular[MAX_DIRECTIONAL_LIGHTS];
    float4 Direction[MAX_DIRECTIONAL_LIGHTS];
};

struct PointLightInfo
{
    int Count;
    float4 Position[MAX_POINT_LIGHTS];
    float4 Diffuse[MAX_POINT_LIGHTS];
    float Range[MAX_POINT_LIGHTS];
};

struct TextureBlendStage
{
    int colorOp;
    int colorArg1;
    int colorArg2;
    int alphaOp;
    int alphaArg1;
    int alphaArg2;
};

struct BlendStageInfo
{
    int Count;
    TextureBlendStage BlendStages[MAX_BLEND_STAGES];
};

struct ColorMixMode
{
    int BumpMappingEnabled;
    int EnvironmentMappingEnabled;
};

float4 CalculateDirectionalDiffuse(float3 N, float3 L, float4 diffuseColor)
{
    float NDotL = dot(N, L);
    float4 finalColor = 0;
    if (NDotL > 0.0f)
    {
        finalColor = max(0, NDotL * diffuseColor);
    }

    return finalColor;
}

float4 CalculateDirectionalSpecular(float3 worldPos, float3 cameraPosition, float3 N, float3 L, float4 specularColor, float specularPower)
{
    float4 finalColor = 0;
    if (specularPower)
    {
        float3 toEye = normalize(cameraPosition - worldPos);
        float3 halfway = normalize(toEye + L);
        float NDotH = saturate(dot(halfway, N));

        finalColor = max(0, pow(NDotH, specularPower) * specularColor);
    }

    return finalColor;
}

inline float4 CalculatePointDiffuse(float3 worldPos, float3 N, float3 L, float range, float4 diffuseColor)
{
    float diffuseLighting = saturate(dot(N, L));

    diffuseLighting *= (range / dot(worldPos, worldPos));
    return diffuseLighting * diffuseColor;
}