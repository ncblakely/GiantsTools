struct FogParams
{
    float FogMin;
    float FogMax;
    float3 Color;
    int Enabled;
};

float CalculateFogFactor(float fogMax, float fogMin, float d)
{
    return saturate((fogMax - d) / (fogMax - fogMin));
}

float4 ApplyPixelFog(float4 pixelColor, float fogFactor, float3 fogColor)
{
    return (fogFactor * pixelColor) + (1.0 - fogFactor) * float4(fogColor, 1);
}