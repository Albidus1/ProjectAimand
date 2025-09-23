Shader "Sprites/DotInvert"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _InvertAmount ("Invert Amount", Range(0, 1)) = 1.0
        _PixelSize ("Pixel Size", Float) = 1.0
        [MaterialToggle] _PreserveOutline ("Preserve Outline", Float) = 1
        _OutlineThreshold ("Outline Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _InvertAmount;
            float _PixelSize;
            float _PreserveOutline;
            float _OutlineThreshold;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // 아웃라인 감지 함수
            float IsOutline(float2 uv, fixed4 currentColor)
            {
                if (currentColor.a < _OutlineThreshold) return 0.0;

                float2 pixelSize = _MainTex_TexelSize.xy * _PixelSize;
                float isOutline = 0.0;

                // 주변 픽셀 검사 (4방향)
                fixed4 up = tex2D(_MainTex, uv + float2(0, pixelSize.y));
                fixed4 down = tex2D(_MainTex, uv + float2(0, -pixelSize.y));
                fixed4 left = tex2D(_MainTex, uv + float2(-pixelSize.x, 0));
                fixed4 right = tex2D(_MainTex, uv + float2(pixelSize.x, 0));

                // 투명한 픽셀이 인접해 있으면 아웃라인으로 판단
                if (up.a < _OutlineThreshold || down.a < _OutlineThreshold || 
                    left.a < _OutlineThreshold || right.a < _OutlineThreshold)
                {
                    isOutline = 1.0;
                }

                return isOutline;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 아웃라인 보존 옵션
                float outlineFactor = 0.0;
                if (_PreserveOutline > 0.5)
                {
                    outlineFactor = IsOutline(IN.texcoord, original);
                }

                // 색상 반전 (알파는 유지)
                fixed4 inverted = fixed4(1.0 - original.r, 1.0 - original.g, 1.0 - original.b, original.a);
                
                // 아웃라인이면 반전 강도 감소
                float finalInvertAmount = _InvertAmount * (1.0 - outlineFactor * 0.7);
                
                // 반전 적용
                fixed4 result = lerp(original, inverted, finalInvertAmount);
                
                // 스프라이트 알파 블렌딩
                result.rgb *= result.a;
                
                return result;
            }
            ENDCG
        }
    }
}