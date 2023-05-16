Shader "PigMotion/PigSupply"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    CGINCLUDE
    #include "UnityCG.cginc"

    sampler2D _ColTex, _tex2;

    fixed4 pigmentSupply (v2f_img i) : SV_Target
    {
        fixed4 col = tex2D(_ColTex, i.uv);
        fixed4 t2 = tex2D(_tex2, i.uv);
        
        float rho = t2.b;
        float wf = t2.a * 1;

        float ps = col.r;
        float pf = col.g;
        float new_pf = pf;

        if (rho + wf > 0) {
            ps -= wf * (ps - pf) / (rho + wf);
            pf += wf * (ps - pf) / (rho + wf);
        }

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
            #pragma vertex vert_img
            #pragma fragment pigmentSupply
            ENDCG
        }
    }
}
