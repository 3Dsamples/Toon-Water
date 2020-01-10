/// Spidermeowth
/// Mar 20, 2019
/// 
/// This is a custom vert and frag shader created to give a simple cartoon-looking water
/// This is a two pass shader, one for draw the water surface, and the second for the foam shoreline 
/// This uses the camera's depth and normals textures to create a foam shoreline.
/// this works together with the "WaterAnimation" script, as "ToonShading + Altitude Fog" and "ProjectorLight + Altitude" custom shaders
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth

Shader "Custom/ToonWater"
{
	Properties
	{
		_ShallowColor("Shallow Color", Color) = (1,1,1,1)
		_DeepColor("Deep Color", Color) = (1,1,1,1)		
		_BumpMap("Surface Distortion", 2D) = "bump" {}
		_Magnitude ("Distortion Magnitude", Range(0,2)) = 0.5
		_SurfaceFoam("Surface Foam ", 2D) = "white" {}
		_SurfMagnitude ("Surface Foam Magnitude", Range(0,1)) = 1
		_FoamColor("Shoreline Color", Color) = (1,1,1,1)		
		_FoamNoise("Shoreline Noise", 2D) = "white" {}		        		
		_FoamCutoff("Shoreline Cutoff", Range(0, 1)) = 0.7		
		_InvFade("Shoreline Fade", Range(0.01,3.0)) = 1.0
		_FoamSmooth("Shoreline Smooth", Range(0.01, 1)) = 0.05
		_FoamMaxDistance("Shoreline Maximum Distance", float) = 0.45
		_FoamMinDistance("Shoreline Minimum Distance", float) = 0.4
		_DepthLevel ("Depth Texture Brigthness", Range(0.1, 3)) = 1.2		
	}	
	
	
	
	CGINCLUDE
		fixed4 _FoamColor;
		fixed4 _ShallowColor;
		fixed4 _DeepColor;		
		sampler2D _BumpMap;
		float4 _BumpMap_ST;
		sampler2D _SurfaceFoam;
		float4 _SurfaceFoam_ST;
		sampler2D _FoamNoise;
		float4 _FoamNoise_ST;
		fixed  _Magnitude;
		fixed _SurfMagnitude;
		sampler2D_float _CameraDepthTexture;
		float4 _CameraDepthTexture_TexelSize;			
		fixed _InvFade;
		fixed _DepthLevel;		
		fixed _FoamCutoff;
		fixed _FoamSmooth;
		fixed _FoamMaxDistance;
		fixed _FoamMinDistance;
		sampler2D _CameraNormalsTexture;	
		uniform fixed _vertex_Y;

		
		
		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
			float3 normal : NORMAL;	
		};

		struct v2f
		{
			float4 vertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 uv1 : TEXCOORD1;
			float4 screenPosition : TEXCOORD2;
			float2 eyeDepth : TEXCOORD3;		
			float3 viewNormal : TEXCOORD4;			
			float2 surfaceFoam : TEXCOORD05;			
			float4 worldPosition : TEXCOORD6;
		};
	
	ENDCG
	
	
	SubShader
	{
		Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Transparent-120"}
		
		
		ZWrite On
		ColorMask RGBA
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		
		
		Pass
		{
			Name "WaterPass"
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog			
			#include "UnityCG.cginc"			
			#define SMOOTHSTEP_AA _FoamSmooth

			
			v2f vert (appdata v) {
			
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				
				// Compute vertex displacement with a sine wave
				half4 wpos = mul(_Object2World, v.vertex);
				wpos.y += _vertex_Y;
				v.vertex = mul(_World2Object, wpos);				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);		
				
				// Compute first bump uv
				o.uv = TRANSFORM_TEX(v.uv, _BumpMap);
				o.uv.x += _Time.y*0.033;
				o.uv.y += _Time.y*0.034;
				
				// Compute second bump uv
				o.uv1 = TRANSFORM_TEX(v.uv, _BumpMap);
				o.uv1.x -= _Time.y*0.032;
				o.uv1.y -= _Time.y*0.035;

				// Compute surface foam uv
				o.surfaceFoam = TRANSFORM_TEX(v.uv, _SurfaceFoam);				
				
				// Compute Screenposition
				o.screenPosition = ComputeScreenPos(o.vertex);
				
				// Compute eye space depth of the vertex
				COMPUTE_EYEDEPTH(o.eyeDepth);
				
				// View Normal
				o.viewNormal = COMPUTE_VIEW_NORMAL;
				
				// vertex world position
				o.worldPosition = mul(_Object2World, v.vertex); 
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target	{

				// Sampling normal
				fixed3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition));				
				// Get dot product between view normal and existing normal
				half normalDot = saturate(dot(existingNormal, i.viewNormal));				
				
				// Sampling surface foam
				half4 surfFoam = tex2D(_SurfaceFoam, i.surfaceFoam);				
				
				// Sampling depth				
				//Actual distance to the camera
                half cameraDist = i.screenPosition.z;				
				// Actual eye space depth
				half eyeDepth = i.eyeDepth;				
				// Get depth from depth texture				
				fixed depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (i.screenPosition.xy / i.screenPosition.w));				
				// This is a factor that takes into account the height of the camera,
				// as higher the camera is, greater it will be, and as it gets closer to the surface, it will be much smaller
				fixed heightfactor = abs((_WorldSpaceCameraPos.y * 0.01)/(i.worldPosition.y + 2));
				// Get the water depth difference
				// We get the depth difference as a function of the height and distance of the camera, 
				// as farther the camera is, the depth fade out, 
				// and as it draws closer it will draw our shoreline
				// Note: _DepthLevel control the brightness of the Depth Texture
				float sceneDepth = saturate(pow((LinearEyeDepth(depth) - (cameraDist * (1 - (eyeDepth * heightfactor)))), _DepthLevel));			
				
				// Get bump sample
				half4 bump = lerp(tex2D(_BumpMap, i.uv),tex2D(_BumpMap, i.uv1),0.5);
				half3 distortion = UnpackNormal(bump);			
				
				// Depth texture fade limit
				fixed fadeLimit = 1 - (eyeDepth * 0.01);				
				
				// The Water transparency of the water, 
				fixed alpha = lerp((_ShallowColor.a + _DeepColor.a)/2, 0.9, cameraDist * 0.05);				
				
				// Lerp water color between _ShallowColor and _DeepColor
				fixed4 c = lerp(_ShallowColor, _DeepColor, sceneDepth);
				c += (half4(0,distortion.yz, alpha) * _Magnitude) + (surfFoam * _SurfMagnitude);
				
				// Get fade value
				fixed fade = saturate(_InvFade * sceneDepth);
				
				// Foam Texture
				fixed3 w;
				
				// Draw Foam 
				if(fade < fadeLimit){
					// Draw water surface
					w = (c.rgb * fade) + (_ShallowColor * (1 - fade)); 				
				}
				
				else{
					// just draw water texture
					w = c.rgb;
				}
				
				// Set col				
				fixed4 col = fixed4(w, alpha);
				
				return col * unity_AmbientSky;				
			}
			ENDCG
		}
		
		
		Pass
		{
			Name "FoamPass"
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog			
			#include "UnityCG.cginc"			
			#define SMOOTHSTEP_AA _FoamSmooth
			
			v2f vert (appdata v) {
			
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				
				// Compute vertex displacement with a sine wave
				half4 wpos = mul(_Object2World, v.vertex);
				wpos.y += _vertex_Y;
				v.vertex = mul(_World2Object, wpos);				
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);				
				
				// Compute first uv
				o.uv = TRANSFORM_TEX(v.uv, _FoamNoise);
				o.uv.x += _Time.y*0.033;
				o.uv.y += _Time.y*0.034;
				
				// Compute second uv
				o.uv1 = TRANSFORM_TEX(v.uv, _FoamNoise);
				o.uv1.x -= _Time.y*0.032;
				o.uv1.y -= _Time.y*0.035;	
				
				// Compute Screenposition
				o.screenPosition = ComputeScreenPos(o.vertex);				
				
				// Compute eye space depth of the vertex
				COMPUTE_EYEDEPTH(o.eyeDepth);
				
				// View Normal
				o.viewNormal = COMPUTE_VIEW_NORMAL;
				
				// vertex world position
				o.worldPosition = mul(_Object2World, v.vertex); 
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target	{
			
				// Sampling normal
				fixed3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPosition));				
				// Get dot product between view normal and existing normal
				//half3 normalDot = saturate(dot(existingNormal, i.viewNormal));
				half normalDot = saturate(dot(existingNormal, i.viewNormal));
				
				// Sampling depth				
				//Actual distance to the camera
                half cameraDist = i.screenPosition.z;				
				// Actual eye space depth
				half eyeDepth = i.eyeDepth;				
				// Get depth from depth texture				
				fixed depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, (i.screenPosition.xy / i.screenPosition.w));
				// This is a factor that takes into account the height of the camera,
				// as higher the camera is, greater it will be, and as it gets closer to the surface, it will be much smaller
				fixed heightfactor = abs((_WorldSpaceCameraPos.y * 0.01)/(i.worldPosition.y + 2));
				
				// Get the water depth difference
				// We get the depth difference as a function of the height and distance of the camera, 
				// as farther the camera is, the depth fade out, 
				// and as it draws closer it will draw our shoreline
				// Note: _DepthLevel control the brightness of the Depth Texture
				float sceneDepth = saturate(pow((LinearEyeDepth(depth) - (cameraDist * (1 - (eyeDepth * heightfactor)))), _DepthLevel));			
				// we discard the pixels that could give us problems
				if(sceneDepth > 0.99 || normalDot < 0.5 && abs(sceneDepth - _CameraDepthTexture_TexelSize.y) < 0.2){
					discard;
				} 
				
				
				// Sampling Noise
				fixed foamNoiseSample = lerp(tex2D(_FoamNoise, i.uv), tex2D(_FoamNoise, i.uv1), 0.5);
				// Adjust the foamDistance depending on the normal
				fixed foamDistance = lerp(_FoamMinDistance, _FoamMaxDistance, normalDot);				
				// Adjust the foam width
				fixed foamDepthDifference = saturate(sceneDepth / foamDistance);				
				// Apply noise cutoff
				fixed foamCutoff = foamDepthDifference * _FoamCutoff;
				// Smooth foam noise
				fixed foamNoise = smoothstep(foamCutoff - SMOOTHSTEP_AA, foamCutoff + SMOOTHSTEP_AA, foamNoiseSample);
				
				// Depth texture fade limit
				fixed fadeLimit = 1 - (eyeDepth * 0.01);				
				
				// Get fade value
				fixed fade = saturate(_InvFade * sceneDepth);
				
				
				// Foam value
				fixed3 f;
				// Aplha value
				fixed a;
				
				// Draw Foam 
				if(fade < fadeLimit){
					// Draw foam
					f = _ShallowColor.rgb + (foamNoise * _FoamColor.rgb);					
					// Set alpha chanel
					a = 1 - sceneDepth;
				}
				
				else{
					// Set alpha to zero
					a = 0;
				}
				
				// Set col				
				fixed4 col = fixed4(f, a * _FoamColor.a);
				clip(col.a - 0.6);

				return col * unity_AmbientSky;
			}
			ENDCG
		}
	}
}
