Shader "Composite/Comp"
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

    sampler2D  _R, _T;
    // float3 _S, _a, _b;

    fixed4 Comp (v2f i) : SV_Target
    {
        float3 r = tex2D(_R, i.uv).rgb;
        float3 t = tex2D(_T, i.uv).rgb;

        float3 c = r + t;
        return float4(c.xyz, 1);
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
            #pragma fragment Comp
            ENDCG
        }
    }
}
