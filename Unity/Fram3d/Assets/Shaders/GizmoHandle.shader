Shader "Fram3d/GizmoHandle"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0, 0, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Overlay+1"
        }

        Pass
        {
            Name "GizmoHandle"
            ZTest Always
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
