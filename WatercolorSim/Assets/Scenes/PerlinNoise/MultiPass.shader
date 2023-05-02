Shader "Custom/MultiPass"
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

    sampler2D _MainTex;

    fixed4 frag (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        
        col.rgb = lerp(float4(1, 0, 0, 1), float4(0, 0, 1, 1), i.uv.x);
        return col;
    }

    fixed4 frag1 (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_MainTex, i.uv);
        
        col.rgb = lerp(float4(0, 1, 0, 1), float4(0, 0, 1, 1), i.uv.x);
        return col;
    }

    ENDCG
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag1
            ENDCG
        }
    }
}
