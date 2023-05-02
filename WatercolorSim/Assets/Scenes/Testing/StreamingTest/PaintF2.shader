Shader "Custom/PaintF2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
    float _x, _y, _w, _h, _val;

    fixed4 drawCircle (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_PrevTex, i.uv);
        if (distance(i.uv, float2(_x, _y)) < _w) {
            col += float4(_val/9, _val/9, _val/9, _val/9);
        }
        return col;
    }

    fixed4 drawRect (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_PrevTex, i.uv);
        if (i.uv.x - _x >= 0 && i.uv.y - _y >= 0) {
            if (i.uv.x - _x < _w && i.uv.y - _y < _h) {
                col += float4(_val/9, _val/9, _val/9, _val/9);
            }
        }
        return col;
    }
    ENDCG
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment drawCircle
            ENDCG
        }

        Pass
        {   
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment drawRect
            ENDCG
        }
    }
}