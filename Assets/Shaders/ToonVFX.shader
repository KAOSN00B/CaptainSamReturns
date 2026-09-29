// Cel-shaded VFX for sword trails and hit particles.
// Instead of soft, glowy fades it draws flat colour shapes with a hard edge, an ink outline
// and an optional bright core - like an effect drawn in a comic.
//
// Trail Shape ON  (for TrailRenderers): the shape comes from the trail itself - a swoosh that is
//                  thick at the blade and tapers to a point at the tail.
// Trail Shape OFF (for particles): the shape comes from the particle texture; fading a particle's
//                  alpha over its lifetime makes it shrink away instead of turning see-through.
Shader "Custom/ToonVFX"
{
    Properties
    {
        [MainTexture] _MainTex ("Shape Texture (particles)", 2D) = "white" {}
        [HDR] _Color ("Body Color", Color) = (1, 1, 1, 1)
        [HDR] _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _InkColor ("Ink Color", Color) = (0, 0, 0, 1)
        _Cutoff ("Shape Cutoff", Range(0, 1)) = 0.1
        _InkWidth ("Ink Width", Range(0, 0.5)) = 0.12
        _CoreThreshold ("Core Size (higher = smaller, above 1 = off)", Range(0, 1.01)) = 0.65
        [Toggle(_TRAIL_SHAPE)] _TrailShape ("Trail Shape (use on TrailRenderers)", Float) = 0
        [Toggle] _FlipTrailTaper ("Flip Trail Taper", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ToonVFX"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _TRAIL_SHAPE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _CoreColor;
                half4 _InkColor;
                half _Cutoff;
                half _InkWidth;
                half _CoreThreshold;
                half _FlipTrailTaper;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // "shape" = how far inside the effect this pixel is (0 = outside, 1 = dead centre)
                #if defined(_TRAIL_SHAPE)
                    float acrossWidth = 1 - abs(input.uv.y * 2 - 1);                // 1 in the middle of the ribbon, 0 at its edges
                    float alongLength = lerp(1 - input.uv.x, input.uv.x, _FlipTrailTaper); // 1 at the blade, 0 at the tail
                    half shape = acrossWidth * alongLength;
                #else
                    half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                    // works for both alpha-shaped textures and black-background (additive-style) textures
                    half textureShape = min(tex.a, max(tex.r, max(tex.g, tex.b)));
                    half shape = textureShape * input.color.a;
                #endif

                clip(shape - _Cutoff);                                               // hard edge: no soft fade-out

                half4 body = _Color * half4(input.color.rgb, 1);
                half isInk = step(shape, _Cutoff + _InkWidth);                       // thin band just inside the edge
                half isCore = step(_CoreThreshold, shape);                           // bright centre

                half3 colour = lerp(body.rgb, _CoreColor.rgb, isCore);
                colour = lerp(colour, _InkColor.rgb, isInk * _InkColor.a);
                return half4(colour, 1);
            }
            ENDHLSL
        }
    }
}
