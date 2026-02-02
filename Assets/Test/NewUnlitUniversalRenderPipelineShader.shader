Shader "Unlit/RTOutline"
{
    Properties
    {
        _MainTex ("RenderTexture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _Thickness ("Thickness (px)", Float) = 2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _OutlineColor;
            float _Thickness;

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS: SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 texel = 1.0 / float2(_ScreenParams.x, _ScreenParams.y); 
                // ↑ 주의: RT가 화면 크기와 다르면 texel 계산을 RT 크기로 맞추는게 더 정확.
                // 일단 개념용. (아래 개선 팁에 해결책 있음)

                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half a = c.a;

                // 주변 알파 샘플(간단히 4방향)
                float2 off = texel * _Thickness;
                half n = 0;
                n = max(n, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( off.x, 0)).a);
                n = max(n, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-off.x, 0)).a);
                n = max(n, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,  off.y)).a);
                n = max(n, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -off.y)).a);

                // 본체는 그대로 두고, 외곽선만 추가
                if (a <= 0.001 && n > 0.001)
                    return _OutlineColor;

                return c; // 원본(실루엣 포함) 출력
            }
            ENDHLSL
        }
    }
}