Shader "Custom/SimpleLerp"
{
    Properties
    {
        [NoScaleOffset] _MixboxLUT ("Mixbox LUT", 2D) = "white" {}
        _Color1 ("Color 1", Color) = (0, 0.129, 0.522, 1) // blue
        _Color2 ("Color 2", Color) = (0.988, 0.827, 0, 1) // yellow
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_lerp

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

            sampler2D _MixboxLUT;
            fixed4 _Color1;
            fixed4 _Color2;

            fixed4 frag_lerp (v2f i) : SV_Target
            {
                fixed4 col = lerp(_Color1, _Color2, i.uv.x);;
                // just invert the colors
                
                return col;
            }
            ENDCG
        }
    }
}
