/// Spidermeowth
/// Mar 20, 2019
/// 
/// This is a custom ProjectorLight shader
/// This shader was modified so that it only projects, below the water level
/// this works together with the "WaterAnimation" script, as "ToonShading + Altitude Fog" and "ToonWater" custom shaders
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth

Shader "Custom/ProjectorLight + Altitude"
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ShadowTex ("Cookie", 2D) = "" {}
		_FalloffTex ("FallOff", 2D) = "" {}
	}
	
	CGINCLUDE
	float4x4 _Projector;
	float4x4 _ProjectorClip;
	fixed4 _Color;
	sampler2D _ShadowTex;
	sampler2D _FalloffTex;	
	uniform fixed _FogMaxHeight;
	uniform fixed _FogMinHeight;		
	uniform fixed _vertex_Y;
	uniform float4 _max;
	uniform float4 _min;
	
	
	bool isInside(float x1, float z1, float x2, float z2, float x, float z) {
		// A point lies inside a rectangle's area if it’s x coordinate lies between the x coordinate of the 
		// given bottom right and top left coordinates of the rectangle and z coordinate lies 
		// between the z coordinate of the given bottom right and top left coordinates.
		if (x > x1 && x < x2 && z > z1 && z < z2) {
			// If it's inside, it's true
			return true;
		}
		
		else{
			// and if not, it is false
			return false;
		}     
	} 
	
		
	struct v2f {
		float4 uvShadow : TEXCOORD0;
		float4 uvFalloff : TEXCOORD1;
		float4 pos : SV_POSITION;		
		float4 worldPosition : TEXCOORD2;
	};
	
	ENDCG
	
	Subshader {
		Tags {"Queue"="Transparent"}
		
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend DstColor One
			Offset -1, -1
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, vertex);
				o.uvShadow = mul (_Projector, vertex);
				o.uvFalloff = mul (_ProjectorClip, vertex);
				o.worldPosition = mul(_Object2World, vertex); // vertex world position
				
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Projector texture
				fixed4 texS = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				texS.rgb *= _Color.rgb;
				texS.a = 1.0-texS.a;
	
				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 res = texS * texF.a;
				
				// Final Color
				fixed4 col;
				
				// if the pixel is under water level and within of our water plane's area
				if(i.worldPosition.y < (_FogMaxHeight + _vertex_Y) && isInside(_min.x, _min.z, _max.x , _max.z, i.worldPosition.x, i.worldPosition.z)){	
					// return projector animation
					col = res;
				}
				
				// if not
				else{
					// don´t project anything
					col = fixed4(0,0,0,0);
				}

				UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(0,0,0,0));
				return col;
			}
			ENDCG
		}
	}
}
