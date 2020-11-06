//--------------------------------------------------------------------------------------
// Water.fx

// Ocean water reflection shader.
//--------------------------------------------------------------------------------------

#include "../fxh/SystemVariables.fxh"

/* Original asm code:
	ps_1_1
	tex t0
	tex t1
	lrp r0, -v0.w, t0, t1 // lrp = linear interpolate
*/

struct PS_INPUT
{
	float4 color : COLOR0;
	float4 texCoord0 : TEXCOORD0;
	float4 texCoord1 : TEXCOORD1;
};

sampler s0, s1;

float4 MainPS(const PS_INPUT input) : COLOR0
{
	// Interpolate linearly between Environment Texture, seabed, and inverted alpha of current pixel
	return lerp(tex2D(s1, input.texCoord1), tex2D(s0, input.texCoord0), 1-input.color.a);
}

// C45: {0.00333, 0, 0, 0}
// C44:
// C27: {0, 1.0, 0.5, 0.25}
// C26: {minfog, maxfog, 1.0f / (maxfog - minfog), 1.0}

float4 const44 : register(c44);
shared matrix<float, 4, 4> g_OceanWorldViewProjection : OceanWorldViewProjection : register(c2);
float4 fog : register (c26);

struct VS_OUTPUT
{
    float4 pos : POSITION;
	float4 texCoord1 : TEXCOORD1;
};

struct VS_INPUT
{
	float4 pos : POSITION;
};

VS_OUTPUT MainVS(const VS_INPUT input)
{
	float4 const27 = {0.0f, 1.0f, 0.5f, 0.75f};
	VS_OUTPUT output;
	
	output.pos = mul(input.pos, g_OceanWorldViewProjection);
	
	// add r0, v0, -c44
	float4 r0 = input.pos + -const44;
	
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
	output.texCoord1.x = -r0 * r0.w;
	output.texCoord1.y = r0 * r0.w;
	output.texCoord1.z = r0 * r0.w;
	output.texCoord1.w = const27.y;
	
	return output;
}

technique t0
{
	pass p0
	{
		Texture[0] = <g_texture0>;	// Seabed texture
		Texture[1] = <g_texture1>;	// Environment texture
		
		// All of these constants are set by the game engine before drawing the shader
		// Each constant register (c# in the asm code) has 4 floating point values
		
		// Special world * view * projection matrix for ocean shader
		VertexShaderConstant[2] = <g_OceanWorldViewProjection[0]>;
		VertexShaderConstant[3] = <g_OceanWorldViewProjection[1]>;
		VertexShaderConstant[4] = <g_OceanWorldViewProjection[2]>;
		VertexShaderConstant[5] = <g_OceanWorldViewProjection[3]>;
		
		VertexShaderConstant[26] = <fog>;	// This constant is calculated from the current fog min/max values
		VertexShaderConstant[27] = {0.0f, 1.0f, 0.5f, 0.75f}; // I don't know what this does but it doesn't change
		VertexShaderConstant[44] = <const44>; // I don't know what this is
		VertexShaderConstant[45] = {0.00033333333f, 0, 0, 0}; // I don't know what this does but it doesn't change
#if 1	// 1 to use asm shader	
		VertexShader =
			asm
			{
				vs_1_1
				dcl_position v0
				dcl_texcoord v2
				
				/* Places the dot product of the world view matrix and v0 in the output register oPos */
				dp4 oPos.x, v0, c2
				dp4 oPos.y, v0, c3
				dp4 oPos.z, v0, c4
				dp4 oPos.w, v0, c5
				
				// this is some kind of vector normalization or transform
				add r0, v0, -c44
				mov r0.z, -r0.z
				dp3 r0.w, r0, r0
				rsq r0.w, r0.w
				
				/* Output register for texture 1 (environment texture) is modified by register 0 */
				mul oT1.x, -r0, r0.w
				mul oT1.y, r0, r0.w
				mul oT1.z, r0, r0.w
				mov oT1.w, c27.y
				
				rcp r0.w, r0.w
				mul r0.w, r0.w, c45.x
				max r0.x, r0.w, c27.x
				min oD0.w, r0.w, c27.y
				mov oT0, v2
				dp4 r0.z, v0, c5
				add r0.x, -r0.z, c26.y
				mul oFog, r0.x, c26.z				
			};
#else
		VertexShader = compile vs_1_1 MainVS();
#endif
			
		PixelShader = compile ps_1_3 MainPS(); // effect will not work > ps 1.3
	}
}
