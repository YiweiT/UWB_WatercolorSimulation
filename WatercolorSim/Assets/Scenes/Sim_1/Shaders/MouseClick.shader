Shader "Sim/MouseClick"
{
    Properties
    {
        _PrevTex ("Previous Texture", 2D) = "white" {}
        _tex1 ("Reference Texture 1", 2D) = "" {} // vx, vy, rho, wf -- using rho
        _tex2 ("Reference Texture 2", 2D) = "" {} // Ps, Pf, Px -- updating Ps
        _tex3 ("Reference Texture 3", 2D) = "" {} // f0, k, rho', ws -- updating ws
        // _recepitivity ("Recepitivity Parameter", Range(0.3, 1)) = 0.3
        // _baseMask ("Base Mask", Float) = 0.3
        // _penCol ("Pen Color", Color) = (1, 0, 0, 1)
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

    sampler2D _ColTex, _RefTex2, _PrevTex, _RefTex3;
    float _x, _y, _brushSize;
    // float _recepitivity, _baseMask;
    float4 _penCol;
    int _hasNewInk;

    // Step 1.1, when mouse click on the drawing canvas, 
    // update ws, the amount of water applied to the surface layer of the paper
    fixed4 updateWS (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        // r   g   b    a
        // vx, vy, rho, ws
        float4 ws = tex2D(_RefTex3, i.uv);
        float rho = tex2D(_RefTex2, i.uv).b;
        // ws.b = rho; // debugging purpose
        if (_hasNewInk == 1) 
        {
            if (distance(i.uv, float2(_x, _y)) < _brushSize)
            {
                // should change a
                ws.a += max(1 - rho / _recepitivity, _baseMask);
                // ws.r = ws.a; // showing
                // values.b = 0;
            }
        }
        return ws;
    }

    // Step 1.21, when mouse click on the drawing canvas, 
    // Update the pigment concentration on the surface layer
    fixed4 updatePS (v2f i) : SV_Target
    { 
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        // r   g   b    a
        // Ps, Pf, Px
        float4 ps = tex2D(_ColTex, i.uv);
        float rho = tex2D(_RefTex2, i.uv).b;
        // ps.b = rho; // for debugging
        if (_hasNewInk == 1) 
        {
            if (distance(i.uv, float2(_x, _y)) < _brushSize)
            {
                ps.r += max(1- rho / _recepitivity, _baseMask);
                // values.r += max(1 - rho / _recepitivity, _baseMask);
                // values.b = 0;
            }
        }
        
        return ps;
    }

    // Display purpose
    fixed4 display (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        // r   g   b    a
        // Ps, Pf, Px
        fixed4 col = tex2D(_PrevTex, i.uv);
        float rho = tex2D(_RefTex2, i.uv).b;
        // ps.b = rho; // for debugging
        if (_hasNewInk == 1) 
        {
            if (distance(i.uv, float2(_x, _y)) < _brushSize)
            {
                float addVal = max(1- rho / _recepitivity, _baseMask);
                col += float4(addVal, addVal, addVal, 1);
                // values.r += max(1 - rho / _recepitivity, _baseMask);
                // values.b = 0;
            }
        }
        return col;
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
            #pragma fragment updateWS    
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment updatePS        
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment display
            ENDCG
        }
    }
}
