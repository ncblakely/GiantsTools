//--------------------------------------------------------------------------------------
// LandBump.fx

// Land bumpmapping and lighting shader.
//--------------------------------------------------------------------------------------

#include "../fxh/Constants.fxh"
#include "../fxh/Lighting.fxh"
#include "../fxh/Transform.fxh"
#include "../fxh/Fog.fxh"

shared WorldTransforms g_WorldTransforms;
shared ViewTransforms g_ViewTransforms;

shared texture g_Texture0;
shared texture g_Texture1;
shared texture g_Texture2;
shared float4x4 g_TexGenMatrix0;
shared float4x4 g_TexGenMatrix1;
shared float4x4 g_TexGenMatrix2;

shared DirectionalLightInfo g_DirectionalLights;
shared PointLightInfo g_PointLights;

shared FogParams g_Fog;

float g_DetailFadeStart = 150;
float g_DetailFadeEnd = 500;

//////////////////////////////////////////////////////

//--------------------------------------------------------------------------------------
// Texture samplers
//--------------------------------------------------------------------------------------
sampler g_LandTextureSampler = 
sampler_state
{
	Texture = <g_Texture0>;
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};

sampler g_LandBumpTextureSampler = 
sampler_state
{
	Texture = <g_Texture1>;
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};

sampler g_LandDetailTextureSampler =
sampler_state
{
	Texture = <g_Texture2>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

//--------------------------------------------------------------------------------------
// Vertex shader output structure
//--------------------------------------------------------------------------------------
struct LandBumpDetailVSOutput
{
	float4 Position   : POSITION;
	float4 LandBumpDiffuse    : COLOR1;
	float4 LandDiffuse		: COLOR0;
	float2 LandBumpTextureUV  : TEXCOORD0;
	float2 LandTextureUV  : TEXCOORD1;
	float2 LandDetailTextureUV : TEXCOORD2;
	float3 WorldPos : TEXCOORD3;
	float3 Normal : TEXCOORD4;
	float Fog : FOG;
};

inline float4 bx2(float4 x)
{
	return float4(2.0f * x.xyzw - 1.0f);
}

float4 CalculateDetailTexture(float3 worldPos, float4 originalColor, float2 detailTextureUV)
{
	if (detailTextureUV.x != 0 || detailTextureUV.y != 0)
	{
		float distance = length(worldPos - g_ViewTransforms.CameraPosition);
		if (distance < g_DetailFadeEnd)
		{
			float4 detailTextureColor = tex2D(g_LandDetailTextureSampler, detailTextureUV) * 1.8f;

			if (distance > g_DetailFadeStart)
			{
				float distNormalized = distance / (g_DetailFadeEnd - g_DetailFadeStart);
				detailTextureColor = lerp(detailTextureColor, float4(1.f, 1.f, 1.f, 1.f), distNormalized);
			}

			originalColor *= detailTextureColor;
		}
	}

	return originalColor;
}

LandBumpDetailVSOutput LandBumpDetailVS(
	float4 position : POSITION,
	float3 normal : NORMAL,
	float4 landBumpDiffuse : COLOR0,
	float4 landDiffuse : COLOR1)
{
	LandBumpDetailVSOutput output;

	// Transform the position from object space to homogeneous projection space
	output.Position = mul(position, g_WorldTransforms.WorldViewProjection);

	output.LandDiffuse = landDiffuse * .5f;
	output.LandDiffuse.a = 1.0f;

	output.LandBumpDiffuse.rgb = landBumpDiffuse;
	output.LandBumpDiffuse.a = 1.0f;

	output.WorldPos = mul(position, g_WorldTransforms.World);

	// Set dynamically generated tex coords
	output.LandBumpTextureUV = mul(position, g_TexGenMatrix0);
	output.LandTextureUV = mul(position, g_TexGenMatrix1);
	output.LandDetailTextureUV = mul(position, g_TexGenMatrix2);

	float3 P = mul(position, g_WorldTransforms.WorldView);
	output.Fog = CalculateFogFactor(g_Fog.FogMax, g_Fog.FogMin, length(P));

	// Transform the normal from object space to world space    
	output.Normal = normalize(mul(normal, (float3x3)g_WorldTransforms.World)); // normal (world space)

	return output;
}

float4 CalculateDot3BumpMap(float4 diffuseColor, float2 uv)
{
	float4 landBumpTextureColor = bx2(tex2D(g_LandBumpTextureSampler, uv));
	diffuseColor = bx2(diffuseColor);

	float4 bumpMapColor;
	bumpMapColor.xyz = saturate(dot(landBumpTextureColor, diffuseColor.rgb));
	bumpMapColor.w = 1.0f;

	return bumpMapColor;
}

struct TNBFrame
{
	float3 Tangent;
	float3 Normal;
	float3 Binormal;
};

/*
TNBFrame CalculateTNBFrame(float3 worldPos, float3 N, float2 uv)
{
	float3 dp1 = ddx(worldPos);
	float3 dp2 = ddy(worldPos);
	float2 duv1 = ddx(uv);
	float2 duv2 = ddy(uv);


	float3 dp2perp = cross(dp2, N);
	float3 dp1perp = cross(N, dp1);
	float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
	float3 B = dp2perp * duv1.y + dp1perp * duv2.y;

	float invmax = rsqrt(max(dot(T, T), dot(B, B)));

	TNBFrame frame;
	frame.Tangent = T * invmax;
	frame.Normal = N;
	frame.Binormal = B * invmax;
	return frame;
}
*/

TNBFrame CalculateTNBFrame(float3 worldPos, float3 N, float2 uv)
{
	float3 dp1 = ddx(worldPos);
	float3 dp2 = ddy(worldPos);
	float2 duv1 = ddx(uv);
	float2 duv2 = ddy(uv);

	float3 t = normalize(duv2.y * dp1 - duv1.y * dp2);
	float3 b = normalize(duv2.x * dp1 - duv1.x * dp2);
	float3 n = normalize(N);
	float3 x = cross(n, t);
	t = cross(x, n);
	t = normalize(t);

	x = cross(b, n);
	b = cross(n, x);
	b = normalize(b);

	TNBFrame frame;
	frame.Tangent = t;
	frame.Normal = n;
	frame.Binormal = b;
	return frame;
}

float4 CalculateNormalMap(float3 worldPos, float3 normal, float2 uv, float4 diffuse)
{
	TNBFrame tnbFrame = CalculateTNBFrame(worldPos, normal, uv);

	// Sample the pixel in the bump map.
	float4 bumpMap = tex2D(g_LandBumpTextureSampler, uv) + diffuse;

	// Expand the range of the normal value from (0, +1) to (-1, +1).
	bumpMap = (bumpMap * 2.0f) - 1.0f;

	// Calculate the normal from the data in the bump map.
	float3 bumpNormal = (bumpMap.x * tnbFrame.Tangent) + (bumpMap.y * tnbFrame.Binormal) + (bumpMap.z * tnbFrame.Normal);

	// Normalize the resulting bump normal.
	bumpNormal = normalize(bumpNormal);

	// Invert the light direction for calculations.
	float3 lightDir = -g_DirectionalLights.Direction[g_DirectionalLights.SunIndex];

	// Calculate the amount of light on this pixel based on the bump map normal value.
	float lightIntensity = saturate(dot(bumpNormal, lightDir));

	// Determine the final diffuse color based on the diffuse color and the amount of light intensity.
	float4 color = saturate(lightIntensity);

	return color;
}

float4 LandBumpDetailPS(LandBumpDetailVSOutput input) : COLOR0
{
	float4 landTextureColor = tex2D(g_LandTextureSampler, input.LandTextureUV);


#if 1
	float4 bumpMapColor = CalculateDot3BumpMap(input.LandBumpDiffuse, input.LandBumpTextureUV);
	float4 finalColor = 2.0 * (bumpMapColor * (landTextureColor) + input.LandDiffuse);
#else
	float4 bumpMapColor = CalculateNormalMap(input.WorldPos, input.Normal, input.LandTextureUV, input.LandBumpDiffuse);
	float4 bumpMapColor = CalculateDirectionalDiffuse(input.Normal, -g_DirectionalLights.Direction[g_DirectionalLights.SunIndex], float4(1, 1, 1, 1));
	
	float4 finalColor = (bumpMapColor * landTextureColor) + input.LandDiffuse;
#endif

	finalColor = CalculateDetailTexture(input.WorldPos, finalColor, input.LandDetailTextureUV);

	for (int i = 0; i < g_PointLights.Count; i++)
	{
		float3 worldPos = g_PointLights.Position[i] - input.WorldPos;
		float3 lightDirection = normalize(input.WorldPos - (g_PointLights.Position[i]));

		finalColor += CalculatePointDiffuse(
			worldPos,
			input.Normal,
			-lightDirection,
			g_PointLights.Range[i],
			g_PointLights.Diffuse[i]);
	}

	if (g_Fog.Enabled)
	{
		finalColor = ApplyPixelFog(finalColor, input.Fog, g_Fog.Color);
	}

	finalColor.a = 1.0f;
	return finalColor;
}

technique LandBumpDetail
{
	pass P0
	{
		VertexShader = compile vs_3_0 LandBumpDetailVS();
		PixelShader = compile ps_3_0 LandBumpDetailPS();
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

matrix Identity =
{
	{ 1, 0, 0, 0 },
	{ 0, 1, 0, 0 },
	{ 0, 0, 1, 0 },
	{ 0, 0, 0, 1 }
};

VS_OUTPUT LandscapeVS(
	float4 vPos : POSITION, 
	float3 vNormal : NORMAL,
	float4 vDiffuse : COLOR0)
{
	VS_OUTPUT Output;
	
	// Transform the position from object space to homogeneous projection space
	Output.Position = mul(vPos, g_WorldTransforms.WorldViewProjection);

	// Transform the normal from object space to world space    
	Output.Normal = normalize(mul(vNormal, (float3x3)g_WorldTransforms.World)); // normal (world space)

	Output.Diffuse.rgb = vDiffuse;
	Output.Diffuse.a = 1.0f;

	Output.WorldPos = mul(vPos, g_WorldTransforms.World);

	//float3 P = mul(vPos, g_WorldView);           //position in view space
	// Set dynamically generated tex coords
	//P* (iTexGenType == TEXGEN_TYPE_CAMERASPACEPOSITION), 0)
	/*
	float4x4 scaleMatrix = Identity;
	int index = G_TexUaxis[g_PrimBufferIndex];
	scaleMatrix[0][0] = g_TexUScale;
	scaleMatrix[1][G_TexVaxis[g_PrimBufferIndex]] = g_TexVScale;
	*/

	//Output.TextureUV = mul(vPos, (scaleMatrix * g_World));
	Output.TextureUV = mul(vPos, g_TexGenMatrix0);

	return Output;    
}

float4 LandscapePS(VS_OUTPUT input) : COLOR0
{ 
	float4 finalColor = 0;

	for (int i = 0; i < g_PointLights.Count; i++)
	{
		float3 worldPos = g_PointLights.Position[i] - input.WorldPos;
		float3 lightDirection = normalize(input.WorldPos - (g_PointLights.Position[i]));

		finalColor += CalculatePointDiffuse(
			worldPos,
			input.Normal,
			-lightDirection,
			g_PointLights.Range[i],
			g_PointLights.Diffuse[i]);
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