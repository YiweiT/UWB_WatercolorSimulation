Shader "Custom/Paint_MRT"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PreviousState0("Texture", 2D) = "white" {}
        _PreviousState1("Texture", 2D) = "white" {}
        // _x ("_x", Float) = -1.0
        // _y ("_y", Float) = -1.0
        // _r ("Radius", Float) = 0.02
        // _PenCol ("PenColor", Color) = (0.0, 1.0, 0.0, 1.0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off 
        // ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_mrt

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

            struct fragOutput 
            {
                float4 col0: SV_TARGET0;
                float4 col1: SV_TARGET1;
            };

            sampler2D _PreviousState0;
            // sampler2D _PreviousState1;
            sampler2D _MainTex;
            int _hasNewInk;
            float4 _PenCol0;
            float4 _PenCol1;
            float _x, _y, _r;

            fragOutput frag_mrt(v2f i)
            {
                fragOutput output;
                output.col0 = tex2D(_PreviousState0, i.uv); 
                // output.col1 = tex2D(_PreviousState1, i.uv);
                if (_hasNewInk == 1) {
                    if (distance(i.uv, float2(_x, _y)) < _r) {
                        output.col0 = _PenCol0;
                        // output.col1 = _PenCol1;
                    }
                }
                output.col1 = 1 - output.col0;
                return output;   
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col =  tex2D(_PreviousState0, i.uv); // 
                if (_hasNewInk == 1) {
                    if (distance(i.uv, float2(_x, _y)) < _r) {
                        col = _PenCol0;
                    }
                }
                return col;                
            }


            ENDCG
        }
    }
}
