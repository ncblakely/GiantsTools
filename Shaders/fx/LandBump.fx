//--------------------------------------------------------------------------------------
// LandBump.fx

// Land bumpmapping and lighting shader.
//--------------------------------------------------------------------------------------

#define MAX_LIGHTS 4

float4x4 g_mWorldViewProjection : WorldViewProjection;
float4x4 g_World : World;

float4x4 g_TexGenTransform0 : TexGenTransform0;
float4x4 g_TexGenTransform1 : TexGenTransform1;
float4x4 g_ShoreGen : TexGenTransform2;

float4 g_LightDiffuseColors[MAX_LIGHTS] : EffectLightColors;
float3 g_LightPositions[MAX_LIGHTS] : EffectLightPositions;
float g_LightRangeSquared[MAX_LIGHTS] : EffectLightRanges;

float4 g_TextureFactor : TextureFactor;
int g_NumLights = 0;

//////////////////////////////////////////////////////
//The sizes of the various terrain UV Coordinates
float detailScale = 1;
float diffuseScale = 5;
float globalScale;
float detailMapStrength = 1;

float3 sunlightVector = float3(.5,.5,.8);

//The Colour (and brightness) of the Sunlight
float3 lightColour = float3(2,2,2);
//The colour (and birghtness) of the ambient light
float3 ambientColour = float3(.1,.1,.1);


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


VS_OUTPUT_BUMP LandBumpVS( float4 vPos : POSITION, 
						float3 vNormal : NORMAL,
						float4 vDiffuse : COLOR0,
						float4 vDiffuse2 : COLOR1)
{
	VS_OUTPUT_BUMP Output;

	// Transform the position from object space to homogeneous projection space
	Output.Position = mul(vPos, g_mWorldViewProjection);

	Output.LandBumpDiffuse = vDiffuse2 * .5f;
	Output.LandBumpDiffuse.a = 1.0f;

	//Output.LandDiffuse
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
#ifdef MIX_4X
	float3 texel = tex2D(g_LandTextureSampler, input.LandTextureUV).rgb * tex2D(g_LandTextureSampler, input.LandTextureUV * -0.25).rgb * 4;
	float3 finalColor = 2.0 * (normalMap * (texel) + input.LandBumpDiffuse);
#else
	float3 finalColor = 2.0 * (normalMap * (tex2D(g_LandTextureSampler, input.LandTextureUV)) + input.LandBumpDiffuse);
	//finalColor *= tex2D(g_ShoreTextureSampler, input.ShoreTextureUV);
#endif

	//finalColor = (g_TextureFactor * (1 - input.LandDiffuse)) + finalColor; 

	for (int i = 0; i < MAX_LIGHTS; i++)
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

	return float4(finalColor, 1);
}

technique LandBump
{
	pass P0
	{    
		VertexShader = compile vs_2_0 LandBumpVS();
		PixelShader  = compile ps_2_0 LandBumpPS();
	}
}

VS_OUTPUT_BUMP LandNormalVS( float4 vPos : POSITION, 
						float3 vNormal : NORMAL,
						float4 vDiffuse : COLOR0,
						float4 vDiffuse2 : COLOR1)
{
	VS_OUTPUT_BUMP Output;

	// Transform the position from object space to homogeneous projection space
	Output.Position = mul(vPos, g_mWorldViewProjection);

	Output.LandBumpDiffuse = vDiffuse2 * .5f;
	Output.LandBumpDiffuse.a = 1.0f;

	//Output.LandDiffuse
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

float4 LandNormalPS(VS_OUTPUT_BUMP input) : COLOR0
{ 
	float4 Output;
	//Get Global Normal from the full terrain normal map
	float3 Normal = tex2D(g_LandBumpTextureSampler, input.LandTextureUV);
	Normal[0] -= .5;
	Normal[1] -= .5;
	Normal[2] -= .5;
	Normal = normalize(Normal);
	
	
	//{
	//
	//	//Get Detail Normal from the detail map
	//	float3 detailNormalMap = (tex2D(g_LandDetailTextureSampler, input.LandTextureUV*100/detailScale));
	//	detailNormalMap[0] -= .5;
	//	detailNormalMap[1] -= .5;
	//	detailNormalMap[2] -= .5;
	//	//Multiply Detail Normal by detailMapStrength
	//	detailNormalMap[0] = mul(detailNormalMap[0], detailMapStrength);
	//	detailNormalMap[1] = mul(detailNormalMap[1], detailMapStrength);
	//
	//	//Normalize detail Normal
	//	detailNormalMap = normalize(detailNormalMap);
	//	
	//	if(false)
	//	{
	//		//Generate the Tangent Basis for the Detail Normal Map.
	//		float3x3 tangentBasis;
	//		
	//		tangentBasis[0] = cross(Normal, float3(1,0,0));
	//		tangentBasis[1] = cross(Normal, tangentBasis[0]);
	//		tangentBasis[2] = Normal;
	//		
	//		detailNormalMap = detailNormalMap, detailMapStrength;
	//	    
	//		Normal = mul(detailNormalMap, tangentBasis);
	//		Normal = normalize(Normal);
	//	} 
	//	else
	//	{
	//		Normal = normalize(Normal*2+detailNormalMap*detailMapStrength);
	//	}
	//}

    float3 sv = normalize(sunlightVector);    
	float3 lightLevel;
	lightLevel[0] = max(dot(Normal, sv), 0)*lightColour[0]*2;//+ambientColour[0];
	lightLevel[1] = max(dot(Normal, sv), 0)*lightColour[1]*2;//+ambientColour[1];
	lightLevel[2] = max(dot(Normal, sv), 0)*lightColour[2]*2;//+ambientColour[2];

	return float4(tex2D(g_LandTextureSampler, input.LandTextureUV) * lightLevel, 1) + input.LandBumpDiffuse;



	float4 normal = bx2(tex2D(g_LandBumpTextureSampler, input.LandBumpTextureUV));
	float4 normalcol = bx2(input.LandDiffuse);

	float3 normalMap;
	normalMap = saturate((float4)dot((float3)normal, (float3)normalcol)).xyz; 
#ifdef MIX_4X
	float3 texel = tex2D(g_LandTextureSampler, input.LandTextureUV).rgb * tex2D(g_LandTextureSampler, input.LandTextureUV * -0.25).rgb * 4;
	float3 finalColor = 2.0 * (normalMap * (texel) + input.LandBumpDiffuse);
#else
	float3 finalColor = 2.0 * (normalMap * (tex2D(g_LandTextureSampler, input.LandTextureUV)) + input.LandBumpDiffuse);
	//finalColor *= tex2D(g_ShoreTextureSampler, input.ShoreTextureUV);
#endif

	//finalColor = (g_TextureFactor * (1 - input.LandDiffuse)) + finalColor; 

	for (int i = 0; i < MAX_LIGHTS; i++)
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

	return float4(finalColor, 1);
}

technique LandNormal
{
	pass P0
	{    
		VertexShader = compile vs_2_0 LandNormalVS();
		PixelShader  = compile ps_2_0 LandNormalPS();
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

VS_OUTPUT LandscapeVS( float4 vPos : POSITION, 
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
		// Get light direction for this fragment
		float3 lightDir = normalize(input.WorldPos - g_LightPositions[i]); // per pixel diffuse lighting

		// Note: Non-uniform scaling not supported
		float diffuseLighting = saturate(dot(input.Normal, -lightDir));

		// Introduce fall-off of light intensity
		diffuseLighting *= (g_LightRangeSquared[i] / dot(g_LightPositions[i] - input.WorldPos, g_LightPositions[i] - input.WorldPos));

		float4 diffuseColor = diffuseLighting * g_LightDiffuseColors[i];

		finalColor += diffuseColor;
	}

#ifdef MIX_4X
	float3 texel = tex2D(g_LandTextureSampler, input.TextureUV).rgb * tex2D(g_LandTextureSampler, input.TextureUV * -0.25).rgb * 4;
#else
	float3 texel = tex2D(g_LandTextureSampler, input.TextureUV);
#endif
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