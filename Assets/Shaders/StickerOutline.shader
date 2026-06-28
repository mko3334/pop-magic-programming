Shader "Custom/StickerOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Float) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST; float4 _MainTex_TexelSize;
            float4 _OutlineColor; float _OutlineSize;
            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }
            half4 fragOutline(Varyings IN) : SV_Target {
                float2 ts = _MainTex_TexelSize.xy * _OutlineSize;
                float a = 0;
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2( ts.x,0)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-ts.x,0)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, ts.y)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0,-ts.y)).a);
                float self = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;
                return half4(_OutlineColor.rgb, saturate(a - self) * _OutlineColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert2
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST; float4 _Color;
            Varyings vert2(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
            }
            ENDHLSL
        }
    }
}
