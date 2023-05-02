Shader "Custom/Paint"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        // _PreviousState("Texture", 2D) = "white" {}
        // _x ("_x", Float) = -1.0
        // _y ("_y", Float) = -1.0
        // _r ("Radius", Float) = 0.02
        // _PenCol ("PenColor", Color) = (0.0, 1.0, 0.0, 1.0)
        _Offset ("Offset", Range(0.001, 0.005)) = 0.001
    }


    SubShader
    {
        // No culling or depth
        Cull Off 
        // ZWrite Off ZTest Always
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

            sampler2D _PreviousState;
            sampler2D _tex1, _tex2;
            int _hasNewInk;
            float4 _PenCol;
            float _x, _y, _brushSize, _recepitivity, _baseMask;

            float _Offset;

            fixed4 frag (v2f i) : SV_Target
            {
                // Step 1.1, when mouse click on the drawing canvas, 
        // update ws, the amount of water applied to the surface layer of the paper
                // r   g   b    a
                // vx, vy, rho, ws
                // fixed4 values = tex2D(_tex1, i.uv);
                // if (_hasNewInk == 1) 
                // {
                //     if (distance(i.uv, float2(_x, _y) < _brushSize))
                //     {
                //         float newA = values.a + max(1 - values.b / 0.3, 0.3);
                //         values = float4(1, 0, 0, 1);
                //     }
                // }
                // return values;
                fixed4 col =  tex2D(_tex1, i.uv); // 
                if (_hasNewInk == 1) {
                    if (distance(i.uv, float2(_x, _y)) < _brushSize) {
                        // col.r += 0.3; 
                        col = float4(0.3, 0, 0, 0);
                        // col.a = 0;
                    } else if (distance(i.uv, float2(_x, _y)) <= _brushSize + _Offset)
                    {
                        
                        // col.r += 0.08; 
                        col = float4(0.08, 0, 0, 0);
                    }
                }
                return col;
            }

            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            sampler2D _PreviousState;
            sampler2D _tex1, _tex2;
            int _hasNewInk;
            float4 _PenCol;
            float _x, _y, _brushSize, _recepitivity, _baseMask;

            fixed4 frag (v2f i) : SV_Target
            {
                // Step 1.21, when mouse click on the drawing canvas, 
        // Update the pigment concentration on the surface layer
                fixed4 col =  tex2D(_tex2, i.uv); // 
                if (_hasNewInk == 1) {
                    if (distance(i.uv, float2(_x, _y)) < _brushSize) {
                        col = float4(0, 1, 0, 1);
                    }
                }
                return col;
            }

            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag1
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

            sampler2D _PreviousState;
            sampler2D _MainTex;
            int _hasNewInk;
            float4 _PenCol;
            float _x, _y, _brushSize, _recepitivity, _baseMask;


        fixed4 frag1 (v2f i) : SV_Target
        {
            fixed4 col =  tex2D(_PreviousState, i.uv); // 
            if (_hasNewInk == 1) {
                if (distance(i.uv, float2(_x, _y)) < _brushSize) {
                    col = float4(0, 0, 1, 1);
                }
            }
            return col;
            
        }
            ENDCG
        }

    }
}
