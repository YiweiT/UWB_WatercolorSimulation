Shader "Hidden/Paint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PrevTex ("Texture", 2D) = "" {}
    }
    CGINCLUDE
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

    sampler2D _MainTex, _PrevTex;
    Float _x, _y, _brushSize;
    float4 _col;
    int _hasNewInk;

    fixed4 paint (v2f i) : SV_Target
    {
        fixed4 col =  tex2D(_PrevTex, i.uv); 
            if (_hasNewInk == 1) {
                if (distance(i.uv, float2(_x, _y)) < _brushSize) {
                    col = float4(0.3, 0.3, 0.3, 1);
                }
            }
            return col;
    }
    ENDCG
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment paint


            ENDCG
        }
    }
}
