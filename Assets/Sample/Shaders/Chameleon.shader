// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Chameleon" {
	Properties {
		_MainColor ("Main Color", Color) = (1,1,1,1)
		_MainTexture ("Main Texture", 2D) = "white" {}
		_BaseColor ("Base Color", Color) = (1,1,1,1)
		_BaseTexture ("Base Texture", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Opacity ("Blend Opacity",Range(0,1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTexture;
		sampler2D _BaseTexture;
		fixed _Opacity;

		struct Input {
			float2 uv_MainTexture;
			float2 uv_BaseTexture;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _MainColor;
		fixed4 _BaseColor;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		fixed4 OverlayBlendMode(fixed4 basePixel, fixed4 blendPixel) {
    		if (basePixel.a == 0) {
        		return blendPixel;
    		} else if (basePixel.a == 1) {
        		return basePixel;
    		} else {
    			return basePixel.a * basePixel + (1 - basePixel.a) * blendPixel;
    		}
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 mainTex = tex2D (_MainTexture, IN.uv_MainTexture) * _MainColor;
			fixed4 baseTex = tex2D (_BaseTexture, IN.uv_BaseTexture) * _BaseColor;
			fixed4 blendedTex = OverlayBlendMode(mainTex, baseTex);

			mainTex = lerp (mainTex, blendedTex, _Opacity);

			o.Albedo = mainTex;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = mainTex.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
	CustomEditor "ChameleonShaderGUI"
}
