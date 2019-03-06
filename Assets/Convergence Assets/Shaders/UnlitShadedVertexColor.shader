//Stauffer for 3DHM
//Custom shader to do unlit coloring based on vertex color, with shading based on vertex position within its quad,
// and no transparency
//Based on https://docs.unity3d.com/Manual/SL-VertexProgramInputs.html
Shader "Custom/UnlitShadedVertexColor" {

	Properties{
		_BodyShadeAmount("Body Shade Amount", Range(0.0,0.9)) = 0.2
		_EdgeShadeAmount("Edge Shade Amount", Range(0.0,0.9)) = 0.2
		_EdgeShadeWidth("Edge Shade Width", Range(0.0,0.5)) = 0.05
		_EdgeShadeHeight("Edge Shade Height", Range(0.0,0.5)) = 0
	}

	SubShader{
	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		float _BodyShadeAmount;
		float _EdgeShadeAmount;
		float _EdgeShadeWidth;
		float _EdgeShadeHeight;

		// vertex input: position, color
		struct appdata {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 uvs : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			fixed4 color : COLOR;
			float2 uvs : TEXCOORD0;
		};

		v2f vert(appdata v) {
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.uvs = v.uvs;

			//simple body shading based on uv's - yields corner-based shading diffs which can yield perceptual difference in adjacent same-colored quads
			float shadeScale = 1.0 - ( (v.uvs.x + v.uvs.y) / 2.0  * _BodyShadeAmount );
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
			float4 color = i.color;
			//distance from an edge
			float dx = 0.5 - abs(0.5 - i.uvs.x);
			if ( _EdgeShadeWidth > 0 && dx < _EdgeShadeWidth)
				color.rgb = color.rgb * (1 - (_EdgeShadeWidth - dx) / _EdgeShadeWidth * _EdgeShadeAmount);
			else {
				float dy = 0.5 - abs(0.5 - i.uvs.y);
				if( _EdgeShadeHeight > 0 && dy < _EdgeShadeHeight)
					color.rgb = color.rgb * (1 - (_EdgeShadeHeight - dy) / _EdgeShadeHeight * _EdgeShadeAmount);
			}
			return color; 
		}
	
		ENDCG
	
	}
	}
}