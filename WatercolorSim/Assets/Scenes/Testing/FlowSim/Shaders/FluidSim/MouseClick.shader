Shader "FlowSim/MouseClick"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _offset ("Offset", Float) = 0.001
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

    sampler2D _mask, _RefTex2, _RefTex3, _ColTex;
    float4 _RefTex3_TexelSize, _ColTex_TexelSize;
    float _x, _y, _size, _pigAmt, _px, _py, _waterAmt;
    float _offset;
    // float4 _penCol;
    // int _newInk;

    float lineSegment(float2 p, float2 a, float2 b)
    {
        float2 ba = b - a;
        float2 pa = p - a;
        float k = saturate(dot(pa, ba) / dot(ba, ba));
        return length(pa - ba * k);

    }

    fixed4 paint (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"
        
        fixed4 col = tex2D(_mask, i.uv);
        float2 _offset2 = float2(_offset, _offset);
        col = float4(_pigAmt, _pigAmt, _pigAmt,_pigAmt) * step(-_size, -lineSegment(i.uv, float2(_px, _py) - _offset2, float2(_x, _y)));
        // col = max(col - float4(wf, wf, wf, wf), 0);
        return col;
    }

    fixed4 paint_ws (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 ws = tex2D(_RefTex3, i.uv);
        float rho = tex2D(_RefTex2, i.uv).b;
        // float wf = tex2D(_RefTex2, i.uv).a;
        float2 _offset2 = float2(_offset, _offset);
        // float val = max(1- rho / _recepitivity, _waterAmt);
        ws.a += _waterAmt * step(lineSegment(i.uv, float2(_px, _py) - _offset2, float2(_x, _y)), _size * _RefTex3_TexelSize.x);
        // ws.a = max(ws.a - wf, 0); // update ws = max(ws - wf, 0)
        return ws;
    }

    fixed4 paint_ps (v2f i) : SV_Target
    {
        #include "Assets/Scenes/Sim_1/Shaders/Includes/SimulationPara.cginc"

        fixed4 ps = tex2D(_ColTex, i.uv);
        // float rho = tex2D(_RefTex2, i.uv).b;
        // float wf = tex2D(_RefTex2, i.uv).a;
        float2 _offset2 = float2(_offset, _offset);
        // float val = max(1- rho / _recepitivity, _baseMask);
        ps.r += _pigAmt * step(lineSegment(i.uv, float2(_px, _py) - _offset2, float2(_x, _y)), _size * _ColTex_TexelSize.x);
        // ps.r = max(ps.r - wf, 0); // update ws = max(ws - wf, 0)
        return ps;
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
            #pragma fragment paint
            ENDCG
        }
        // Pass
        // {
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment paint_c
        //     ENDCG
        // }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment paint_ws
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment paint_ps
            ENDCG
        }
    }
}
