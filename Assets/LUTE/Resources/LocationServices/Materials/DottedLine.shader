Shader "Custom/ImprovedDottedLine"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _DotSpacing ("Dot Spacing", Range(0, 1)) = 0.3
        _DotSize ("Dot Size", Range(0, 1)) = 0.5
        _LineWidth ("Line Width", Range(0, 0.1)) = 0.01
        _Direction ("Direction", Range(-1, 1)) = 0 // 0 for horizontal, 1 for vertical
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _DotSpacing;
            float _DotSize;
            float _LineWidth;
            float _Direction;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Interpolate between horizontal and vertical based on _Direction
                float coord = lerp(i.uv.x, i.uv.y, _Direction);
                
                // Create a repeating pattern with controlled dot spacing and size
                float dotPattern = frac(coord / (_DotSpacing + _DotSize));
                float dotMask = step(1.0 - _DotSize, dotPattern);
                
                // Add line width control
                float lineWidth = _LineWidth;
                float smoothedDot = smoothstep(1.0 - _DotSize - lineWidth, 1.0 - _DotSize + lineWidth, dotPattern);
                
                // Combine color and alpha
                return fixed4(_Color.rgb, smoothedDot * _Color.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}