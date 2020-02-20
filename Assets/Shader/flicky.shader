Shader "Unlit/flicky"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainColor ("Main Color", Color) = (1, 1, 1, 1)
		_BlinkColor ("Blink Color", Color) = (0, 0, 0, 1)
		_FlickerFrequnecy ("Flicker Frequnecy", Float) = 1
		_Phase ("Phase", Float) = 0
		_BlinkThreshold ("Blink Threshold - (units: positiv sin amplitude)", Float) = 0
		_StartTime ("Start Time", Float) = 0
		_Phase ("Phase", Float) = 0
		_Amplitude ("Amplitude", Float) = 1
		_ModulationFrequency ("Modulation Frequency", Float) = -1
		_Modulation ("Modulation", Int) = 0
		//_DebugValue("Debug Value", Float) = 0

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainColor;
			float4 _BlinkColor;
			float _FlickerFrequnecy;
			float _BlinkThreshold;
			float _StartTime;
			float _Phase = 0;
			float _ModulationFrequency = -1;
			int _Modulation = 0;
			float _Amplitude = 1;
			//float _DebugValue;


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				float twoPi = 6.2831853072;
				float localWaveForm;
				
				if (_Modulation == 0 || _ModulationFrequency == -1) {
					localWaveForm = cos((_Time.y - _StartTime) * twoPi * _FlickerFrequnecy + _Phase);
				}
				else if (_Modulation == 1) {
					localWaveForm = cos((_Time.y - _StartTime) * twoPi * _FlickerFrequnecy + _Phase);
					localWaveForm *= cos((_Time.y - _StartTime) * twoPi * _ModulationFrequency + _Phase);
				}

				//_DebugValue = localWaveForm;
				
				
				if (localWaveForm < _BlinkThreshold) {
					col *= _BlinkColor;
				}
				else {
					col *= _MainColor;
				}
				
				
				/*
				localWaveForm = (localWaveForm + 1) * 0.5;
				col *= _MainColor;
				col *= (1, 1, 1, localWaveForm);
				*/
				return col;
			}
			ENDCG
		}
	}
}
