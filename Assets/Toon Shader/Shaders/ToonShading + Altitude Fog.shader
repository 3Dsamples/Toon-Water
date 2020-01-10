/// Spidermeowth
/// Ago 21, 2017
/// 
/// This is a custom vert and frag shader, customized to give a Toon appearance
/// This can give an effect of the object is submerged under the water, by rendering a false color fog from a certain height
/// Works in conjunction with the "WaterAnimation" script, to generate some movement in the water, and update the height of the fog,
/// just in case we have different bodies of water, at different heights
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth

Shader "Custom/ToonShading + Altitude Fog"{
	
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Ramp ("Light Toon Ramp (RGB)", 2D) = "gray" {}
		_refValue ("Stencil Ref Value", Range(0, 255)) = 1
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
	}
	
	CGINCLUDE
	
		sampler2D _MainTex;
		float4 _MainTex_ST;
		fixed4 _Color;		
		sampler2D _Ramp;
		uniform fixed4 _FogColor;
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

	
		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float3 worldNormal : TEXCOORD1;
			float4 worldPosition : TEXCOORD2;
		};	
	ENDCG
	
	SubShader
	{
		
		Tags { "Queue"="Geometry-1" "RenderType"="Opaque" "LightMode" = "ForwardBase" "PassFlags" = "OnlyDirectional"}
		
		ZWrite On
		Blend One Zero
		Cull Off	
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			
			v2f vert (appdata v){
			
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal =  UnityObjectToWorldNormal(v.normal); // world normal 
				o.worldPosition = mul(_Object2World, v.vertex); // vertex world position
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Custom lighting function that uses a texture ramp based
				// on angle between light direction and normal
				float3 normal = normalize(i.worldNormal);
				half d = dot(normal, _WorldSpaceLightPos0)*0.5 + 0.5;				
				half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;				
				half3 distanceToLight = _WorldSpaceLightPos0.xyz - i.worldPosition.xyz;
				float attenuation = 1-(distanceToLight / (1 / _LightPositionRange.w));				
				float light = saturate(floor(d * 3) / (2 - 0.5)) * (_LightColor0.rgb * ramp * (attenuation * 2));				
				
				// sample colors and texture
				fixed4 texCol = tex2D(_MainTex, i.uv)*_Color;
				fixed4 shallowCol = texCol*_FogColor;
				fixed4 deepCol = lerp (texCol*_FogColor, _FogColor, 0.15);
				
				// Altitude Fog
				float lerpValue = clamp((i.worldPosition.y - (_FogMinHeight + _vertex_Y)) / ((_FogMaxHeight + _vertex_Y) - (_FogMinHeight + _vertex_Y)), 0, 1);			
				
				// Final Color
				fixed4 col;
				
				// if the pixel is under water level and within of our water plane's area
				if(i.worldPosition.y < (_FogMaxHeight + _vertex_Y) && isInside(_min.x, _min.z, _max.x , _max.z, i.worldPosition.x, i.worldPosition.z)){
					// return underwater color
					col = lerp (deepCol, shallowCol, lerpValue);

				}
				
				// if not
				else{
					// return the texture map
					col = texCol;
				}
				
				return col * (light + unity_AmbientSky);
			}
			ENDCG
		}
	}
}