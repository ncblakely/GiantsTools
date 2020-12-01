struct WorldTransforms
{
    float4x4 World;
    float4x4 WorldInverse;
    float4x4 WorldInverseTranspose;
    float4x4 WorldView;
    float4x4 WorldViewProjection;
    float4x4 WorldViewProjectionTranspose;
};

struct ViewTransforms
{
    float4x4 View;
    float4x4 ViewInverse;
    float3 CameraPosition;
};