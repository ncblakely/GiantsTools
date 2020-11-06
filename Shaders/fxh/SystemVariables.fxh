#include "Constants.fxh"
#include "Lighting.fxh"

// Lighting
shared float4 g_DirectionalAmbientLightSum : DirectionalLightAmbientSum;
shared float4 g_DirectionalLightDiffuse[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightDiffuse;
shared float3 g_DirectionalLightDirection[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightDirection;
shared float4 g_DirectionalLightSpecular[MAX_DIRECTIONAL_LIGHTS] : DirectionalLightSpecular;
shared int g_numDirectionalLights : DirectionalLightCount;
shared float4 g_PointLightDiffuse[MAX_LIGHTS] : PointLightDiffuse;
shared float3 g_PointLightPosition[MAX_LIGHTS] : PointLightPosition;
shared float g_PointLightRangeSquared[MAX_LIGHTS] : PointLightRange;
shared int g_numPointLights : PointLightCount;
shared Material g_Material : Material;

// Camera
shared float3 g_CameraPosition : CameraPosition;

// Transforms
shared float4x4 g_WorldViewProjection : WorldViewProjection;
shared float4x4 g_WorldView : WorldView;
shared float4x4 g_World : World;

// Texturing
shared texture g_texture0 : Texture0;
shared texture g_texture1 : Texture1;
shared texture g_texture2 : Texture2;
shared texture g_texture3 : Texture3;
shared float4x4 g_texGenMatrix0 : TexGenTransform0;
shared float4x4 g_texGenMatrix1 : TexGenTransform1;
shared float4 g_textureFactor : TextureFactor;
shared TextureBlendStage g_blendStages[MAX_BLEND_STAGES] : BlendStages;
shared int g_numBlendStages : BlendStageCount;