Shader "Custom/BoundaryCheck"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "" {}
        _RefTex ("Reference Texture", 2D) = "" {}
        _Theta ("_Theta", Float) = 0.3
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

    sampler2D _MainTex, _RefTex;
    float _Theta;
    float4 _RefTex_TexelSize;


    int canFlow(sampler2D tex, float2 uv)
    {
        float waterAmount = tex2D(tex, uv).r;
        if (waterAmount < _Theta )
        {
            return 0;
        } else {
            return 1;
        }
    }
    


    float4 detectBoundary (sampler2D tex, float2 uv, float4 size)
    {
        
        // float c = canFlow(tex, uv + float2(-size.x, size.y)) 
        //         + canFlow(tex, uv + float2(0, size.y)) 
        //         + canFlow(tex, uv + float2(size.x, size.y)) 
        //         + canFlow(tex, uv + float2(-size.x, 0)) 
        //         + canFlow(tex, uv + float2(size.x, 0))
        //         + canFlow(tex, uv + float2(-size.x, -size.y)) 
        //         + canFlow(tex, uv + float2(0, -size.y)) 
        //         + canFlow(tex, uv + float2(size.x, -size.y));

        float4 c = tex2D(tex, uv + float2(-size.x, size.y)) 
                + tex2D(tex, uv + float2(0, size.y)) 
                + tex2D(tex, uv + float2(size.x, size.y)) 
                + tex2D(tex, uv + float2(-size.x, 0)) 
                + tex2D(tex, uv + float2(size.x, 0))
                + tex2D(tex, uv + float2(-size.x, -size.y)) 
                + tex2D(tex, uv + float2(0, -size.y)) 
                + tex2D(tex, uv + float2(size.x, -size.y));
        
        // if (c <= 0) 
        // {
        //     // some cell(s) with enough water (> _Theta) to flow
        //     // not a boundary cell
        //     return float4(0, 0, 1, 1); // showing as the original color
        // } else 
        // if (c <= 4){
        //     // all neighbor cells do not have enough water to flow
        //     // boundary cell
        //     return float4(1, 0, 0, 1); // showing as blue
        // } else {
        //     return float4(0, 1, 0, 1);
        // }
        return c ;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_RefTex, i.uv);
        if (col.r < 0.001) {
            col = detectBoundary(_RefTex, i.uv, _RefTex_TexelSize);
        } else {
            col = float4(0, 1, 0, 1);
        }
        // if (col.r > 0) {
        //     col = float4(0, 0, 1, 1);
        // }
        // col.r += 1;
        return col;
    }
    ENDCG
    SubShader
    {
        // No culling or depth
        // Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
