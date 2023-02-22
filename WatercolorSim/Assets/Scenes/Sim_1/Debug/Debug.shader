Shader "Custom/Debug"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // _ShowOpt ("Display Option", int) = 0
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

    sampler2D _MainTex;
    // int _ShowOpt;

    fixed4 display_r (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        col = float4(col.r, col.r, col.r, 1);
        return col;
    }

    fixed4 display_g (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        col = float4(col.g, col.g, col.g, 1);
        return col;
    }

    fixed4 display_b (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        col = float4(col.b, col.b, col.b, 1);
        return col;
    }

    fixed4 display_a (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        col = float4(col.a, col.a, col.a, 1);
        return col;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment display_r
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment display_g
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment display_b
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment display_a
            ENDCG
        }
    }
}
