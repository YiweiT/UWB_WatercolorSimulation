Shader "Composite/KM"
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

    sampler2D _ColTex, _R0, _T0;
    float4 _S, _a, _b;

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
        float4 pigments = tex2D(_ColTex, uv);
        float x = pigments.x + pigments.y + pigments.z;
        float3 R0 = tex2D(_R0, uv).rgb;
        float3 T0 = tex2D(_T0, uv).rgb;
        float3 R1, T1;

        km(_S.xyz, x, _a.xyz, _b.xyz, R1, T1);
        Composite(R0, T0, R1, T1, R, T);
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

    fixed4 Comp (v2f i) : SV_Target
    {
        float3 R, T;

        mainFunc(i.uv, R, T);

        float3 C = R + T;
        return float4(C.xyz, 1);
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
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment Comp
            ENDCG
        }
    }
}
