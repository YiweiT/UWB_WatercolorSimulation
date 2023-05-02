Shader "Custom/DetectBoundary"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Theta ("Theta", Range(0.05, 0.2)) = 0.1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _RefTex;
            float4 _MainTex_TexelSize;
            float _Theta;

            // Check the amount of water at the cell is above the min amount of water to flow (_Theta)
            // If waterAmount >= _Theta --> return 1
            // return 0, otherwise
            int canFlow(sampler2D tex, float2 uv)
            {
                float waterAmount = tex2D(tex, uv).r;
                if (waterAmount < _Theta && waterAmount > 0)
                {
                    return 0;
                } else {
                    return 1;
                }
            }

            float4 detectBoundary (sampler2D tex, float2 uv, float4 size)
            {
                float c = canFlow(tex, uv + float2(-size.x, size.y)) 
                         + canFlow(tex, uv + float2(0, size.y)) 
                         + canFlow(tex, uv + float2(size.x, size.y)) 
                         + canFlow(tex, uv + float2(-size.x, 0)) 
                         + canFlow(tex, uv + float2(size.x, 0))
                         + canFlow(tex, uv + float2(-size.x, -size.y)) 
                         + canFlow(tex, uv + float2(0, -size.y)) 
                         + canFlow(tex, uv + float2(size.x, -size.y));
                if (c > 1)
                {
                    return float4(0, 1, 0, 1);
                } else 
                {
                    return float4(0, 0, 1, 1);
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = detectBoundary(tex, i.uv, _MainTex_TexelSize);
                return col;
            }
            ENDCG
        }
    }
}
