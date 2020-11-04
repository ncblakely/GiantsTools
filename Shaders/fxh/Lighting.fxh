#pragma once

struct Material
{
    float4 Diffuse;
    float4 Ambient;
    float4 Specular;
    float4 Emissive;
    float Power;
};

struct Lighting
{
    float4 Diffuse      : COLOR0;
    float4 Specular     : COLOR1;
};