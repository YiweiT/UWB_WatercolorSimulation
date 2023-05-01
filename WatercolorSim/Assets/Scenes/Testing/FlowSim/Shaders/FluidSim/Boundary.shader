Shader "FlowSim/Boundary"
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

    sampler2D _tex2, _tex3, _tex4;
    float4 _tex2_TexelSize;

    // Check whether the given cell has enough amount of water (rho) that can flow
    int hasEnoughWater(sampler2D tex, float2 uv)
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        float rho = tex2D(tex, uv).b;
        return lerp(1, 0, step(rho, theta));
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
        return lerp(1, 0, step(c, 0));
    }

    fixed4 updateK_ws (v2f i) : SV_Target
    {
        // update ws (alpha), and K (green)
        float4 ref2 = tex2D(_tex2, i.uv);
        float rho = ref2.b;
        float wf = ref2.a;
        float h = tex2D(_tex4, i.uv).r;
        float4 val = tex2D(_tex3, i.uv);
        if (rho > 0)
        {
            val.g = h; // K = h
        } else {
            // if all neighbor cells' rho less than _Theta, K = 1
        //    if (detectBoundary(_tex2, i.uv, _tex2_TexelSize) > 0)
        //    {
        //     val.g = h; // k = h
        //    } else {
        //     val.g = 1; // boundary cell
        //    }
           val.g = lerp(h,  1, step(detectBoundary(_tex2, i.uv, _tex2_TexelSize), 0));
            // else: k = h
        }
        val.a = max(val.a - wf, 0); // update ws = max(ws - wf, 0)
        val.b = rho; // update rho'
        // val.r = val.a; // debugging
        
        return val;
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
            #pragma fragment updateK_ws
            ENDCG
        }
    }
}
