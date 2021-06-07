Shader "Unlit/Unwrap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TextureSize ("Texture Size", Float) = 1
        _Scale ("Scale", Vector) = (1,1,1,1)
        _Size ("Size", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Pass
        {
            CGPROGRAM
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
                float4 vertex_Original : POSITION1;
            };

            sampler2D _MainTex;
            float4 _Scale;
            float _Size;
            float4 _MainTex_ST;
            float4x4 _ProjectionVPMatrix;
            float _MaxObjectSize;
            

            float4 ProjectionToClipPos(in float3 pos)
            {
                return mul(_ProjectionVPMatrix, mul(unity_ObjectToWorld, float4(pos, 1.0)));
            }

            v2f vert (appdata v)
            {
                v2f o;
                float3 unwrapVertPos = float3(v.uv1.x, 0, v.uv1.y * -1);
                
                unwrapVertPos -= float3(0.5, 0, -0.5);
                unwrapVertPos /= _Scale.xyz;
                unwrapVertPos *= _Size;
                unwrapVertPos *= float3(1,1,-1);
                o.vertex = ProjectionToClipPos(unwrapVertPos);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.uv1;
                o.vertex_Original = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = fixed4((i.vertex_Original.rgb / _MaxObjectSize) + 0.5, 1);
                return col;
            }

            ENDCG
        }
    }
}
