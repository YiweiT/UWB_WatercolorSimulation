Shader "Test/PaintA"
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

    sampler2D _MainTex, _PrevTex0, _PrevTex1, _PrevTex3;
    float _x, _y, _w, _h, _val;

    fixed4 drawTex0 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_PrevTex0, i.uv);
        if (distance(i.uv, float2(_x, _y)) < _w) {
            col += float4(_val/9, _val/9, _val/9, _val/9);
        }
        return col;
    }

    fixed4 drawTex1 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_PrevTex1, i.uv);
        if (distance(i.uv, float2(_x, _y)) < _w) {
            col += float4(_val/9, _val/9, _val/9, _val/9);
        }
        return col;
    }

    fixed4 drawTex3 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_PrevTex3, i.uv);
        if (distance(i.uv, float2(_x, _y)) < _w) {
            col.r += _val/9;
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
            #pragma fragment drawTex0
            ENDCG
        }

        Pass
        {   
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment drawTex1
            ENDCG
        }
        Pass
        {   
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment drawTex3
            ENDCG
        }
    }
}
