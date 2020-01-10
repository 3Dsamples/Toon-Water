/// This shader would render normals of its GameObjects
/// This is part of the roystan.net "Toon Water Shader tutorial for Unity" https://roystan.net/articles/toon-water.html
/// at the end of the article you can find their Source code https://github.com/IronWarrior/ToonWaterShader
/// where you can find this shader and all the scripts they used for their tutorial

Shader "Custom/Normals Shader"
{
	Properties
    {
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType" = "Opaque" 
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				float3 viewNormal : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.viewNormal = COMPUTE_VIEW_NORMAL;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return float4(i.viewNormal, 0);
            }
            ENDCG
        }
    }
}
