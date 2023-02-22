Shader "Hidden/Streaming"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "Includes/SimulationPara.cginc"

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

    sampler2D _RefTex0, _RefTex1, _RefTex3;
    float4 _RefTex3_TexelSize;

    float findLinkedK(sampler2D tex, float2 uv, float2 dir, float4 size)
    {
        return tex2D(tex, uv - float2(dir.x * size.x, dir.y * size.y)).g;
    }
    fixed4 stream1 (v2f i) : SV_Target
    {
        // f1 - f4
        float4 fs = tex2D(_RefTex0, i.uv);
        float ki = tex2D(_RefTex3, i.uv).g;
        float f1 = fs.r;
        float f2 = fs.g;
        float f3 = fs.b;
        float f4 = fs.a;
        fixed4 outF;

        float k1 = findLinkedK(_RefTex3, i.uv, e1, _RefTex3_TexelSize);
        float k2 = findLinkedK(_RefTex3, i.uv, e2, _RefTex3_TexelSize);
        float k3 = findLinkedK(_RefTex3, i.uv, e3, _RefTex3_TexelSize);
        float k4 = findLinkedK(_RefTex3, i.uv, e4, _RefTex3_TexelSize);

        float k;
        // f1
        if (ki == 1 || k1 == 1)
        {
            k = 1
        } else {
            k = (ki + k1) / 2;
        }

        // f2


        // f3


        // f4

        float f1_ = 

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
            #pragma fragment frag
            ENDCG
        }
    }
}
