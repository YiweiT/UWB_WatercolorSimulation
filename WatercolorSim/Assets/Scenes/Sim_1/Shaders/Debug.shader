Shader "Debug/Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
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
            int _option;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_RefTex, i.uv);
                // option:  0   1   2   3
                // display: r   g   b   a   
                if (_option == 0) 
                {
                    return float4(col.r, col.r, col.r, 1);
                } else if (_option == 1)
                {
                    return float4(col.g, col.g, col.g, 1);
                } else if (_option == 2)
                {
                    return float4(col.b, col.b, col.b, 1);
                } else if (_option == 3) 
                {
                    return float4(col.a, col.a, col.a, 1);
                } else {
                    return col;
                }
                
            }
            ENDCG
        }
    }
}
