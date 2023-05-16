Shader "FlowSim/Stream"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _tex0, _tex1, _tex3;
    float4 _tex0_TexelSize, _tex1_TexelSize, _tex3_TexelSize;
    
    float4 neighbor_lookUp(sampler2D tex, float2 uv)
    {
        uv = saturate(uv);
        return tex2D(tex, uv);
    }

    float streaming(sampler2D distTex, float4 distSize, float fo, float2 dir, float ki, float2 uv, float inComing)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        float ke = neighbor_lookUp(_tex3, uv - float2(dir.x * _tex3_TexelSize.x, dir.y * _tex3_TexelSize.y)).g;
        float ka = 1.0; 
        if (ki != 1 && ke != 1)
        {
            ka = (ki + ke) / 2;
        }

        // float new_fi = inComing; // pure flow sim
        float new_fi = ka * fo + (1 - ka) * inComing;
        new_fi = max(new_fi - step(-1.0, -ka) * eps_b, 0); // evaporation at boundary
        return new_fi;
    }

    fixed4 stream_straight (v2f_img i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        fixed4 distFuncs = tex2D(_tex0, i.uv);
        float ki = tex2D(_tex3, i.uv).g;
        float f1 = distFuncs.r;
        float f2 = distFuncs.g;
        float f3 = distFuncs.b;
        float f4 = distFuncs.a;

        float inComing1 = neighbor_lookUp(_tex0, i.uv - float2(e1.x * _tex0_TexelSize.x, e1.y * _tex0_TexelSize.y)).r;
        float inComing2 = neighbor_lookUp(_tex0, i.uv - float2(e2.x * _tex0_TexelSize.x, e2.y * _tex0_TexelSize.y)).g;
        float inComing3 = neighbor_lookUp(_tex0, i.uv - float2(e3.x * _tex0_TexelSize.x, e3.y * _tex0_TexelSize.y)).b;
        float inComing4 = neighbor_lookUp(_tex0, i.uv - float2(e4.x * _tex0_TexelSize.x, e4.y * _tex0_TexelSize.y)).a;
        
        distFuncs.r = streaming(_tex0, _tex0_TexelSize, f3, e1, ki, i.uv, inComing1); // E
        distFuncs.g = streaming(_tex0, _tex0_TexelSize, f4, e2, ki, i.uv, inComing2); // N
        distFuncs.b = streaming(_tex0, _tex0_TexelSize, f1, e3, ki, i.uv, inComing3); // W
        distFuncs.a = streaming(_tex0, _tex0_TexelSize, f2, e4, ki, i.uv, inComing4); // S
        
        return distFuncs;
    }

    fixed4 stream_diagonal (v2f_img i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        fixed4 distFuncs = tex2D(_tex1, i.uv);
        float ki = tex2D(_tex3, i.uv).g;
        float f5 = distFuncs.r;
        float f6 = distFuncs.g;
        float f7 = distFuncs.b;
        float f8 = distFuncs.a;

        float inComing5 = neighbor_lookUp(_tex1, i.uv - float2(e5.x * _tex1_TexelSize.x, e5.y * _tex1_TexelSize.y)).r;
        float inComing6 = neighbor_lookUp(_tex1, i.uv - float2(e6.x * _tex1_TexelSize.x, e6.y * _tex1_TexelSize.y)).g;
        float inComing7 = neighbor_lookUp(_tex1, i.uv - float2(e7.x * _tex1_TexelSize.x, e7.y * _tex1_TexelSize.y)).b;
        float inComing8 = neighbor_lookUp(_tex1, i.uv - float2(e8.x * _tex1_TexelSize.x, e8.y * _tex1_TexelSize.y)).a;
        
        distFuncs.r = streaming(_tex1, _tex1_TexelSize, f7, e5, ki, i.uv, inComing5); // NE
        distFuncs.g = streaming(_tex1, _tex1_TexelSize, f8, e6, ki, i.uv, inComing6); // NW
        distFuncs.b = streaming(_tex1, _tex1_TexelSize, f5, e7, ki, i.uv, inComing7); // SW
        distFuncs.a = streaming(_tex1, _tex1_TexelSize, f6, e8, ki, i.uv, inComing8); // SE
        
        return distFuncs;
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
            #pragma vertex vert_img
            #pragma fragment stream_straight
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment stream_diagonal
            ENDCG
        }
    }
}
