Shader "Unlit/GradientShader"
{
	Properties
	{
		_Dir("Direction", Vector) = (0.5, 0.5, 0, 0)
		_ColorA("ColorA", Color) = (1,0,0,1)
		_ColorB("ColorB", Color) = (0,1,0,1)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float4 _Dir;
			fixed4 _ColorA;
			fixed4 _ColorB;

			struct vInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct pInput
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			pInput vert(vInput inp)
			{
				pInput outp;
				outp.pos = mul(UNITY_MATRIX_MVP, inp.pos);
				outp.uv = inp.uv;
				return outp;
			}

			fixed4 frag(pInput inp) : SV_Target
			{
				float k = 0.5+dot(inp.uv-float2(0.5, 0.5), normalize(_Dir));
				half4 res = lerp(_ColorA, _ColorB, k);
				if (length(inp.uv - float2(0.5, 0.5)) > 0.5) res = half4(0, 0, 0, 0);
				return res;
			}
			ENDCG
		}
	}
}