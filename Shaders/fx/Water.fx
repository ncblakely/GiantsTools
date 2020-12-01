//--------------------------------------------------------------------------------------
// Water.fx

// Very basic shader that implements D3DTOP_ADDSMOOTH functionality in HLSL, replicating
// the original fixed-function water rendering behavior.
//--------------------------------------------------------------------------------------

#include "../fxh/Transform.fxh"
#include "../fxh/Lighting.fxh"
#include "../fxh/Fog.fxh"

shared texture g_Texture0;
shared WorldTransforms g_WorldTransforms;
shared ViewTransforms g_ViewTransforms;

shared float4 g_TextureFactor;

const float3 g_WaterNormal = float3(0, 0, 1); // Water is always a flat plane currently, so we can just hardcode this to (0, 0, 1)
shared Material g_WaterMaterial;

shared PointLightInfo g_PointLights;
shared DirectionalLightInfo g_DirectionalLights;

shared FogParams g_Fog;

sampler2D g_WaterTextureSampler = 
sampler_state
{
	Texture = <g_Texture0>;
};

struct VS_OUTPUT
{
	float4 Position   : POSITION;   // vertex position 
	float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
	float4 Color	  : COLOR0;
	float Fog : FOG;
	float3 WorldPos : TEXCOORD1;
	/*float3 Normal : TEXCOORD1;*/
	float3 ViewDirection : TEXCOORD2;
};

VS_OUTPUT RenderSceneVS(float4 inPos : POSITION, 
						float4 inDiffuse : COLOR0,
						float4 inTextureUV : TEXCOORD0
						/*float3 Normal : NORMAL*/)
{
	VS_OUTPUT Output;

	Output.Position = mul(inPos, g_WorldTransforms.WorldViewProjection);
	Output.TextureUV = inTextureUV;
	Output.Color = inDiffuse;

	float3 P = mul(inPos, g_WorldTransforms.WorldView);
	float d = length(P);
	Output.Fog = CalculateFogFactor(g_Fog.FogMax, g_Fog.FogMin, d);
	//Output.Normal = mul(Normal, (float3x3)g_WorldTransforms.World);

	Output.WorldPos = mul(inPos, g_WorldTransforms.World);
	Output.ViewDirection = g_ViewTransforms.CameraPosition - Output.WorldPos;

	return Output;    
}

float4 RenderScenePS(VS_OUTPUT input) : COLOR0
{
	float4 texel = tex2D(g_WaterTextureSampler, input.TextureUV);
	float4 result = saturate(texel +  (1 - texel) * g_TextureFactor); // equivalent to saturate((texel + g_TextureFactor) - (texel * g_TextureFactor));

	float3 reflection = -reflect(normalize(-g_DirectionalLights.Direction[g_DirectionalLights.SunIndex]), g_WaterNormal);
	float specular = dot(normalize(reflection), normalize(input.ViewDirection));

	if (specular > 0.0f)
	{
		specular = pow(specular, g_WaterMaterial.Power);
		result = saturate(result + (specular));
	}

	for (int i = 0; i < g_PointLights.Count; i++)
	{
		float3 worldPos = g_PointLights.Position[i] - input.WorldPos;
		float3 lightDirection = normalize(input.WorldPos - (g_PointLights.Position[i]));

		result += CalculatePointDiffuse(
			worldPos,
			g_WaterNormal,
			-lightDirection,
			g_PointLights.Range[i],
			g_PointLights.Diffuse[i]);
	}

	if (g_Fog.Enabled)
	{
		result = ApplyPixelFog(result, input.Fog, g_Fog.Color);
	}

	result.a = input.Color.a * g_TextureFactor.a;

	return result;
}

technique RenderScene
{
	pass P0
	{       
		VertexShader = compile vs_3_0 RenderSceneVS();
		PixelShader = compile ps_3_0 RenderScenePS();
	}
}