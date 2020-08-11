//--------------------------------------------------------------------------------------
// Sky.fx

// Experimental sky shader. Does nothing currently.
//--------------------------------------------------------------------------------------

float4x4 g_mWorldViewProjection : WorldViewProjection;
float4x4 g_World : World;
float4x4 g_View : View;
float4x4 g_Projection : Projection;
float intensityThreshold = 1.f;
float colorMultiplier = 1.f;
texture	g_SkyTexture;

sampler g_SkyTextureSampler = 
sampler_state
{
    Texture		= <g_SkyTexture>;    
    MipFilter	= LINEAR;
    MinFilter	= LINEAR;
    MagFilter	= LINEAR;
};
void RenderSceneVS( 
	float3 iPosition : POSITION, 
	float2 iTexCoord0 : TEXCOORD0,

	out float4 oPosition : POSITION,
    out float4 oColor0 : COLOR0,
    out float2 oTexCoord0 : TEXCOORD0 )
{
	oPosition = mul(iPosition, g_mWorldViewProjection);
	//oPosition = mul(iPosition, g_World);
	//oPosition = mul(oPosition, g_View);
	//oPosition = mul(oPosition, g_Projection);
    oColor0 = float4(1, 1, 1, 1);
    oTexCoord0 = iTexCoord0;
}

float4 RenderScenePS( 
	float4 iColor : COLOR,
	float2 iTexCoord0 : TEXCOORD0) : COLOR0
{ 
	//return float4(1, 1, 1, 1);
	return tex2D(g_SkyTextureSampler, iTexCoord0);
	//oColor0 = tex2D(TextureDiffuse0Sampler, iTexCoord0);
	//float intensity = dot(oColor0.rgb, float3(0.3, 0.59, 0.11));
	//float p = smoothstep(intensityThreshold, 1.f, intensity);
	//oColor0 = oColor0 * p * colorMultiplier;
}

technique RenderWithPixelShader
{
    pass Pass0
    {   	        
        VertexShader = compile vs_1_1 RenderSceneVS();
        PixelShader = compile ps_2_0 RenderScenePS();
        ZEnable = FALSE;
        ZWriteEnable = FALSE;
    }
}

