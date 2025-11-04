Shader "Custom/LogoSplitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SplitAmount ("Split Amount", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SplitAmount;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float centerX = 0.5;
                float halfWidth = max(_SplitAmount * 0.5, 0.0001);
                float edgeSoftness = 0.02;

                float leftEdge  = centerX - halfWidth;
                float rightEdge = centerX + halfWidth;

                float alphaLeft  = saturate( (uv.x - leftEdge) / edgeSoftness );
                float alphaRight = saturate( (rightEdge - uv.x) / edgeSoftness );

                float alpha = alphaLeft * alphaRight;

                half4 col = tex2D(_MainTex, uv);
                col.a *= alpha;

                return col;
            }
            ENDHLSL
        }
    }
}