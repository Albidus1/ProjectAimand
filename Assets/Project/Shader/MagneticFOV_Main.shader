// URP 대응으로 만듬

Shader"Custom/MagneticFOV_Main"
{
    Properties
    {
        _Color ("FOV Color", Color) = (0, 0.5, 1, 0.3)
        _Alpha ("Alpha", Range(0, 1)) = 0.3
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "MainFOV"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            
            // 스텐실: 렌더링한 영역을 1로 마킹
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float _Alpha;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = _Color;
                color.a = _Alpha;
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}