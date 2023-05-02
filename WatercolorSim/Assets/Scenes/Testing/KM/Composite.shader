Shader "Debug/Comp"
{
    Properties
    {
        _R1 ("Texture", 2D) = "black" {}
        _T1 ("Texture", 2D) = "black" {}
        _R0 ("Texture", 2D) = "black" {}
        _T0 ("Texture", 2D) = "black" {}
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

    sampler2D _R0, _T0, _R1, _T1;


    void Composite(float3 R0, float3 T0, float3 R1, float3 T1, out float3 R, out float3 T)
    {
        float3 temp = 1 / (1 - R0 * R1);
        R = R0 + T0 * T0 * R1 * temp;
        T = T0 * T1 * temp;
        
    }
    
    float sum(float3 n)
    {
        return n.x + n.y + n.z;
    }

    void mainFunc(float2 uv, out float3 R, out float3 T)
    {
        float3 r0 = tex2D(_R0, uv).rgb;
        float3 t0 = tex2D(_T0, uv).rgb;
        float3 r1 = tex2D(_R1, uv).rgb;
        float3 t1 = tex2D(_T1, uv).rgb;
        // if (sum(r1) > 0 && sum(t1) > 0){
        //     Composite(r0, t0, r1, t1, R, T);
        // } else {
        //     R = r0;
        //     T = t0;
        // }
        Composite(r0, t0, r1, t1, R, T);

        
    }

    fixed4 Comp_R (v2f i) : SV_Target
    {
        float3 R, T;
        mainFunc(i.uv, R, T);
        return float4(R.xyz, 1);
    }

    fixed4 Comp_T (v2f i) : SV_Target
    {
        float3 R, T;
        mainFunc(i.uv, R, T);
        return float4(T.xyz, 1);
    }
    
    fixed4 Comp_C (v2f i) : SV_Target
    {
        // float x = _offset + _mul * tex2D(_noise, i.uv).r;
        // float3 bSx = _b.xyz * _S.xyz * float3(x, x, x);
        // float3 sinh_bSx = sinh(bSx);
        // float3 c = _a.xyz * sinh_bSx + _b.xyz * cosh(bSx);
        // float3 r = sinh_bSx / c;
        // float3 t = _b.xyz / c;
        float3 r, t;
        // km(_S.xyz, x, _a.xyz, _b.xyz, r, t);
        mainFunc(i.uv, r, t);
        return float4((r+t).xyz , 1);
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
            #pragma fragment Comp_C
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment Comp_R
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment Comp_T
            ENDCG
        }
    }
}
