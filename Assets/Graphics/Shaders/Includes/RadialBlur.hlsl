float4 Blur_float(UnityTexture2D tex, UnitySamplerState texSampler, float2 uv, float texelDistance, int sampleCount, float texelSize, out float4 result)
{
    float2 offset = float2(0,texelDistance * texelSize);
    result = tex.Sample(texSampler, uv);
    float angle = 360 / max(sampleCount, 1);
    for(int i = 0; i < sampleCount; i++)
    {
        result += tex.Sample(texSampler, uv + offset);
        offset = float2(offset.x * cos(angle) - offset.y * sin(angle),
                        offset.y * cos(angle) - offset.x * sin(angle));
    }
    result /= max(sampleCount + 1, 1);
    return result;
}