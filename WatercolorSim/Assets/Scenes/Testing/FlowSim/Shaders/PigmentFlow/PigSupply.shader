Shader "WaterColorSim/PigSupply"
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

    sampler2D _ColTex, _tex2, _tex3;

    fixed4 pigmentSupply (v2f i) : SV_Target
    {
        fixed4 col = tex2D(_ColTex, i.uv);
        fixed4 t2 = tex2D(_tex2, i.uv);
        
        float rho = t2.b;
        float wf = t2.a * 10;

        float ps = col.r;
        float pf = col.g;
        float new_pf = pf;
        // ps -= pf;

        // rho = 1;
        if (rho > 0) {
            // new_pf = (pf * rho + ps * wf) / (rho + wf);
            
            // new_pf = (new_pf - pf);
            // ps -= new_pf;
            // pf += new_pf;

            ps -= wf * (ps - pf) / (rho + wf);
            pf += wf * (ps - pf) / (rho + wf);
        }
        // if (rho > 0) {
        //     // new_pf = (pf * rho + ps * wf) / (rho + wf);
        //     // pf = (pf * rho + ps * wf);
        //     ps -= ps * wf / (rho );
        //     // pf += pf - new_pf;
        //     pf += ps * wf / (rho );
        // }
        

        // ps -= pf;
        // pf = new_pf;
        // pf += ps * wf;
        // ps -= ps * wf;

        return float4(ps, pf, col.b, col.a);
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
            #pragma fragment pigmentSupply
            ENDCG
        }
    }
}
