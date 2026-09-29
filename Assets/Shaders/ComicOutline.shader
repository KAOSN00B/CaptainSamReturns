// Full-screen comic-book outline.
// Draws hard lines wherever the depth or the surface direction (normal) jumps between neighbouring pixels:
// depth jumps = object silhouettes, normal jumps = creases and corners inside an object.
// Runs through URP's Full Screen Pass Renderer Feature (needs Depth + Normal requirements).
Shader "Custom/ComicOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Thickness ("Thickness (pixels)", Range(0.5, 4)) = 1.5
        _DepthThreshold ("Depth Sensitivity (lower = more lines)", Range(0.01, 1)) = 0.1
        _NormalThreshold ("Crease Sensitivity (lower = more lines)", Range(0.01, 1)) = 0.4
        _FadeStartDistance ("Fade Start (meters)", Float) = 40
        _FadeEndDistance ("Fade End (meters)", Float) = 80
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "ComicOutline"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float4 _OutlineColor;
            float _Thickness;
            float _DepthThreshold;
            float _NormalThreshold;
            float _FadeStartDistance;
            float _FadeEndDistance;

            static const float2 NeighbourOffsets[4] = { float2(1, 0), float2(-1, 0), float2(0, 1), float2(0, -1) };

            float EyeDepth(float2 uv)
            {
                return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float2 pixelSize = _Thickness / _ScreenParams.xy;
                float centreDepth = EyeDepth(uv);
                float3 centreNormal = SampleSceneNormals(uv);

                float depthEdge = 0;
                float normalEdge = 0;

                [unroll]
                for (int i = 0; i < 4; i++)
                {
                    float2 neighbourUV = uv + NeighbourOffsets[i] * pixelSize;

                    // relative to distance, so far-away objects don't turn solid black
                    float depthJump = abs(EyeDepth(neighbourUV) - centreDepth) / centreDepth;
                    depthEdge = max(depthEdge, step(_DepthThreshold, depthJump));

                    // 0 = facing the same way, 1 = 90 degrees apart
                    float creaseAmount = 1 - dot(centreNormal, SampleSceneNormals(neighbourUV));
                    normalEdge = max(normalEdge, step(_NormalThreshold, creaseAmount));
                }

                float edge = max(depthEdge, normalEdge);

                // thin the lines out in the distance so the skyline doesn't become noise
                edge *= 1 - saturate((centreDepth - _FadeStartDistance) / (_FadeEndDistance - _FadeStartDistance));

                return half4(lerp(sceneColor.rgb, _OutlineColor.rgb, edge * _OutlineColor.a), sceneColor.a);
            }
            ENDHLSL
        }
    }
}
