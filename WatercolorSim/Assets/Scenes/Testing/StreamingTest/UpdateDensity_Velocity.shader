Shader "Test/UpdateDensity_Velocity"
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

    sampler2D _RefTex0, _RefTex1, _RefTex2;

    fixed4 frag (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        
        fixed4 col0 = tex2D(_RefTex0, i.uv); // f1 - f4
        fixed4 col1 = tex2D(_RefTex1, i.uv); // f5 - f8
        fixed4 col2 = tex2D(_RefTex2, i.uv); // f0 = r
        float f0 = col2.r;
        float f1 = col0.r;
        float f2 = col0.g;
        float f3 = col0.b;
        float f4 = col0.a;
        float f5 = col1.r;
        float f6 = col1.g;
        float f7 = col1.b;
        float f8 = col1.a;  

        float rho = f0 + f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8;
        float2 velocity = (1 / rho) * (f1*e1 + f2*e2 + f3*e3 + f4*e4 + f5*e5 + f6*e6 + f7*e7 + f8*e8);
        return float4(velocity.x, velocity.y, rho, 1);
    }
    ENDCG
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
