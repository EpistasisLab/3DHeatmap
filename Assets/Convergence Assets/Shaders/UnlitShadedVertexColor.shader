//Stauffer for 3DHM
//Custom shader to do unlit coloring based on vertex color, with shading based on vertex position within its quad,
// and no transparency
//Based on https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
Shader "Custom/UnlitShadedVertexColor" {

	Properties{
		_ShadeAmount("Shade Amount", Range(0.0,0.9)) = 0.2

	}

	SubShader{
	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		float _ShadeAmount;

		// vertex input: position, color
		struct appdata {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 uvs : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			fixed4 color : COLOR;
		};

		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;

			//simple shading based on uv's
			float shadeScale = 1.0 - ( (v.uvs.x + v.uvs.y) / 2.0  * _ShadeAmount );
			o.color *= shadeScale;

			//debug uvs
			/*
			o.color.r = v.uvs.x;
			o.color.g = v.uvs.y;
			o.color.b = 0;
			*/

			return o;
		}

		fixed4 frag(v2f i) : SV_Target{
			return i.color; 
		}
	
		ENDCG
	
	}
	}
}