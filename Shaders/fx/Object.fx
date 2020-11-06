#include "../fxh/SystemVariables.fxh"

sampler g_ObjTextureSampler =
sampler_state
{
    Texture = <g_texture0>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

// =======================================================
// Textured per pixel lighting
//

float4 CalculateDiffuse(float3 N, float3 L, float4 diffuseColor)
{
    float NDotL = dot(N, L);
    float4 finalColor = 0;
    if (NDotL > 0.0f)
    {
        finalColor = max(0, NDotL * diffuseColor);
    }

    return finalColor;
}

float4 CalculateSpecular(float3 worldPos, float3 N, float3 L, float4 specularColor)
{
    float4 finalColor = 0;
    if (g_Material.Power > 0)
    {
        float3 toEye = normalize(g_CameraPosition.xyz - worldPos);
        float3 halfway = normalize(toEye + L);
        float NDotH = saturate(dot(halfway, N));

        finalColor = max(0, pow(NDotH, g_Material.Power) * specularColor);
    }

    return finalColor;
}

Lighting DoDirectionalLight(float3 worldPos, float3 N, int i)
{
    Lighting Out;
    Out.Diffuse = CalculateDiffuse(
        N,
        -g_DirectionalLightDirection[i],
        g_DirectionalLightDiffuse[i]);
    Out.Specular = CalculateSpecular(
        worldPos,
        N,
        -g_DirectionalLightDirection[i],
        g_DirectionalLightSpecular[i]);
    return Out;
}

Lighting ComputeLighting(float3 worldPos, float3 N)
{
    Lighting finalLighting = (Lighting)0;

    for (int i = 0; i < g_numDirectionalLights; i++)
    {
        Lighting lighting = DoDirectionalLight(worldPos, N, i);
        finalLighting.Diffuse += lighting.Diffuse;
        finalLighting.Specular += lighting.Specular;
    }

    float4 ambient = g_Material.Ambient * g_DirectionalAmbientLightSum;
    float4 diffuse = g_Material.Diffuse * finalLighting.Diffuse;
    float4 specular = g_Material.Specular * finalLighting.Specular;

    finalLighting.Diffuse = saturate(ambient + diffuse);
    finalLighting.Specular = saturate(specular);

    return finalLighting;
}

struct PixelLightingVSOutput
{
    float4 Pos : POSITION;
    float2 Tex0 : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

PixelLightingVSOutput PixelLightingVS(
    float4 vPosition : POSITION0,
    float3 vNormal : NORMAL0,
    float2 tc : TEXCOORD0)
{
    // Simple transform, pre-compute as much as we can for the pixel shader
    PixelLightingVSOutput output;

    output.Pos = mul(vPosition, g_WorldViewProjection);
    output.Normal = mul(normalize(vNormal), (float3x3)g_World);
    output.WorldPos = mul(vPosition, g_World);
    output.Tex0 = tc;
    return output;
}

float4 GetColorArg(int colorArg, float4 textureColor, float4 diffuseColor)
{
    float4 result;
    if (colorArg == D3DTA_TEXTURE) result = textureColor;
    else if (colorArg == D3DTA_DIFFUSE) result = diffuseColor;
    else if (colorArg == D3DTA_TFACTOR) result = g_textureFactor;
    else result = float4(1.f, 1.f, 1.f, 1.f);

    return result;
}

float4 Modulate(int stageIndex, float4 textureColor, float4 diffuseColor, float factor)
{
    float4 left = GetColorArg(g_blendStages[stageIndex].colorArg1, textureColor, diffuseColor); 
    float4 right = GetColorArg(g_blendStages[stageIndex].colorArg2, textureColor, diffuseColor);

    return (left * right) * factor;
}

float4 ProcessStages(float4 textureColor, float4 diffuseColor)
{
    float4 output = 0;
    for (int i = 0; i < g_numBlendStages; i++)
    {
        if (g_blendStages[i].colorOp == D3DTOP_MODULATE4X)
        {
            output += Modulate(i, textureColor, diffuseColor, 4.0f);
        }
        else
        {
            output += Modulate(i, textureColor, diffuseColor, 1.0f);
        }
    }

    return output;
}

struct PixelLightingPSOutput
{
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
};

PixelLightingPSOutput PixelLightingPS(PixelLightingVSOutput input)
{
    float4 color = tex2D(g_ObjTextureSampler, input.Tex0);

    Lighting lighting = ComputeLighting(input.WorldPos, input.Normal);

    PixelLightingPSOutput output;
    output.Diffuse = ProcessStages(color, lighting.Diffuse);
    output.Specular = color * lighting.Specular;
    return output;
}

technique PixelLighting
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelLightingPS();
        VertexShader = compile vs_3_0 PixelLightingVS();
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

float fFogStart = 25.f;
float fFogEnd = 1525.f;

ColorPerVertexVSOutput ColorPerVertexVS(
    float4 vPosition : POSITION0,
    float2 tc : TEXCOORD0,
    float4 color : COLOR0,
    float fog : FOG)
{
    // Simple transform, pre-compute as much as we can for the pixel shader
    ColorPerVertexVSOutput output = (ColorPerVertexVSOutput)0;

    output.Pos = mul(vPosition, g_WorldViewProjection);
    output.Tex0 = tc;
    output.Color = color;

    float3 P = mul(vPosition, g_WorldView);           //position in view space
    float d = length(P);

    output.Fog = 0.0; //saturate((fFogEnd - d) / (fFogEnd - fFogStart));
    return output;
}

technique ColorPerVertex
{
    pass P0
    {
        //PixelShader = compile ps_3_0 ColorPerVertexPS();
        VertexShader = compile vs_3_0 ColorPerVertexVS();
    }
}