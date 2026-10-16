Shader "Universal Render Pipeline/Unlit/ScrollingShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DispTex ("Displacement Texture", 2D) = "white" {}
		_Speed ("Scroll Speed", Float) = 5
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Geometry"
		}

		Pass
		{
			Name "UniversalForward"
			Tags { "LightMode" = "UniversalForward" }

			Cull Off
			ZWrite Off
			ZTest Always

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			TEXTURE2D(_DispTex);
			SAMPLER(sampler_DispTex);
			float _Speed;

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

			Varyings vert(Attributes input)
			{
				Varyings output;
				output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uv = input.uv;
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				half displacement = SAMPLE_TEXTURE2D(_DispTex, sampler_DispTex, input.uv).r;
				float2 uv = input.uv + float2(_Time.x * _Speed * displacement, 0);
				return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
			}
			ENDHLSL
		}
	}
}
