// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/Tangent Visualizer"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal    : NORMAL;    // The vertex normal in model space.
				float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
				float3 normal   : TEXCOORD1;   // The tangent vector in Model Space (used for normal mapping).

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.tangent = mul((float3x3)unity_ObjectToWorld, v.tangent);
				o.normal = v.normal;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				return float4(i.tangent, 1);
			}
			ENDCG
		}
	}
}