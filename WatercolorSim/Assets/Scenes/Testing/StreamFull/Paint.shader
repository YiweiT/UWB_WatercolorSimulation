Shader "StreamFull/Paint"
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

    // _tex01 set dynamically when calling paint function
    // _tex3 is rt[3]
    sampler2D _mask, _tex0, _tex1, _tex3;
    float _x, _y, _size, _val;

    fixed4 paint (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_mask, i.uv);
        // if (distance(i.uv, float2(_x, _y)) < _size)
        // {
        //     col += float4(_val, _val, _val, _val);
        // }
        col += float4(0.5, 0.5, 0.5, 0.5) * step(-1 * _size, -1 * distance(i.uv, float2(_x, _y)));
        return col;
    }

    fixed4 paint_0 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_tex0, i.uv);
        // if (distance(i.uv, float2(_x, _y)) < _size)
        // {
        //     col += float4(_val, _val, _val, _val);
        // }
        col += float4(_val, _val, _val, _val) * step(-1 * _size, -1 * distance(i.uv, float2(_x, _y)));
        return col;
    }

    
    fixed4 paint_1 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_tex1, i.uv);
        // if (distance(i.uv, float2(_x, _y)) < _size)
        // {
        col += float4(_val, _val, _val, _val) * step(-1 * _size, -1 * distance(i.uv, float2(_x, _y)));
        
        return col;
    }

    fixed4 paint_3 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_tex3, i.uv);
        col += float4(_val, 0, 0, 0) * step(-1 * _size, -1 * distance(i.uv, float2(_x, _y)));
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
            #pragma fragment paint_0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment paint_1
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment paint
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment paint_3
            ENDCG
        }
    }
}
