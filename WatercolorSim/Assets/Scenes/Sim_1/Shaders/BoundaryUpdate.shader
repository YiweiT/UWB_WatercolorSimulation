Shader "Sim/BoundaryUpdate"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        // _Theta ("Theta", Range(0.05, 0.2)) = 0.1
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

    sampler2D _MainTex;
    sampler2D _RefTex2, _RefTex3, _RefTex4;
    float4 _RefTex2_TexelSize;
    // float _Theta;

    // Check whether the given cell has enough amount of water (rho) that can flow
    int hasEnoughWater(sampler2D tex, float2 uv)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        float rho = tex2D(tex, uv).g;
        if (rho < theta)
        {
            return 0;
        } else {
            return 1;
        }
    }

    int detectBoundary(sampler2D tex, float2 uv, float4 size)
    {
        // check all 8 neighbors with hasEnoughWater function. 
        // if the sum is greater than 0, --> at least one cell contains enough water to flow out, it becomes flow cell 1
        // else --> boundary cell 0
        int c = hasEnoughWater(tex, uv + float2(-size.x, size.y))   // NW
              + hasEnoughWater(tex, uv + float2(0, size.y))         // N
              + hasEnoughWater(tex, uv + float2(size.x, size.y))    // NE
              + hasEnoughWater(tex, uv + float2(-size.x, 0))        // W
              + hasEnoughWater(tex, uv + float2(size.x, 0))         // E
              + hasEnoughWater(tex, uv + float2(-size.x, -size.y))  // SW
              + hasEnoughWater(tex, uv + float2(0, -size.y))        // S
              + hasEnoughWater(tex, uv + float2(size.x, -size.y));  // SE
        if (c > 0)
        {
            return 1;
        } else {
            return 0;
        }
    }

    fixed4 update_ws_k (v2f i) : SV_Target
    {
        // update ws (alpha), and K (green)
        float4 ref2 = tex2D(_RefTex2, i.uv);
        float rho = ref2.b;
        float4 val = tex2D(_RefTex3, i.uv);
        if (rho > 0)
        {
            val.g = tex2D(_RefTex4, i.uv).r; // K = h
        } else {
            // if all neighbor cells' rho less than _Theta, K = 1
           if (detectBoundary(_RefTex2, i.uv, _RefTex2_TexelSize) > 0)
           {
            val.g = tex2D(_RefTex4, i.uv).r; // k = h
           } else {
            val.g = 1; // boundary cell
           }
            // else: k = h
        }
        val.a = max(val.a - ref2.a, 0); // update ws
        val.b = rho; // update rho'
        // val.r = val.a; // debugging
        
        return val;
    }



    ENDCG
    SubShader
    {
        // No culling or depth
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment update_ws_k
            ENDCG
        }       
    }
}
