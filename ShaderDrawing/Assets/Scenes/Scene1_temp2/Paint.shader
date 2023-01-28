Shader "Custom/Paint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _x ("_x", Float) = -1.0
        _y ("_y", Float) = -1.0
        _r ("Radius", Float) = 0.02
        _PenCol ("PenColor", Color) = (0.0, 0.0, 0.0, 1.0)
        
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
            float4 _PenCol;
            float _x, _y, _r;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (_x >= 0 && _y >= 0) {
                    if (distance(i.uv, float2(_x, _y)) < _r) {
                        col = _PenCol;
                    }
                }
                
                // just invert the colors
                
                return col;
            }
            ENDCG
        }
    }
}