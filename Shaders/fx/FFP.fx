//======================================================================================//
// filename: FixedFuncShader.fx                                                         //
//                                                                                      //
// author:   Pedro V. Sander                                                            //
//           ATI Research, Inc.                                                         //
//           3D Application Research Group                                              //
//           email: psander@ati.com                                                     //
//                                                                                      //
// Description: A programmable shader that emulates the fixed function pipeline         //
//                                                                                      //
//======================================================================================//
//   (C) 2003 ATI Research, Inc.  All rights reserved.                                  //
//======================================================================================//

#define PI  3.14f

//this file contains light, fog, and texture types
// (originally a include, inserted here)

#define FOG_TYPE_NONE            0
#define FOG_TYPE_EXP             1
#define FOG_TYPE_EXP2            2
#define FOG_TYPE_LINEAR          3
#define FOG_NUM_TYPES            4

#define TEX_TYPE_NONE            0
#define TEX_TYPE_CUBEMAP         1
#define TEX_NUM_TYPES            2

#define TEXGEN_TYPE_NONE                          0
#define TEXGEN_TYPE_CAMERASPACENORMAL             1
#define TEXGEN_TYPE_CAMERASPACEPOSITION           2
#define TEXGEN_TYPE_CAMERASPACEREFLECTIONVECTOR   3
#define TEXGEN_NUM_TYPES                          4

#define MAX_DIRECTIONAL_LIGHTS 3

// Structs and variables with default values

//fog settings
int iFogType = FOG_TYPE_LINEAR;
float4 vFogColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
float fFogStart = 0;
float fFogEnd = 8845.00000;
float fFogDensity = .02f;
bool bFogRange : register(b4) = true;

int iTexType = TEX_TYPE_NONE;
int iTexGenType = TEXGEN_TYPE_NONE;

float4 DirectionalLightAmbient[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightAmbient;
float4 DirectionalLightDiffuse[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightDiffuse;
float3 DirectionalLightDirection[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightDirection;
bool DirectionalLightEnabled[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightEnabled;
float4 DirectionalLightSpecular[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightSpecular;
float3 g_CameraPosition : CameraPosition;

struct Material
{
    float4 Diffuse;
    float4 Ambient;
    float4 Specular;
    float4 Emissive;
    float Power;
};

Material g_Material : Material;

//transformation matrices
float4x4 matWorldViewProjection : WorldViewProjection;
float4x4 matWorldView      : WorldView;
float4x4 matWorld          : World;
float4x4 matView : View;

//function output structures
struct VS_OUTPUT
{
   float4 Pos           : POSITION;
   float4 Color         : COLOR0;
   float4 ColorSpec     : COLOR1;
   float4 Tex0          : TEXCOORD0;
   float  Fog           : FOG;
};

struct COLOR_PAIR
{
   float4 Color         : COLOR0;
   float4 ColorSpec     : COLOR1;
};

float4 CalculateAmbientLight()
{
    float4 ambient = 0;
    for (int i = 0; i < MAX_DIRECTIONAL_LIGHTS; i++)
    {
        if (DirectionalLightEnabled[i])
        {
            ambient += DirectionalLightAmbient[i];
        }
    }

    return ambient;
}

COLOR_PAIR DoDirectionalLight(float3 worldPos, float3 N, int i)
{
    COLOR_PAIR Out = (COLOR_PAIR)0;
    float NdotL = dot(N, -DirectionalLightDirection[i]);
    if (NdotL > 0.f)
    {
        //compute diffuse color
        Out.Color += max(0, NdotL * DirectionalLightDiffuse[i]);

        if (g_Material.Power > 0)
        {
            float3 toEye = g_CameraPosition.xyz - worldPos;
            toEye = normalize(toEye);
            float3 halfway = normalize(toEye + -DirectionalLightDirection[i]);
            float NDotH = saturate(dot(halfway, N));

            float4 spec = DirectionalLightSpecular[i];
            Out.ColorSpec += spec * pow(NDotH, g_Material.Power);
        }
    }
    return Out;
}

COLOR_PAIR ComputeLighting(float3 worldPos, float3 N)
{
    COLOR_PAIR finalResult = (COLOR_PAIR)0;

    for (int i = 0; i < MAX_DIRECTIONAL_LIGHTS; i++)
    {
        COLOR_PAIR lightResult = (COLOR_PAIR)0;
        if (DirectionalLightEnabled[i])
        {
            lightResult = DoDirectionalLight(worldPos, N, i);
        }

        finalResult.Color += lightResult.Color;
        finalResult.ColorSpec += lightResult.ColorSpec;
    }

    float4 ambient = g_Material.Ambient * CalculateAmbientLight();
    float4 diffuse = g_Material.Diffuse * finalResult.Color;
    float4 specular = g_Material.Specular * finalResult.ColorSpec;

    finalResult.Color = saturate(ambient + diffuse);
    finalResult.ColorSpec = saturate(specular);

    return finalResult;
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
//   Out.ColorSpec = 0;
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
//         Out.ColorSpec = pow(max(0, dot(H, N)), fMaterialPower) * lights[i].vSpecular;
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
//      Out.ColorSpec *= fAtten;
//   }
//   return Out;
//}

//-----------------------------------------------------------------------------
// Name: vs_main()
// Desc: The vertex shader
//-----------------------------------------------------------------------------
VS_OUTPUT vs_main (float4 vPosition  : POSITION0, 
                           float3 vNormal    : NORMAL0, 
                           float2 tc         : TEXCOORD0)
{
   VS_OUTPUT Out = (VS_OUTPUT) 0;

   vNormal = normalize(vNormal);
   Out.Pos = mul(vPosition, matWorldViewProjection);

   //automatic texture coordinate generation
   Out.Tex0.xy = tc;
   /*Out.Tex0 = float4((2.f * dot(V,N) * N - V) * (iTexGenType == TEXGEN_TYPE_CAMERASPACEREFLECTIONVECTOR)
            + N * (iTexGenType == TEXGEN_TYPE_CAMERASPACENORMAL)
            + P * (iTexGenType == TEXGEN_TYPE_CAMERASPACEPOSITION), 0);
   Out.Tex0.xy += tc * (iTexGenType == TEXGEN_TYPE_NONE);*/

   //light computation

   //directional lights
   float4 worldPos  = mul(vPosition, matWorld);           //position in view space
   float3 N = mul(vNormal, (float3x3)matWorld);
   COLOR_PAIR lighting = ComputeLighting(worldPos, N);

   ////point lights
   //for(int i = 0; i < iLightPointNum; i++)
   //{
   //   COLOR_PAIR ColOut = DoPointLight(vPosition, N, V, i+iLightPointIni);
   //   Out.Color += ColOut.Color;
   //   Out.ColorSpec += ColOut.ColorSpec;
   //}

   //apply material color
   Out.Color = lighting.Color;
   Out.ColorSpec = lighting.ColorSpec;

   //apply fog
   float4 P = mul(matWorldView, vPosition);           //position in view space
   float d;
   if(bFogRange)
      d = length(P);
   else
      d = P.z;

   Out.Fog = 1.f * (iFogType == FOG_TYPE_NONE)
             + 1.f/exp(d * fFogDensity) * (iFogType == FOG_TYPE_EXP)
             + 1.f/exp(pow(d * fFogDensity, 2)) * (iFogType == FOG_TYPE_EXP2)
             + saturate((fFogEnd - d)/(fFogEnd - fFogStart)) * (iFogType == FOG_TYPE_LINEAR);

   return Out;
}

// Techniques

//the technique for the programmable shader (simply sets the vertex shader)
technique basic_with_shader
{
   pass P0
   {
      VertexShader = compile vs_2_0 vs_main();
   }
}

TEXTURE tex1;
TEXTURE tex2;

//Sampler for the diff mode
sampler DiffSampler1 = sampler_state
{
   Texture = (tex1);

   MinFilter = Point;
   MagFilter = Point;
   MipFilter = Point;
   AddressU  = Wrap;
   AddressV  = Wrap;
   AddressW  = Wrap;
   MaxAnisotropy = 8;
};

sampler DiffSampler2 = sampler_state
{
   Texture = (tex2);

   MinFilter = Point;
   MagFilter = Point;
   MipFilter = Point;
   AddressU  = Wrap;
   AddressV  = Wrap;
   AddressW  = Wrap;
   MaxAnisotropy = 8;
};

bool bDiffSensitivity = false;

//-----------------------------------------------------------------------------
// Name: ps_diff()
// Desc: Pixel shader for the diff mode
//       Tiny errors: green. Larger errors: yellow to red.
//-----------------------------------------------------------------------------
float4 ps_diff (float2 tcBase : TEXCOORD0) : COLOR
{
   float E = length(tex2D(DiffSampler1, tcBase) - tex2D(DiffSampler2, tcBase))/sqrt(3);
   float4 C = float4(0.f,0.f,0.f,E);
   
   if(E > 0.f)
   {
      if(E <= 1.f/255.f)
      {
         if(bDiffSensitivity)
         {
            C = float4(0.f,1.f,0.f,E);
         }
      }
      else
      {
         C = lerp(float4(1.f,1.f,0.f,E), float4(1.f,0.f,0.f,E),E);
      }
   }
   return C;
}

//technique for the diff mode
technique technique_diff
{
   pass P0
   {
      PixelShader = compile ps_2_0 ps_diff();
   }
}
