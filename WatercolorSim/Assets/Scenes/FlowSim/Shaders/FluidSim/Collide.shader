Shader "StreamFull/Collision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _RefTex0, _RefTex1, _RefTex2,  _RefTex3;
    float omega;

    float2 get_velocity(float2 uv)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 t0 = tex2D(_RefTex0, uv);
        fixed4 t1 = tex2D(_RefTex1, uv);

        float f1 = t0.r;
        float f2 = t0.g;
        float f3 = t0.b;
        float f4 = t0.a;

        float f5 = t1.r;
        float f6 = t1.g;
        float f7 = t1.b;
        float f8 = t1.a;

        return (f1*e1 + f2*e2 + f3*e3 + f4*e4 + f5*e5 + f6*e6 + f7*e7 + f8*e8) / rho_0 * spdMul;
    }
    float newDistributeFunction(float fi, float feq)
    {
        // #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        return (1 - omega) * fi + omega * feq;
    }

    float eqmFunction(float2 dir, float weight, float rho, float2 velocity, float alpha)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        
        return weight * rho * (1 + alpha * (c_squ * dot(dir, velocity) + 0.5 * pow(dot(dir, velocity)* c_squ , 2) - 0.5 * c_squ * dot(velocity, velocity)));
    }
    fixed4 colliding_f14 (v2f_img i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        
        fixed4 origF = tex2D(_RefTex0, i.uv);
        fixed4 col = tex2D(_RefTex2, i.uv);
        float2 velocity = get_velocity(i.uv);
        float rho = col.b;
        float alpha = smoothstep(0, lambda, rho);
        float new_f1 = eqmFunction(e1, w2, rho, velocity, alpha);
        float new_f2 = eqmFunction(e2, w2, rho, velocity, alpha);
        float new_f3 = eqmFunction(e3, w2, rho, velocity, alpha);
        float new_f4 = eqmFunction(e4, w2, rho, velocity, alpha);

        new_f1 = newDistributeFunction(origF.r, new_f1);
        new_f2 = newDistributeFunction(origF.g, new_f2);
        new_f3 = newDistributeFunction(origF.b, new_f3);
        new_f4 = newDistributeFunction(origF.a, new_f4);

        new_f1 = saturate(new_f1);
        new_f2 = saturate(new_f2);
        new_f3 = saturate(new_f3);
        new_f4 = saturate(new_f4);


        return float4(new_f1, new_f2, new_f3, new_f4);
    }

    fixed4 colliding_f58 (v2f_img i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
 
        fixed4 origF = tex2D(_RefTex1, i.uv);
        fixed4 col = tex2D(_RefTex2, i.uv);
        float2 velocity = get_velocity(i.uv);
        float rho = col.b;
        float alpha = smoothstep(0, lambda, rho);
        float new_f5 = eqmFunction(e5, w3, rho, velocity, alpha);
        float new_f6 = eqmFunction(e6, w3, rho, velocity, alpha);
        float new_f7 = eqmFunction(e7, w3, rho, velocity, alpha);
        float new_f8 = eqmFunction(e8, w3, rho, velocity, alpha);

        new_f5 = newDistributeFunction(origF.r, new_f5);
        new_f6 = newDistributeFunction(origF.g, new_f6);
        new_f7 = newDistributeFunction(origF.b, new_f7);
        new_f8 = newDistributeFunction(origF.a, new_f8);

        new_f5 = saturate(new_f5);
        new_f6 = saturate(new_f6);
        new_f7 = saturate(new_f7);
        new_f8 = saturate(new_f8);
        return float4(new_f5, new_f6, new_f7, new_f8);
    }

    fixed4 colliding_f0 (v2f_img i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 col = tex2D(_RefTex2, i.uv);
        fixed4 output = tex2D(_RefTex3, i.uv);
        float2 velocity = float2(col.r, col.g); 
        float rho = col.b;
        float alpha = smoothstep(0, lambda, rho);
        float new_f0 = eqmFunction(e0, w1, rho, velocity, alpha);
        
        new_f0 = newDistributeFunction(output.r, new_f0);
        // output.r = new_f0;
        new_f0 = saturate(new_f0);

        return float4(new_f0, output.g, output.b, output.a);
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
            #pragma fragment colliding_f14
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment colliding_f58
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment colliding_f0
            ENDCG
        }
    }
}