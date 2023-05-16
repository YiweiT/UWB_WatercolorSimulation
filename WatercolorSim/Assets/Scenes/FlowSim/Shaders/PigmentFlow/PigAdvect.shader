Shader "PigMotion/PigAdvection"
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

    sampler2D _ColTex, _tex2, _tex3, _tex0, _tex1;
    float4 _ColTex_TexelSize, _tex0_TexelSize, _tex1_TexelSize;
    float sigma;

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

        return (f1*e1 + f2*e2 + f3*e3 + f4*e4 + f5*e5 + f6*e6 + f7*e7 + f8*e8) / rho_0 * spdMul;
    }
    float get_pf(sampler2D tex, float2 uv, float2 dir, float4 size)
    {
        return tex2D(tex, uv - float2(dir.x * size.x, dir.y * size.y)).g;
    }
    
    float newlyWetPf(float2 uv)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 t0 = tex2D(_tex0, uv);
        fixed4 t1 = tex2D(_tex1, uv);
        float mul = 2;

        float f1 = t0.r * get_pf(_ColTex, uv, e1, _ColTex_TexelSize) * mul;
        float f2 = t0.g * get_pf(_ColTex, uv, e2, _ColTex_TexelSize) * mul;
        float f3 = t0.b * get_pf(_ColTex, uv, e3, _ColTex_TexelSize) * mul;
        float f4 = t0.a * get_pf(_ColTex, uv, e4, _ColTex_TexelSize) * mul;

        float f5 = t1.r * get_pf(_ColTex, uv, e5, _ColTex_TexelSize) * mul;
        float f6 = t1.g * get_pf(_ColTex, uv, e6, _ColTex_TexelSize) * mul;
        float f7 = t1.b * get_pf(_ColTex, uv, e7, _ColTex_TexelSize) * mul;
        float f8 = t1.a * get_pf(_ColTex, uv, e8, _ColTex_TexelSize) * mul;      

        return (f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8);

    }

    float simulateHindrance(float pf, float new_pf, float2 v, float k)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        float hindR = lerp(1, k, smoothstep(0, sigma, length(v)));
        return lerp(new_pf, pf, hindR);
    }
    

    fixed4 pigmentAdvect (v2f i) : SV_Target
    {

        fixed4 col = tex2D(_ColTex, i.uv);

        fixed4 t2 = tex2D(_tex2, i.uv);
        fixed4 t3 = tex2D(_tex3, i.uv);
        
        float2 v = get_velocity(i.uv);
        float rho = t2.b;
        float wf = t2.a;

        float ps = col.r;
        float pf = col.g;

        float rho_p = t3.b;
        float k = t3.g;

        float new_pf = 0;
        
        if (rho_p > 0) {
            float2 offset = float2(clamp(v.x, -_ColTex_TexelSize.x, _ColTex_TexelSize.x), clamp(v.y, -_ColTex_TexelSize.y, _ColTex_TexelSize.y));
            new_pf = tex2D(_ColTex, i.uv - offset).g * step(0, rho);
            // new_pf = get_pf(_ColTex, i.uv, offset, _ColTex_TexelSize) * step(0, rho);
        } 
        else{
            // new_pf = (rho > 0) ? newlyWetPf(i.uv) / rho : 0;
            new_pf = newlyWetPf(i.uv) / rho;
        }
       
        new_pf = simulateHindrance(pf, new_pf, v, k);
        // ps = saturate(ps - new_pf);
        return float4(ps, new_pf , col.b, col.a);
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
            #pragma fragment pigmentAdvect
            ENDCG
        }
    }
}
