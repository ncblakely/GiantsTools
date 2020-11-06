#include "../fxh/Constants.fxh"
#include "../fxh/Lighting.fxh"

// Lighting state
float4 g_DirectionalLightAmbient[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightAmbient;
float4 g_DirectionalLightDiffuse[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightDiffuse;
float3 g_DirectionalLightDirection[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightDirection;
bool g_DirectionalLightEnabled[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightEnabled;
float4 g_DirectionalLightSpecular[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightSpecular;

texture g_ObjTexture : ObjTexture;
sampler g_ObjTextureSampler =
sampler_state
{
    Texture = <g_ObjTexture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

// Camera
float3 g_CameraPosition : CameraPosition;

// Current material
Material g_Material : Material;

// Transforms
float4x4 g_WorldViewProjection : WorldViewProjection;
float4x4 g_World : World;

struct VSOutputLit
{
    float4 Pos : POSITION;
    float4 Diffuse : COLOR0;
    float4 Specular : COLOR1;
    float4 Tex0 : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

struct VSOutput
{
    float4 Pos : POSITION;
    float4 Tex0 : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

float4 CalculateAmbientLight()
{
    float4 ambient = 0;
    for (int i = 0; i < MAX_DIRECTIONAL_LIGHTS; i++)
    {
        if (g_DirectionalLightEnabled[i])
        {
            ambient += g_DirectionalLightAmbient[i];
        }
    }

    return ambient;
}

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

    for (int i = 0; i < MAX_DIRECTIONAL_LIGHTS; i++)
    {
        if (g_DirectionalLightEnabled[i])
        {
            Lighting lighting = DoDirectionalLight(worldPos, N, i);
            finalLighting.Diffuse += lighting.Diffuse;
            finalLighting.Specular += lighting.Specular;
        }
    }

    float4 ambient = g_Material.Ambient * CalculateAmbientLight();
    float4 diffuse = g_Material.Diffuse * finalLighting.Diffuse;
    float4 specular = g_Material.Specular * finalLighting.Specular;

    finalLighting.Diffuse = saturate(ambient + diffuse);
    finalLighting.Specular = saturate(specular);

    return finalLighting;
}

//-----------------------------------------------------------------------------
// Name: DoPointLight()
// Desc: Point light computation
//-----------------------------------------------------------------------------
//COLOR_PAIR DoPointLight(float4 vPosition, float3 N, float3 V, int i)
//{
//   float3 L = mul((float3x3)matViewIT, normalize((lights[i].vPos-(float3)mul(matWorld,vPosition))));
//   COLOR_PAIR Out;
//   float NdotL = dot(N, L);
//   Out.Color = lights[i].vAmbient;
//   Out.Specular = 0;
//   float fAtten = 1.f;
//   if(NdotL >= 0.f)
//   {
//      //compute diffuse color
//      Out.Color += NdotL * lights[i].vDiffuse;
//
//      //add specular component
//      if(bSpecular)
//      {
//         float3 H = normalize(L + V);   //half vector
//         Out.Specular = pow(max(0, dot(H, N)), fMaterialPower) * lights[i].vSpecular;
//      }
//
//      float LD = length(lights[i].vPos-(float3)mul(matWorld,vPosition));
//      if(LD > lights[i].fRange)
//      {
//         fAtten = 0.f;
//      }
//      else
//      {
//         fAtten *= 1.f/(lights[i].vAttenuation.x + lights[i].vAttenuation.y*LD + lights[i].vAttenuation.z*LD*LD);
//      }
//      Out.Color *= fAtten;
//      Out.Specular *= fAtten;
//   }
//   return Out;
//}

VSOutputLit VSMainLighting(
    float4 vPosition  : POSITION0,
    float3 vNormal : NORMAL0,
    float2 tc : TEXCOORD0)
{
    VSOutputLit Out = (VSOutputLit)0;

    vNormal = normalize(vNormal);
    Out.Pos = mul(vPosition, g_WorldViewProjection);

    //automatic texture coordinate generation
    Out.Tex0.xy = tc;

    //directional lights
    float4 worldPos = mul(vPosition, g_World);           //position in view space
    float3 normal = mul(vNormal, (float3x3)g_World);
    Lighting lighting = ComputeLighting(worldPos, normal);

    ////point lights
    //for(int i = 0; i < iLightPointNum; i++)
    //{
    //   COLOR_PAIR ColOut = DoPointLight(vPosition, N, V, i+iLightPointIni);
    //   Out.Color += ColOut.Color;
    //   Out.Specular += ColOut.Specular;
    //}

    Out.Diffuse = lighting.Diffuse;
    Out.Specular = lighting.Specular;

    return Out;
}

VSOutput VSMain(
    float4 vPosition  : POSITION0,
    float3 vNormal : NORMAL0,
    float2 tc : TEXCOORD0)
{
    VSOutput Out = (VSOutput)0;
    Out.Pos = mul(vPosition, g_WorldViewProjection);
    Out.Normal = normalize(vNormal);
    Out.WorldPos = mul(vPosition, g_World);
    Out.Tex0.xy = tc;
    return Out;
}

float4 PSMain(VSOutput input) : COLOR0
{
    float4 color = tex2D(g_ObjTextureSampler, input.Tex0);

    Lighting lighting = ComputeLighting(input.WorldPos, input.Normal);

    color = (lighting.Diffuse + lighting.Specular) * color;
    return color;
}

technique TexturedVertexLighting
{
    pass P0
    {
        //PixelShader = compile ps_2_0 ps_main();
        VertexShader = compile vs_2_0 VSMainLighting();
    }
}

technique TexturedPixelLighting
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSMain();
        VertexShader = compile vs_2_0 VSMain();
    }
}