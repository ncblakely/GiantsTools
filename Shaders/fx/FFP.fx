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
#define NUM_LIGHTS               5

#define LIGHT_TYPE_NONE          0
#define LIGHT_TYPE_POINT         1
#define LIGHT_TYPE_SPOT          2
#define LIGHT_TYPE_DIRECTIONAL   3
#define LIGHT_NUM_TYPES          4

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

int g_NumLights;

struct DirectionalLight
{
    float4 Diffuse;
    float4 Specular;
    float4 Ambient;
    float3 Position;
    float3 Direction;
};

struct Material
{
    float4 Diffuse;
    float4 Ambient;
    float4 Specular;
    float4 Emissive;
    float Power;
};

DirectionalLight DirectionalLights[2] : DirectionalLights =
{
    {
        float4(0.398, 0.391, 0.523, 0.764),
        float4(0.398, 0.391, 0.523, 0.764),
        float4(0.398, 0.391, 0.523, 0.764),
        float3(-1141, -182, 133),
        float3(0.93, 0.35, 0)
    },
    {
        float4(0.434, 0.402, 0.398, 0.712),
        float4(0.434, 0.402, 0.398, 0.712),
        float4(0.434, 0.402, 0.398, 0.712),
        float3(-1138, -168, 133),
        float3(-0.75, -0.165, -0.633)
    },
};

Material g_Material : Material;

//transformation matrices
float4x4 matWorldViewProjection : WorldViewProjection;
float4x4 matWorldView      : WorldView;
float4x4 matView           : View;
float4x4 matWorld          : World;
float4x4 matProjection : Projection;
float4x4 matWorldViewIT : WorldViewInverseTranspose;
float4x4 matViewIT : ViewInverseTranspose;

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


//-----------------------------------------------------------------------------
// Name: DoDirLight()
// Desc: Directional light computation
//-----------------------------------------------------------------------------
COLOR_PAIR DoDirLight(float3 N, float3 V, int i)
{
   COLOR_PAIR Out;
   float3 L = mul((float3x3)matViewIT, -normalize(DirectionalLights[i].Direction));
   float NdotL = dot(N, L);
   Out.Color = 0;// DirectionalLights[i].Ambient;
   Out.ColorSpec = 0;
   if(NdotL > 0.f)
   {
      //compute diffuse color
      Out.Color += NdotL * DirectionalLights[i].Diffuse;

      //add specular component
      if(g_Material.Power > 0)
      {
         float3 H = normalize(L + V);   //half vector
         Out.ColorSpec = pow(max(0, dot(H, N)), g_Material.Power) * DirectionalLights[i].Specular;
      }
   }
   return Out;
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

   float3 P = mul(matWorldView, vPosition);           //position in view space
   float3 N = mul((float3x3)matWorldViewIT, vNormal); //normal in view space
   float3 V = -normalize(P);                          //viewer

   //automatic texture coordinate generation
   Out.Tex0.xy = tc;
   /*Out.Tex0 = float4((2.f * dot(V,N) * N - V) * (iTexGenType == TEXGEN_TYPE_CAMERASPACEREFLECTIONVECTOR)
            + N * (iTexGenType == TEXGEN_TYPE_CAMERASPACENORMAL)
            + P * (iTexGenType == TEXGEN_TYPE_CAMERASPACEPOSITION), 0);
   Out.Tex0.xy += tc * (iTexGenType == TEXGEN_TYPE_NONE);*/

   //light computation
   Out.Color = g_Material.Ambient;
   Out.ColorSpec = 0;

   //directional lights
   for(int i = 0; i < 2; i++)
   {
      COLOR_PAIR ColOut = DoDirLight(N, V, i);
      Out.Color += ColOut.Color;
      Out.ColorSpec += ColOut.ColorSpec;
   }

   ////point lights
   //for(int i = 0; i < iLightPointNum; i++)
   //{
   //   COLOR_PAIR ColOut = DoPointLight(vPosition, N, V, i+iLightPointIni);
   //   Out.Color += ColOut.Color;
   //   Out.ColorSpec += ColOut.ColorSpec;
   //}

   ////spot lights
   //for(int i = 0; i < iLightSpotNum; i++)
   //{
   //   COLOR_PAIR ColOut = DoSpotLight(vPosition, N, V, i+iLightSpotIni);
   //   Out.Color += ColOut.Color;
   //   Out.ColorSpec += ColOut.ColorSpec;
   //}

   //apply material color
   Out.Color *= g_Material.Diffuse;
   Out.ColorSpec *= g_Material.Specular;

   // saturate
   Out.Color = min(1, Out.Color);
   Out.ColorSpec = min(1, Out.ColorSpec);

   //apply fog
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
      SPECULARENABLE = (g_Material.Power > 0);
      FOGENABLE = (iFogType != FOG_TYPE_NONE);
      FOGCOLOR = (vFogColor);
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
