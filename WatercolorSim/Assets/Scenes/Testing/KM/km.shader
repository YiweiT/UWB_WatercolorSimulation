Shader "Debug/KM"
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

    sampler2D _r0, _t0, _noise;
    float4 _S, _a, _b;
    float _offset, _mul; // 0.05, 0.05
    


    void km(float3 S, float x, float3 a, float3 b, out float3 R, out float3 T)
    {
        float3 bSx = b * S * float3(x, x, x);
        float3 sinh_bSx = sinh(bSx);
        float3 c = a * sinh_bSx + b * cosh(bSx);
        R = sinh_bSx / c;
        T = b / c;
    }


    void Composite(float3 R0, float3 T0, float3 R1, float3 T1, out float3 R, out float3 T)
    {
        float3 temp = 1 / (1 - R0 * R1);
        R = R0 + T0 * T0 * R1 * temp;
        T = T0 * T1 * temp;
        
    }

    void mainFunc(float2 uv, out float3 R, out float3 T)
    {
        float x = _offset + _mul * tex2D(_noise, uv).r;
        float3 r1, t1;
        km(_S.xyz, x, _a.xyz, _b.xyz, R, T);
        // Composite(_r0, _t0, r1, t1, R, T);
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
    
    fixed4 frag (v2f i) : SV_Target
    {
        float3 r, t;
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
            #pragma fragment frag
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
