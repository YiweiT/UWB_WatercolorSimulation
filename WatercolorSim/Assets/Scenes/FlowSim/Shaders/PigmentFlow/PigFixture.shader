Shader "PigMotion/PigFixture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _ColTex, _tex2, _tex3, _tex0, _tex1, _tex4;
    float drynessPara, deposite_base;

    float2 get_velocity(float2 uv)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 t0 = tex2D(_tex0, uv);
        fixed4 t1 = tex2D(_tex1, uv);

        float f1 = t0.r;
        float f2 = t0.g;
        float f3 = t0.b;
        float f4 = t0.a;

        float f5 = t1.r;
        float f6 = t1.g;
        float f7 = t1.b;
        float f8 = t1.a;

        return (f1*e1 + f2*e2 + f3*e3 + f4*e4 + f5*e5 + f6*e6 + f7*e7 + f8*e8) / rho_0;
    }

    fixed4 pigmentFixture (v2f_img i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 col = tex2D(_ColTex, i.uv);
        fixed4 t2 = tex2D(_tex2, i.uv);
        fixed4 t3 = tex2D(_tex3, i.uv);
        float h = tex2D(_tex4, i.uv).r;
        
        float2 v = get_velocity(i.uv);
        float rho = t2.b;
        float rho_p = t3.b;
        float ws = t3.a;

        // float ps = col.r;
        float pf = col.g;
        float px = col.b;
        float fixFactor = 0;
        float backrun = -rho * px * re_absorb * step(settlingSpeed, length(v));
// float backrun = 0;
        float wl = max(rho_p - rho, 0);
        
        fixFactor = (rho_p != 0) ? wl / rho_p : 0;
        
        
        // Granulation
        // by modulating the rate of pigment transfer from the flow layer to the fixture layer
        // if speed is less than the settling speed, granulation occurs
        deposite_base += (1 - smoothstep(0, granulThres, h)) * granularity * step(length(v), settlingSpeed);


        // fixFactor = max(fixFactor * (1 - smoothstep(0, drynessPara, rho)), deposite_base);      
        fixFactor = clamp(fixFactor * (1 - smoothstep(0, drynessPara, rho)), 0.05, deposite_base);
        // fixFactor = lerp(fixFactor * (1 - smoothstep(0, drynessPara, rho)), 0.05, deposite_base);
         fixFactor = lerp( 0.05, deposite_base, fixFactor * (1 - smoothstep(0, drynessPara, rho)));
        // fixFactor = 0.1;
        pf = pf - fixFactor * pf - backrun;
        px = px + fixFactor * pf + backrun;


        return float4(col.r, pf, px, col.a);
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
            #pragma fragment pigmentFixture
            ENDCG
        }
    }
}
