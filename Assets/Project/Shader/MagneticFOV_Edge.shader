// URP 대응
// 단일 사용하지 말고 MAgneticFOV_Main을 먼저 사용 후 멀티 머테리얼로 사용 바람

Shader"Custom/MagneticFOV_Edge"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 0.8)
        _Alpha ("Alpha", Range(0, 1)) = 0.8
        _EdgeWidth ("Edge Width", Range(0.001, 1.0)) = 0.02
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+1"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "EdgeFOV"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            
            // 스텐실: 1이 아닌 영역만 렌더링 (메인 FOV 영역 제외)
            Stencil
            {
                Ref 1
                Comp NotEqual
                Pass Keep
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
            half4 _EdgeColor;
            float _Alpha;
            float _EdgeWidth;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 로컬 스페이스에서 중심점으로부터 확장
                float3 localPos = input.positionOS.xyz;
                float3 centerPos = float3(0, 0, 0); // 메시의 중심점
                
                // 중심에서 현재 버텍스로의 방향과 거리
                float3 direction = localPos - centerPos;
                float distance = length(direction);
                
                float expansionFactor = 0;
                
                // 중심점이 아닌 버텍스만 확장 (distance > 0.001로 중심점 제외)
                if (distance > 0.001)
                {
                    // 방향 단위벡터
                    direction = normalize(direction);
                    
                    // EdgeWidth만큼 바깥쪽으로 확장
                    localPos += direction * _EdgeWidth;
                    expansionFactor = 1.0;
                }
                
                output.positionHCS = TransformObjectToHClip(localPos);
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = _EdgeColor;
                
                color.a *= _Alpha;
                return color;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}