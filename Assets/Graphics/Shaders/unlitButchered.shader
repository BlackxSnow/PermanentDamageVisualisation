Shader "unlitButchered"
    {
        Properties
        {

        }
        SubShader
        {
            Tags
            {
                "RenderPipeline"="HDRenderPipeline"
                "RenderType"="HDUnlitShader"
                "Queue"="Geometry+0"
            }
            Pass
            {
                Name "MotionVectors"
                Tags
                {
                    "LightMode" = "MotionVectors"
                }
    
                // Render State
                Cull [_CullMode]
                ZWrite On
                ColorMask [_ColorMaskNormal] 1
                ColorMask 0 2
                Stencil
                    {
                        WriteMask [_StencilWriteMaskMV]
                        Ref [_StencilRefMV]
                        CompFront Always
                        PassFront Replace
                        CompBack Always
                        PassBack Replace
                    }
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma vertex Vert
                #pragma fragment Frag

    
                // Keywords

                // GraphKeywords: <None>
    
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
                #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
                //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
                //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
                //#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderGraphHeader.hlsl" // Need to be here for Gradient struct definition
    

    
                // --------------------------------------------------
                // Structs and Packing
    
                struct AttributesMesh
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 uv2 : TEXCOORD2;
                    float4 uv3 : TEXCOORD3;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                
                // --------------------------------------------------
                // Main

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f Vert(AttributesMesh inputMesh) 
                {
                    v2f o;
                    o.uv = inputMesh.uv0;
                    o.vertex = TransformWorldToHClip(TransformObjectToWorld(inputMesh.positionOS));
                    return o;
                }

                float4 Frag() : SV_TARGET
                {
                    return float4(1,1,1,1);
                }
    
                ENDHLSL
            }
        }
        CustomEditor "Rendering.HighDefinition.HDUnlitGUI"
        FallBack "Hidden/Shader Graph/FallbackError"
    }
