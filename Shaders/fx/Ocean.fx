//--------------------------------------------------------------------------------------
// Ocean.fx

// Ocean water reflection shader.
//--------------------------------------------------------------------------------------

#include "../fxh/Transform.fxh"
#include "../fxh/Fog.fxh"

shared texture g_Texture0; // Seabed texture
shared texture g_Texture1; // Environment texture

shared WorldTransforms g_WorldTransforms;
shared ViewTransforms g_ViewTransforms;

shared FogParams g_Fog;

struct PS_INPUT
{
	float4 color : COLOR0;
	float4 texCoord0 : TEXCOORD0;
	float4 texCoord1 : TEXCOORD1;
	float Fog : FOG;
};

sampler g_SeabedSampler : register(s0) = sampler_state
{
	Texture = <g_Texture0>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

sampler g_EnvironmentSampler = sampler_state
{
	Texture = <g_Texture1>;
	MipFilter = LINEAR;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
};

float4 MainPS(const PS_INPUT input) : COLOR0
{
	// Interpolate linearly between Environment Texture, seabed, and inverted alpha of current pixel
	float4 result = lerp(texCUBE(g_EnvironmentSampler, input.texCoord1), tex2D(g_SeabedSampler, input.texCoord0.xy), 1-input.color.a);

	// Note: fog disabled for now as it doesn't quite match up with FFP fog.
	/*
	if (g_Fog.Enabled)
	{
		result = ApplyPixelFog(result, input.Fog, g_Fog.Color);
	}
	*/

	return result;
}

// C44: (dynamic)
float4 const44 : register(c44);

struct VS_OUTPUT
{
    float4 pos : POSITION;
	float4 texCoord0 : TEXCOORD0;
	float4 texCoord1 : TEXCOORD1;
	float4 color : COLOR0;
	float Fog : FOG;
};

struct VS_INPUT
{
	float4 pos : POSITION;
	float4 texCoord0 : TEXCOORD0;
};

VS_OUTPUT MainVS(const VS_INPUT input)
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	
	output.pos = mul(input.pos, g_WorldTransforms.WorldViewProjection);
	
	// add r0, v0, -c44
	float4 r0 : register(r0) = input.pos + -const44;
	
	// mov r0.z, -r0.z
	r0.z = -r0.z;
	
	// dp3 r0.w, r0, r0
	r0.w = dot(r0.xyz, r0.xyz);
	
	// rsq r0.w, r0.w
	r0.w = rsqrt(r0.w);
	
	// mul oT1.x, -r0, r0.w
	// mul oT1.y, r0, r0.w
	// mul oT1.z, r0, r0.w
	// mov oT1.w, c27.y
	output.texCoord1.x = r0.w * -r0.x;
	output.texCoord1.y = r0.w * r0.y;
	output.texCoord1.z = r0.w * r0.z;
	output.texCoord1.w = 1.0f;

	//rcp r0.w, r0.w
	r0.w = 1.0f / r0.w;

	//mul r0.w, r0.w, c45.x
	//max r0.x, r0.w, c27.x
	//min oD0.w, r0.w, c27.y
	//mov oT0, v2
	r0.w = r0.w * 0.00033333333f;
	output.color.a = min(r0.w, 1.0);
	output.texCoord0 = input.texCoord0;

	/*
	float3 P = mul(input.pos, g_WorldTransforms.WorldView);
	float d = length(P);
	output.Fog = CalculateFogFactor(g_Fog.FogMax, g_Fog.FogMin, d);
	*/

	return output;
}

technique ReflectionLow
{
	pass P0
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 MainPS();
	}
}

struct ReflectionHighVSOutput
{
	float4 pos : POSITION;
	float4 texCoord0 : TEXCOORD0;
	float4 texCoord1 : TEXCOORD1;
	float4 color : COLOR0;
	float Fog : FOG;
	float3 Normal : TEXCOORD2;
	float3 Reflection : TEXCOORD3;
};

struct ReflectionHighVSInput
{
	float4 pos : POSITION;
	float4 texCoord0 : TEXCOORD0;
	float3 Normal : NORMAL0;
};

ReflectionHighVSOutput ReflectionHighVS(const ReflectionHighVSInput input)
{
	ReflectionHighVSOutput output = (ReflectionHighVSOutput)0;

	output.pos = mul(input.pos, g_WorldTransforms.WorldViewProjection);
	output.Normal = normalize(input.Normal);

	float3 normal = normalize(mul(input.Normal, g_WorldTransforms.WorldInverseTranspose));
	float3 viewDirection = g_ViewTransforms.CameraPosition - mul(input.pos, g_WorldTransforms.World);
	output.Reflection = reflect(-normalize(viewDirection), normal);
	return output;
}

float4 ReflectionHighPS(ReflectionHighVSOutput input) : COLOR0
{
	return texCUBE(g_EnvironmentSampler, normalize(input.Reflection));

}

technique ReflectionHigh
{
	pass P0
	{
		VertexShader = compile vs_2_0 ReflectionHighVS();
		PixelShader = compile ps_2_0 ReflectionHighPS();
		//VertexShader = compile vs_2_0 MainVS();
		//PixelShader = compile ps_2_0 MainPS();
	}
}