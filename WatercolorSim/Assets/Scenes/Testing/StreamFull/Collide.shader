Shader "StreamFull/Collision"
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

    sampler2D _RefTex2,  _RefTex3;

    fixed4 colliding_f14 (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        
        fixed4 col = tex2D(_RefTex2, i.uv);
        float2 velocity = col.rg;
        float rho = col.b;
        float alpha = smoothstep(0, lambda, rho);
        float new_f1 = w2 * (rho + alpha * (3 * dot(e1, velocity) + 9/2 * pow(dot(e1, velocity), 2) - 3/2 * dot(velocity, velocity)));
        float new_f2 = w2 * (rho + alpha * (3 * dot(e2, velocity) + 9/2 * pow(dot(e2, velocity), 2) - 3/2 * dot(velocity, velocity)));
        float new_f3 = w2 * (rho + alpha * (3 * dot(e3, velocity) + 9/2 * pow(dot(e3, velocity), 2) - 3/2 * dot(velocity, velocity)));
        float new_f4 = w2 * (rho + alpha * (3 * dot(e4, velocity) + 9/2 * pow(dot(e4, velocity), 2) - 3/2 * dot(velocity, velocity)));
        return float4(new_f1, new_f2, new_f3, new_f4);
    }

    fixed4 colliding_f58 (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
 
        fixed4 col = tex2D(_RefTex2, i.uv);
        float2 velocity = col.rg;
        float rho = col.b;
        float alpha = smoothstep(0, lambda, rho);
        float new_f5 = w3 * (rho + alpha * (3 * dot(e5, velocity) + 9/2 * pow(dot(e5, velocity), 2) - 3/2 * dot(velocity, velocity)));
        float new_f6 = w3 * (rho + alpha * (3 * dot(e6, velocity) + 9/2 * pow(dot(e6, velocity), 2) - 3/2 * dot(velocity, velocity)));
        float new_f7 = w3 * (rho + alpha * (3 * dot(e7, velocity) + 9/2 * pow(dot(e7, velocity), 2) - 3/2 * dot(velocity, velocity)));
        float new_f8 = w3 * (rho + alpha * (3 * dot(e8, velocity) + 9/2 * pow(dot(e8, velocity), 2) - 3/2 * dot(velocity, velocity)));
        return float4(new_f5, new_f6, new_f7, new_f8);
    }

    fixed4 colliding_f0 (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 col = tex2D(_RefTex2, i.uv);
        fixed4 output = tex2D(_RefTex3, i.uv);
        float2 velocity = col.rg;
        float rho = col.b;
        float alpha = smoothstep(0, lambda, rho);
        output.r = w1 * (rho + alpha * (3 * dot(e0, velocity) + 9/2 * pow(dot(e0, velocity), 2) - 3/2 * dot(velocity, velocity)));
        
        return output;
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
            #pragma fragment colliding_f14
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment colliding_f58
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment colliding_f0
            ENDCG
        }
    }
}