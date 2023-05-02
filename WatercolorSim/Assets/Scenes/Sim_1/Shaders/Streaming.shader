Shader "Sim/Streaming"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // _eps ("Evaporation Rate", Float) = 0.00005
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

    sampler2D _RefTex0, _RefTex1, _RefTex3;
    float4 _RefTex3_TexelSize, _RefTex0_TexelSize, _RefTex1_TexelSize;
    // float _eps

    float4 neighbor_lookUp1(sampler2D tex, float2 uv)
    {
        // uv = clamp(uv, float2(0, 0), float2(1, 1));
        uv = saturate(uv);
        return tex2D(tex, uv);
    }

    float streaming(sampler2D distTex, float4 distSize, float fi, float fo, float2 dir, float ki, float2 uv, float inComing)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        float ke = neighbor_lookUp1(_RefTex3, uv - float2(dir.x * _RefTex3_TexelSize.x, dir.y * _RefTex3_TexelSize.y)).g;
        float ka = 1.0; 
        if (ki != 1 && ke != 1)
        {
            ka = (ki + ke) / 2;
        }

        float new_fi = ka * fo + (1 - ka) * inComing;
        new_fi = max(new_fi - step(1.0, ka) * eps_b, 0);
        return new_fi;
    }

    fixed4 stream_straight (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        fixed4 col = tex2D(_RefTex0, i.uv);
        float ki = tex2D(_RefTex3, i.uv).g;
        float f1 = col.r;
        float f2 = col.g;
        float f3 = col.b;
        float f4 = col.a;

        float inComing1 = neighbor_lookUp1(_RefTex0, i.uv - float2(e1.x * _RefTex0_TexelSize.x, e1.y * _RefTex0_TexelSize.y)).r;
        float inComing2 = neighbor_lookUp1(_RefTex0, i.uv - float2(e2.x * _RefTex0_TexelSize.x, e2.y * _RefTex0_TexelSize.y)).g;
        float inComing3 = neighbor_lookUp1(_RefTex0, i.uv - float2(e3.x * _RefTex0_TexelSize.x, e3.y * _RefTex0_TexelSize.y)).b;
        float inComing4 = neighbor_lookUp1(_RefTex0, i.uv - float2(e4.x * _RefTex0_TexelSize.x, e4.y * _RefTex0_TexelSize.y)).a;
        
        // float2 e2 = float2(0, 1);
        col.r = streaming(_RefTex0, _RefTex0_TexelSize, f1, f3, e1, ki, i.uv, inComing1); // E
        col.g = streaming(_RefTex0, _RefTex0_TexelSize, f2, f4, e2, ki, i.uv, inComing2); // N
        col.b = streaming(_RefTex0, _RefTex0_TexelSize, f3, f1, e3, ki, i.uv, inComing3); // W
        col.a = streaming(_RefTex0, _RefTex0_TexelSize, f4, f2, e4, ki, i.uv, inComing4); // S
        
        // col.g = neighbor_loopUp(_RefTex0, i.uv, e2, _RefTex0_TexelSize);

        return col;
    }

    fixed4 stream_diagonal (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        fixed4 col = tex2D(_RefTex0, i.uv);
        float ki = tex2D(_RefTex3, i.uv).g;
        float f1 = col.r;
        float f2 = col.g;
        float f3 = col.b;
        float f4 = col.a;

        float inComing1 = neighbor_lookUp1(_RefTex0, i.uv - float2(e5.x * _RefTex0_TexelSize.x, e5.y * _RefTex0_TexelSize.y)).r;
        float inComing2 = neighbor_lookUp1(_RefTex0, i.uv - float2(e6.x * _RefTex0_TexelSize.x, e6.y * _RefTex0_TexelSize.y)).g;
        float inComing3 = neighbor_lookUp1(_RefTex0, i.uv - float2(e7.x * _RefTex0_TexelSize.x, e7.y * _RefTex0_TexelSize.y)).b;
        float inComing4 = neighbor_lookUp1(_RefTex0, i.uv - float2(e8.x * _RefTex0_TexelSize.x, e8.y * _RefTex0_TexelSize.y)).a;
        
        // float2 e2 = float2(0, 1);
        col.r = streaming(_RefTex0, _RefTex0_TexelSize, f1, f3, e5, ki, i.uv, inComing1); // E
        col.g = streaming(_RefTex0, _RefTex0_TexelSize, f2, f4, e6, ki, i.uv, inComing2); // N
        col.b = streaming(_RefTex0, _RefTex0_TexelSize, f3, f1, e7, ki, i.uv, inComing3); // W
        col.a = streaming(_RefTex0, _RefTex0_TexelSize, f4, f2, e8, ki, i.uv, inComing4); // S
        
        // col.g = neighbor_loopUp(_RefTex0, i.uv, e2, _RefTex0_TexelSize);

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
            #pragma fragment stream_straight
            ENDCG
        }
    
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment stream_diagonal
            ENDCG
        }
    }
}
