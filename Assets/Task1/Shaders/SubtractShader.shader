Shader "Saleh/SubtractShader"
{
    Properties
    {
        // Two Textures, B will be subtracted from A (B-A)
        _TexA ("TextureA", 2D) = "white" {}
        _TexB ("TextureB", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };


            sampler2D _TexA;
            float4 _TexA_ST;
          
            sampler2D _TexB;
           
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _TexA);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 colA = tex2D(_TexA, i.uv);
                fixed4 colB = tex2D(_TexB, i.uv);

                // returning subtracted pixel value (B-A)
                return (colB - colA);

            }
            ENDCG
        }
    }
}
