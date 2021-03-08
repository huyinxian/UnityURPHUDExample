Shader "Unlit/HUDSprite"
{
	Properties
	{
		_MainTex ("Alpha (A)", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Off
		Fog { Mode Off }
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{	
			CGPROGRAM
			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			
			v2f vert (appdata_t v)
			{
				v2f o;

				float4 vFinal = float4(v.vertex.xyz, 1.0);
				o.vertex = UnityObjectToClipPos(vFinal);

				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				return col * i.color;
			}
			
			#pragma vertex vert
			#pragma fragment frag
			ENDCG 
		}
	}	
}
