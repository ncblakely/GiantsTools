#include "../fxh/Constants.fxh"
#include "../fxh/Lighting.fxh"
#include "../fxh/Transform.fxh"
#include "../fxh/Fog.fxh"

shared texture g_Texture0;
shared texture g_Texture1;

shared DirectionalLightInfo g_DirectionalLights;
shared Material g_Material;

shared WorldTransforms g_WorldTransforms;
shared float4x4 g_EnvironmentTextureTransform;

shared float4 g_TextureFactor;
shared BlendStageInfo g_BlendStages;

shared ViewTransforms g_ViewTransforms;

shared ColorMixMode g_ColorMixMode;

sampler g_ObjTextureSampler : register(s0) =
sampler_state
{
    Texture = <g_Texture0>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

sampler g_EnvTextureSampler = 
sampler_state
{
    Texture = <g_Texture1>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

shared FogParams g_Fog;

// =======================================================
// Pixel and vertex lighting techniques
//

Lighting DoDirectionalLight(float3 worldPos, float3 N, int i)
{
    Lighting Out;
    Out.Diffuse = CalculateDirectionalDiffuse(
        N,
        -g_DirectionalLights.Direction[i],
        g_DirectionalLights.Diffuse[i]);
    Out.Specular = CalculateDirectionalSpecular(
        worldPos,
        g_ViewTransforms.CameraPosition.xyz,
        N,
        -g_DirectionalLights.Direction[i],
        g_DirectionalLights.Specular[i],
        g_Material.Power);
    return Out;
}

Lighting ComputeLighting(float3 worldPos, float3 N)
{
    Lighting finalLighting = (Lighting)0;

    for (int i = 0; i < g_DirectionalLights.Count; i++)
    {
        Lighting lighting = DoDirectionalLight(worldPos, N, i);
        finalLighting.Diffuse += lighting.Diffuse;
        finalLighting.Specular += lighting.Specular;
    }

    float4 ambient = g_Material.Ambient * g_DirectionalLights.Ambient;
    float4 diffuse = g_Material.Diffuse * finalLighting.Diffuse;
    float4 specular = g_Material.Specular * finalLighting.Specular;

    finalLighting.Diffuse = saturate(ambient + diffuse + g_Material.Emissive);
    finalLighting.Specular = saturate(specular);

    return finalLighting;
}

struct PixelLightingVSOutput
{
    float4 Pos : POSITION;
    float2 Tex0 : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
    float3 EnvMapTex : TEXCOORD3;
    float4 BumpColor : TEXCOORD4;
    float Fog : FOG;
};

float4 GetColorArg(int colorArg, float4 textureColor, float4 diffuseColor)
{
    float4 result;
    if (colorArg == D3DTA_TEXTURE) result = textureColor;
    else if (colorArg == D3DTA_DIFFUSE) result = diffuseColor;
    else if (colorArg == D3DTA_TFACTOR) result = g_TextureFactor;
    else result = float4(1.f, 1.f, 1.f, 1.f);

    return result;
}

float4 Modulate(int stageIndex, float4 textureColor, float4 diffuseColor, float factor)
{
    float4 left = GetColorArg(g_BlendStages.BlendStages[stageIndex].colorArg1, textureColor, diffuseColor); 
    float4 right = GetColorArg(g_BlendStages.BlendStages[stageIndex].colorArg2, textureColor, diffuseColor);

    return (left * right) * factor;
}

float4 ProcessStages(float4 textureColor, float4 diffuseColor)
{
    float4 output = 0;
    for (int i = 0; i < g_BlendStages.Count; i++)
    {
        if (g_BlendStages.BlendStages[i].colorOp == D3DTOP_MODULATE4X || g_BlendStages.BlendStages[i].colorOp == D3DTOP_MODULATE)
        {
            float modulateFactor = 
                (4.0f * (g_BlendStages.BlendStages[i].colorOp == D3DTOP_MODULATE4X)) +
                (1.0f * (g_BlendStages.BlendStages[i].colorOp == D3DTOP_MODULATE));

            output += Modulate(i, textureColor, diffuseColor, modulateFactor);
        }
        else if (g_BlendStages.BlendStages[i].colorOp == D3DTOP_DOTPRODUCT3)
        {
            output = float4(1, 0, 0, 1);
        }
    }

    return output;
}

struct PixelLightingPSOutput
{
    float4 Diffuse : COLOR0;
};

PixelLightingVSOutput PixelLightingVS(
    float4 vPosition : POSITION0,
    float3 vNormal : NORMAL0,
    float4 color : COLOR0,
    float2 tc : TEXCOORD0)
{
    // Simple transform, pre-compute as much as we can for the pixel shader
    PixelLightingVSOutput output = (PixelLightingVSOutput)0;

    vNormal = normalize(vNormal);
    output.Pos = mul(vPosition, g_WorldTransforms.WorldViewProjection);
    output.Normal = mul(vNormal, (float3x3)g_WorldTransforms.World);
    output.WorldPos = mul(vPosition, g_WorldTransforms.World);
    output.Tex0 = tc;
    output.BumpColor = color;

    float3 P = mul(vPosition, g_WorldTransforms.WorldView);
    float d = length(P);
    output.Fog = CalculateFogFactor(g_Fog.FogMax, g_Fog.FogMin, d);

    if (g_ColorMixMode.EnvironmentMappingEnabled)
    {
        // Generate cube texture coordinates
        // DX9 FFP formula: R = 2(E dot N) * N - E
        float3 E = normalize(g_ViewTransforms.CameraPosition.xyz - output.WorldPos);
        float3 N = mul(vNormal, (float3x3)g_WorldTransforms.WorldView);
        float4 R = float4((2.f * dot(E, N) * N - E), 0);
        output.EnvMapTex = mul(g_EnvironmentTextureTransform, R);
    }

    return output;
}

float4 PixelLightingPS(PixelLightingVSOutput input) : COLOR0
{
    float4 color = tex2D(g_ObjTextureSampler, input.Tex0);

    Lighting lighting = ComputeLighting(input.WorldPos, input.Normal);

    // Emulate FFP texture stages
    float4 finalColor = ProcessStages(color, lighting.Diffuse) + lighting.Specular;

    // Apply cubic environment mapping if enabled
    if (g_ColorMixMode.EnvironmentMappingEnabled)
    {
        float4 envMapColor = texCUBE(g_EnvTextureSampler, input.EnvMapTex);

        if (g_BlendStages.BlendStages[1].colorOp == D3DTOP_BLENDFACTORALPHA)
            finalColor = (finalColor * g_TextureFactor.a) + (envMapColor * (1 - g_TextureFactor.a));
        else if (g_BlendStages.BlendStages[1].colorOp == D3DTOP_BLENDCURRENTALPHA)
            finalColor = float4(1, 0, 0, 0);
    }

    // Apply linear pixel fog
    if (g_Fog.Enabled)
    {
        finalColor = ApplyPixelFog(finalColor, input.Fog, g_Fog.Color);
    }

    /*
    if (input.BumpColor.r != 0)
    {
        finalColor = float4(1, 1, 1, 1);
    }
    */

    return finalColor;
}

technique PixelLighting
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelLightingPS();
        VertexShader = compile vs_3_0 PixelLightingVS();
    }
}

struct VertexLightingVSOutput
{
    float4 Pos : POSITION;
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
    float2 Tex0 : TEXCOORD0;
    float4 BumpColor : TEXCOORD1;
    float Fog : FOG;
};

VertexLightingVSOutput VertexLightingVS(
    float4 vPosition  : POSITION0,
    float3 vNormal : NORMAL0,
    float4 color : COLOR0,
    float2 tc : TEXCOORD0,
    float fog : FOG)
{
    VertexLightingVSOutput output;
    output.Pos = mul(vPosition, g_WorldTransforms.WorldViewProjection);

    float4 worldPos = mul(vPosition, g_WorldTransforms.World);
    float3 normal = mul(normalize(vNormal), (float3x3)g_WorldTransforms.World);

    Lighting lighting = ComputeLighting(worldPos, normal);

    output.Diffuse = lighting.Diffuse;
    output.Specular = lighting.Specular;
    output.BumpColor = color;

    output.Fog = fog;
    output.Tex0 = tc;
    return output;
}

technique VertexLighting
{
    pass P0
    {
        VertexShader = compile vs_2_0 VertexLightingVS();
    }
}

// =======================================================
// Color per vertex
//

struct ColorPerVertexVSOutput
{
    float4 Pos : POSITION;
    float2 Tex0 : TEXCOORD0;
    float4 Color : COLOR0;
    float Fog : FOG;
};

float4 ColorPerVertexPS(ColorPerVertexVSOutput input) : COLOR0
{
    float4 color = tex2D(g_ObjTextureSampler, input.Tex0); //* input.Color;
    return color;
}

ColorPerVertexVSOutput ColorPerVertexVS(
    float4 vPosition : POSITION0,
    float2 tc : TEXCOORD0,
    float4 color : COLOR0,
    float fog : FOG)
{
    // Simple transform, pre-compute as much as we can for the pixel shader
    ColorPerVertexVSOutput output = (ColorPerVertexVSOutput)0;

    output.Pos = mul(vPosition, g_WorldTransforms.WorldViewProjection);
    output.Tex0 = tc;
    output.Color = color;

    float3 P = mul(vPosition, g_WorldTransforms.WorldView);           //position in view space
    float d = length(P);

    output.Fog = fog;
    return output;
}

technique ColorPerVertex
{
    pass P0
    {
        //PixelShader = compile ps_3_0 ColorPerVertexPS();
        VertexShader = compile vs_2_0 ColorPerVertexVS();
    }
}