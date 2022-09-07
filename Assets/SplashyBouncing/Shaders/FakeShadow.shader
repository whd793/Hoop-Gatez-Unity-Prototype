Shader "Custom/FakeShadow" {
	Properties {
		_ShadowTex ("Shadow Main Tex", 2D) = "gray" {}
		_FalloffTex ("FallOff Text", 2D) = "white" {}
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 ShadowTex : TEXCOORD0;
				float4 FalloffTex : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.ShadowTex = mul (unity_Projector, vertex);
				o.FalloffTex = mul (unity_ProjectorClip, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.ShadowTex));
				texS.a = 1.0-texS.a;

				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.FalloffTex));
				fixed4 res = lerp(fixed4(1,1,1,0), texS, texF.a);

				UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(1,1,1,1));
				return res;
			}
			ENDCG
		}
	}
}
