Shader "Blit/UVDilate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {} //old UV Map
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Pass
        {
            HLSLPROGRAM
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
            float _TextureSize;

            #include "Includes/UVDilate.hlsl"


            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.uv1;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(Dilate(_MainTex, i.uv1, _TextureSize, 64), 1);
            }

            ENDHLSL
        }
    }
}
