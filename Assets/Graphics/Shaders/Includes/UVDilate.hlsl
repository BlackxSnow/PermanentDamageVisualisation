//////////////// uv Positional Dilation ///////////////////////////
//** tex **// Input texture Object storing Volume Data
//** uv **// Input float2 for UVs
//** textureSize **// Resolution of render target
//** MaxSteps **// Pixel Radius to search
float3 Dilate(sampler2D tex, float2 uv, float textureSize, int stepCount) 
{
    float texelsize = 1 / textureSize;
    float mindist = 10000000;
    float2 offsets[8] = {float2(-1,0), float2(1,0), float2(0,1), float2(0,-1), float2(-1,1), float2(1,1), float2(1,-1), float2(-1,-1)};

    float3 sample = tex2Dlod(tex, float4(uv, 0, 0));
    float3 curminsample = sample;

    if(sample.x == 0 && sample.y == 0 && sample.z == 0)
    {
        for(int i = 0; i < stepCount; ++i)
        { 
            for (int j = 0; j < 8; j++)
            {
                float2 curUV = uv + offsets[j] * texelsize * i;
                float3 offsetsample = tex2Dlod(tex, float4(curUV, 0, 0));

                if(offsetsample.x != 0 || offsetsample.y != 0 || offsetsample.z != 0)
                {
                    float curdist = length(uv - curUV);

                    if (curdist < mindist)
                    {
                        float2 projectUV = curUV + offsets[j] * texelsize * i * 0.25;
                        float3 direction = tex2Dlod(tex, float4(projectUV, 0, 0));
                        mindist = curdist;

                        if(direction.x != 0 || direction.y != 0 || direction.z != 0)
                        {
                            float3 delta = offsetsample - direction;
                            curminsample = offsetsample + delta * 4;
                        }

                    else
                        {
                            curminsample = offsetsample;
                        }
                    }
                }
            }
        }
    }

    return curminsample;
}