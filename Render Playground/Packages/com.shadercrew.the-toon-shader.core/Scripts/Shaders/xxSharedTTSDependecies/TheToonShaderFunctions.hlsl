#ifndef THETOONSHADER_FUNCTION
#define THETOONSHADER_FUNCTION








        































struct GeneralStylingData
{
    half enableDistanceFade;
    float distanceFadeStartDistance;
    float distanceFadeFalloff;
    half adjustDistanceFadeValue;
    float distanceFadeValue;
};


struct StylingData
{
    half isEnabled;
    half style;
    half type;
    float4 color;
    float rotation;
    float rotationBetweenCells;
    float density;
    float offset;
    float size;
    float sizeControl;
    float sizeFalloff;
    float roundness;
    float roundnessFalloff;
    float hardness;
    float opacity;
    float opacityFalloff;
};

struct StylingRandomData
{
    float enableRandomizer;
    float perlinNoiseSize;
    float perlinNoiseSeed;
    float whiteNoiseSeed;
    
    float noiseIntensity;
    
    half spacingRandomMode;
    float spacingRandomIntensity;

    half opacityRandomMode; 
    float opacityRandomIntensity;

    half lengthRandomMode;
    float lengthRandomIntensity;

    half hardnessRandomMode;
    float hardnessRandomIntensity;

    half thicknessRandomMode; 
    float thicknesshRandomIntensity;
    
   
   

};

struct AdditionalStylingSpecularData
{
    
};

struct AdditionalStylingRimData
{
    
};

struct PositionAndBlendingData
{
    half position;
    half blending;
    half isInverted;
};

struct UVSpaceData
{
    half drawSpace;
    half coordinateSystem;
    half polarCenterMode;
    float4 polarCenter;
    half sSCameraDistanceScaled;
    half anchorSSToObjectsOrigin;
};


struct NoiseSampleData
{
    float perlinNoise;
    float perlinNoiseFloored;
    float whiteNoise;
    float whiteNoiseFloored;
};

struct RequiredNoiseData
{
    bool perlinNoise;
    bool perlinNoiseFloored;
    bool whiteNoise;
    bool whiteNoiseFloored;
};


#define UNITY_TWO_PI        6.28318530718f
float sum(
float3 ll0
)
{
   return dot(ll0, float3(1, 1, 1));
}
float invLerp(
float llll0, float lllll0, float llllll0
)
{
    return (llllll0 - llll0) / (lllll0 - llll0);
}
float4 invLerp(
float4 llll0, float4 lllll0, float4 llllll0
)
{
    return (llllll0 - llll0) / (lllll0 - llll0);
}
float remap(
float llllllllllll0, float lllllllllllll0, float llllllllllllll0, float lllllllllllllll0, float llllll0
)
{
    float lllllllllllllllll0 = invLerp(llllllllllll0, lllllllllllll0, llllll0);
    return lerp(llllllllllllll0, lllllllllllllll0, lllllllllllllllll0);
}
float2 GetScreenUV(
float2 lllllllllllllllllll0, float llllllllllllllllllll0
)
{
#if _URP
    float4 lllllllllllllllllllll0 = TransformObjectToHClip(float3(0, 0, 0));
#else
    float4 lllllllllllllllllllll0 = UnityObjectToClipPos(float3(0, 0, 0));
#endif
    float2 lllllllllllllllllllllll0 = float2(lllllllllllllllllll0.x, lllllllllllllllllll0.y);
    float llllllllllllllllllllllll0 = _ScreenParams.y / _ScreenParams.x;
    lllllllllllllllllllllll0.x -= lllllllllllllllllllll0.x / (lllllllllllllllllllll0.w);
    lllllllllllllllllllllll0.y -= lllllllllllllllllllll0.y / (lllllllllllllllllllll0.w);
    lllllllllllllllllllllll0.y *= llllllllllllllllllllllll0;
    lllllllllllllllllllllll0 *= 1 / llllllllllllllllllll0;
    lllllllllllllllllllllll0 *= lllllllllllllllllllll0.z;
    return lllllllllllllllllllllll0;
};
float2 toPolar(
float2 llllllllllllllllllllllllll0
)
{
    float lllllllllllllllllllllllllll0 = length(llllllllllllllllllllllllll0);
    float llllllllllllllllllllllllllll0 = atan2(llllllllllllllllllllllllll0.y, llllllllllllllllllllllllll0.x);
    return float2(llllllllllllllllllllllllllll0 / UNITY_TWO_PI, lllllllllllllllllllllllllll0);
}
float2 ConvertToDrawSpace(
#if _URP
    InputData inputData, 
#else
    float3 llllllllllllllllllllllllllllll0,
#endif
float2 lllllllllllllllllllllllllllllll0, UVSpaceData uvSpaceData , float4 lllllllllllllllllllllll0
)
{
    if (uvSpaceData.drawSpace == 0)    
    {
    }
    else if (uvSpaceData.drawSpace = 1)    
    {
#if _URP
        float3 llllllllllllllllllllllllllllll0 = inputData.positionWS;
#endif
        float4 lllllllllllllllllll0 = mul(UNITY_MATRIX_VP, float4(llllllllllllllllllllllllllllll0, 1.0));
        float4 llll1 = ComputeScreenPos(lllllllllllllllllll0);
        lllllllllllllllllllllllllllllll0 = ((llll1.xy) / llll1.w); 
        if (uvSpaceData.anchorSSToObjectsOrigin)
        {
            float4 lllll1 = mul(UNITY_MATRIX_VP, float4(_WorldSpaceCameraPos, 1.0));
            float2 llllll1 = lllll1.xy / lllll1.w;
            float2 lllllll1 = lllllllllllllllllllllll0.xy;
            lllllllllllllllllllllllllllllll0 = lllllllllllllllllllllllllllllll0 - lllllll1; 
        }
    }
    else 
    {
    }
    if (uvSpaceData.coordinateSystem == 1) 
    {
        if (uvSpaceData.drawSpace == 1)
        {
            if (uvSpaceData.polarCenterMode == 0) 
            {
                lllllllllllllllllllllllllllllll0.xy -= uvSpaceData.polarCenter.xy;
            }
            else 
            {
                uvSpaceData.polarCenter.a = 1;
                float4 llllllll1 = mul(UNITY_MATRIX_VP, uvSpaceData.polarCenter);
                float4 lllllllll1 = ComputeScreenPos(llllllll1);
                float2 llllllllll1 = lllllllll1.xy / lllllllll1.w;
                lllllllllllllllllllllllllllllll0.xy -= llllllllll1;
            }
        }
        else
        {
            lllllllllllllllllllllllllllllll0.xy -= uvSpaceData.polarCenter.xy;
        }
    }
    if (uvSpaceData.coordinateSystem == 1) 
    {
        lllllllllllllllllllllllllllllll0 = toPolar(lllllllllllllllllllllllllllllll0);
    }
    if (uvSpaceData.drawSpace == 1)
    {
        if (uvSpaceData.sSCameraDistanceScaled == 1)
        {
            float3 lllllllllll1 = mul(UNITY_MATRIX_M, float4(0, 0, 0, 1.0)).xyz;
            lllllllllllllllllllllllllllllll0.xy *= distance(_WorldSpaceCameraPos, lllllllllll1);
        }
        float llllllllllll1 = _ScreenParams.x / _ScreenParams.y;
        lllllllllllllllllllllllllllllll0.x *= llllllllllll1;
    }
    return lllllllllllllllllllllllllllllll0;
}
float CalculateSpecularMaskSkipDot(
float llllllllllllll1, float3 lllllllllllllll1, float llllllllllllllll1, float lllllllllllllllll1, float llllllllllllllllll1
)
{
    float lllllllllllllllllll1 = 0;
    float llllllllllllllllllll1 = (1 - (llllllllllllllll1)) * 10; 
    llllllllllllll1 = max(llllllllllllll1, 0); 
    float lllllllllllllllllllll1 = pow(llllllllllllll1, llllllllllllllllllll1 * llllllllllllllllllll1);
    float llllllllllllllllllllll1 = smoothstep(0.8, 0.8 + lllllllllllllllll1 / 1, lllllllllllllllllllll1);
    if (llllllllllllllllll1 > 0.0)
    {
        lllllllllllllllllll1 = llllllllllllllllllllll1;
    }
    return lllllllllllllllllll1;
}
float CalculateSpecularMask(
float3 llllllllllllllllllllllll1, float3 lllllllllllllllllllllllll1, float3 lllllllllllllll1, float llllllllllllllll1, float lllllllllllllllll1, float llllllllllllllllll1
)
{
    float lllllllllllllllllll1 = 0;
    float3 lllllllllllllllllllllllllllllll1 = normalize(lllllllllllllllllllllllll1 + lllllllllllllll1);
    float llllllllllllll1 = dot(llllllllllllllllllllllll1, lllllllllllllllllllllllllllllll1);
    lllllllllllllllllll1 = CalculateSpecularMaskSkipDot(llllllllllllll1, lllllllllllllll1, llllllllllllllll1, lllllllllllllllll1, llllllllllllllllll1);
    return lllllllllllllllllll1;
}
float CalculateRimMask(
float3 lll2, float3 lllllllllllllll1, float lllll2, float llllll2, float llllllllllllllllll1,
                        half llllllll2, half lllllllll2, half llllllllll2, float lllllllllll2
)
{
    float llllllllllll2 = 0;         
    float lllllllllllll2 = saturate(1 - dot(lllllllllllllll1, lll2));
    lllll2 = 1 - lllll2;
    float llllllllllllll2 = smoothstep(saturate(lllll2 - llllll2), lllll2, lllllllllllll2);
    if ((llllllll2 == 0 && llllllllllllllllll1 > 0.0 && ((lllllllllll2 >= 0 || lllllllll2 == 0) || llllllllll2 == 0))
    || (llllllll2 == 1 && (llllllllllllllllll1 <= 0.0 || (lllllllllll2 <= 2 && lllllllll2 == 1)))
    || llllllll2 == 2 )
    {
        if (llllllll2 == 1)
        {
            float lllllllllllllll2 = llllllllllllllllll1;
            if (lllllllll2)
            {
                if (llllllllllllllllll1 > 0)
                {
                    llllllllllllllllll1 *= lllllllllll2;
                }
            }
            {
                float llllllllllllllll2 = 1 - abs(min(llllllllllllllllll1 * 2 , 0)); 
                if (lllllllllllllll2 > 0)
                {
                    llllllllllllllll2 = lllllllllll2;
                }
                llllllllllll2 = llllllllllllll2 * (1 - llllllllllllllll2);
            }
        }
        else if (llllllll2 == 0)
        {
            llllllllllll2 = llllllllllllll2 * (llllllllllllllllll1 * 2) * (lllllllllll2);
        }
        else if (llllllll2 == 2)
        {
            llllllllllll2 = llllllllllllll2;
        }
    }
    return llllllllllll2;
}
float CalculateRimMask2(
float3 lll2, float3 lllllllllllllll1, float lllll2, float llllll2, float llllllllllllllllll1,
                        half llllllll2, half lllllllll2, half llllllllll2, float lllllllllll2
)
{
    float llllllllllll2 = 0;        
    float lllllllllllll2 = saturate(1 - dot(lllllllllllllll1, lll2));
    lllll2 = 1 - lllll2;
    float llllllllllllll2 = smoothstep(saturate(lllll2 - llllll2), lllll2, lllllllllllll2);
    if ((llllllll2 == 0 && llllllllllllllllll1 > 0.0 && ((lllllllllll2 >= 0 || lllllllll2 == 0) || llllllllll2 == 0))
    || (llllllll2 == 1 && (llllllllllllllllll1 <= 0.0 || (lllllllllll2 <= 2 && lllllllll2 == 1)))
    || llllllll2 == 2)
    {
        if (llllllll2 == 1)
        {
            if (lllllllll2)
            {
                llllllllllll2 = llllllllllllll2 * (1 - lllllllllll2);
            }
            else
            {
                float llllllllllllllll2 = 1 - abs(min(llllllllllllllllll1 * 2, 0)); 
                float ll0 = lerp(0, llllllllllllllll2 * 4, llllll2);
                llllllllllll2 = llllllllllllll2 * (1 - llllllllllllllll2);
            }
        }
        else if (llllllll2 == 2)
        {
            llllllllllll2 = llllllllllllll2; 
        }
        else
        {
            llllllllllll2 = llllllllllllll2 * (llllllllllllllllll1 * 2) * (lllllllllll2);
        }
    }
    return llllllllllll2;
}
float2 RotateUV(
float2 lllllllllllllllllllllllllllllll0, float llllllllllllllllllllllllllll0
)
{
    float llll3 = radians(llllllllllllllllllllllllllll0);
    float lllll3= cos(llll3);
    float llllll3= sin(llll3);
    float2 lllllll3;
    lllllll3.x = lllllllllllllllllllllllllllllll0.x * lllll3 - lllllllllllllllllllllllllllllll0.y * llllll3;
    lllllll3.y = lllllllllllllllllllllllllllllll0.x * llllll3 + lllllllllllllllllllllllllllllll0.y * lllll3;
    return lllllll3;
}
float2 RotateUVRadians(
float2 lllllllllllllllllllllllllllllll0, float llllllllll3
)
{
    float llll3 = llllllllll3;                
    float lllll3 = cos(llll3);
    float llllll3 = sin(llll3);
    float2 lllllll3;
    lllllll3.x = lllllllllllllllllllllllllllllll0.x * lllll3 - lllllllllllllllllllllllllllllll0.y * llllll3;
    lllllll3.y = lllllllllllllllllllllllllllllll0.x * llllll3 + lllllllllllllllllllllllllllllll0.y * lllll3;
    return lllllll3;
}
NoiseSampleData SampleNoiseData(
float2 lllllllllllllllllllllllllllllll0, StylingData stylingData, StylingRandomData stylingRandomData, RequiredNoiseData requiredNoiseData, sampler2D llllllllllllllll3, sampler2D lllllllllllllllll3
)
{
    NoiseSampleData noiseSampleData;
    if (stylingRandomData.enableRandomizer == 1)
    {
        if (stylingData.style == 1)
        {
            if (fmod(floor(lllllllllllllllllllllllllllllll0.y * stylingData.density), 2) == 0)
            {
                lllllllllllllllllllllllllllllll0.x += stylingData.offset / stylingData.density;
            }
        }
        float llllllllllllllllll3 = 0;
        if (requiredNoiseData.perlinNoiseFloored == 1)
        {
            float2 lllllllllllllllllll3 = lllllllllllllllllllllllllllllll0;
            lllllllllllllllllll3.x = floor(lllllllllllllllllllllllllllllll0.x * stylingData.density) / stylingData.density;
            if (stylingData.style == 0)
            {
            }
            else if (stylingData.style == 1)
            {
                lllllllllllllllllll3.y = floor(lllllllllllllllllllllllllllllll0.y * stylingData.density) / stylingData.density;
            }
            lllllllllllllllllll3 *= stylingRandomData.perlinNoiseSize;
            llllllllllllllllll3 = tex2Dlod(llllllllllllllll3, float4(lllllllllllllllllll3, 0.0, 0.0)).x; 
        }
        float llllllllllllllllllll3 = 0;
        if (requiredNoiseData.perlinNoise == 1)
        {
            float2 lllllllllllllllllllll3 = lllllllllllllllllllllllllllllll0 * stylingRandomData.perlinNoiseSize;
            llllllllllllllllllll3 = tex2Dlod(llllllllllllllll3, float4(lllllllllllllllllllll3, 0.0, 0.0)).x; 
        }
        float llllllllllllllllllllll3 = 0;
        if (requiredNoiseData.whiteNoise == 1)
        {
            float2 lllllllllllllllllllllll3 = lllllllllllllllllllllllllllllll0;
            lllllllllllllllllllllll3.x = floor(lllllllllllllllllllllllllllllll0.x * stylingData.density) / stylingData.density;
            if (stylingData.style == 0)
            {
                lllllllllllllllllllllll3.y = 0.1;
            }
            else
            if (stylingData.style == 1)
            {
                lllllllllllllllllllllll3.y = floor(lllllllllllllllllllllllllllllll0.y * stylingData.density) / stylingData.density;
            }
            llllllllllllllllllllll3 = tex2Dlod(lllllllllllllllll3, float4(lllllllllllllllllllllll3, 0.0, 0.0)).x; 
        }
        float llllllllllllllllllllllll3;
        if (requiredNoiseData.whiteNoiseFloored == 1)
        {
            float2 lllllllllllllllllllllllll3 = lllllllllllllllllllllllllllllll0;
            lllllllllllllllllllllllll3.x = floor(lllllllllllllllllllllllllllllll0.x * stylingData.density) / stylingData.density;
            if (stylingData.style == 1)
            {
                lllllllllllllllllllllllll3.y = 0.1;
            }
            llllllllllllllllllllllll3 = tex2Dlod(lllllllllllllllll3, float4(lllllllllllllllllllllllll3, 0.0, 0.0)).x; 
        }
        noiseSampleData.perlinNoise = llllllllllllllllllll3;
        noiseSampleData.perlinNoiseFloored = llllllllllllllllll3;
        noiseSampleData.whiteNoise = llllllllllllllllllllll3;
        noiseSampleData.whiteNoiseFloored = llllllllllllllllllllllll3;
    }
    else
    {
        noiseSampleData.perlinNoise = 0;
        noiseSampleData.perlinNoiseFloored = 0;
        noiseSampleData.whiteNoise = 0;
        noiseSampleData.whiteNoiseFloored = 0;
    }
    return noiseSampleData;
}
float Hatching(
float llllll0, float2 lllllllllllllllllllllllllllllll0, StylingData hatchingData, StylingRandomData stylingRandomData, NoiseSampleData noiseSampleData, half lllllllllllllllllllllllllllll3
)
{
    llllll0 = 1 - llllll0;   
    float2 llllllllllllllllllllllllllllll3 = lllllllllllllllllllllllllllllll0;      
    float lllllllllllllllllllllllllllllll3 = hatchingData.size / 2;    
    float l4 = llllllllllllllllllllllllllllll3.x;            
    l4 *= hatchingData.density;
    if (stylingRandomData.enableRandomizer == 1)
    {
        l4 += noiseSampleData.perlinNoise * stylingRandomData.noiseIntensity;
        float ll4 = 0;
        if (stylingRandomData.thicknessRandomMode == 0)
        {
            ll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.thicknessRandomMode == 1) 
        {
            ll4 = noiseSampleData.perlinNoiseFloored;
        }
        else 
        {
            ll4 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
        }
        ll4 *= stylingRandomData.thicknesshRandomIntensity;
        float lll4 = remap(0, 1, 0.0, lllllllllllllllllllllllllllllll3, ll4);
        lllllllllllllllllllllllllllllll3 -= lll4;
        float llll4 = 0;
        if (stylingRandomData.spacingRandomMode == 0)
        {
            llll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.spacingRandomMode == 1) 
        {
            llll4 = noiseSampleData.perlinNoiseFloored;
        }
        else 
        {
            llll4 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
        }
        float lllll4 = remap(0, 1, -0.5 + lllllllllllllllllllllllllllllll3, 0.5 - lllllllllllllllllllllllllllllll3, llll4);
        l4 += lllll4 * stylingRandomData.spacingRandomIntensity * saturate(1 - stylingRandomData.noiseIntensity); 
    }
    l4 = abs(frac(l4) - 0.5);
    float llllll4 = 0;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float lllllll4 = 0;
        if (stylingRandomData.lengthRandomMode == 0)
        {
            lllllll4 = noiseSampleData.whiteNoise * saturate(1 - stylingRandomData.noiseIntensity); 
        }
        else if (stylingRandomData.lengthRandomMode == 1)
        {
            lllllll4 = noiseSampleData.perlinNoiseFloored; 
        }
        else
        {
            lllllll4 = ((noiseSampleData.perlinNoiseFloored + (noiseSampleData.whiteNoise * saturate(1 - stylingRandomData.noiseIntensity))) / 2); 
        }
        float llllllll4 = lllllll4 * stylingRandomData.lengthRandomIntensity;
        llllll4 = remap(0, 1 - llllllll4, 0, 1, llllll0);    
    }
    else
    {
        llllll4 = remap(0, 1, 0, 1, llllll0);;
    }    
    float lllllllll4 = smoothstep(min(1 - hatchingData.sizeFalloff, 0.99), 1, llllll4);
    lllllllll4 = max(lllllllllllllllllllllllllllllll3 - lllllllll4, 0);
    float llllllllll4 = 0;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float lllllllllll4 = 0;
        if (stylingRandomData.hardnessRandomMode == 0) 
        {
            lllllllllll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.hardnessRandomMode == 1) 
        {
            lllllllllll4 = noiseSampleData.perlinNoiseFloored * 5;
        }
        else
        {
            lllllllllll4 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
        }
        llllllllll4 = remap(0, 1, 0, lllllllll4, min(saturate(hatchingData.hardness - lllllllllll4 * stylingRandomData.hardnessRandomIntensity), hatchingData.hardness));
    }
    else
    {
        llllllllll4 = remap(0, 1, 0, lllllllll4, hatchingData.hardness);
    }
    if (lllllllll4 != 0 )
    {
        float llllllllllll4 = 0;
        if (lllllllllllllllllllllllllllll3)
        {
            llllllllllll4 = fwidth(l4); 
        }
        if (lllllllll4 == lllllllllllllllllllllllllllllll3 && hatchingData.size == 1)
        {
            llllllllllll4 = 0;
        }                        
        if (llllllllll4 - llllllllllll4 < 0) 
        {
            llllllllllll4 = 0;
        }
        l4 = smoothstep(llllllllll4 - llllllllllll4, lllllllll4 + llllllllllll4, l4);
    }
    else
    {
        l4 = 1; 
    }
    l4 = 1 - l4;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float lllllllllllll4;
        if (stylingRandomData.opacityRandomMode == 0) 
        {
            lllllllllllll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.opacityRandomMode == 1) 
        {
            lllllllllllll4 = noiseSampleData.perlinNoiseFloored * 5;
        }
        else 
        {
            lllllllllllll4 = ((noiseSampleData.perlinNoiseFloored * 5) + noiseSampleData.whiteNoise) / 2;
            lllllllllllll4 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
        }
        l4 = saturate(l4 - (lllllllllllll4 * stylingRandomData.opacityRandomIntensity));
    }
    float llllllllllllll4 = smoothstep(min(1-hatchingData.opacityFalloff, 0.99), 1, llllll4);
    l4 *= 1 - llllllllllllll4;
    l4 *= hatchingData.opacity;
    return l4;
}
float Halftones(
float llllll0, float2 lllllllllllllllllllllllllllllll0, StylingData halftonesData, StylingRandomData stylingRandomData, NoiseSampleData noiseSampleData
)
{            
    float2 llllllllllllllllll4 = lllllllllllllllllllllllllllllll0;               
    llllllllllllllllll4 *= halftonesData.density;
    if (stylingRandomData.enableRandomizer == 1)
    {
        llllllllllllllllll4 += noiseSampleData.perlinNoise * stylingRandomData.noiseIntensity;
    }
    if (fmod(floor(llllllllllllllllll4.y), 2) == 0)
    {
        llllllllllllllllll4.x += halftonesData.offset;
    }
    if (stylingRandomData.enableRandomizer == 1)
    {
        float lllllll4 = 0;
        if (stylingRandomData.lengthRandomMode == 0)
        {
            lllllll4 = noiseSampleData.whiteNoiseFloored * saturate(1 - stylingRandomData.noiseIntensity); 
        }
        else if (stylingRandomData.lengthRandomMode == 1)
        {
            lllllll4 = noiseSampleData.perlinNoiseFloored; 
        }
        else
        {
            lllllll4 = ((noiseSampleData.perlinNoiseFloored + (noiseSampleData.whiteNoise * saturate(1 - stylingRandomData.noiseIntensity))) / 2); 
        }
        float llllllll4 = lllllll4 * stylingRandomData.lengthRandomIntensity;
        llllll0 -= llllllll4;
    }
    float lllllllllllllllllllll4 = halftonesData.size;
    if (halftonesData.sizeControl == 1)  
    {
        lllllllllllllllllllll4 *= llllll0;
    }
    else
    {
        float llllllllllllllllllllll4 = smoothstep(min(1 - halftonesData.sizeFalloff, 1), 1, (1 - llllll0));
        lllllllllllllllllllll4 = max(lllllllllllllllllllll4 - llllllllllllllllllllll4, 0);
    }
    lllllllllllllllllllll4 /= 2;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float ll4 = 0;
        if (stylingRandomData.thicknessRandomMode == 0)
        {
            ll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.thicknessRandomMode == 1) 
        {
            ll4 = noiseSampleData.perlinNoiseFloored;
        }
        else 
        {
            ll4 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
        }
        float llllllllllllllllllllllll4 = remap(0, 1, 0.0, lllllllllllllllllllll4, ll4 * stylingRandomData.thicknesshRandomIntensity);
        lllllllllllllllllllll4 -= llllllllllllllllllllllll4;
    }
    float lllllllllllllllllllllllll4 = 1 - halftonesData.roundness;
    float llllllllllllllllllllllllll4 = smoothstep(halftonesData.roundnessFalloff, 1, 1 - llllll0);
    lllllllllllllllllllllllll4 = max(lllllllllllllllllllllllll4 - llllllllllllllllllllllllll4 * 4, 0);
    lllllllllllllllllllllllll4 /= 2;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float llll4 = 0;
        if (stylingRandomData.spacingRandomMode == 0)
        {
            llll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.spacingRandomMode == 1) 
        {
            llll4 = noiseSampleData.perlinNoiseFloored;
        }
        else 
        {
            llll4 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
        }
        float lllll4 = remap(0, 1, -0.5 + lllllllllllllllllllll4, 0.5 - lllllllllllllllllllll4, llll4);
        llllllllllllllllll4 += lllll4 * stylingRandomData.spacingRandomIntensity * saturate(1 - stylingRandomData.noiseIntensity); 
    }
    float lllllllllllllllllllllllllllll4 = halftonesData.hardness;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float lllllllllll4 = 0;
        if (stylingRandomData.hardnessRandomMode == 0) 
        {
            lllllllllll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.hardnessRandomMode == 1) 
        {
            lllllllllll4 = noiseSampleData.perlinNoiseFloored * 5;
        }
        else
        {
            lllllllllll4 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
        }
        lllllllllllllllllllllllllllll4 = min(saturate(halftonesData.hardness - lllllllllll4 * stylingRandomData.hardnessRandomIntensity), halftonesData.hardness);
    }
    float lllllllllllllllllllllllllllllll4 = remap(0, 1, 0, lllllllllllllllllllll4, lllllllllllllllllllllllllllll4);
    float lllllllllllllllllllllllllll0 = length(max(abs(frac(llllllllllllllllll4) - 0.5) - lllllllllllllllllllllllll4 * lllllllllllllllllllllllllllllll4 * 2, 0.0)) + lllllllllllllllllllllllll4 * lllllllllllllllllllllllllllllll4 * 2;
    float ll5 = smoothstep(lllllllllllllllllllllllllllllll4, lllllllllllllllllllll4, lllllllllllllllllllllllllll0);
    ll5 = 1 - ll5;
    if (stylingRandomData.enableRandomizer == 1)
    {
        float lllllllllllll4;
        if (stylingRandomData.opacityRandomMode == 0) 
        {
            lllllllllllll4 = noiseSampleData.whiteNoise;
        }
        else if (stylingRandomData.opacityRandomMode == 1) 
        {
            lllllllllllll4 = noiseSampleData.perlinNoiseFloored * 5;
        }
        else 
        {
            lllllllllllll4 = ((noiseSampleData.perlinNoiseFloored * 5) + noiseSampleData.whiteNoise) / 2;
            lllllllllllll4 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
        }
        ll5 = saturate(ll5 - (lllllllllllll4 * stylingRandomData.opacityRandomIntensity));
    }
    float llll5 = smoothstep(min(1-halftonesData.opacityFalloff, 0.99), 1, 1 - llllll0);
    if (halftonesData.type == 1 || halftonesData.opacityFalloff != 0)
    {
        ll5 *= 1 - llll5;
    }
    ll5 *= halftonesData.opacity;
    ll5 = 1 - ll5;
    return ll5;
}
void DoBlending(
inout float4 lllll5, float llllll0, float lllllll5, float4 llllllll5
)
{
    if (lllllll5 == 0) 
    {
        lllll5 = lerp(lllll5, llllllll5, llllll0);
    }
    else if (lllllll5 == 1) 
    {        
        lllll5 += (llllllll5 * llllll0);
    }
    else if (lllllll5 == 2) 
    {
        lllll5 *= 1-llllll0 + (llllllll5 * llllll0); 
    }
    else if (lllllll5 == 3) 
    {
        lllll5 -= (llllllll5 * llllll0);
    }
    else if (lllllll5 == 4) 
    {
        lllll5 = lerp(lllll5, llllllll5, llllll0);
    }
}
void DoToonShading(
#if _URP
    InputData inputData, 
    SurfaceData surface,
#else
#if _USESPECULAR || _USESPECULARWORKFLOW || _SPECULARFROMMETALLIC
                 SurfaceOutputStandardSpecular o,
#elif _BDRFLAMBERT || _BDRF3 || _SIMPLELIT
                 SurfaceOutput o,
#else
                 SurfaceOutputStandard o,
#endif
    UnityGI gi,
#if !_PASSFORWARDADD
    UnityGIInput giInput,
#endif
#endif
    ShaderData d,
#if _URP
    #if UNITY_VERSION >= 202120
    float3 lllllllll5,
    #endif
#endif
    inout float4 lllll5, int lllllllllll5, float llllllllllll5, half lllllllllllll5, half llllllllllllll5,
    float2 lllllllllllllllllllllllllllllll0, float4 lllllllllllllllllllllll0, sampler2D lllllllllllllllll5,
    half llllllllllllllllll5, half lllllllllllllllllll5, 
    half llllllllllllllllllll5, half lllllllllllllllllllll5,
    sampler2D llllllllllllllllllllll5, float4 lllllllllllllllllllllll5, half llllllllllllllllllllllll5, half lllllllllllllllllllllllll5, float llllllllllllllllllllllllll5,
    half lllllllllllllllllllllllllll5, float4 llllllllllllllllllllllllllll5, float lllllllllllllllllllllllllllll5, float llllllllllllllllllllllllllllll5, float4 lllllllllllllllllllllllllllllll5,
    float l6, float ll6, float lll6, half llll6, float4 lllll6,
    half llllll6,
    half lllllll6, half llllllll6, float4 lllllllll6, float llllllllll6, float lllllllllll6, float llllllllllll6, half lllllllllllll6, half llllllllllllll6,
    half lllllllllllllll6, half llllllllllllllll6, float4 lllllllllllllllll6, float llllllllllllllllll6, float lllllllllllllllllll6, float llllllllllllllllllll6, half lllllllllllllllllllll6, half llllllllllllllllllllll6,
    half lllllllllllllllllllllll6, 
    GeneralStylingData generalStylingData, half llllllllllllllllllllllll6, half lllllllllllllllllllllllllllll3,
    half llllllllllllllllllllllllll6,
    half lllllllllllllllllllllllllll6,
    float llllllllllllllllllllllllllll6, float lllllllllllllllllllllllllllll6, float llllllllllllllllllllllllllllll6, 
    PositionAndBlendingData positionAndBlendingDataShading, UVSpaceData uvSpaceDataShading, StylingData stylingDataShading, StylingRandomData stylingRandomDataShading,
    half lllllllllllllllllllllllllllllll6, 
    half l7,
    half ll7, float lll7,
    PositionAndBlendingData positionAndBlendingDataCastShadows, UVSpaceData uvSpaceDataCastShadows, StylingData stylingDataCastShadows, StylingRandomData stylingRandomDataCastShadows,
    half llll7,
    half lllll7, float llllll7, float lllllll7, half llllllll7, half lllllllll7,
    half llllllllll7,
    PositionAndBlendingData positionAndBlendingDataSpecular, UVSpaceData uvSpaceDataSpecular, StylingData stylingDataSpecular, StylingRandomData stylingRandomDataSpecular,
    half lllllllllll7, 
    half llllllllllll7, float lllllllllllll7, float llllllllllllll7, half lllllllllllllll7,
    half llllllllllllllll7,
    PositionAndBlendingData positionAndBlendingDataRim, UVSpaceData uvSpaceDataRim, StylingData stylingDataRim, StylingRandomData stylingRandomDataRim,
    sampler2D llllllllllllllll3, sampler2D lllllllllllllllll3, 
    float4 lllllllllllllllllll7,
    float3 llllllllllllllllllll7
)
{
    float lllllllllllllllllllll7 = 0;
    float4 llllllllllllllllllllll7 = lllll5;
    int lllllllllllllllllllllll7 = lllllllllll5;
#if _USE_OPTIMIZATION_DEFINES
#if _ENABLE_TOON_SHADING
    llllllllllllllllllll5 = 1;
#else
    llllllllllllllllllll5 = 0;
#endif
#if _SHADING_COLOR
    llllllllllllllllll5 = 0;
#else
    llllllllllllllllll5 = 1;
#endif  
#if _ENABLE_STYLING
    lllllllllllllllllllllll6 = 1;
#else
    lllllllllllllllllllllll6 = 0;
#endif
#if _ENABLE_SHADING_STYLING
    llllllllllllllllllllllllll6 = 1;
#else
    llllllllllllllllllllllllll6 = 0;
#endif
#if _ENABLE_CASTSHADOWS_STYLING
    lllllllllllllllllllllllllllllll6 = 1;
#else
    lllllllllllllllllllllllllllllll6 = 0;
#endif
#if _ENABLE_SPECULAR_STYLING
    llll7 = 1;
#else
    llll7 = 0;
#endif
#if _ENABLE_SPECULAR
    lllllll6 = 1;
#else
    lllllll6 = 0;
#endif
#if _SUM_LIGHTS_BEFORE_POSTERIZATION
    lllllllllllll5 = 1;
#else
    lllllllllllll5 = 0;
#endif
#if _SHADING_USE_LIGHT_COLORS
    llllllllllllll5 = 1;
#else
    llllllllllllll5 = 0;
#endif
#if _SPECULAR_USE_LIGHT_COLORS
    llllllllllllll6 = 1;
#else
    llllllllllllll6 = 0;
#endif
#if _STYLING_SPECULAR_USE_LIGHT_COLORS
    lllllllll7 = 1;
#else
    lllllllll7 = 0;
#endif  
#endif
    float3 llllllllllllllllllllllll7;
    if (llllll6 == 0)
    {
        llllllllllllllllllllllll7 = llllllllllllllllllll7;
    }
    else
    {
#if _URP 
        llllllllllllllllllllllll7 = inputData.normalWS;
#else
        llllllllllllllllllllllll7 = o.Normal;
#endif
    }
    float3 llllllllllllllllllllllll1;
    if (lllllllllllll6 == 0)
    {
        llllllllllllllllllllllll1 = llllllllllllllllllll7;
    }
    else
    {
#if _URP 
        llllllllllllllllllllllll1 = inputData.normalWS;
#else
        llllllllllllllllllllllll1 = o.Normal;
#endif
    }
    float3 llllllllllllllllllllllllll7;
    if (llllllllllllllllllllllll6 == 0)
    {
        llllllllllllllllllllllllll7 = llllllllllllllllllll7;
    }
    else
    {
#if _URP 
        llllllllllllllllllllllllll7 = inputData.normalWS;
#else
        llllllllllllllllllllllllll7 = o.Normal;
#endif
    }
    float3 lllllllllllllll1 = normalize(d.worldSpaceViewDir);
    float4 llllllllllllllllllllllllllll7 = 0;
    float llllllllllllllllll1 = -1;
    half3 llllllllllllllllllllllllllllll7 = 0;
    float lllllllllllllllllllllllllllllll7 = -1;
    float lllllllllll2 = 0; 
    float ll8 = 0; 
    float lllllllllllllllllll1 = 0;
    half3 llll8 = 0;
    float lllll8 = 0;
    half3 llllll8 = 0;
    ToonShadingData toonShadingData;
    toonShadingData.enableToonShading = llllllllllllllllllll5;
#if _URP
    toonShadingData.normalWS = inputData.normalWS;
#endif
    toonShadingData.normalWSNoMap = llllllllllllllllllll7;
    toonShadingData.cellTransitionSmoothness = llllllllllll5;
    toonShadingData.numberOfCells = lllllllllllllllllllllll7;
    toonShadingData.specularEdgeSmoothness = lllllllllll6;
    toonShadingData.shadingAffectByNormalMap = llllll6;
    toonShadingData.specularAffectedByNormalMap = lllllllllllll6;
#if _URP   
    if ((llllllllllllllllll5 == 0 && llllllllllllllllllll5 == 1 && (lllllllllllllllllllllllllll5 == 1 || lllllll6 == 1 || l6 == 1)) || (lllllllllllllllllllllll6 == 1 && (llllllllllllllllllllllllll6 == 1 || lllllllllllllllllllllllllllllll6 == 1 || llll7 == 1)))
    {
        bool lllllll8 = llllllllllllllllll5 == 0 && llllllllllllllllllll5 == 1;
        bool llllllll8 = lllllllllllllllllllllll6 == 1 && (llllllllllllllllllllllllll6 == 1 || lllllllllllllllllllllllllllllll6 == 1 || llll7 == 1);
        bool lllllllll8 = llllll6 == llllllllllllllllllllllll6;
        bool llllllllll8 = lllllllllllll6 == llllllllllllllllllllllll6;
        float lllllllllll8 = 1;
        float llllllllllll8 = 1;
        Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
        float lllllllllllll8 = max(mainLight.color.x, mainLight.color.y);
        lllllllllllll8 = max(lllllllllllll8, mainLight.color.z);
        float3 llllllllllllll8 = llllllllllllllllllllllll7;
        float llllllllllllllll1 = llllllllll6;
        float lllllllllllllllll1 = lllllllllll6;
        float lllllllllllllllll8 = llllllllllll6;
        float llllllllllllllllll8 = llllllllllllll6;
        half lllllllllllllllllll8 = lllllll6;
        half llllllllllllllllllll8 = lllllllllllllllllllllllllll5;
        if (!lllllll8)
        {
            llllllllllllll8 = llllllllllllllllllllllllll7;
            llllllllllllllllllllllll1 = llllllllllllllllllllllllll7;            
            llllllllllllllll1 = llllll7;
            lllllllllllllllll1 = lllllll7;
            lllllllllllllllll8 = _StylingSpecularOpacity;
            llllllllllllllllll8 = lllllllll7;
            lllllllllllllllllll8 = llll7;
            llllllllllllllllllll8 = llllllllllllllllllllllllll6;
        } 
        else 
        {
            if(lllllllllllllllllllllllllll5 == 0) 
            {
                llllllllllllll8 = llllllllllllllllllllllllll7;
                llllllllllllllllllll8 = llllllllllllllllllllllllll6;
            }
            if(lllllll6 == 0) 
            {
                llllllllllllllllllllllll1 = llllllllllllllllllllllllll7;           
                lllllllllllllllllll8 = llll7;
            }
            else 
            {
                if(llllllll8 && llll7 == 1 && lllll7 == 1) 
                {
                    llllll7 = llllllllll6;
                    lllllll7 = lllllllllll6;
                }
            }
        }
        float lllllllllllllllllllll8 = 1;
        if (mainLight.color.r > 0.0 || mainLight.color.g > 0.0 || mainLight.color.b > 0.0)
        {
            lllllllllllllllllllll8 = (mainLight.shadowAttenuation * mainLight.distanceAttenuation);
            float llllllllllllllllllllll8 = dot(mainLight.direction, llllllllllllll8);
            if (llllllllllllllllllllll8 > 0)
            {
                llllllllllllllllll1 = llllllllllllllllllllll8 * mainLight.distanceAttenuation * lllllllllllll8; 
            }
            else
            {
                llllllllllllllllll1 = llllllllllllllllllllll8;
            }
            if (lllllllllllllllllll8)
            {
                lllllllllllllllllll1 = CalculateSpecularMask(llllllllllllllllllllllll1, mainLight.direction, lllllllllllllll1, llllllllllllllll1, lllllllllllllllll1, llllllllllllllllllllll8);
                lllllllllllllllllll1 *= lllllllllllllllll8;
                if( (lllllll8 && l6) || (lllllllllllllllllllllll6 && lllllllllllllllllllllllllllllll6))
                {
                    lllllllllllllllllll1 = min(lllllllllllllllllll1, mainLight.shadowAttenuation);
                }
                if (llllllllllllllllll8 == 1)
                {
                    llll8 = lllllllllllllllllll1 * mainLight.color;
                }
            }
            if (!lllllll8)
            {
                lllllllllllllllllllllllllllllll7 = llllllllllllllllll1;
                lllll8 = lllllllllllllllllll1;
                llllll8 = llll8;
                lllllllllllllllllll1 = 0;
                llll8 = 0;
            } 
            else
            {
                if(lllllllllllllllllllllllllll5 == 0) 
                {
                    lllllllllllllllllllllllllllllll7 = llllllllllllllllll1;
                }
                if(lllllll6 == 0) 
                {
                    lllll8 = lllllllllllllllllll1;
                    llllll8 = llll8;
                    lllllllllllllllllll1 = 0;
                    llll8 = 0;
                }
            }
            if (llllllll8 && lllllll8) 
            {
                float lllllllllllllllllllllll8 = 0;
                if (lllllllll8)
                {
                    lllllllllllllllllllllllllllllll7 = llllllllllllllllll1;
                    lllllllllllllllllllllll8 = llllllllllllllllllllll8;
                }
                else
                {
                    lllllllllllllllllllllll8 = dot(mainLight.direction, llllllllllllllllllllllllll7);
                    if (lllllllllllllllllllllll8 > 0)
                    {
                        lllllllllllllllllllllllllllllll7 = lllllllllllllllllllllll8 * mainLight.distanceAttenuation * lllllllllllll8; 
                    }
                    else
                    {
                        lllllllllllllllllllllllllllllll7 = lllllllllllllllllllllll8;
                    }
                }
                if (llll7 == 1)
                {
                    if (lllllllll8 && llllllllll8 && lllll7 == 1)
                    {
                        lllll8 = lllllllllllllllllll1;
                    }
                    else
                    {
                        lllll8 = CalculateSpecularMask(llllllllllllllllllllllllll7, mainLight.direction, lllllllllllllll1, llllll7, lllllll7, lllllllllllllllllllllll8);
                        if(l6 || lllllllllllllllllllllllllllllll6)
                        {
                            lllll8 = min(lllll8, mainLight.shadowAttenuation);
                        }
                        if (lllllllll7 == 1)
                        {
                            llllll8 = lllll8 * mainLight.color;
                        }
                    }
                    if (llll7 == 1 && lllllllll7 == 1)
                    {
                        llllll8 = lllll8 * mainLight.color; 
                    }
                }
            }
            if (llllllllllllllllllllll8 > 0 )
            {
                lllllllllll8 = lllllllllllllllllllll8;
            }
        }
        else
        {
            lllllllllll8 = 1;
            lllllllllllllllllllll8 = 1;
            llllllllllllllllll1 = -1;
            lllllllllllllllllllllllllllllll7 = -1;
        }
        float llllllllllllllllllllllll8 = 0;
        float lllllllllllllllllllllllll8 = 0;
        float llllllllllllllllllllllllll8 = 0;
        float lllllllllllllllllllllllllll8 = 2;
        float llllllllllllllllllllllllllll8 = 0;
        float lllllllllllllllllllllllllllll8 = 1;
#if defined(_ADDITIONAL_LIGHTS)  
    #if UNITY_VERSION >= 202200
        uint meshRenderingLayers = GetMeshRenderingLayer();
    #else
        uint meshRenderingLayers = GetMeshRenderingLightLayer();
    #endif
#if USE_CLUSTER_LIGHT_LOOP
        [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
        {
            Light addLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));       
    #ifdef _LIGHT_LAYERS
            if (IsMatchingLightLayer(addLight.layerMask, meshRenderingLayers))
    #endif
            {
                float llllllllllllllllllllllllllllll8 = max(addLight.color.x, addLight.color.y);
                llllllllllllllllllllllllllllll8 = max(llllllllllllllllllllllllllllll8, addLight.color.z);
                half lllllllllllllllllllllllllllllll8 = addLight.distanceAttenuation;
                    lllllllllllllllllllllllllllllll8 *= llllllllllllllllllllllllllllll8;
                float l9 = smoothstep(0, 0.1, addLight.distanceAttenuation);
                float ll9 = smoothstep(0, 0.01, addLight.distanceAttenuation);            
                llllllllllllllllllllllllllll8 += addLight.shadowAttenuation * lllllllllllllllllllllllllllllll8;
                float lll9 = dot(addLight.direction, llllllllllllll8);   
                float llll9 = lerp(-1, lll9, l9);
                if(lll9>0) 
                {
                    lllllllllllllllllllllllllllll8 = min(lllllllllllllllllllllllllllll8, lerp(1, addLight.shadowAttenuation, ll9));
                }
                float lllll9 = saturate(llll9) * lllllllllllllllllllllllllllllll8;
                lllllllllllllllllllllllll8 += lllll9;
                if (lllllll8)
                {
                    if (l6 == 1)
                    {
                        lllll9 *= addLight.shadowAttenuation;
                    }
                    llllllllllllllllllllllll8 += lllll9;
                    if (sign(llll9) == -1 && lllllllllllllllllllllllll8 == 0)
                    {
                        float llllll9 = abs(llll9);
                        lllllllllllllllllllllllllll8 = min(lllllllllllllllllllllllllll8, llllll9);
                    }
                    if (llllllllllllll5 == 1)
                    {
                        llllllllllllllllllllllllllllll7 += saturate(lllll9 * (addLight.color));
                    }
                }
                float lllllll9 = 0;
                if (lllllll6)
                {
                    lllllll9 = CalculateSpecularMask(llllllllllllllllllllllll1, addLight.direction, lllllllllllllll1, llllllllllllllll1, lllllllllllllllll1, lll9);
                    lllllll9 = lllllll9;
                    if(l6 || lllllllllllllllllllllllllllllll6)
                    {
                        lllllll9 *= addLight.shadowAttenuation;
                    }
                    lllllllllllllllllll1 += lllllll9;
                    if (llllllllllllllllll8 == 1)
                    {
                        llll8 += addLight.color * lllllll9;
                    }
                }
                if (llllllll8 && lllllll8) 
                {
                    float llllllll9 = 0;
                    if (lllllllll8)
                    {
                        llllllllllllllllllllllllll8 = lllllllllllllllllllllllll8;
                    }
                    else
                    {
                        llllllll9 = dot(addLight.direction, llllllllllllllllllllllllll7);
                        float lllllllll9 = lerp(-1, llllllll9, l9);
                        llllllllllllllllllllllllll8 += saturate(lllllllll9) * lllllllllllllllllllllllllllllll8;
                    }
                    if (llll7 == 1)
                    {
                        float llllllllll9 = 0;
                        if (lllllllll8 && llllllllll8 && lllll7 == 1)
                        {
                            lllll8 = lllllllllllllllllll1;
                            llllllllll9 = lllllll9;
                        }
                        else
                        {
                            llllllllll9 = CalculateSpecularMask(llllllllllllllllllllllll1, addLight.direction, lllllllllllllll1, llllll7, lllllll7, lll9);
                            llllllllll9 = llllllllll9;
                            if(l6 || lllllllllllllllllllllllllllllll6)
                            {
                                llllllllll9 *= addLight.shadowAttenuation;
                            }
                            lllll8 += llllllllll9;
                        }
                        if (lllllllll7 == 1)
                        {
                            llllll8 += addLight.color * llllllllll9;
                        }
                    }
                }
            }
        }
#endif
        uint pixelLightCount = GetAdditionalLightsCount();
        LIGHT_LOOP_BEGIN(pixelLightCount)
        Light addLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1, 1, 1, 1));
#ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(addLight.layerMask, meshRenderingLayers))
#endif
        {  
            float llllllllllllllllllllllllllllll8 = max(addLight.color.x, addLight.color.y);
            llllllllllllllllllllllllllllll8 = max(llllllllllllllllllllllllllllll8, addLight.color.z);
            half lllllllllllllllllllllllllllllll8 = addLight.distanceAttenuation;
                lllllllllllllllllllllllllllllll8 *= llllllllllllllllllllllllllllll8;
            float l9 = smoothstep(0, 0.1, addLight.distanceAttenuation);
            float ll9 = smoothstep(0, 0.01, addLight.distanceAttenuation);            
            llllllllllllllllllllllllllll8 += addLight.shadowAttenuation * lllllllllllllllllllllllllllllll8;
            float lll9 = dot(addLight.direction, llllllllllllll8);   
            float llll9 = lerp(-1, lll9, l9);
            if(lll9>0) 
            {
                lllllllllllllllllllllllllllll8 = min(lllllllllllllllllllllllllllll8, lerp(1, addLight.shadowAttenuation, ll9));
            }
            float lllll9 = saturate(llll9) * lllllllllllllllllllllllllllllll8;
            lllllllllllllllllllllllll8 += lllll9;
            if (lllllll8)
            {
                if (l6 == 1)
                {
                    lllll9 *= addLight.shadowAttenuation;
                }
                llllllllllllllllllllllll8 += lllll9;
                if (sign(llll9) == -1 && lllllllllllllllllllllllll8 == 0)
                {
                    float llllll9 = abs(llll9);
                    lllllllllllllllllllllllllll8 = min(lllllllllllllllllllllllllll8, llllll9);
                }
                if (llllllllllllll5 == 1)
                {
                    llllllllllllllllllllllllllllll7 += saturate(lllll9 * (addLight.color));
                }
            }
            float lllllll9 = 0;
            if (lllllll6)
            {
                lllllll9 = CalculateSpecularMask(llllllllllllllllllllllll1, addLight.direction, lllllllllllllll1, llllllllllllllll1, lllllllllllllllll1, lll9);
                lllllll9 = lllllll9;
                if(l6 || lllllllllllllllllllllllllllllll6)
                {
                    lllllll9 *= addLight.shadowAttenuation;
                }
                lllllllllllllllllll1 += lllllll9;
                if (llllllllllllllllll8 == 1)
                {
                    llll8 += addLight.color * lllllll9;
                }
            }
            if (llllllll8 && lllllll8) 
            {
                float llllllll9 = 0;
                if (lllllllll8)
                {
                    llllllllllllllllllllllllll8 = lllllllllllllllllllllllll8;
                }
                else
                {
                    llllllll9 = dot(addLight.direction, llllllllllllllllllllllllll7);
                    float lllllllll9 = lerp(-1, llllllll9, l9);
                    llllllllllllllllllllllllll8 += saturate(lllllllll9) * lllllllllllllllllllllllllllllll8;
                }
                if (llll7 == 1)
                {
                    float llllllllll9 = 0;
                    if (lllllllll8 && llllllllll8 && lllll7 == 1)
                    {
                        lllll8 = lllllllllllllllllll1;
                        llllllllll9 = lllllll9;
                    }
                    else
                    {
                        llllllllll9 = CalculateSpecularMask(llllllllllllllllllllllll1, addLight.direction, lllllllllllllll1, llllll7, lllllll7, lll9);
                        llllllllll9 = llllllllll9;
                        if(l6 || lllllllllllllllllllllllllllllll6)
                        {
                            llllllllll9 *= addLight.shadowAttenuation;
                        }
                        lllll8 += llllllllll9;
                    }
                    if (lllllllll7 == 1)
                    {
                        llllll8 += addLight.color * llllllllll9;
                    }
                }
            }
        }
        LIGHT_LOOP_END
#endif
        if (llllllllllllllllllll5 == 1 && lllllllllllllllllllllllllll5 == 1 && llllllllllllll5 == 1)
        {
            float3 lllllllllllllllllllllll9 = saturate(saturate(llllllllllllllllll1) * (mainLight.color));
            if(l6 == 1)
            {
                lllllllllllllllllllllll9 *= lllllllllllllllllllll8;
            }
            llllllllllllllllllllllllllllll7 += saturate(lllllllllllllllllllllll9);
            llllllllllllllllllllllllllllll7 = Posterize(saturate(llllllllllllllllllllllllllllll7), toonShadingData);
        }
        if (!lllllll8)
        {
            llllllllllllllllllllllllll8 = lllllllllllllllllllllllll8;
            lllll8 = lllllllllllllllllll1;
            llllll8 = llll8;
            lllllllllllllllllll1 = 0;
            llll8 = 0;
        }
        float llllllllllllllllllllllll9 = saturate(llllllllllllllllll1);
        float lllllllllllllllllllllllll9 = saturate(llllllllllllllllllllllll8);
        if (lllllllllllllllllllll5 == 0)
        {
            if (lllllllllllll5 == 0)
            {
                llllllllllllllllllllllll9 = Posterize(llllllllllllllllllllllll9, toonShadingData);
                lllllllllllllllllllllllll9 = Posterize(lllllllllllllllllllllllll9, toonShadingData);
            }
        }
        if (llllllllllllllllllll5 == 1 && l6 == 1 && (lllllllllllllllllllllllllll5 == 0 || (lllllllllllllll6 && lllllllllllllllllllll6==1) ) )
        {
            float llllllllllllllllllllllllll9 = min(lllllllllll8, lllllllllllllllllllllllllllll8);
            float lllllllllllllllllllllllllll9 = lllllllllllllllllllll8 * saturate(llllllllllllllllll1) + saturate(lllllllllllllllllllllllll8) * llllllllllllllllllllllllllll8;
            float llllllllllllllllllllllllllll9 = ((1 - llllllllllllllllllllllllll9) * (lllllllllllllllllllllllllll9)) + llllllllllllllllllllllllll9; 
            lllllllllll2 = llllllllllllllllllllllllllll9;
        }
        if (lllllllllllllllllllllll6 == 1)
        {
            if (lllllllllllllllllllllllllllllll6 == 1)
            {
                float llllllllllllllllllllllllll9 = min(lllllllllll8, lllllllllllllllllllllllllllll8);
                float lllllllllllllllllllllllllll9 = lllllllllllllllllllll8 * saturate(lllllllllllllllllllllllllllllll7) + saturate(llllllllllllllllllllllllll8) * llllllllllllllllllllllllllll8;
                float llllllllllllllllllllllllllll9 = ((1 - llllllllllllllllllllllllll9) * (lllllllllllllllllllllllllll9)) + llllllllllllllllllllllllll9; 
                ll8 = llllllllllllllllllllllllllll9;
            }
            lllllllllllllllllllllllllllllll7 = saturate(lllllllllllllllllllllllllllllll7) + saturate(llllllllllllllllllllllllll8);
        }
        if (llllllllllllllllll1 > 0)
        {
            llllllllllllllllll1 = saturate(llllllllllllllllllllllll9);
            if(l6 == 1)
            {
                llllllllllllllllll1 *= lllllllllllllllllllll8;
            }
        }
        if (lllllllllllllllllllllllll8 > 0)
        {
            llllllllllllllllll1 = saturate(llllllllllllllllll1);
            llllllllllllllllll1 += saturate(lllllllllllllllllllllllll9);
        }
        else
        {
            if (lllllllllllllllllllllllllll8 > 0)
            {
                llllllllllllllllll1 = max(llllllllllllllllll1, -1 * lllllllllllllllllllllllllll8);
            }
        }
        if (llllllllllllllllll1 < 0)
        {
        }
        else
        {
            if (lllllllllllllllllllll5 == 0 && lllllllllllll5 == 1)
            {
                llllllllllllllllll1 = Posterize(saturate(llllllllllllllllll1), toonShadingData);
            }
        }
    }
#else 
    UnityLight light = gi.light;
    llllllllllllllllll1 = dot(light.dir, llllllllllllllllllllllll7);
    lllllllllllllllllllllllllllllll7 = dot(light.dir, llllllllllllllllllllllllll7);
#if !_PASSFORWARDADD    
    if (llllllllllllllllll1 > 0)
    {
        lllllllllll2 = giInput.atten;
    }
    else
    {
        lllllllllll2 = 1;
    }
    ll8 = lllllllllll2;
#else    
    lllllllllll2 = 0;    
    lllllllllllllll6 = 0;    
    lllllllllllllllllllllll6 = 0;    
    lllllllllll7 = 0;
    llllllllllllllllllllllllll6 = 0;
    lllllllllllllllllllllllllllllll6 = 0;
    stylingDataShading.color = 0;
    stylingDataSpecular.color = half4(gi.light.color,1);
#endif
#endif
    float l10 = lllllllllll2;
    float ll10 = 0;
    float4 lll10 = 0;
    float3 lll2;
    if (llllllllllllllllllllll6 == 0)
    {
        lll2 = llllllllllllllllllll7;
    }
    else
    {
#if _URP 
        lll2 = inputData.normalWS;
#else
        lll2 = o.Normal;
#endif
    }
    float llllllllllllllllllllllllll9 = 0;      
    if (llllllllllllllllll5 == 0) 
    {
        l10 = lllllllllll2;
        if (llllllllllllllllllll5 == 1)
        {
            if (lllllllllllllllllllll5 == 0)
            {
                if (lllllllllllllllllllllllllll5 == 1)
                {
                    float ll10 = saturate(llllllllllllllllll1);
                    #if _URP
                        if (llllllllllllll5 == 1)
                        {
                            lllll5 *= float4(llllllllllllllllllllllllllllll7, 1);
                        }
                    #else
                        ll10 = Posterize(ll10, toonShadingData);
                    #endif
                    lllll5 = lerp(llllllllllllllllllllllllllll5, lllll5, ll10);
                }
            }
            else
            {
                float lllllll10 = min(0.95, llllllllllllllllll1); 
                if (llllllllllllllllllllllll5 == 1 && lllllllllllllllllllllllllll5 == 0 && llllllllllllllllll1 < 0)
                {
                    lllllll10 = 0;
                }
                lllllll10 = (lllllll10 + 1) / 2;
                float4 llllllll10 = float4(0, 0, 0, 0);
                float lllllllll10 = lllllllllllllllllllllll5.z;
                float llllllllll10 = lllllll10 * (lllllllll10 - 1);
                float2 lllllllllll10 = (llllllllll10 + 0.5) * lllllllllllllllllllllll5.xy;
                llllllll10 = tex2D(llllllllllllllllllllll5, lllllllllll10);
                DoBlending(lllll5, llllllllllllllllllllllllll5, lllllllllllllllllllllllll5, llllllll10);
            }
            if (l6 == 0 && (lllllllllllllllllllllll6 == 0 || lllllllllllllllllllllllllllllll6 == 0))
            {
                lllllllllll2 = 1;
            }
            if (lllllllllllllllllllllllllll5 == 1 && lllllllllllllllllllll5 == 0)
            {
                if (llllllllllllllllll1 < 0.0)
                {
                    lllll5 = lllllllllllllllllllllllllllllll5;
                    llllllllllllllllllllllllllllll5 = 1 - llllllllllllllllllllllllllllll5;
                    float llllllllllll10 = llllllllllllllllllllllllllllll5 * lllllllllllllllllllllllllllll5;
                    float lllllllllllll10 = smoothstep(-llllllllllll10 + 0.01, -lllllllllllllllllllllllllllll5, llllllllllllllllll1);
                    lllll5 = lerp(llllllllllllllllllllllllllll5, lllllllllllllllllllllllllllllll5, lllllllllllll10);
                }
            }
            if (lllllllllllllllllllllllllll5 == 0 && lllllllllllllllllllll5 == 0 && l6 == 1)
            {
                lllll5 = lerp(llllllllllllllllllllllllllll5, lllll5, saturate(lllllllllll2));
            }
        }
#if _ENABLE_SPECULAR || !_USE_OPTIMIZATION_DEFINES
        if (lllllll6 == 1)
        {
#if _URP
#else
            lllllllllllllllllll1 = CalculateSpecularMask(llllllllllllllllllllllll1, light.dir, lllllllllllllll1, llllllllll6, lllllllllll6, llllllllllllllllll1);
            lllllllllllllllllll1 *= llllllllllll6;
            if (l6 == 1)
            {
                lllllllllllllllllll1 *= lllllllllll2;
            }
#endif
#if _USE_OPTIMIZATION_DEFINES
#ifdef _SPECULAR_BLENDING
                    llllllll6 = _SPECULAR_BLENDING;
#endif
#endif
            half4 llllllllllllll10;
        #if _URP
            if (llllllllllllll6 == 1)
            {
                llllllllllllll10 = half4(llll8, 1);
            }
            else
        #endif
            {
                llllllllllllll10 = lllllllll6;
            }
            DoBlending(lllll5, lllllllllllllllllll1, llllllll6, llllllllllllll10);
        }
#endif
#if _URP
        lllll5 += half4(surface.emission, 0);
#else
    lllll5 += half4(o.Emission, 0);
#endif
    }
    else 
    {
        ToonShadingData toonShadingData;
        toonShadingData.enableToonShading = llllllllllllllllllll5;
#if _URP
        toonShadingData.normalWS = inputData.normalWS;
#endif
        toonShadingData.normalWSNoMap = llllllllllllllllllll7;
        toonShadingData.cellTransitionSmoothness = llllllllllll5;
        toonShadingData.numberOfCells = lllllllllllllllllllllll7;
        toonShadingData.specularEdgeSmoothness = lllllllllll6;
        toonShadingData.shadingAffectByNormalMap = llllll6;
        toonShadingData.specularAffectedByNormalMap = lllllllllllll6;
#if _USE_OPTIMIZATION_DEFINES
#if _ENABLE_TOON_SHADING 
                toonShadingData.enableToonShading = 1;
#else
                toonShadingData.enableToonShading = 0;
#endif
#endif
#if _SHADING_BLINNPHONG       
        if (lllllllllllllllllll5 == 0) 
        {
#if _URP
        #if UNITY_VERSION >= 202120
            lllll5 = UniversalFragmentBlinnPhong(inputData, surface.albedo, half4(surface.specular, surface.smoothness), surface.smoothness, surface.emission, surface.alpha,lllllllll5, toonShadingData);
        #else
            lllll5 = UniversalFragmentBlinnPhong(inputData, surface.albedo, half4(surface.specular, surface.smoothness), surface.smoothness, surface.emission, surface.alpha, toonShadingData);
        #endif
#else
#endif
        }
#endif        
#if _SHADING_PBR
        if (lllllllllllllllllll5 == 1) 
        {      
#if _URP
            lllll5 = UniversalFragmentPBR(inputData, surface, toonShadingData);
#else
#if !_PASSFORWARDADD
    #if _USESPECULAR || _USESPECULARWORKFLOW || _SPECULARFROMMETALLIC
    #else
        LightingStandard_GI_Toon(o, giInput, gi, toonShadingData);
        #if defined(_OVERRIDE_BAKEDGI)
            gi.indirect.diffuse = l.DiffuseGI;
            gi.indirect.specular = l.SpecularGI;
        #endif
        lllll5 = LightingStandard_Toon (o, d.worldSpaceViewDir, gi, toonShadingData);
    #endif     
#else
    #if _USESPECULAR
#elif _BDRF3 || _SIMPLELIT
#else
                  lllll5 = LightingStandard_Toon (o, d.worldSpaceViewDir, gi, toonShadingData);
#endif
#endif
#endif
        }
#endif
    }
    float llllllllllll2 = 0;
    if (llllllllllllllllllll5 == 1)
    {
    #if _URP
        Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
        float llllllllllllllll10 = dot(mainLight.direction, lll2);
        float lllllllllllllllll10 = mainLight.shadowAttenuation;
    #else
        float llllllllllllllllll1 = dot(light.dir, lll2);
    #endif
        #if _ENABLE_RIM|| !_USE_OPTIMIZATION_DEFINES
        #if !_USE_OPTIMIZATION_DEFINES
            if (lllllllllllllll6 == 1)
        #endif
            {
        #if _URP
            llllllllllll2 = CalculateRimMask(lll2, lllllllllllllll1, llllllllllllllllll6, lllllllllllllllllll6, llllllllllllllll10, lllllllllllllllllllll6, l6, lllllllllllllllllllllllllll5, lllllllllllllllll10);
#else
            llllllllllll2 = CalculateRimMask(lll2, lllllllllllllll1, llllllllllllllllll6, lllllllllllllllllll6, llllllllllllllllll1, lllllllllllllllllllll6, l6, lllllllllllllllllllllllllll5, lllllllllll2);
#endif
                llllllllllll2 *= llllllllllllllllllll6;
        #if _USE_OPTIMIZATION_DEFINES
        #ifdef _RIM_BLENDING
                        llllllllllllllll6 = _RIM_BLENDING;
        #endif
        #endif
                    DoBlending(lllll5, llllllllllll2, llllllllllllllll6, lllllllllllllllll6);
                }
        #endif
    }
#if _ENABLE_STYLING || !_USE_OPTIMIZATION_DEFINES   
    #if !_USE_OPTIMIZATION_DEFINES
    if (lllllllllllllllllllllll6 == 1)
    #endif
    {
#if !_URP
        if (llll7 == 1)
        {
            if (lllllll6 == 0 || lllll7 == 0) 
            {
                float lllllllllllllllllll10 = saturate(llllllllllllllllll1);
                lllll8 = CalculateSpecularMask(llllllllllllllllllllllllll7, light.dir, lllllllllllllll1, llllll7, lllllll7, lllllllllllllllllll10);
                lllll8 = saturate(lllll8);
                lllll8 *= l10;
            }
            else
            {
                lllll8 = saturate(lllllllllllllllllll1);
            }
        }
#endif
        if (llllllll7 == 1)
        {
            lllllllllllllllllllllllllllllll7 = 1 - lllllllllllllllllllllllllllllll7 - lllll8 * 10;
            lllllllllllllllllllllllllllllll7 = 1 - lllllllllllllllllllllllllllllll7;
            l10 = 1 - ((1 - l10) - lllll8 * 10);
        }
        #if _USE_OPTIMIZATION_DEFINES
            #ifdef _SHADING_STYLING_DRAWSPACE
        uvSpaceDataShading.drawSpace = _SHADING_STYLING_DRAWSPACE;
            #endif
            #ifdef _SHADING_STYLING_COORDINATESYSTEM
        uvSpaceDataShading.coordinateSystem = _SHADING_STYLING_COORDINATESYSTEM;
            #endif
        #endif
    #if _URP
        float2 llllllllllllllllllll10 = ConvertToDrawSpace(inputData, lllllllllllllllllllllllllllllll0, uvSpaceDataShading, lllllllllllllllllllllll0);
    #else
        float2 llllllllllllllllllll10 = ConvertToDrawSpace(d.worldSpacePosition, lllllllllllllllllllllllllllllll0, uvSpaceDataShading, lllllllllllllllllllllll0);
    #endif
        float llllllllllllllllllllll10 = stylingDataShading.density;
        float lllllllllllllllllllllllllllllll3 = stylingDataShading.size;
        half4 llllllllllllllllllllllll10 = tex2D(llllllllllllllll3, lllllllllllllllllllllllllllllll0.xy); 
        float lllllllllllllllllllllllll10 = 1;
#if _ENABLE_SHADING_STYLING || !_USE_OPTIMIZATION_DEFINES   
    #if !_USE_OPTIMIZATION_DEFINES
        if (llllllllllllllllllllllllll6 != 0)
    #endif        
        {
            float llllllllllllllllllllllllll10 = 0;            
        #if _USE_OPTIMIZATION_DEFINES
            #ifdef _SHADING_STYLING_BLENDING
                    positionAndBlendingDataShading.blending = _SHADING_STYLING_BLENDING;
            #endif                   
            #ifdef _SHADING_STYLE
                stylingDataShading.style = _SHADING_STYLE;
            #endif
            #if _SHADING_STYLING_RANDOMIZER
                stylingRandomDataShading.enableRandomizer = 1;
            #else
                stylingRandomDataShading.enableRandomizer = 0;
            #endif
        #endif
            RequiredNoiseData requiredNoiseDataShading;
    #if _USE_OPTIMIZATION_DEFINES
        #ifdef _SHADING_STYLING_RANDOMIZER_PERLIN
            requiredNoiseDataShading.perlinNoise = 1;
        #else
            requiredNoiseDataShading.perlinNoise = 0;
        #endif
        #ifdef _SHADING_STYLING_RANDOMIZER_PERLIN_FLOORED
            requiredNoiseDataShading.perlinNoiseFloored = 1;
        #else
            requiredNoiseDataShading.perlinNoiseFloored = 0;
        #endif         
        #ifdef _SHADING_STYLING_RANDOMIZER_WHITE
            requiredNoiseDataShading.whiteNoise = 1;
        #else
            requiredNoiseDataShading.whiteNoise = 0;
        #endif
        #ifdef _SHADING_STYLING_RANDOMIZER_WHITE_FLOORED
            requiredNoiseDataShading.whiteNoiseFloored = 1;
        #else
            requiredNoiseDataShading.whiteNoiseFloored = 0;
        #endif            
    #else            
            requiredNoiseDataShading.perlinNoise = 1;
            requiredNoiseDataShading.perlinNoiseFloored = 1;
            requiredNoiseDataShading.whiteNoise = 1;
            requiredNoiseDataShading.whiteNoiseFloored = 1;
    #endif
            float lllllllllllllllllllllllllll10 = saturate(lllllllllllllllllllllllllllllll7);
            if (positionAndBlendingDataShading.isInverted == 1)
            {
                lllllllllllllllllllllllllll10 = 1 - lllllllllllllllllllllllllll10;
            }
            if (stylingDataShading.style == 0) 
            {                             
                float llllllllllllllllllllll10 = stylingDataShading.density;
                float lllllllllllllllllllllllllllllll3 = stylingDataShading.size;
                lllllllllllllllllllllllllllllll3 = stylingDataShading.size / 2;
                if (llllllllllllllllllllllllllll6 == 0)
                {
                    lllllllllllllllllllllll7 = lllllllllllllllllllllllllllll6;
                }
                else
                {
                    lllllllllllllllllllllll7 = lllllllllll5;
                }
            #if _USE_OPTIMIZATION_DEFINES            
                #ifdef _SHADING_STYLING_NUMBER_OF_CELLS_HATCHING
                        lllllllllllllllllllllll7 = _SHADING_STYLING_NUMBER_OF_CELLS_HATCHING;
                #endif                            
            #endif
                float llllllllllllllllllllllllllllll10 = (1. / lllllllllllllllllllllll7) * llllllllllllllllllllllllllllll6;
                int lllllllllllllllllllllllllllllll10 = ceil((max(lllllllllllllllllllllllllll10 - llllllllllllllllllllllllllllll10, 0)) * lllllllllllllllllllllll7);
                lllllllllllllllllllllllllllllll10 = lllllllllllllllllllllll7 - lllllllllllllllllllllllllllllll10;
                float l11 = stylingDataShading.rotation;
                float ll11 = radians(l11);
                float lll11 = stylingDataShading.rotationBetweenCells;
                float llll11 = radians(lll11);
                float2 lllll11; 
                NoiseSampleData noiseSampleData; 
                lllllllllllllllllllllllll10 = 1;
                float lllllll1 = 0;
    #if _USE_OPTIMIZATION_DEFINES            
        #ifdef _SHADING_STYLING_NUMBER_OF_CELLS_HATCHING
                lllllllllllllllllllllllllllllll10 = _SHADING_STYLING_NUMBER_OF_CELLS_HATCHING;
        #endif
                [unroll(lllllllllllllllllllllllllllllll10)]
    #else
                [unroll(15)]
    #endif
                for (int i = 1; i <= lllllllllllllllllllllllllllllll10; i++)
                { 
                    lllllllllllllllllllllllllllllll3 = stylingDataShading.size / 2;
                        float lllllll11 = i - 1;
                        float llllllllll3 = ll11 + llll11 * lllllll11;
                        llllllllllllllllllll10 += lllllll1; 
                        lllll11 = RotateUVRadians(llllllllllllllllllll10, llllllllll3);
                        noiseSampleData = SampleNoiseData(lllll11, stylingDataShading, stylingRandomDataShading, requiredNoiseDataShading, llllllllllllllll3, lllllllllllllllll3);
                    lllllll1 += (float) stylingDataShading.density;
                    float lllllllll11 = lllll11.x;                          
                    lllllllll11 *= stylingDataShading.density;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        lllllllll11 += noiseSampleData.perlinNoise * stylingRandomDataShading.noiseIntensity;
                        float ll4 = 0;
                        if (stylingRandomDataShading.thicknessRandomMode == 0)
                        {
                            ll4 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.thicknessRandomMode == 1) 
                        {
                            ll4 = noiseSampleData.perlinNoiseFloored;
                        }
                        else 
                        {
                            ll4 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
                        }
                        float lll4 = remap(0, 1, 0.0, lllllllllllllllllllllllllllllll3, ll4 * stylingRandomDataShading.thicknesshRandomIntensity);
                        lllllllllllllllllllllllllllllll3 -= lll4;
                        float llll4 = 0;
                        if (stylingRandomDataShading.spacingRandomMode == 0)
                        {
                            llll4 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.spacingRandomMode == 1) 
                        {
                            llll4 = noiseSampleData.perlinNoiseFloored;
                        }
                        else 
                        {
                            llll4 = ((noiseSampleData.perlinNoiseFloored) + noiseSampleData.whiteNoise) / 2;
                        }
                        float lllll4 = remap(0, 1, -0.5 + lllllllllllllllllllllllllllllll3, 0.5 - lllllllllllllllllllllllllllllll3, llll4);
                        lllllllll11 += lllll4 * stylingRandomDataShading.spacingRandomIntensity * saturate(1 - stylingRandomDataShading.noiseIntensity); 
                    }
                    lllllllll11 = abs(frac(lllllllll11) - 0.5);
                    float llllllllllllll11 = (float) (lllllllllllllllllllllll7 - i) / lllllllllllllllllllllll7;
                    float lllllllllllllll11 = remap(0, 1, 0, llllllllllllllllllllllllllllll10, llllllllllllllllllllllllllllll6);
                    float llllll4;
                    float llllllll4;
                    float llllllllllllllllll11 = 0;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float lllllll4 = 0;
                        if (stylingRandomDataShading.lengthRandomMode == 0)
                        {
                            lllllll4 = noiseSampleData.whiteNoise * saturate(1 - stylingRandomDataShading.noiseIntensity);
                        }
                        else if (stylingRandomDataShading.lengthRandomMode == 1)
                        {
                            lllllll4 = noiseSampleData.perlinNoiseFloored; 
                        }
                        else
                        {
                            lllllll4 = ((noiseSampleData.perlinNoiseFloored + (noiseSampleData.whiteNoise * saturate(1 - stylingRandomDataShading.noiseIntensity))) / 2); 
                        }
                        llllllll4 = lllllll4 * stylingRandomDataShading.lengthRandomIntensity;
                        llllllllllllllllll11 = remap(0, 1, 0, llllllllllllll11 + lllllllllllllll11, llllllll4);           
                    }
                    llllll4 = remap(0, llllllllllllll11 + lllllllllllllll11 - llllllllllllllllll11, 0, 1, lllllllllllllllllllllllllll10);
                    if (i == lllllllllllllllllllllll7 && sign(lllllllllllllllllllllllllll10) == 1)
                    {
                        float llllllllllllllllll11 = 0;
                        if (stylingRandomDataShading.enableRandomizer == 1)
                        {
                            llllllllllllllllll11 = remap(0, 1, 0, 1 - llllllllllllllllllllllllllllll10, llllllll4);
                        }
                        llllll4 = remap(0, llllllllllllllllllllllllllllll10, 1 - llllllllllllllllllllllllllllll10 + llllllllllllllllll11, 1 + llllllllllllllllll11, lllllllllllllllllllllllllll10);
                    }
                    if (i == lllllllllllllllllllllll7 && sign(lllllllllllllllllllllllllll10) == -1)
                    {
                        float lllllllllllllllllllll11 = (float) 1. / lllllllllllllllllllllll7;
                        lllllllllllllll11 = remap(0, 1, 0, lllllllllllllllllllll11, llllllllllllllllllllllllllllll6);
                        float llllllllllllllllll11 = 0;
                        if (stylingRandomDataShading.enableRandomizer == 1)
                        {
                            llllllllllllllllll11 = remap(0, 1, 0, 1 - lllllllllllllll11, llllllll4);
                        }
                        llllll4 = remap(0, -1, 1 - lllllllllllllll11 + llllllllllllllllll11, 0, lllllllllllllllllllllllllll10);
                    }
                    float lllllllll4 = smoothstep(1-stylingDataShading.sizeFalloff, 1, llllll4);
                    if (l10 <= 0 && lllllllllllllllllllllllllll10 > 0)
                    {
                    }
                    lllllllll4 = max(lllllllllllllllllllllllllllllll3 - lllllllll4, 0);            
                    float llllllllll4;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float lllllllllll4 = 0;
                        if (stylingRandomDataShading.hardnessRandomMode == 0) 
                        {
                            lllllllllll4 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.hardnessRandomMode == 1) 
                        {
                            lllllllllll4 = noiseSampleData.perlinNoiseFloored * 5;
                        }
                        else
                        {
                            lllllllllll4 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
                        }
                        llllllllll4 = remap(0, 1, 0, lllllllll4, min(saturate(stylingDataShading.hardness - lllllllllll4 * stylingRandomDataShading.hardnessRandomIntensity), stylingDataShading.hardness));
                    }
                    else
                    {
                        llllllllll4 = remap(0, 1, 0, lllllllll4, stylingDataShading.hardness);
                    }
                    if (lllllllll4 != 0 )
                    {
                        float llllllllllll4 = 0; 
                        if (lllllllllllllllllllllllllllll3)
                        {
                            llllllllllll4 = fwidth(lllllllll11); 
                        }
                        if (lllllllll4 == lllllllllllllllllllllllllllllll3 && stylingDataShading.size == 1)
                        {
                            llllllllllll4 = 0;
                        }
                        if (llllllllll4 - llllllllllll4 < 0)
                        {
                            llllllllllll4 = 0;
                        }
                        lllllllll11 = smoothstep(llllllllll4 - llllllllllll4, lllllllll4 + llllllllllll4, lllllllll11);
                    }
                    else
                    {
                        lllllllll11 = 1; 
                    }
                    lllllllll11 = 1 - lllllllll11;
                    if (stylingRandomDataShading.enableRandomizer == 1)
                    {
                        float lllllllllllll4;
                        if (stylingRandomDataShading.opacityRandomMode == 0) 
                        {
                            lllllllllllll4 = noiseSampleData.whiteNoise;
                        }
                        else if (stylingRandomDataShading.opacityRandomMode == 1) 
                        {
                            lllllllllllll4 = noiseSampleData.perlinNoiseFloored * 5;
                        }
                        else 
                        {
                            lllllllllllll4 = ((noiseSampleData.perlinNoiseFloored + noiseSampleData.whiteNoise) / 2) * 5;
                        }
                        lllllllll11 = saturate(lllllllll11 - (lllllllllllll4 * stylingRandomDataShading.opacityRandomIntensity));
                    }
                    float llllllllllllll4 = smoothstep(saturate(min(1 - stylingDataShading.opacityFalloff, 1)), 1, llllll4);
                    lllllllll11 *= 1 - llllllllllllll4;
                    lllllllll11 = 1 - lllllllll11;
                    lllllllllllllllllllllllll10 = min(lllllllll11, lllllllllllllllllllllllll10);
                }
                lllllllllllllllllllllllll10 = 1 - lllllllllllllllllllllllll10;
                lllllllllllllllllllllllll10 *= stylingDataShading.opacity;
                lllllllllllllllllllllllll10 = 1 - lllllllllllllllllllllllll10;
                llllllllllllllllllllllllll10 = lllllllllllllllllllllllll10;             
            }
            else if (stylingDataShading.style == 1) 
            {               
                float2 llllllllllllllllll4 = llllllllllllllllllll10;
                float2 lllllll3 = RotateUV(llllllllllllllllll4, stylingDataShading.rotation);
                NoiseSampleData noiseSampleData = SampleNoiseData(lllllll3, stylingDataShading, stylingRandomDataShading, requiredNoiseDataShading, llllllllllllllll3, lllllllllllllllll3);
                if (false)
                {
                } 
                float lllllllllllllllllllllllllllllll11 = 1 - lllllllllllllllllllllllllll10;
                float ll5 = Halftones(lllllllllllllllllllllllllllllll11, lllllll3, stylingDataShading, stylingRandomDataShading, noiseSampleData);
                llllllllllllllllllllllllll10 = ll5;
            }
            if (false)
            {
            }
            #if _USE_OPTIMIZATION_DEFINES
                #if _ENABLE_STYLING_DISTANCEFADE
                     generalStylingData.enableDistanceFade = 1;
                #else
                    generalStylingData.enableDistanceFade = 0;
                #endif
            #endif
            if (generalStylingData.enableDistanceFade == 1)
            {
                float ll12 = lllllllllllllllllllllllllll10;
                if (stylingDataShading.style == 0)
                {
                    int lllllllllllllllllllllll7;
                    if (llllllllllllllllllllllllllll6 == 0)
                    {
                        lllllllllllllllllllllll7 = lllllllllllllllllllllllllllll6;
                    }
                    else
                    {
                        lllllllllllllllllllllll7 = lllllllllll5;
                    }
                    float llllllllllllllllllllllllllllll10 = (1. / lllllllllllllllllllllll7) * llllllllllllllllllllllllllllll6;
                    float lllllllllllllll11 = remap(0, 1, 0, llllllllllllllllllllllllllllll10, llllllllllllllllllllllllllllll6);
                    ll12 -= -1 + ((lllllllllllllllllllllll7 - 1.) / lllllllllllllllllllllll7) + lllllllllllllll11;
                }
                float llllll12 = distance(_WorldSpaceCameraPos, d.worldSpacePosition);
                float lllllll12 = max(ll12, 1 - stylingDataShading.opacityFalloff);
                lllllll12 = remap(1 - stylingDataShading.opacityFalloff, 1, 0, 1, lllllll12);
                float llllllll12 = max(ll12, 1 - stylingDataShading.sizeFalloff);
                llllllll12 = remap(1 - stylingDataShading.sizeFalloff, 1, 0, 1, llllllll12);
                float lllllllll12 = lerp(0.0, 1, saturate(1 - stylingDataShading.size)); 
                if (generalStylingData.adjustDistanceFadeValue == 1)
                {
                    lllllllll12 = generalStylingData.distanceFadeValue;
                }
                llllllll12 = max(lllllllll12, llllllll12 * 2);
                lllllll12 = max(lllllllll12, lllllll12);
                float llllllllll12 = max(llllllll12, lllllll12);
                llllllllll12 = saturate(llllllllll12);
                llllllllllllllllllllllllll10 = lerp(llllllllllllllllllllllllll10, llllllllll12, saturate(((llllll12 - generalStylingData.distanceFadeStartDistance) / generalStylingData.distanceFadeFalloff)));
            }
            if (positionAndBlendingDataShading.isInverted == 1)
            {
                llllllllllllllllllllllllll10 = 1 - llllllllllllllllllllllllll10;
            }
            DoBlending(lllll5, 1 - llllllllllllllllllllllllll10, positionAndBlendingDataShading.blending, stylingDataShading.color);
            if (false)
            {                
            }
            if (false)
            {
            }
        }
#endif
#if _ENABLE_CASTSHADOWS_STYLING || !_USE_OPTIMIZATION_DEFINES   
    #if !_USE_OPTIMIZATION_DEFINES
        if (lllllllllllllllllllllllllllllll6)   
    #endif
        {
        #if _USE_OPTIMIZATION_DEFINES
            #ifdef _CASTSHADOWS_STYLING_BLENDING
                positionAndBlendingDataCastShadows.blending = _CASTSHADOWS_STYLING_BLENDING;
            #endif
            #ifdef _CASTSHADOWS_STYLING_DRAWSPACE
                uvSpaceDataCastShadows.drawSpace = _CASTSHADOWS_STYLING_DRAWSPACE;
            #endif
            #ifdef _CASTSHADOWS_STYLING_COORDINATESYSTEM
                uvSpaceDataCastShadows.coordinateSystem = _CASTSHADOWS_STYLING_COORDINATESYSTEM;
            #endif            
            #ifdef _CASTSHADOWS_STYLE
                stylingDataCastShadows.style = _CASTSHADOWS_STYLE;
            #endif
            #if _CASTSHADOWS_STYLING_RANDOMIZER
                stylingRandomDataCastShadows.enableRandomizer = 1;
            #else
                stylingRandomDataCastShadows.enableRandomizer = 0;
            #endif
        #endif
            RequiredNoiseData requiredNoiseDataCastShadows;
    #if _USE_OPTIMIZATION_DEFINES
        #ifdef _CASTSHADOWS_STYLING_RANDOMIZER_PERLIN
            requiredNoiseDataCastShadows.perlinNoise = 1;
        #else
            requiredNoiseDataCastShadows.perlinNoise = 0;
        #endif
        #ifdef _CASTSHADOWS_STYLING_RANDOMIZER_PERLIN_FLOORED
            requiredNoiseDataCastShadows.perlinNoiseFloored = 1;
        #else
            requiredNoiseDataCastShadows.perlinNoiseFloored = 0;
        #endif         
        #ifdef _CASTSHADOWS_STYLING_RANDOMIZER_WHITE
            requiredNoiseDataCastShadows.whiteNoise = 1;
        #else
            requiredNoiseDataCastShadows.whiteNoise = 0;
        #endif
        #ifdef _CASTSHADOWS_STYLING_RANDOMIZER_WHITE_FLOORED
            requiredNoiseDataCastShadows.whiteNoiseFloored = 1;
        #else
            requiredNoiseDataCastShadows.whiteNoiseFloored = 0;
        #endif            
        #else            
            requiredNoiseDataCastShadows.perlinNoise = 1;
            requiredNoiseDataCastShadows.perlinNoiseFloored = 1;
            requiredNoiseDataCastShadows.whiteNoise = 1;
            requiredNoiseDataCastShadows.whiteNoiseFloored = 1;
        #endif
    #if _URP
            float2 lllllllllll12 = ConvertToDrawSpace(inputData, lllllllllllllllllllllllllllllll0, uvSpaceDataCastShadows, lllllllllllllllllllllll0);           
    #else
            float2 lllllllllll12 = ConvertToDrawSpace(d.worldSpacePosition, lllllllllllllllllllllllllllllll0, uvSpaceDataCastShadows, lllllllllllllllllllllll0);
    #endif
            l10 = ll8;
            float llllllllllllllllllllllllll10 = 0;
            if (stylingDataCastShadows.style == 0) 
            {
                float llllllllllllll12 = stylingDataCastShadows.rotation;
                float lllllllllllllll12 = radians(llllllllllllll12);
                float llllllllllllllll12 = stylingDataCastShadows.rotationBetweenCells;
                float lllllllllllllllll12 = radians(llllllllllllllll12);
                float llllllllllllllllll12 = lll7;
                llllllllllllllllll12 = min(llllllllllllllllll12, 0.99);
                float lllllllllllllllllll12 = 1;
                float lllllllllllllllllllllll7 = ll7;
            #if _USE_OPTIMIZATION_DEFINES            
                #ifdef _CASTSHADOWS_STYLING_NUMBER_OF_CELLS_HATCHING
                        lllllllllllllllllllllll7 = _CASTSHADOWS_STYLING_NUMBER_OF_CELLS_HATCHING;
                #endif                           
                [unroll(lllllllllllllllllllllll7)]
            #else
                [unroll(15)]
            #endif
                for (int j = 1; j <= lllllllllllllllllllllll7; j++)
                {
                    ll8 = min(j / lllllllllllllllllllllll7, l10);
                    if (lllllllllllllllllllllll7 != 1)
                    {
                        float lllllllllllllllllllll12 = 0;
                        if (lllllllllllllllllllllll7 <= 1)
                        {
                            lllllllllllllllllllll12 = 0.0;
                        }
                        else
                        {
                            float llllllllllllllllllllll12 = (float) j - 1;
                            float lllllllllllllllllllllll12 = (float) (lllllllllllllllllllllll7 - 1);
                            float llllllllllllllllllllllll12 = llllllllllllllllllllll12 / lllllllllllllllllllllll12;
                            lllllllllllllllllllll12 = lerp(1.0, llllllllllllllllllllllll12, llllllllllllllllll12);
                        }
                        float lllllllllllllllllllllllll12 = min(lllllllllllllllllllll12, l10); 
                        lllllllllllllllllllllllll12 = remap(0, lllllllllllllllllllll12, 0, 1, l10);
                        ll8 = lllllllllllllllllllllllll12;
                        ll8 = max(lllllllllll2, l10);
                    }
                    else
                    {
                        ll8 = l10;
                    }
                    float lllllll11 = j - 1;
                    float llllllllll3 = lllllllllllllll12 + lllllllllllllllll12 * lllllll11;
                    float2 lllll11 = RotateUVRadians(lllllllllll12, llllllllll3);
                    lllll11.x += (j - 1) / (float) lllllllllllllllllllllll7 * stylingDataCastShadows.density; 
                    NoiseSampleData noiseSampleData = SampleNoiseData(lllll11, stylingDataCastShadows, stylingRandomDataCastShadows, requiredNoiseDataCastShadows, llllllllllllllll3, lllllllllllllllll3);
                    float lllllllllllllllllllllllllllll12 = Hatching(1 - ll8, lllll11, stylingDataCastShadows, stylingRandomDataCastShadows, noiseSampleData, lllllllllllllllllllllllllllll3);
                    lllllllllllllllllllllllllllll12 = 1 - lllllllllllllllllllllllllllll12;
                    {
                        lllllllllllllllllll12 = min(lllllllllllllllllllllllllllll12, lllllllllllllllllll12);
                    }
                }
                llllllllllllllllllllllllll10 = lllllllllllllllllll12;
            }
            else if (stylingDataCastShadows.style == 1) 
            {                        
                float2 lllllll3 = RotateUV(lllllllllll12, stylingDataCastShadows.rotation);
                NoiseSampleData noiseSampleData = SampleNoiseData(lllllll3, stylingDataCastShadows, stylingRandomDataCastShadows, requiredNoiseDataCastShadows, llllllllllllllll3, lllllllllllllllll3);
                float ll5 = Halftones(1 - ll8, lllllll3, stylingDataCastShadows, stylingRandomDataCastShadows, noiseSampleData);
                llllllllllllllllllllllllll10 = ll5;            
            }
            DoBlending(lllll5, 1 - llllllllllllllllllllllllll10, positionAndBlendingDataCastShadows.blending, stylingDataCastShadows.color);                    
        }
#endif        
#if _ENABLE_SPECULAR_STYLING || !_USE_OPTIMIZATION_DEFINES   
    #if !_USE_OPTIMIZATION_DEFINES
        if (llll7)   
    #endif
        {
        #if _USE_OPTIMIZATION_DEFINES
            #ifdef _SPECULAR_STYLING_BLENDING
                positionAndBlendingDataSpecular.blending = _SPECULAR_STYLING_BLENDING;
            #endif
            #ifdef _SPECULAR_STYLING_DRAWSPACE
                uvSpaceDataSpecular.drawSpace = _SPECULAR_STYLING_DRAWSPACE;
            #endif
            #ifdef _SPECULAR_STYLING_COORDINATESYSTEM
                uvSpaceDataSpecular.coordinateSystem = _SPECULAR_STYLING_COORDINATESYSTEM;
            #endif            
            #ifdef _SPECULAR_STYLE
                stylingDataSpecular.style = _SPECULAR_STYLE;
            #endif
            #if _SPECULAR_STYLING_RANDOMIZER
                stylingRandomDataSpecular.enableRandomizer = 1;
            #else
                stylingRandomDataSpecular.enableRandomizer = 0;
            #endif
        #endif
            RequiredNoiseData requiredNoiseDataSpecular;
#if _USE_OPTIMIZATION_DEFINES            
#ifdef _SPECULAR_STYLING_RANDOMIZER_PERLIN
                    requiredNoiseDataSpecular.perlinNoise = 1;
#else
                    requiredNoiseDataSpecular.perlinNoise = 0;
#endif
#ifdef _SPECULAR_STYLING_RANDOMIZER_PERLIN_FLOORED
                    requiredNoiseDataSpecular.perlinNoiseFloored = 1;
#else
                    requiredNoiseDataSpecular.perlinNoiseFloored = 0;
#endif         
#ifdef _SPECULAR_STYLING_RANDOMIZER_WHITE
                    requiredNoiseDataSpecular.whiteNoise = 1;
#else
                    requiredNoiseDataSpecular.whiteNoise = 0;
#endif
#ifdef _SPECULAR_STYLING_RANDOMIZER_WHITE_FLOORED
                    requiredNoiseDataSpecular.whiteNoiseFloored = 1;
#else
                    requiredNoiseDataSpecular.whiteNoiseFloored = 0;
#endif      
#else            
            requiredNoiseDataSpecular.perlinNoise = 1;
            requiredNoiseDataSpecular.perlinNoiseFloored = 1;
            requiredNoiseDataSpecular.whiteNoise = 1;
            requiredNoiseDataSpecular.whiteNoiseFloored = 1;
#endif
        #if _URP
            float2 l13 = ConvertToDrawSpace(inputData, lllllllllllllllllllllllllllllll0, uvSpaceDataSpecular, lllllllllllllllllllllll0);
        #else
            float2 l13 = ConvertToDrawSpace(d.worldSpacePosition, lllllllllllllllllllllllllllllll0, uvSpaceDataSpecular, lllllllllllllllllllllll0);
        #endif
                float2 lllllll3 = RotateUV(l13, stylingDataSpecular.rotation);
                l13 = lllllll3;
            NoiseSampleData noiseSampleData = SampleNoiseData(l13, stylingDataSpecular, stylingRandomDataSpecular, requiredNoiseDataSpecular, llllllllllllllll3, lllllllllllllllll3);
    #if _USE_OPTIMIZATION_DEFINES 
        #ifdef _SPECULAR_STYLE
            stylingDataSpecular.style = _SPECULAR_STYLE;
        #endif
    #endif
            float llllllllllllllllllllllllll10 = 0;     
            if (stylingDataSpecular.style == 0) 
            {                 
                llllllllllllllllllllllllll10 = Hatching(lllll8, l13, stylingDataSpecular, stylingRandomDataSpecular, noiseSampleData, lllllllllllllllllllllllllllll3);
                llllllllllllllllllllllllll10 = 1 - llllllllllllllllllllllllll10;
            }
            else if (stylingDataSpecular.style == 1) 
            {
                float ll5 = Halftones(lllll8, l13, stylingDataSpecular, stylingRandomDataSpecular, noiseSampleData);
                llllllllllllllllllllllllll10 = ll5;              
            }
            #if _USE_OPTIMIZATION_DEFINES
                #ifdef _SPECULAR_STYLING_BLENDING
                     positionAndBlendingDataSpecular.blending = _SPECULAR_STYLING_BLENDING;
                #endif
            #endif
            half4 llllllllllllll10;
            if (lllllllll7 == 1)
            {
                llllllllllllll10 = half4(llllll8, 1);
            }
            else
            {
                llllllllllllll10 = stylingDataSpecular.color;
            }
            DoBlending(lllll5, 1 - llllllllllllllllllllllllll10, positionAndBlendingDataSpecular.blending, llllllllllllll10);
        }
#endif
#if _ENABLE_RIM_STYLING || !_USE_OPTIMIZATION_DEFINES   
        #if !_USE_OPTIMIZATION_DEFINES
        if (lllllllllll7)
        #endif
        {
        #if _USE_OPTIMIZATION_DEFINES
            #ifdef _RIM_STYLING_BLENDING
                    positionAndBlendingDataRim.blending = _RIM_STYLING_BLENDING;
            #endif
            #ifdef _RIM_STYLING_DRAWSPACE
                uvSpaceDataRim.drawSpace = _RIM_STYLING_DRAWSPACE;
            #endif
            #ifdef _RIM_STYLING_COORDINATESYSTEM
                uvSpaceDataRim.coordinateSystem = _RIM_STYLING_COORDINATESYSTEM;
            #endif        
            #ifdef _RIM_STYLE
                stylingDataRim.style = _RIM_STYLE;
            #endif
            #if _RIM_STYLING_RANDOMIZER
                stylingRandomDataRim.enableRandomizer = 1;
            #else
                stylingRandomDataRim.enableRandomizer = 0;
            #endif
        #endif
            RequiredNoiseData requiredNoiseDataRim;
        #if _USE_OPTIMIZATION_DEFINES
            #ifdef _RIM_STYLING_RANDOMIZER_PERLIN
                requiredNoiseDataRim.perlinNoise = 1;
            #else
                requiredNoiseDataRim.perlinNoise = 0;
            #endif
            #ifdef _RIM_STYLING_RANDOMIZER_PERLIN_FLOORED
                requiredNoiseDataRim.perlinNoiseFloored = 1;
            #else
                requiredNoiseDataRim.perlinNoiseFloored = 0;
            #endif         
            #ifdef _RIM_STYLING_RANDOMIZER_WHITE
                requiredNoiseDataRim.whiteNoise = 1;
            #else
                requiredNoiseDataRim.whiteNoise = 0;
            #endif
            #ifdef _RIM_STYLING_RANDOMIZER_WHITE_FLOORED
                requiredNoiseDataRim.whiteNoiseFloored = 1;
            #else
                requiredNoiseDataRim.whiteNoiseFloored = 0;
            #endif      
        #else            
            requiredNoiseDataRim.perlinNoise = 1;
            requiredNoiseDataRim.perlinNoiseFloored = 1;
            requiredNoiseDataRim.whiteNoise = 1;
            requiredNoiseDataRim.whiteNoiseFloored = 1;
        #endif
    #if _URP
            float2 lllllll13 = ConvertToDrawSpace(inputData, lllllllllllllllllllllllllllllll0, uvSpaceDataRim, lllllllllllllllllllllll0);
    #else
            float2 lllllll13 = ConvertToDrawSpace(d.worldSpacePosition, lllllllllllllllllllllllllllllll0, uvSpaceDataRim, lllllllllllllllllllllll0);
    #endif
            float2 lllllll3 = RotateUV(lllllll13, stylingDataRim.rotation);
            NoiseSampleData noiseSampleData = SampleNoiseData(lllllll3, stylingDataRim, stylingRandomDataRim, requiredNoiseDataRim, llllllllllllllll3, lllllllllllllllll3);
            if (lllllllllllllll6 == 0 || llllllllllll7 == 0) 
            {
            #if _URP
                Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
                float llllllllllllllll10 = dot(mainLight.direction, lll2);
                float lllllllllllllllll10 = mainLight.shadowAttenuation;
                llllllllllll2 = CalculateRimMask(llllllllllllllllllllllllll7, lllllllllllllll1, lllllllllllll7, llllllllllllll7, llllllllllllllll10, lllllllllllllll7, l6, lllllllllllllllllllllllllll5, lllllllllllllllll10);
            #else
                llllllllllll2 = CalculateRimMask(llllllllllllllllllllllllll7, lllllllllllllll1, lllllllllllll7, llllllllllllll7, llllllllllllllllll1, lllllllllllllll7, l6, lllllllllllllllllllllllllll5, lllllllllll2);
            #endif
            }
            llllllllllll2 = saturate(llllllllllll2 - lllllllllllllllllll1 * 10);
            float llllllllllllllllllllllllll10 = 0;
            if (stylingDataRim.style == 0) 
            {
                llllllllllllllllllllllllll10 = Hatching(llllllllllll2, lllllll3, stylingDataRim, stylingRandomDataRim, noiseSampleData, lllllllllllllllllllllllllllll3);
                llllllllllllllllllllllllll10 = 1 - llllllllllllllllllllllllll10;
            }
            else if (stylingDataRim.style == 1) 
            {
                float ll5 = Halftones(llllllllllll2, lllllll3, stylingDataRim, stylingRandomDataRim, noiseSampleData);
                llllllllllllllllllllllllll10 = ll5;
            }
            DoBlending(lllll5, 1-llllllllllllllllllllllllll10, positionAndBlendingDataRim.blending, stylingDataRim.color);
        }
    #endif
    }
#endif
    #if _URP
        AlphaDiscard(surface.alpha, 0.5);
    #else
    #endif


}




void AddTheToonShader(inout float4 albedo,

#if _URP
    InputData inputData, 
    SurfaceData surface,
#else
    #if _USESPECULAR || _USESPECULARWORKFLOW || _SPECULARFROMMETALLIC
                 SurfaceOutputStandardSpecular o,
    #elif _BDRFLAMBERT || _BDRF3 || _SIMPLELIT

                 SurfaceOutput o,
    #else
                 SurfaceOutputStandard o,
    #endif

    UnityGI gi,
#if !_PASSFORWARDADD
    UnityGIInput giInput,
#endif
#endif

 ShaderData d
#if _URP
    #if UNITY_VERSION >= 202120
, float3 normalTS
    #endif
#endif
)
{
    
    
    float2 uv = d.texcoord0.xy;
    
    

    
    float3 pureNormal = d.worldSpaceNormal;

    float4 screenUV = d.extraV2F0;
    

    
    UVSpaceData uvSpaceDataShading;
    uvSpaceDataShading.drawSpace = _DrawSpace;
    uvSpaceDataShading.coordinateSystem = _CoordinateSystem;
    uvSpaceDataShading.polarCenterMode = _PolarCenterMode;
    uvSpaceDataShading.polarCenter = _PolarCenter;
    uvSpaceDataShading.sSCameraDistanceScaled = _SSCameraDistanceScaled;
    uvSpaceDataShading.anchorSSToObjectsOrigin = _AnchorSSToObjectsOrigin;
    
    UVSpaceData uvSpaceDataCastShadows;
    uvSpaceDataCastShadows.drawSpace = _CastShadowsDrawSpace;
    uvSpaceDataCastShadows.coordinateSystem = _CastShadowsCoordinateSystem;
    uvSpaceDataCastShadows.polarCenterMode = _CastShadowsPolarCenterMode;
    uvSpaceDataCastShadows.polarCenter = _CastShadowsPolarCenter;
    uvSpaceDataCastShadows.sSCameraDistanceScaled = _CastShadowsSSCameraDistanceScaled;
    uvSpaceDataCastShadows.anchorSSToObjectsOrigin = _CastShadowsAnchorSSToObjectsOrigin;
    
    UVSpaceData uvSpaceDataSpecular;
    uvSpaceDataSpecular.drawSpace = _SpecularDrawSpace;
    uvSpaceDataSpecular.coordinateSystem = _SpecularCoordinateSystem;
    uvSpaceDataSpecular.polarCenterMode = _SpecularPolarCenterMode;
    uvSpaceDataSpecular.polarCenter = _SpecularPolarCenter;
    uvSpaceDataSpecular.sSCameraDistanceScaled = _SpecularSSCameraDistanceScaled;
    uvSpaceDataSpecular.anchorSSToObjectsOrigin = _SpecularAnchorSSToObjectsOrigin;

    UVSpaceData uvSpaceDataRim;
    uvSpaceDataRim.drawSpace = _RimDrawSpace;
    uvSpaceDataRim.coordinateSystem = _RimCoordinateSystem;
    uvSpaceDataRim.polarCenterMode = _RimPolarCenterMode;
    uvSpaceDataRim.polarCenter = _RimPolarCenter;
    uvSpaceDataRim.sSCameraDistanceScaled = _RimSSCameraDistanceScaled;
    uvSpaceDataRim.anchorSSToObjectsOrigin = _RimAnchorSSToObjectsOrigin;



    GeneralStylingData generalStylingData;
    generalStylingData.enableDistanceFade = _EnableStylingDistanceFade;
    generalStylingData.distanceFadeStartDistance = _StylingDFStartingDistance;
    generalStylingData.distanceFadeFalloff = _StylingDFFalloff;
    generalStylingData.adjustDistanceFadeValue = _StylingAdjustDistanceFadeValue;
    generalStylingData.distanceFadeValue = _StylingDistanceFadeValue;

    StylingData stylingDataShading;
    stylingDataShading.style = _ShadingStyle;
    stylingDataShading.type = 0;
    stylingDataShading.color = _StylingColor;
    stylingDataShading.rotation = _StylingShadingInitialDirection;
    stylingDataShading.rotationBetweenCells = _StylingShadingRotationBetweenCells;
    stylingDataShading.density = _StylingShadingDensity;
    stylingDataShading.offset = _StylingShadingHalftonesOffset;
    stylingDataShading.size = _StylingShadingThickness;
    stylingDataShading.sizeControl = _StylingShadingThicknessControl;
    stylingDataShading.sizeFalloff = _StylingShadingThicknessFalloff;
    stylingDataShading.roundness = _StylingShadingHalftonesRoundness;
    stylingDataShading.roundnessFalloff = _StylingShadingHalftonesRoundnessFalloff;
    stylingDataShading.hardness = _StylingShadingHardness;
    stylingDataShading.opacity = _StylingShadingOpacity;
    stylingDataShading.opacityFalloff = _StylingShadingOpacityFalloff;

    
    
    
    StylingData stylingDataCastShadows;    
    
    stylingDataCastShadows.style = _CastShadowsStyle;
    stylingDataCastShadows.type = 1;
    stylingDataCastShadows.color = _StylingCastShadowsColor;
    stylingDataCastShadows.rotation = _StylingCastShadowsInitialDirection;
    stylingDataCastShadows.rotationBetweenCells = _StylingCastShadowsRotationBetweenCells;
    stylingDataCastShadows.density = _StylingCastShadowsDensity;
    stylingDataCastShadows.offset = _StylingCastShadowsHalftonesOffset;
    stylingDataCastShadows.size = _StylingCastShadowsThickness;
    stylingDataCastShadows.sizeControl = _StylingCastShadowsThicknessControl;
    stylingDataCastShadows.sizeFalloff = _StylingCastShadowsThicknessFalloff;
    stylingDataCastShadows.roundness = _StylingCastShadowsHalftonesRoundness;
    stylingDataCastShadows.roundnessFalloff = _StylingCastShadowsHalftonesRoundnessFalloff;
    stylingDataCastShadows.hardness = _StylingCastShadowsHardness;
    stylingDataCastShadows.opacity = _StylingCastShadowsOpacity;
    stylingDataCastShadows.opacityFalloff = _StylingCastShadowsOpacityFalloff;
    
    StylingData stylingDataSpecular;
    stylingDataSpecular.style = _SpecularStyle;
    stylingDataSpecular.type = 1;
    stylingDataSpecular.color = _StylingSpecularColor;
    stylingDataSpecular.rotation = _StylingSpecularRotation;
    stylingDataSpecular.density = _StylingSpecularDensity;
    stylingDataSpecular.offset = _StylingSpecularHalftonesOffset;
    stylingDataSpecular.size = _StylingSpecularThickness;
    stylingDataSpecular.sizeControl = _StylingSpecularThicknessControl;
    stylingDataSpecular.sizeFalloff = _StylingSpecularThicknessFalloff;
    stylingDataSpecular.roundness = _StylingSpecularHalftonesRoundness;
    stylingDataSpecular.roundnessFalloff = _StylingSpecularHalftonesRoundnessFalloff;
    stylingDataSpecular.hardness = _StylingSpecularHardness;
    stylingDataSpecular.opacity = _StylingSpecularOpacity;
    stylingDataSpecular.opacityFalloff = _StylingSpecularOpacityFalloff;

    StylingData stylingDataRim;
    stylingDataRim.style = _RimStyle;
    stylingDataRim.type = 1;
    stylingDataRim.color = _StylingRimColor;
    stylingDataRim.rotation = _StylingRimRotation;
    stylingDataRim.density = _StylingRimDensity;
    stylingDataRim.offset = _StylingRimHalftonesOffset;
    stylingDataRim.size = _StylingRimThickness;
    stylingDataRim.sizeControl = _StylingRimThicknessControl;
    stylingDataRim.sizeFalloff = _StylingRimThicknessFalloff;
    stylingDataRim.roundness = _StylingRimHalftonesRoundness;
    stylingDataRim.roundnessFalloff = _StylingRimHalftonesRoundnessFalloff;
    stylingDataRim.hardness = _StylingRimHardness;
    stylingDataRim.opacity = _StylingRimOpacity;
    stylingDataRim.opacityFalloff = _StylingRimOpacityFalloff;

    
 
    
    PositionAndBlendingData positionAndBlendingDataShading;
            
    positionAndBlendingDataShading.blending = _StylingShadingBlending;
    positionAndBlendingDataShading.isInverted = _StylingShadingIsInverted;

    PositionAndBlendingData positionAndBlendingDataCastShadows;
    positionAndBlendingDataCastShadows.blending = _StylingCastShadowsBlending;
    positionAndBlendingDataCastShadows.isInverted = _StylingCastShadowsIsInverted;
    
    PositionAndBlendingData positionAndBlendingDataSpecular;
            
    positionAndBlendingDataSpecular.blending = _StylingSpecularBlending;
    positionAndBlendingDataSpecular.isInverted = _StylingSpecularIsInverted;

    PositionAndBlendingData positionAndBlendingDataRim;
            
    positionAndBlendingDataRim.blending = _StylingRimBlending;
    positionAndBlendingDataRim.isInverted = _StylingRimIsInverted;


    StylingRandomData stylingRandomDataShading;
    stylingRandomDataShading.enableRandomizer = _EnableShadingRandomizer;
    stylingRandomDataShading.perlinNoiseSize = _ShadingNoise1Size;
    stylingRandomDataShading.perlinNoiseSeed = _ShadingNoise1Seed;
    stylingRandomDataShading.whiteNoiseSeed = _ShadingNoise2Seed;
    stylingRandomDataShading.noiseIntensity = _NoiseIntensity;
    stylingRandomDataShading.spacingRandomMode = _SpacingRandomMode;
    stylingRandomDataShading.spacingRandomIntensity = _SpacingRandomIntensity;
    stylingRandomDataShading.opacityRandomMode = _OpacityRandomMode;
    stylingRandomDataShading.opacityRandomIntensity = _OpacityRandomIntensity;
    stylingRandomDataShading.lengthRandomMode = _LengthRandomMode;
    stylingRandomDataShading.lengthRandomIntensity = _LengthRandomIntensity;
    stylingRandomDataShading.hardnessRandomMode = _HardnessRandomMode;
    stylingRandomDataShading.hardnessRandomIntensity = _HardnessRandomIntensity;
    stylingRandomDataShading.thicknessRandomMode = _ThicknessRandomMode;
    stylingRandomDataShading.thicknesshRandomIntensity = _ThicknesshRandomIntensity;
    
    StylingRandomData stylingRandomDataCastShadows;
    stylingRandomDataCastShadows.enableRandomizer = _EnableCastShadowsRandomizer;
    stylingRandomDataCastShadows.perlinNoiseSize = _CastShadowsNoise1Size;
    stylingRandomDataCastShadows.perlinNoiseSeed = _CastShadowsNoise1Seed;
    stylingRandomDataCastShadows.whiteNoiseSeed = _CastShadowsNoise2Seed;
    stylingRandomDataCastShadows.noiseIntensity = _CastShadowsNoiseIntensity;
    stylingRandomDataCastShadows.spacingRandomMode = _CastShadowsSpacingRandomMode;
    stylingRandomDataCastShadows.spacingRandomIntensity = _CastShadowsSpacingRandomIntensity;
    stylingRandomDataCastShadows.opacityRandomMode = _CastShadowsOpacityRandomMode;
    stylingRandomDataCastShadows.opacityRandomIntensity = _CastShadowsOpacityRandomIntensity;
    stylingRandomDataCastShadows.lengthRandomMode = _CastShadowsLengthRandomMode;
    stylingRandomDataCastShadows.lengthRandomIntensity = _CastShadowsLengthRandomIntensity;
    stylingRandomDataCastShadows.hardnessRandomMode = _CastShadowsHardnessRandomMode;
    stylingRandomDataCastShadows.hardnessRandomIntensity = _CastShadowsHardnessRandomIntensity;
    stylingRandomDataCastShadows.thicknessRandomMode = _CastShadowsThicknessRandomMode;
    stylingRandomDataCastShadows.thicknesshRandomIntensity = _CastShadowsThicknesshRandomIntensity;

    StylingRandomData stylingRandomDataSpecular;
    stylingRandomDataSpecular.enableRandomizer = _EnableSpecularRandomizer;
    stylingRandomDataSpecular.perlinNoiseSize = _SpecularNoise1Size;
    stylingRandomDataSpecular.perlinNoiseSeed = _SpecularNoise1Seed;
    stylingRandomDataSpecular.whiteNoiseSeed = _SpecularNoise2Seed;
    stylingRandomDataSpecular.noiseIntensity = _SpecularNoiseIntensity;
    stylingRandomDataSpecular.spacingRandomMode = _SpecularSpacingRandomMode;
    stylingRandomDataSpecular.spacingRandomIntensity = _SpecularSpacingRandomIntensity;
    stylingRandomDataSpecular.opacityRandomMode = _SpecularOpacityRandomMode;
    stylingRandomDataSpecular.opacityRandomIntensity = _SpecularOpacityRandomIntensity;
    stylingRandomDataSpecular.lengthRandomMode = _SpecularLengthRandomMode;
    stylingRandomDataSpecular.lengthRandomIntensity = _SpecularLengthRandomIntensity;
    stylingRandomDataSpecular.hardnessRandomMode = _SpecularHardnessRandomMode;
    stylingRandomDataSpecular.hardnessRandomIntensity = _SpecularHardnessRandomIntensity;
    stylingRandomDataSpecular.thicknessRandomMode = _SpecularThicknessRandomMode;
    stylingRandomDataSpecular.thicknesshRandomIntensity = _SpecularThicknesshRandomIntensity;

    StylingRandomData stylingRandomDataRim;
    stylingRandomDataRim.enableRandomizer = _EnableRimRandomizer;
    stylingRandomDataRim.perlinNoiseSize = _RimNoise1Size;
    stylingRandomDataRim.perlinNoiseSeed = _RimNoise1Seed;
    stylingRandomDataRim.whiteNoiseSeed = _RimNoise2Seed;
    stylingRandomDataRim.noiseIntensity = _RimNoiseIntensity;
    stylingRandomDataRim.spacingRandomMode = _RimSpacingRandomMode;
    stylingRandomDataRim.spacingRandomIntensity = _RimSpacingRandomIntensity;
    stylingRandomDataRim.opacityRandomMode = _RimOpacityRandomMode;
    stylingRandomDataRim.opacityRandomIntensity = _RimOpacityRandomIntensity;
    stylingRandomDataRim.lengthRandomMode = _RimLengthRandomMode;
    stylingRandomDataRim.lengthRandomIntensity = _RimLengthRandomIntensity;
    stylingRandomDataRim.hardnessRandomMode = _RimHardnessRandomMode;
    stylingRandomDataRim.hardnessRandomIntensity = _RimHardnessRandomIntensity;
    stylingRandomDataRim.thicknessRandomMode = _RimThicknessRandomMode;
    stylingRandomDataRim.thicknesshRandomIntensity = _RimThicknesshRandomIntensity;


    
    DoToonShading(
#if _URP
    inputData,
    surface,
#else
    o,
    gi,
    #if !_PASSFORWARDADD
    giInput,
    #endif
#endif
    d,
#if _URP
    #if UNITY_VERSION >= 202120
    normalTS,
    #endif
#endif
            albedo, _NumberOfCells, _CellTransitionSmoothness, _SumLightsBeforePosterization, _ShadingUseLightColors,
    
            uv, screenUV, _HatchingMap,
            
            _ShadingMode, _LightFunction,

            _EnableToonShading, _ShadingFunction,

            _GradientTex, _GradientTex_TexelSize, _GradientMode, _GradientBlending, _GradientBlendFactor,

            _EnableShadows, _CoreShadowColor, _TerminatorWidth, _TerminatorSmoothness, _FormShadowColor,
            _EnableCastShadows, _CastShadowsStrength, _CastShadowsSmoothness, _CastShadowColorMode, _CastShadowColor,
            _ShadingAffectedByNormalMap,
            
            _EnableSpecular, _SpecularBlending, _SpecularColor, _SpecularSize, _SpecularSmoothness, _SpecularOpacity, _SpecularAffectedByNormalMap, _SpecularUseLightColors,
            
            _EnableRim, _RimBlending, _RimColor, _RimSize, _RimSmoothness, _RimOpacity, _RimAffectedArea, _RimAffectedByNormalMap,
            
    
            _EnableStyling, 
    
            generalStylingData, _HatchingAffectedByNormalMap, _EnableAntiAliasing,
    
            _EnableShadingStyling, 
            _StylingShadingSyncWithOtherStyling,
            _SyncWithLightPartitioning, _NumberOfCellsHatching, _StylingOvermodelingFactor,
            positionAndBlendingDataShading, uvSpaceDataShading, stylingDataShading, stylingRandomDataShading,
    
            _EnableCastShadowsStyling,
            _StylingCastShadowsSyncWithOtherStyling,
            _CastShadowsNumberOfCellsHatching, _StylingCastShadowsSmoothness, 
            positionAndBlendingDataCastShadows, uvSpaceDataCastShadows, stylingDataCastShadows, stylingRandomDataCastShadows,
    
            _EnableSpecularStyling,
            _SyncWithSpecular, _StylingSpecularSize, _StylingSpecularSmoothness, _StylingSpecularCutOutShading, _StylingSpecularUseLightColors,
            _StylingSpecularSyncWithOtherStyling,
            positionAndBlendingDataSpecular, uvSpaceDataSpecular, stylingDataSpecular, stylingRandomDataSpecular,
    
            _EnableRimStyling,
            _SyncWithRim, _StylingRimSize, _StylingRimSmoothness, _StylingRimAffectedArea, 
            _StylingRimSyncWithOtherStyling,
            positionAndBlendingDataRim, uvSpaceDataRim, stylingDataRim, stylingRandomDataRim,


            _NoiseMap1, _NoiseMap2, _NoiseTex2_TexelSize,   
            
            pureNormal);
}





#endif
