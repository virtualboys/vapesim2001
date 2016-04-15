Shader "Custom/RayTrace" {
	Properties {
		_FluidDensity ("FluidDensity", 3D) = "" {}
		_MainTex ("RayData", 2D) = "" {} // ray data
		_EyeOnGrid ("EyeOnGrid", Vector) = (0,0,0,0)
		_GridDim ("GridDim",Vector)=(0,0,0,0)
		_RecGridDim ("RecGridDim",Vector)=(0,0,0,0)
	}

	SubShader {
		BlendOp Add
		Blend SrcAlpha OneMinusSrcAlpha

	Pass {
		
		CGPROGRAM
	
		#pragma vertex vert
        #pragma fragment frag
		#include "UnityCG.cginc"

		uniform sampler3D _FluidDensity;
		uniform sampler2D _MainTex;
		uniform float4 _EyeOnGrid;
		uniform float4 _GridDim;
		uniform float4 _RecGridDim;

		SamplerState _LinearClamp;
	
		struct v2f {
            float4 pos : SV_POSITION;
			float4 texCoord : TEXCOORD0;
        };
		
		void DoSample(float weight,float3 pos, inout float4 color){
			float4 sample;
			float t;
			float OPACITY_MODULATOR=.3f;

			sample = weight * tex3Dlod(_FluidDensity, float4(pos,0));
			sample.a = (sample.r) * OPACITY_MODULATOR;

			t = sample.a * (1.0-color.a);
			color.rgb += t * sample.r*float3(.8,.93,.85);
			color.a += t;
		}

        v2f vert (appdata_base v)
        {
            v2f o;
            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.texCoord = v.texcoord;
            return o;
        }
	
        float4 frag (v2f i) : SV_Target
        {
			float4 rayData = tex2D(_MainTex, i.texCoord.xy);
			if(rayData.a == 0)
				return float4(0,0,0,0);

			float3 stepVec = normalize((rayData.xyz-_EyeOnGrid.xyz)*_GridDim.xyz)*_RecGridDim.xyz*.5f;
			//return float4(rayData.a,rayData.a,rayData.a,rayData.a);
			float stepLength=length(stepVec);
			float4 color;
			while(rayData.w>0&&color.a<.99f){
				
				DoSample(1,rayData.xyz,color);
				rayData.xyz+=stepVec;
				rayData.w-=stepLength;
				
			}
			return color;
        }
	
		ENDCG
	}

	} 
}
