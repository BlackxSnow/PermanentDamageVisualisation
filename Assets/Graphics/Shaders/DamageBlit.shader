Shader "Unlit/DamageBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {} //Old Damage Map
        _UVPositionMap ("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _UVPositionMap;
            float4 _UVPositionMap_ST;
            float _MaxObjectSize;

            bool _ComputeDamage;
            int _BufferSize;

            //Up to two positions and directions can be registered per hit
            float4 _HitPositions[256];
            float4 _LineDirections[256];
            float4 _HitData[128]; //Size, Falloff, Strength, IsLine

            #include "Includes/FloatComparisons.hlsl"


            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.uv1;
                return o;
            }

            static const float DecayAmount = 0.1;

            fixed EllipseDepth(float distance, float radius, float maxDepth)
            {
                return sqrt( (1-pow(distance,2)/pow(radius,2)) * pow(maxDepth, 2));
            }

            fixed ParabolicLip(float distance, float radius, float height)
            {
                const float x_1 = -radius;
                float peakX = (x_1 + radius) / 2;
                float a = height / ((peakX - x_1) * (peakX - radius));
                return a * (distance - x_1) * (distance - radius);
            }

            fixed CalculateDamage(float distance, float size, float falloff, float strength, out float absoluteDamage)
            {
                float baseRadius = size/2;
                float maxRadius = baseRadius + falloff;

                float damage;
                if (distance < baseRadius) 
                {
                    damage = EllipseDepth(distance, baseRadius, strength);
                    absoluteDamage = abs(damage);
                    return damage;
                }
                else if (distance < baseRadius + size/3)
                {
                    damage = -ParabolicLip(distance - baseRadius, size/3, strength/3);
                    absoluteDamage = abs(damage);
                    return damage;
                }
                absoluteDamage = 0;
                return 0;
            }

            fixed CalculateHeat(float distance, float size, float falloff, float strength)
            {
                float maxDist = size / 2 + falloff;
                if(distance < maxDist)
                {
                    return pow(EllipseDepth(distance, maxDist, strength), 2);
                }
                return 0;
            }

            //TODO update to match CalculateLineDamage outputs
            fixed CalculatePointDamage(float2 uv, float4 position, float size, float falloff, float strength) 
            {
                float actualDistance = distance((tex2D(_UVPositionMap, uv).xyz - 0.5) * _MaxObjectSize, position.xyz);
                float absoluteDamage;
                return CalculateDamage(actualDistance, size, falloff, strength, absoluteDamage);

            }

            fixed DistanceFromLine(float3 position, float4 lineStart, float4 lineVector)
            {
                float minT = clamp( dot(position - lineStart, lineVector) / pow(length(lineVector), 2), 0, 1);
                return distance(lineStart + minT * lineVector, position);
            } 

            fixed CalculateLineDamage(float2 uv, int currentPositionIndex, int currentLineIndex, float size, float falloff, float strength, out float heat, out float absoluteDamage)
            {
                float3 pixelPosition = (tex2D(_UVPositionMap, uv).xyz - 0.5) * _MaxObjectSize;

                float distanceFromFirst = DistanceFromLine(pixelPosition, _HitPositions[currentPositionIndex], _LineDirections[currentLineIndex]);
                float distanceFromLast = DistanceFromLine(pixelPosition, _HitPositions[currentPositionIndex + 1], _LineDirections[currentLineIndex + 1]);
                float minDistance = min(distanceFromFirst, distanceFromLast);

                strength = strength / max(length(_LineDirections[currentLineIndex]) + length(_LineDirections[currentLineIndex + 1]), 1);
                float damage = CalculateDamage(minDistance, size, falloff, strength, absoluteDamage);
                heat = CalculateHeat(minDistance, size, falloff, strength);

                fixed lastFrameDamage = tex2D(_MainTex, uv).b;
                float startDistance = distance(_HitPositions[currentPositionIndex], pixelPosition);
                float endDistance = distance(_HitPositions[currentPositionIndex + 1], pixelPosition);
                if(startDistance < size / 2 || endDistance < size / 2)
                {
                    heat /= 2;
                }
                if (sign(damage) >= 0)
                {
                    absoluteDamage = max(lastFrameDamage, damage) - lastFrameDamage;
                    return absoluteDamage;
                }
                else
                {
                    // absoluteDamage = max(abs(lastFrameDamage), abs(damage)) - abs(lastFrameDamage);
                    // return -absoluteDamage;
                    damage = -max(abs(lastFrameDamage), abs(damage)) + abs(lastFrameDamage);
                    absoluteDamage = abs(damage);
                    return damage;
                }
                
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float accumulatedDamage = 0;
                float accumulatedHeat = 0;
                float accumulatedAbsoluteDamage = 0;
                int positionsIndex = 0;
                int lineIndex = 0;
                if (_ComputeDamage)
                {
                                    
                    for(int j = 0; j < _BufferSize; j++, positionsIndex++)
                    {
                        if (_HitData[j].w == 0)
                        {
                            accumulatedDamage += CalculatePointDamage(i.uv1, _HitPositions[positionsIndex], _HitData[j].x, _HitData[j].y, _HitData[j].z);
                        }
                        else
                        {
                            float heat;
                            float absoluteDamage;
                            accumulatedDamage += CalculateLineDamage(i.uv1, positionsIndex, lineIndex, _HitData[j].x, _HitData[j].y, _HitData[j].z, heat, absoluteDamage);
                            accumulatedHeat += heat;
                            accumulatedAbsoluteDamage += absoluteDamage;
                            positionsIndex++;
                            lineIndex += 2;
                        }
                        
                    }
                }
                fixed4 oldSample = tex2D(_MainTex, i.uv1);
                //Damage, Heat, LastFrameDamage, null
                //Damage clamped to stop visual issues. No technical issue with unclamping
                //Heat and lastDamage decay
                fixed4 col = fixed4(
                    clamp(oldSample.r - accumulatedDamage, 0, 1), 
                    accumulatedHeat + clamp(oldSample.g - (oldSample.g * DecayAmount * unity_DeltaTime.x), 0, 1),
                    accumulatedDamage + oldSample.b * pow(0.98, unity_DeltaTime.x),
                    clamp(accumulatedAbsoluteDamage + oldSample.a, 0, 1));
                return col;
            }

            ENDHLSL
        }
    }
}
