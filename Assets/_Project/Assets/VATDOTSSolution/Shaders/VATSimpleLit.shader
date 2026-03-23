Shader "Universal Render Pipeline/VAT Simple Lit DOTS"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map (RGB) Smoothness / Alpha (A)", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Clipping", Range(0,1)) = 0.5

        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _SpecColor("Specular Color", Color) = (0.2,0.2,0.2,1.0)
        _SpecGlossMap("Specular Map", 2D) = "white" {}
        _SpecularHighlights("Specular Highlights", Float) = 1.0

        [HideInInspector] _BumpScale("Bump Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}

        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)
        [NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2

        [NoScaleOffset] _VATPositionTex("VAT Position", 2D) = "black" {}
        [NoScaleOffset] _VATNormalTex("VAT Normal", 2D) = "gray" {}
        [NoScaleOffset] _VATMetaTex("VAT Meta", 2D) = "black" {}
        _VATBoundsMin("VAT Bounds Min", Vector) = (0,0,0,0)
        _VATBoundsExtent("VAT Bounds Extent", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="SimpleLit"
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "VATCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SpecGlossMap);
            SAMPLER(sampler_SpecGlossMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
                float _Smoothness;
                float4 _SpecColor;
                float _SpecularHighlights;
                float _BumpScale;
                float4 _EmissionColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 tangentWS : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                half fogFactor : TEXCOORD5;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half3 SampleNormalWS(Varyings input)
            {
                half3 n = NormalizeNormalPerPixel(input.normalWS);
                #if defined(_NORMALMAP)
                    half3 tangentWS = normalize(input.tangentWS.xyz);
                    half3 bitangentWS = normalize(cross(n, tangentWS) * input.tangentWS.w);
                    half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                    half3x3 tbn = half3x3(tangentWS, bitangentWS, n);
                    n = NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tbn));
                #endif
                return n;
            }

            void SampleSpecular(float2 uv, half baseAlpha, out half3 specularColor, out half smoothness)
            {
                specularColor = _SpecColor.rgb;
                smoothness = _Smoothness;

                #if defined(_SPECGLOSSMAP)
                    half4 specSample = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv);
                    specularColor = specSample.rgb;
                    #if !defined(_GLOSSINESS_FROM_BASE_ALPHA)
                        smoothness = specSample.a;
                    #endif
                #endif

                #if defined(_GLOSSINESS_FROM_BASE_ALPHA)
                    smoothness = baseAlpha;
                #endif

                smoothness = saturate(smoothness);
            }

            half3 EvaluateLight(half3 normalWS, half3 viewDirWS, half3 albedo, half3 specularColor, half smoothness, Light light)
            {
                half atten = light.distanceAttenuation * light.shadowAttenuation;
                half3 radiance = light.color * atten;

                half3 diffuse = LightingLambert(radiance, light.direction, normalWS) * albedo;
                half3 specular = 0;

                if (_SpecularHighlights > 0.5)
                {
                    half3 halfDir = SafeNormalize(light.direction + viewDirWS);
                    half nh = saturate(dot(normalWS, halfDir));
                    half specPower = max(1.0h, exp2(10.0h * smoothness + 1.0h));
                    half spec = pow(nh, specPower) * smoothness;
                    specular = spec * specularColor * radiance;
                }

                return diffuse + specular;
            }

            half3 AccumulateAdditionalLights(InputData inputData, half3 albedo, half3 specularColor, half smoothness)
            {
                half3 lighting = 0;

                #if USE_CLUSTER_LIGHT_LOOP
                UNITY_LOOP for (uint lightIndex = 0u; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                {
                    Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
                    lighting += EvaluateLight(inputData.normalWS, inputData.viewDirectionWS, albedo, specularColor, smoothness, additionalLight);
                }
                #endif

                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light additionalLight = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
                    lighting += EvaluateLight(inputData.normalWS, inputData.viewDirectionWS, albedo, specularColor, smoothness, additionalLight);
                LIGHT_LOOP_END

                return lighting;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPosOS;
                float3 animatedNormalOS;
                VATSampleBlended(input.vertexID, animatedPosOS, animatedNormalOS);
                float4 animatedTangentOS = VATRebuildTangent(input.tangentOS, animatedNormalOS);

                VertexPositionInputs posInputs = GetVertexPositionInputs(animatedPosOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(animatedNormalOS, animatedTangentOS);

                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.tangentWS = half4(normalize(normalInputs.tangentWS), animatedTangentOS.w);
                output.shadowCoord = GetShadowCoord(posInputs);
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                #if defined(_ALPHATEST_ON)
                    clip(baseSample.a - _Cutoff);
                #endif

                half3 normalWS = SampleNormalWS(input);
                half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);

                half3 specularColor;
                half smoothness;
                SampleSpecular(input.uv, baseSample.a, specularColor, smoothness);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = input.shadowCoord;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.shadowMask = half4(1,1,1,1);
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = half3(0,0,0);

                half3 color = inputData.bakedGI * baseSample.rgb;

                Light mainLight = GetMainLight(inputData.shadowCoord);
                color += EvaluateLight(normalWS, viewDirWS, baseSample.rgb, specularColor, smoothness, mainLight);

                #if defined(_ADDITIONAL_LIGHTS)
                    color += AccumulateAdditionalLights(inputData, baseSample.rgb, specularColor, smoothness);
                #endif

                #if defined(_EMISSION)
                    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;
                    color += emission;
                #endif

                color = MixFog(color, inputData.fogCoord);
                return half4(color, baseSample.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "VATCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPosOS;
                float3 animatedNormalOS;
                VATSampleBlended(input.vertexID, animatedPosOS, animatedNormalOS);
                output.positionCS = TransformObjectToHClip(animatedPosOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                #if defined(_ALPHATEST_ON)
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(alpha - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "VATCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPosOS;
                float3 animatedNormalOS;
                VATSampleBlended(input.vertexID, animatedPosOS, animatedNormalOS);
                output.positionCS = TransformObjectToHClip(animatedPosOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 DepthFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                #if defined(_ALPHATEST_ON)
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(alpha - _Cutoff);
                #endif
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "VATCommon.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
                float _BumpScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half4 tangentWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthNormalsVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 animatedPosOS;
                float3 animatedNormalOS;
                VATSampleBlended(input.vertexID, animatedPosOS, animatedNormalOS);
                float4 animatedTangentOS = VATRebuildTangent(input.tangentOS, animatedNormalOS);

                VertexNormalInputs normalInputs = GetVertexNormalInputs(animatedNormalOS, animatedTangentOS);
                output.positionCS = TransformObjectToHClip(animatedPosOS);
                output.normalWS = NormalizeNormalPerVertex(normalInputs.normalWS);
                output.tangentWS = half4(normalize(normalInputs.tangentWS), animatedTangentOS.w);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 DepthNormalsFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                #if defined(_ALPHATEST_ON)
                    half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                    clip(alpha - _Cutoff);
                #endif

                half3 normalWS = NormalizeNormalPerPixel(input.normalWS);
                #if defined(_NORMALMAP)
                    half3 tangentWS = normalize(input.tangentWS.xyz);
                    half3 bitangentWS = normalize(cross(normalWS, tangentWS) * input.tangentWS.w);
                    half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);
                    half3x3 tbn = half3x3(tangentWS, bitangentWS, normalWS);
                    normalWS = NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tbn));
                #endif

                return half4(normalWS * 0.5h + 0.5h, 1.0h);
            }
            ENDHLSL
        }
    }
}
