Shader "Custom/SimpleBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurAmount ("Blur Amount", Range(0, 10)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float blurSize = _BlurAmount * _MainTex_TexelSize.xy;
                fixed4 col = fixed4(0, 0, 0, 0);
                
                // Simple box blur (9 samples)
                col += tex2D(_MainTex, i.uv + float2(-1, -1) * blurSize);
                col += tex2D(_MainTex, i.uv + float2(0, -1) * blurSize);
                col += tex2D(_MainTex, i.uv + float2(1, -1) * blurSize);
                col += tex2D(_MainTex, i.uv + float2(-1, 0) * blurSize);
                col += tex2D(_MainTex, i.uv);
                col += tex2D(_MainTex, i.uv + float2(1, 0) * blurSize);
                col += tex2D(_MainTex, i.uv + float2(-1, 1) * blurSize);
                col += tex2D(_MainTex, i.uv + float2(0, 1) * blurSize);
                col += tex2D(_MainTex, i.uv + float2(1, 1) * blurSize);
                
                col /= 9.0;
                return col;
            }
            ENDCG
        }
    }
}
