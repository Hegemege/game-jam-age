Shader "Custom/Flat shading (4 levels)" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color Tint", Color) = (1, 1, 1, 1)
		_Shadows("Shadows(0/1)", Int) = 1
	}
	
	SubShader{
		Tags{
			"RenderType" = "Opaque"
		}

		CGPROGRAM

		#pragma surface surf SimpleLambert

		struct SurfaceOutputCustom {
			fixed3 Albedo;
			fixed3 Normal;
			fixed3 Emission;
			half Specular;
			fixed Gloss;
			fixed Alpha;
			int Shadows;
		};

		half4 LightingSimpleLambert(SurfaceOutputCustom s, half3 lightDir, half atten) {
			half NdotL = dot(s.Normal, lightDir);
			NdotL = ceil(2 * NdotL) * 0.5;
			NdotL = (NdotL + 1) * 0.5;
			half4 c;
			if (s.Shadows == 0) {
				NdotL = 1;
			}
			c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
			c.a = s.Alpha;
			return c;
		}

		struct Input {
			float2 uv_MainTex;
		};

		sampler2D _MainTex; 
		fixed4 _Color;
		int _Shadows;

		void surf(Input IN, inout SurfaceOutputCustom o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
			o.Shadows = _Shadows;
		}

		ENDCG
	}

	Fallback "Diffuse"
}