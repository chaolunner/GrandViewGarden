//-----------------------------------------------------
// Forward screen space decal multiply shader. Version 0.8
// Copyright (c) 2019 by Sycoforge
// Based on LWRP template https://gist.github.com/urschanselmann/06b7f4bb294ae2fe059aa517483149d4
//-----------------------------------------------------
Shader "Easy Decal/SSD/URP - Lit SSD"
{
	Properties
	{
		// Specular vs Metallic workflow
		[HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0

		[MainColor] _BaseColor("Color", Color) = (0.5,0.5,0.5,1)
		[MainTexture] _BaseMap("Albedo", 2D) = "white" {}

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
		_GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
		_SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

		[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_MetallicGlossMap("Metallic", 2D) = "white" {}

		_SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
		_SpecGlossMap("Specular", 2D) = "white" {}

		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

		_BumpScale("Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
		_OcclusionMap("Occlusion", 2D) = "white" {}

		_EmissionColor("Color", Color) = (0,0,0)
		_EmissionMap("Emission", 2D) = "white" {}

		// Blending state
		[HideInInspector] _Surface("__surface", Float) = 0.0
		[HideInInspector] _Blend("__blend", Float) = 0.0
		[HideInInspector] _AlphaClip("__clip", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _Cull("__cull", Float) = 2.0

		_ReceiveShadows("Receive Shadows", Float) = 1.0

		// Editmode props
		[HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
	}
	
	SubShader
	{
		// With SRP we introduce a new "RenderPipeline" tag in Subshader. This allows to create shaders
		// that can match multiple render pipelines. If a RenderPipeline tag is not set it will match
		// any render pipeline. In case you want your subshader to only run in LWRP set the tag to
		// "LightweightPipeline"
		Tags{"RenderType" = "Transparent" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True"}
		LOD 300

		ZWrite On 
		ZTest Always

		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
		//Blend One OneMinusSrcAlpha // Premultiplied transparency
		//Blend One One // Additive
		//Blend OneMinusDstColor One // Soft Additive
		//Blend DstColor Zero // Multiplicative
		//Blend DstColor SrcColor // 2x Multiplicative

		Offset -1,-1

	Pass
	{
		// "Lightmode" tag must be "LightweightForward" or not be defined in order for
		// to render objects.
		Name "StandardLit"
		Tags{"LightMode" = "LightweightForward"}


		HLSLPROGRAM

		#pragma vertex LitPassVertex
		#pragma fragment LitPassFragment

		// Required to compile gles 2.0 with standard SRP library
		// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x
		#pragma target 4.0

		// -------------------------------------
		// Material Keywords
		// unused shader_feature variants are stripped from build automatically
		#pragma shader_feature _NORMALMAP
		#pragma shader_feature _ALPHATEST_ON
		#pragma shader_feature _ALPHAPREMULTIPLY_ON
		#pragma shader_feature _EMISSION
		#pragma shader_feature _METALLICSPECGLOSSMAP
		#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		#pragma shader_feature _OCCLUSIONMAP

		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature _SPECULAR_SETUP
		#pragma shader_feature _RECEIVE_SHADOWS_OFF

		// -------------------------------------
		// Lightweight Render Pipeline keywords
		// When doing custom shaders you most often want to copy and past these #pragmas
		// These multi_compile variants are stripped from the build depending on:
		// 1) Settings in the LWRP Asset assigned in the GraphicsSettings at build time
		// e.g If you disable AdditionalLights in the asset then all _ADDITIONA_LIGHTS variants
		// will be stripped from build
		// 2) Invalid combinations are stripped. e.g variants with _MAIN_LIGHT_SHADOWS_CASCADE
		// but not _MAIN_LIGHT_SHADOWS are invalid and therefore stripped.
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
		#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
		#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
		#pragma multi_compile _ _SHADOWS_SOFT
		#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

		// -------------------------------------
		// Unity defined keywords
		#pragma multi_compile _ DIRLIGHTMAP_COMBINED
		#pragma multi_compile _ LIGHTMAP_ON
		#pragma multi_compile_fog

		//--------------------------------------
		// GPU Instancing
		#pragma multi_compile_instancing



		//#include "SycoFSSD.cginc"

		// Including the following two function is enought for shading with Lightweight Pipeline. Everything is included in them.
		// Core.hlsl will include SRP shader library, all constant buffers not related to materials (perobject, percamera, perframe).
		// It also includes matrix/space conversion functions and fog.
		// Lighting.hlsl will include the light functions/data to abstract light constants. You should use GetMainLight and GetLight functions
		// that initialize Light struct. Lighting.hlsl also include GI, Light BDRF functions. It also includes Shadows.

		// Required by all Lightweight Render Pipeline shaders.
		// It will include Unity built-in shader variables (except the lighting variables)
		// (https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
		// It will also include many utilitary functions. 
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"

		// Include this if you are doing a lit shader. This includes lighting shader variables,
		// lighting and shadow functions
		#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"

		// Material shader variables are not defined in SRP or LWRP shader library.
		// This means _BaseColor, _BaseMap, _BaseMap_ST, and all variables in the Properties section of a shader
		// must be defined by the shader itself. If you define all those properties in CBUFFER named
		// UnityPerMaterial, SRP can cache the material properties between frames and reduce significantly the cost
		// of each drawcall.
		// In this case, for sinmplicity LitInput.hlsl is included. This contains the CBUFFER for the material
		// properties defined above. As one can see this is not part of the ShaderLibrary, it specific to the
		// LWRP Lit shader.
		#include "Packages/com.unity.render-pipelines.lightweight/Shaders/LitInput.hlsl"
		#include "Util.cginc"
		//#include "UnityCG.cginc"

		#define ATLAS_TEXCOORDS_2(uv, st, localPos) (uv.xy * st.xy) * (localPos.xz + 0.5f) + (uv.zw + st.zw)
		#define ATLAS_TEX2D_2(tex, sampl, st, uv, localPos) SAMPLE_TEXTURE2D(tex, sampl, ATLAS_TEXCOORDS_2(uv, st, localPos))

		#ifdef _SPECULAR_SETUP
			#define _METALLICSPECULAR_MAP _SpecGlossMap
		#else
			#define _METALLICSPECULAR_MAP _MetallicGlossMap
		#endif

		#ifdef _SPECULAR_SETUP
			#define _METALLICSPECULAR_MAP_ST sampler_SpecGlossMap
		#else
			#define _METALLICSPECULAR_MAP_ST sampler_MetallicGlossMap
		#endif


		struct FragmentInputFSSD
		{
			float4 position		: SV_POSITION;
			float3 normal		: NORMAL;

			float4 uv			: TEXCOORD0;
			float4x4 mpv_inv	: TEXCOORD1;
			float4 screenPos	: TEXCOORD5;
			float3 ray			: TEXCOORD6;

			float4 color	: COLOR;
		};

		// Tranforms position from world to homogenous space
		inline float4 UnityWorldToClipPos(in float3 pos)
		{
			return mul(UNITY_MATRIX_VP, float4(pos, 1.0));
		}

		inline void InitializeStandardLitSurfaceDataInternal(float4 uv, float3 localPos, out SurfaceData outSurfaceData)
		{
			half4 st = _BaseMap_ST;

			half4 albedoAlpha = SampleAlbedoAlpha(ATLAS_TEXCOORDS_2(uv, st, localPos), TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
			outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

			half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
			outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

		#if _SPECULAR_SETUP
			outSurfaceData.metallic = 1.0h;
			outSurfaceData.specular = specGloss.rgb;
		#else
			outSurfaceData.metallic = specGloss.r;
			outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
		#endif

			outSurfaceData.smoothness = specGloss.a;
			outSurfaceData.normalTS = SampleNormal(ATLAS_TEXCOORDS_2(uv, st, localPos), TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
			outSurfaceData.occlusion = SampleOcclusion(ATLAS_TEXCOORDS_2(uv, st, localPos));
			outSurfaceData.emission = SampleEmission(ATLAS_TEXCOORDS_2(uv, st, localPos), _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
		}

		struct Attributes
		{
			float4 positionOS   : POSITION;
			float3 normalOS     : NORMAL;
			float4 tangentOS    : TANGENT;
			float2 uv           : TEXCOORD0;
			float2 uv2          : TEXCOORD1;
			float2 uvLM         : TEXCOORD2;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct Varyings
		{
			float4 uv                       : TEXCOORD0;
			float2 uvLM                     : TEXCOORD1;
			float4 positionWSAndFogFactor   : TEXCOORD2; // xyz: positionWS, w: vertex fog factor
			half3  normalWS                 : TEXCOORD3;

#if _NORMALMAP
			half3 tangentWS                 : TEXCOORD4;
			half3 bitangentWS               : TEXCOORD5;
#endif

#ifdef _MAIN_LIGHT_SHADOWS
			float4 shadowCoord              : TEXCOORD6; // compute shadow coord per-vertex for the main light
#endif
			float4 positionCS               : SV_POSITION;



			float4x4 mpv_inv	: TEXCOORD7;
			float4 screenPos	: TEXCOORD11;
			float3 ray			: TEXCOORD12;

			float4 color	: COLOR;
		};

		Varyings LitPassVertex(Attributes input)
		{
			Varyings output;

			// VertexPositionInputs contains position in multiple spaces (world, view, homogeneous clip space)
			// Our compiler will strip all unused references (say you don't use view space).
			// Therefore there is more flexibility at no additional cost with this struct.
			VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

			// Similar to VertexPositionInputs, VertexNormalInputs will contain normal, tangent and bitangent
			// in world space. If not used it will be stripped.
			VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

			// Computes fog factor per-vertex.
			float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

			// TRANSFORM_TEX is the same as the old shader library.
			float2 uv = TRANSFORM_TEX(input.uv, _BaseMap);
			float2 uvLM = input.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw;

			output.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
			output.normalWS = vertexNormalInput.normalWS;

			// Here comes the flexibility of the input structs.
			// In the variants that don't have normal map defined
			// tangentWS and bitangentWS will not be referenced and
			// GetVertexNormalInputs is only converting normal
			// from object to world space
#ifdef _NORMALMAP
			output.tangentWS = vertexNormalInput.tangentWS;
			output.bitangentWS = vertexNormalInput.bitangentWS;
#endif

#ifdef _MAIN_LIGHT_SHADOWS
			// shadow coord for the main light is computed in vertex.
			// If cascades are enabled, LWRP will resolve shadows in screen space
			// and this coord will be the uv coord of the screen space shadow texture.
			// Otherwise LWRP will resolve shadows in light space (no depth pre-pass and shadow collect pass)
			// In this case shadowCoord will be the position in light space.
			output.shadowCoord = GetShadowCoord(vertexInput);
#endif
			// We just use the homogeneous clip position from the vertex input
			output.positionCS = vertexInput.positionCS;

			float ortho = unity_OrthoParams.w;
			float4 pp = float4(input.positionOS.xyz, 1.0f);
			//float4 pp2 = float4(vertexInput.positionWS.xyz, 1.0f);

			//float4 p = vertexInput.positionCS;
			float4 p = vertexInput.positionCS;

			float4x4 mi = ortho ? inverseMat(UNITY_MATRIX_MVP) : mul(unity_WorldToObject, unity_CameraToWorld);
			//float4 worldPos = mul(unity_ObjectToWorld, input.vertex);
			//output.position = p;
			/*(o).normal = input.normal;*/
			output.screenPos = ComputeScreenPos(p);
			output.ray = mul(UNITY_MATRIX_MV, pp).xyz * float3(-1, -1, 1);

			//output.uv = float4(input.uv, input.uvLM);
			//output.uv = float4(input.uv, input.uv2);
			output.uv = float4(uv, uvLM);

			//output.color = input.color;
			output.mpv_inv = mi;


			return output;
		}

		TEXTURE2D(_CameraDepthTexture);
		SAMPLER(sampler_CameraDepthTexture);

		inline float SampleDepthTexture(float4 uv)
		{
			float2 screenParams = _ScreenParams.xy;
			uint2 pixelCoords = uint2((uv.xy / uv.w) * screenParams);
			return LOAD_TEXTURE2D_LOD(_CameraDepthTexture, pixelCoords, 0).r;
		}

		half4 LitPassFragment(Varyings input) : SV_Target
		{
			float ortho = unity_OrthoParams.w;
			float4x4 invMVP = input.mpv_inv;
			input.ray = ortho ? input.ray : input.ray * (_ProjectionParams.z / input.ray.z);
			float2 screenUV = input.screenPos.xy / input.screenPos.w;
			float depth = SAMPLE_TEXTURE2D(TEXTURE2D_ARGS(_CameraDepthTexture, sampler_CameraDepthTexture), screenUV).r;//#   define SAMPLE_DEPTH_TEXTURE(sampler, uv) (tex2D(sampler, uv).r)
			depth = ortho ? depth : Linear01Depth(depth, _ZBufferParams);
			float4 viewPos = ortho ? float4(input.ray.xy, depth, 1.0f) : float4(input.ray * depth, 1.0f);
			float4 worldPos = mul(invMVP, viewPos);
			float3 localPos = worldPos.xyz / worldPos.w;

			// Clip outside
			clip(0.5f - abs(localPos));


			// Surface data contains albedo, metallic, specular, smoothness, occlusion, emission and alpha
			// InitializeStandarLitSurfaceData initializes based on the rules for standard shader.
			// You can write your own function to initialize the surface data of your shader.
			SurfaceData surfaceData;
			InitializeStandardLitSurfaceDataInternal(input.uv, localPos, surfaceData);

			
			//return half4(surfaceData.smoothness, surfaceData.smoothness, surfaceData.smoothness, surfaceData.alpha);
			//return half4(surfaceData.albedo, surfaceData.alpha);


#if _NORMALMAP
				half3 normalWS = TransformTangentToWorld(surfaceData.normalTS,
				half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
#else
				half3 normalWS = input.normalWS;
#endif
				normalWS = normalize(normalWS);
				//return half4(normalWS, surfaceData.alpha);

#ifdef LIGHTMAP_ON
				// Normal is required in case Directional lightmaps are baked
				half3 bakedGI = SampleLightmap(input.uvLM, normalWS);
#else
				// Samples SH fully per-pixel. SampleSHVertex and SampleSHPixel functions
				// are also defined in case you want to sample some terms per-vertex.
				half3 bakedGI = SampleSH(normalWS);
#endif

				float3 positionWS = input.positionWSAndFogFactor.xyz;
				half3 viewDirectionWS = SafeNormalize(GetCameraPositionWS() - positionWS);

				// BRDFData holds energy conserving diffuse and specular material reflections and its roughness.
				// It's easy to plugin your own shading fuction. You just need replace LightingPhysicallyBased function
				// below with your own.
				BRDFData brdfData;
				InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

				// Light struct is provide by LWRP to abstract light shader variables.
				// It contains light direction, color, distanceAttenuation and shadowAttenuation.
				// LWRP take different shading approaches depending on light and platform.
				// You should never reference light shader variables in your shader, instead use the GetLight
				// funcitons to fill this Light struct.
#ifdef _MAIN_LIGHT_SHADOWS
				// Main light is the brightest directional light.
				// It is shaded outside the light loop and it has a specific set of variables and shading path
				// so we can be as fast as possible in the case when there's only a single directional light
				// You can pass optionally a shadowCoord (computed per-vertex). If so, shadowAttenuation will be
				// computed.
				Light mainLight = GetMainLight(input.shadowCoord);
#else
				Light mainLight = GetMainLight();
#endif

				// Mix diffuse GI with environment reflections.
				half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWS, viewDirectionWS);

				// LightingPhysicallyBased computes direct light contribution.
				color += LightingPhysicallyBased(brdfData, mainLight, normalWS, viewDirectionWS);

				// Additional lights loop
#ifdef _ADDITIONAL_LIGHTS

				// Returns the amount of lights affecting the object being renderer.
				// These lights are culled per-object in the forward renderer
				int additionalLightsCount = GetAdditionalLightsCount();
				for (int i = 0; i < additionalLightsCount; ++i)
				{
					// Similar to GetMainLight, but it takes a for-loop index. This figures out the
					// per-object light index and samples the light buffer accordingly to initialized the
					// Light struct. If _ADDITIONAL_LIGHT_SHADOWS is defined it will also compute shadows.
					Light light = GetAdditionalLight(i, positionWS);

					// Same functions used to shade the main light.
					color += LightingPhysicallyBased(brdfData, light, normalWS, viewDirectionWS);
				}
#endif
				// Emission
				color += surfaceData.emission;

				float fogFactor = input.positionWSAndFogFactor.w;

				// Mix the pixel color with fogColor. You can optionaly use MixFogColor to override the fogColor
				// with a custom one.
				color = MixFog(color, fogFactor);
				return half4(color, surfaceData.alpha);
		}

		ENDHLSL
	}

		// Used for depth prepass
		// If shadows cascade are enabled we need to perform a depth prepass. 
		// We also need to use a depth prepass in some cases camera require depth texture
		// (e.g, MSAA is enabled and we can't resolve with Texture2DMS
		UsePass "Lightweight Render Pipeline/Lit/DepthOnly"

		// Used for Baking GI. This pass is stripped from build.
		UsePass "Lightweight Render Pipeline/Lit/Meta"
	}
	
	// Uses a custom shader GUI to display settings. Re-use the same from Lit shader as they have the
	// same properties.
	CustomEditor "UnityEditor.Rendering.LWRP.ShaderGUI.LitShader"
}