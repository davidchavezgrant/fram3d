Shader "Fram3d/InfiniteGrid"
{
    Properties
    {
        _GridColor    ("Grid Color",    Color) = (0.5, 0.5, 0.5, 1)
        _MajorSpacing ("Major Spacing", Float) = 1.0
        _MinorCount   ("Minor Count",   Float) = 4.0
        _LineWidth    ("Line Width",    Float) = 1.0
        _FadeDistance  ("Fade Distance", Float) = 80.0
        _MajorAlpha   ("Major Alpha",   Float) = 0.4
        _MinorAlpha   ("Minor Alpha",   Float) = 0.15
        _AxisAlpha    ("Axis Alpha",    Float) = 0.6
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent-100"
        }

        Pass
        {
            Name "InfiniteGrid"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _GridColor;
                float  _MajorSpacing;
                float  _MinorCount;
                float  _LineWidth;
                float  _FadeDistance;
                float  _MajorAlpha;
                float  _MinorAlpha;
                float  _AxisAlpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(worldPos);
                output.positionWS = worldPos;
                return output;
            }

            // Returns line intensity for a grid at the given spacing.
            // Uses screen-space derivatives for anti-aliased lines.
            float gridLine(float2 worldXZ, float spacing)
            {
                float2 grid = abs(frac(worldXZ / spacing - 0.5) - 0.5) / fwidth(worldXZ / spacing);
                float  line = min(grid.x, grid.y);
                return 1.0 - saturate(line - _LineWidth);
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 xz = input.positionWS.xz;

                // Minor grid
                float minorSpacing = _MajorSpacing / _MinorCount;
                float minor        = gridLine(xz, minorSpacing);

                // Major grid
                float major = gridLine(xz, _MajorSpacing);

                // Axis lines (X = red at Z=0, Z = blue at X=0)
                float2 axisDist  = abs(xz) / fwidth(xz);
                float  xAxisLine = 1.0 - saturate(axisDist.y - _LineWidth);
                float  zAxisLine = 1.0 - saturate(axisDist.x - _LineWidth);

                // Distance fade
                float dist = length(input.positionWS - _WorldSpaceCameraPos);
                float fade = 1.0 - saturate(dist / _FadeDistance);
                fade *= fade; // quadratic falloff

                // Composite: axes override major, major overrides minor
                float3 color = _GridColor.rgb;
                float  alpha = minor * _MinorAlpha;
                alpha = max(alpha, major * _MajorAlpha);

                // Axis coloring
                float3 axisColor = color;
                float  axisAlpha = alpha;

                if (xAxisLine > 0.01)
                {
                    axisColor = lerp(axisColor, float3(0.8, 0.2, 0.2), xAxisLine);
                    axisAlpha = max(axisAlpha, xAxisLine * _AxisAlpha);
                }

                if (zAxisLine > 0.01)
                {
                    axisColor = lerp(axisColor, float3(0.2, 0.2, 0.8), zAxisLine);
                    axisAlpha = max(axisAlpha, zAxisLine * _AxisAlpha);
                }

                color = lerp(color, axisColor, step(0.01, xAxisLine + zAxisLine));
                alpha = max(alpha, axisAlpha * step(0.01, xAxisLine + zAxisLine));

                return float4(color, alpha * fade);
            }
            ENDHLSL
        }
    }
}
