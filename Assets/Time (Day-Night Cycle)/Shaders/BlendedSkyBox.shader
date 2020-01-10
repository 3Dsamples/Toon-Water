/// Spidermeowth
/// Mar 20, 2019
/// 
/// This is a custom skybox shader, simply modified to lerp between two sets of textures 
/// to simulate the passage between day and night
///
/// Note:
/// You can use this script or modify it freely, and if you think it was a good job and it is in your possibilities,
/// you can help me get a better laptop by inviting me a coffee, I will thank you very much
/// https://ko-fi.com/spidermeowth

Shader "Custom/Blended SkyBox"
{
Properties {
	_Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
	[Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
	_Rotation ("Rotation", Range(0, 360)) = 0
	_Blend ("Blend", Range(0.0,1.0)) = 0.5
	[NoScaleOffset] _FrontTex ("Front Day [+Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _BackTex ("Back Day [-Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _LeftTex ("Left Day [+X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _RightTex ("Right Day [-X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _UpTex ("Up Day [+Y]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _DownTex ("Down Day [-Y]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _FrontTex2 ("Front Night [+Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _BackTex2 ("Back Night [-Z]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _LeftTex2 ("Left Night [+X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _RightTex2 ("Right Night [-X]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _UpTex2 ("Up Night [+Y]   (HDR)", 2D) = "grey" {}
	[NoScaleOffset] _DownTex2 ("Down Night [-Y]   (HDR)", 2D) = "grey" {}
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off
	
	CGINCLUDE
	#include "UnityCG.cginc"

	half4 _Tint;
	half _Exposure;
	float _Rotation;

	float4 RotateAroundYInDegrees (float4 vertex, float degrees)
	{
		float alpha = degrees * UNITY_PI / 180.0;
		float sina, cosa;
		sincos(alpha, sina, cosa);
		float2x2 m = float2x2(cosa, -sina, sina, cosa);
		return float4(mul(m, vertex.xz), vertex.yw).xzyw;
	}
	
	struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	struct v2f {
		float4 vertex : SV_POSITION;
		float2 texcoord : TEXCOORD0;
	};
	v2f vert (appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, RotateAroundYInDegrees(v.vertex, _Rotation));
		o.texcoord = v.texcoord;
		return o;
	}
	half4 skybox_frag (v2f i, sampler2D smp, half4 smpDecode)
	{
		half4 tex = tex2D (smp, i.texcoord);
		half3 c = DecodeHDR (tex, smpDecode);
		c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
		c *= _Exposure;
		return half4(c, 1);
	}
	ENDCG
	
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _FrontTex;
		sampler2D _FrontTex2;
		half4 _FrontTex_HDR;
		half4 _FrontTex2_HDR;
		fixed _Blend;		
		half4 frag (v2f i) : SV_Target { return lerp(skybox_frag(i,_FrontTex, _FrontTex_HDR), skybox_frag(i,_FrontTex2, _FrontTex2_HDR), _Blend); }
		ENDCG 
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _BackTex;
		sampler2D _BackTex2;
		half4 _BackTex_HDR;
		half4 _BackTex2_HDR;
		fixed _Blend;
		half4 frag (v2f i) : SV_Target { return lerp(skybox_frag(i,_BackTex, _BackTex_HDR), skybox_frag(i,_BackTex2, _BackTex2_HDR), _Blend); }
		ENDCG 
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _LeftTex;
		sampler2D _LeftTex2;
		half4 _LeftTex_HDR;
		half4 _LeftTex2_HDR;
		fixed _Blend;
		half4 frag (v2f i) : SV_Target { return lerp(skybox_frag(i,_LeftTex, _LeftTex_HDR), skybox_frag(i,_LeftTex2, _LeftTex2_HDR), _Blend); }
		ENDCG
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _RightTex;
		sampler2D _RightTex2;
		half4 _RightTex_HDR;
		half4 _RightTex2_HDR;
		fixed _Blend;
		half4 frag (v2f i) : SV_Target { return lerp(skybox_frag(i,_RightTex, _RightTex_HDR), skybox_frag(i,_RightTex2, _RightTex2_HDR), _Blend); }
		ENDCG
	}	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _UpTex;
		sampler2D _UpTex2;
		half4 _UpTex_HDR;
		half4 _UpTex2_HDR;
		fixed _Blend;
		half4 frag (v2f i) : SV_Target { return lerp(skybox_frag(i,_UpTex, _UpTex_HDR), skybox_frag(i,_UpTex2, _UpTex2_HDR), _Blend); }
		ENDCG
	}	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		sampler2D _DownTex;
		sampler2D _DownTex2;
		half4 _DownTex_HDR;
		half4 _DownTex2_HDR;
		fixed _Blend;
		half4 frag (v2f i) : SV_Target { return lerp(skybox_frag(i,_DownTex, _DownTex_HDR), skybox_frag(i,_DownTex2, _DownTex2_HDR), _Blend ); }
		ENDCG
	}
}
}
