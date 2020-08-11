//--------------------------------------------------------------------------------------
// Water.fx

// Very basic shader that implements D3DTOP_ADDSMOOTH functionality in HLSL, replicating
// the original fixed-function water rendering behavior.
//--------------------------------------------------------------------------------------

float4x4 g_WorldViewProjection : WorldViewProjection;
float4 g_TextureFactor : TextureFactor;
//float4 g_Fog : Fog;

texture2D g_WaterTexture;
sampler2D g_WaterTextureSampler = 
sampler_state
{
	Texture = <g_WaterTexture>;
};

struct VS_OUTPUT
{
	float4 Position   : POSITION;   // vertex position 
	float2 TextureUV  : TEXCOORD0;  // vertex texture coords 
	float4 Color	  : COLOR0;
	//float Fog		  : FOG;
};

VS_OUTPUT RenderSceneVS(float4 inPos : POSITION, 
						float4 inDiffuse : COLOR0,
						float4 inTextureUV : TEXCOORD0)
{
	VS_OUTPUT Output;

	Output.Position = mul(inPos, g_WorldViewProjection);
	Output.TextureUV = inTextureUV;
	Output.Color = inDiffuse;

	// Don't think this is right, but we can use FFP fog unless this is compiled for SM 3.0
	//float4 r0;
	//r0.z = inPos.z * g_WorldViewProjection[3];
	//r0.x = -r0.z + g_Fog.y;
	//Output.Fog.x = r0.x * g_Fog.z;

	return Output;    
}

float4 RenderScenePS(VS_OUTPUT input) : COLOR0
{ 
	float4 texel = tex2D(g_WaterTextureSampler, input.TextureUV);
	float4 result = saturate(texel +  (1 - texel) * g_TextureFactor); // equivalent to saturate((texel + g_TextureFactor) - (texel * g_TextureFactor));
	result.a = input.Color.a * g_TextureFactor.a;

	return result;
}

technique RenderScene
{
	pass P0
	{       
		VertexShader = compile vs_2_0 RenderSceneVS();
		PixelShader = compile ps_2_0 RenderScenePS();
	}
}