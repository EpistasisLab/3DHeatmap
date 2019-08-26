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
		_NanTex("NaN/NoData Texture", 2D) = "white" {}
		_NanTexHeight("Height NaN/NoData Texture", 2D) = "white" {}
		_NanTexAll("All NaN/NoData Texture", 2D) = "white" {}
		_NanColor("Nan Color", Color) = (0.5, 0.5, 0.5, 1.0)
		_LODfarCutoff("LOD far cutoff", Range(0.0,200.0)) = 70
		_LODnearCutoff("LOD near cutoff", Range(0.0,200.0)) = 40
	}

	SubShader{
	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"

		//Global variable set from script
		//
		//The height of the scene floor/bottom/minimum. 
		float _gSceneCornerY;
		//The minimum ridge/bar height, so we can always see the side colors
		float _gMinimumHeight;

		//Material properties
		//
		float _BodyShadeAmount;
		float _EdgeShadeAmount;
		float _EdgeShadeWidth;
		float _EdgeShadeHeight;
		float _LODfarCutoff; //Distance from camera at which to show no details
		float _LODnearCutoff; //Distance from camera at which to start fading out details

		// vertex input: position, color
		struct appdata {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 uvs : TEXCOORD0;
			float2 uvIsANumber : TEXCOORD1;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			fixed4 color : COLOR;
			float2 uvs : TEXCOORD0;
			float2 uvIsANumber : TEXCOORD1;
			float distToCamera : TEXCOORD2;
		};

		v2f vert(appdata v) {
			v2f o;
			
			//2nd uv pair is used to store whether values are number of NaN (1 for true, 0 for false)
			// x = height is a number
			// y = at least one of the three values (height, side, top) is a number 
			float atLeastOneNumber = v.uvIsANumber.y;

			//Force minimum height for vertices above the bottom of graph.
			//_gSceneCornerY is the bottom y pos of the graph.
			//We add a fixed offset to get a minimium height, while still being
			// able to use simple txf scaling in code to adjust max height.
			//However if none of the values at this data position is a valid number,
			// then do 0 height.
			if (v.vertex.y > _gSceneCornerY)
				v.vertex.y += ( _gMinimumHeight * atLeastOneNumber);

			o.pos = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.uvs = v.uvs;
			o.uvIsANumber = v.uvIsANumber;
			o.distToCamera = length(WorldSpaceViewDir(v.vertex));

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

		// texture for Nan/NoData
		sampler2D _NanTex;
		// texture for Nan/NoData in height variable
		sampler2D _NanTexHeight;
		sampler2D _NanTexAll;
		// texture for showing no/low detail - just grey
		float4 _NanColor;

		fixed4 frag(v2f i) : SV_Target{
			float4 color = i.color;

			//LOD scale. 1 = do details, 0 is no details.
			float LODscale = 1 - clamp( (i.distToCamera - _LODnearCutoff) / (_LODfarCutoff - _LODnearCutoff), 0, 1);

			//If all values are NaN, use this texture
			if (i.uvIsANumber.y == 0) 
			{
				color = tex2D(_NanTexAll, i.uvs) * LODscale + _NanColor * (1 - LODscale);
				return color;
			}

			//If side/top is NaN/NoData, use special texture
			//We're using color alpha to code for NaN for top and side colors
			if (color.a == 0) 
			{
				color = tex2D(_NanTex, i.uvs) * LODscale + _NanColor * (1 - LODscale);
			}

			//If height value is NaN, overlay a texture for that.
			//Using 2nd set of uvs to track this.
			if (i.uvIsANumber.x == 0) 
			{
				//Overlay the opaque parts from the texture
				float4 tex = tex2D(_NanTexHeight, i.uvs) ;
				if (tex.a > 0)
					color.rgb = tex.rgb * LODscale + _NanColor.rgb * (1 - LODscale);
			}

			//Draw some simple edges based on uv values
			//
			//dx - distance from an edge
			float dx = 0.5 - abs(0.5 - i.uvs.x);
			if ( _EdgeShadeWidth > 0 && dx < _EdgeShadeWidth)
				color.rgb = color.rgb * (1 - (_EdgeShadeWidth - dx) / _EdgeShadeWidth * _EdgeShadeAmount * LODscale);
			else {
				float dy = 0.5 - abs(0.5 - i.uvs.y);
				if( _EdgeShadeHeight > 0 && dy < _EdgeShadeHeight)
					color.rgb = color.rgb * (1 - (_EdgeShadeHeight - dy) / _EdgeShadeHeight * _EdgeShadeAmount * LODscale);
			}
			
			return color; 
		}
	
		ENDCG
	
	}
	}
}