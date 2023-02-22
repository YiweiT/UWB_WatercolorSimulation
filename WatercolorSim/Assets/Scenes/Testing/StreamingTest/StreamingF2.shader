Shader "Custom/StreamingF2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _eps ("Evaporation Rate", Float) = 0.00005
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

    sampler2D _MainTex, _RefTex0, _RefTex3;
    float4 _RefTex3_TexelSize, _RefTex0_TexelSize;
    float _eps;

    float4 findVal(sampler2D tex, float2 uv, float2 dir, float4 size)
    {
        return tex2D(tex, uv - float2(dir.x * size.x, dir.y * size.y));
    }

    float streaming_r(float fi, float fo, float2 dir, float ki, float2 uv)
    {
        float ke = findVal(_RefTex3, uv, dir, _RefTex3_TexelSize).g;
        float ka = 1.0;
        
        if (ki != 1 && ke != 1)
        {
            ka = (ki + ke) / 2;
        }
        float new_fi = ka * fo + (1 - ka) * findVal(_RefTex0, uv, dir, _RefTex0_TexelSize).r;
        
        new_fi = max(new_fi - step(1.0, ka) * _eps, 0);
        return new_fi;        
    }

    float streaming_g(float fi, float fo, float2 dir, float ki, float2 uv)
    {
        float ke = findVal(_RefTex3, uv, dir, _RefTex3_TexelSize).g;
        float ka = 1.0;
        
        if (ki != 1 && ke != 1)
        {
            ka = (ki + ke) / 2;
        }
        float new_fi = ka * fo + (1 - ka) * findVal(_RefTex0, uv, dir, _RefTex0_TexelSize).g;
        
        new_fi = max(new_fi - step(1.0, ka) * _eps, 0);
        return new_fi;        
    }

    float streaming_b(float fi, float fo, float2 dir, float ki, float2 uv)
    {
        float ke = findVal(_RefTex3, uv, dir, _RefTex3_TexelSize).g;
        float ka = 1.0;
        
        if (ki != 1 && ke != 1)
        {
            ka = (ki + ke) / 2;
        }
        float new_fi = ka * fo + (1 - ka) * findVal(_RefTex0, uv, dir, _RefTex0_TexelSize).b;
        
        new_fi = max(new_fi - step(1.0, ka) * _eps, 0);
        return new_fi;        
    }

    float streaming_a(float fi, float fo, float2 dir, float ki, float2 uv)
    {
        float ke = findVal(_RefTex3, uv, dir, _RefTex3_TexelSize).g;
        float ka = 1.0;
        
        if (ki != 1 && ke != 1)
        {
            ka = (ki + ke) / 2;
        }
        float new_fi = ka * fo + (1 - ka) * findVal(_RefTex0, uv, dir, _RefTex0_TexelSize).a;
        
        new_fi = max(new_fi - step(1.0, ka) * _eps, 0);
        return new_fi;        
    }



    fixed4 streamF2 (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Scripts/Test/Shaders/Includes/SimulationPara.cginc"
        fixed4 col = tex2D(_RefTex0, i.uv);
        float ki = tex2D(_RefTex3, i.uv).g;
        float f1 = col.r;
        float f2 = col.g;
        float f3 = col.b;
        float f4 = col.a;
        // float2 e2 = float2(0, 1);
        col.r = streaming_r(f1, f3, e1, ki, i.uv); // E
        col.g = streaming_g(f2, f4, e2, ki, i.uv); // N
        col.b = streaming_b(f3, f1, e3, ki, i.uv); // W
        col.a = streaming_a(f4, f2, e4, ki, i.uv); // S
        
        // col.g = findVal(_RefTex0, i.uv, e2, _RefTex0_TexelSize);

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
            #pragma fragment streamF2


            ENDCG
        }
    }
}
