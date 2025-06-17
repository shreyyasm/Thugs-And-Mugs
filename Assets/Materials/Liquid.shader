Shader "Custom/LiquidWobble"
{
    Properties
    {
        _Fill("Fill", Range(0,1)) = 0.5
        _FillMin("Fill Min", Float) = -0.5
        _FillMax("Fill Max", Float) = 0.5

        _Color("Color", Color) = (0, 0.5, 1, 1)
        _EmissionColor("Emission Color", Color) = (0, 1, 1, 1)

        _WobbleX("WobbleX", Float) = 0
        _WobbleY("WobbleY", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Fill;
            float _FillMin;
            float _FillMax;
            float4 _Color;
            float4 _EmissionColor;
            float _WobbleX;
            float _WobbleY;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float worldY : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 pos = v.vertex.xyz;

                // Wobble distortion
                float wobbleOffset = sin(_Time.y * 3 + pos.x * 5) * _WobbleX +
                                     cos(_Time.y * 2 + pos.y * 4) * _WobbleY;

                pos.y += wobbleOffset * 0.02;

                o.vertex = UnityObjectToClipPos(float4(pos, 1));
                o.uv = v.uv;
                o.worldY = pos.y;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Accurate fill based on actual mesh height
                float fillY = lerp(_FillMin, _FillMax, _Fill);
                float alpha = i.worldY < fillY ? 1 : 0;

                // Base color
                float4 color = float4(_Color.rgb, alpha * _Color.a);

                // Emission only inside the liquid area
                color.rgb += _EmissionColor.rgb * alpha;

                return color;
            }
            ENDCG
        }
    }
}
