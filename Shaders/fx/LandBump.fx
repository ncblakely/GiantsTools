//--------------------------------------------------------------------------------------
// LandBump.fx

// Land bumpmapping and lighting shader.
//--------------------------------------------------------------------------------------

#include "Constants.fxh"

float4x4 g_mWorldViewProjection : WorldViewProjection;
float4x4 g_World : World;

float4x4 g_TexGenTransform0 : TexGenTransform0;
float4x4 g_TexGenTransform1 : TexGenTransform1;
float4x4 g_ShoreGen : TexGenTransform2;

float4 g_LightDiffuseColors[MAX_LIGHTS] : PointLightDiffuse;
float3 g_LightPositions[MAX_LIGHTS] : PointLightPosition;
float g_LightRangeSquared[MAX_LIGHTS] : PointLightRange;
bool g_LightEnabled[MAX_LIGHTS] : PointLightEnabled;

float4 g_TextureFactor : TextureFactor;

//////////////////////////////////////////////////////

texture g_LandTexture;
texture g_LandBumpTexture;
texture g_ShoreTexture;
texture g_LandDetailTexture;

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
sampler g_LandTextureSampler = 
sampler_state
{
	Texture = <g_LandTexture>;
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};

sampler g_LandBumpTextureSampler = 
sampler_state
{
	Texture = <g_LandBumpTexture>;
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};

sampler g_LandDetailTextureSampler = 
sampler_state
{
	Texture = <g_LandDetailTexture>;
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};

sampler g_ShoreTextureSampler = 
sampler_state
{
	Texture = <g_LandBumpTexture>;
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_OUTPUT_BUMP
{
	float4 Position   : POSITION;
	float4 LandBumpDiffuse    : COLOR1;
	float4 LandDiffuse		: COLOR0;
	float2 LandBumpTextureUV  : TEXCOORD0;
	float2 LandTextureUV  : TEXCOORD1;
	float3 WorldPos : TEXCOORD2;
	float3 Normal : TEXCOORD3;
	float3 ShoreTextureUV : TEXCOORD4;
};

float4 bx2(float4 x)
{
   return float4(2.0f * x.xyzw - 1.0f);
}

VS_OUTPUT_BUMP LandBumpVS(
	float4 vPos : POSITION, 
	float3 vNormal : NORMAL,
	float4 vDiffuse : COLOR0,
	float4 vDiffuse2 : COLOR1)
{
	VS_OUTPUT_BUMP Output;

	// Transform the position from object space to homogeneous projection space
	Output.Position = mul(vPos, g_mWorldViewProjection);

	Output.LandBumpDiffuse = vDiffuse2 * .5f;
	Output.LandBumpDiffuse.a = 1.0f;

	Output.LandDiffuse.rgb = vDiffuse;
	Output.LandDiffuse.a = 1.0f;

	Output.WorldPos = mul(vPos, g_World);

	// Set dynamically generated tex coords
	Output.LandBumpTextureUV = mul(vPos, g_TexGenTransform0);
	Output.LandTextureUV = mul(vPos, g_TexGenTransform1);
	Output.ShoreTextureUV = mul(vPos, g_ShoreGen);
	
	// Transform the normal from object space to world space    
	Output.Normal = normalize(mul(vNormal, (float3x3)g_World)); // normal (world space)

	return Output;    
}

float4 LandBumpPS(VS_OUTPUT_BUMP input) : COLOR0
{ 
	float4 normal = bx2(tex2D(g_LandBumpTextureSampler, input.LandBumpTextureUV));
	float4 normalcol = bx2(input.LandDiffuse);

	float3 normalMap;
	normalMap = saturate((float4)dot((float3)normal, (float3)normalcol)).xyz; 
	float3 finalColor = 2.0 * (normalMap * (tex2D(g_LandTextureSampler, input.LandTextureUV)) + input.LandBumpDiffuse);

	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		if (g_LightEnabled[i])
		{
			// Get light direction for this fragment
			float3 lightDir = normalize(input.WorldPos - g_LightPositions[i]); // per pixel diffuse lighting

			// Note: Non-uniform scaling not supported
			float diffuseLighting = saturate(dot(input.Normal, -lightDir));

			// Introduce fall-off of light intensity
			diffuseLighting *= (g_LightRangeSquared[i] / dot(g_LightPositions[i] - input.WorldPos, g_LightPositions[i] - input.WorldPos));

			float4 diffuseColor = diffuseLighting * g_LightDiffuseColors[i];

			finalColor += diffuseColor;
		}
	}

	return float4(finalColor, 1);
}

technique LandBump
{
	pass P0
	{    
		VertexShader = compile vs_2_0 LandBumpVS();
		PixelShader = compile ps_2_0 LandBumpPS();
	}
}

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
	float4 Position   : POSITION;
	float4 Diffuse    : COLOR0;
	float2 TextureUV  : TEXCOORD0;
	float3 Normal : TEXCOORD1;
	float3 WorldPos : TEXCOORD2;
};

VS_OUTPUT LandscapeVS(
	float4 vPos : POSITION, 
	float3 vNormal : NORMAL,
	float4 vDiffuse : COLOR0)
{
	VS_OUTPUT Output;

	// Transform the position from object space to homogeneous projection space
	Output.Position = mul(vPos, g_mWorldViewProjection);

	// Transform the normal from object space to world space    
	Output.Normal = normalize(mul(vNormal, (float3x3)g_World)); // normal (world space)

	Output.Diffuse.rgb = vDiffuse;
	Output.Diffuse.a = 1.0f;

	Output.WorldPos = mul(vPos, g_World);

	// Set dynamically generated tex coords
	Output.TextureUV = mul(vPos, g_TexGenTransform0);

	return Output;    
}

float4 LandscapePS(VS_OUTPUT input) : COLOR0
{ 
	float4 finalColor = 0;

	for (int i = 0; i < MAX_LIGHTS; i++)
	{
		if (g_LightEnabled[i])
		{
			// Get light direction for this fragment
			float3 lightDir = normalize(input.WorldPos - g_LightPositions[i]); // per pixel diffuse lighting

			// Note: Non-uniform scaling not supported
			float diffuseLighting = saturate(dot(input.Normal, -lightDir));

			// Introduce fall-off of light intensity
			diffuseLighting *= (g_LightRangeSquared[i] / dot(g_LightPositions[i] - input.WorldPos, g_LightPositions[i] - input.WorldPos));

			float4 diffuseColor = diffuseLighting * g_LightDiffuseColors[i];

			finalColor += diffuseColor;
		}
	}

	float3 texel = tex2D(g_LandTextureSampler, input.TextureUV);
	return float4(saturate((texel.xyz + input.Diffuse) + (finalColor)), 1.0f);
}

technique Landscape
{
	pass P0
	{    
		VertexShader = compile vs_2_0 LandscapeVS();
		PixelShader  = compile ps_2_0 LandscapePS();
	}
}