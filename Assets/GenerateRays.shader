Shader "Custom/GenerateRays" {
	Properties {
		_FluidDensity ("FluidDensity", 3D) = "" {}
	}

	SubShader {

	// get exit depth
	Pass {
		Cull Front
		ZWrite Off
		LOD 200
		
		CGPROGRAM
	
		#pragma vertex vert
        #pragma fragment frag
		#include "UnityCG.cginc"
	
		struct v2f {
            float4 pos : SV_POSITION;
			float4 viewPos : TEXCOORD1;
        };
	
        v2f vert (appdata_base v)
        {
            v2f o;
            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.viewPos = mul (UNITY_MATRIX_MV, v.vertex);
            return o;
        }
	
        float4 frag (v2f i) : SV_Target
        {
			float depth =i.pos.z / i.pos.w;
            return float4(depth,depth,depth,1);
        }
	
		ENDCG
	}

	// get entry depth, subtract exit depth
	Pass {
		Cull Back
		ZWrite Off
		//BlendOp Sub
		//Blend One One
		LOD 200
		
		CGPROGRAM
		#pragma vertex vert
        #pragma fragment frag
		#include "UnityCG.cginc"

		uniform sampler3D _FluidDensity;

		struct v2f {
            float4 pos : SV_POSITION;
			float4 texCoord : TEXCOORD0;
			float4 viewPos : TEXCOORD1;
        };

        v2f vert (appdata_base v)
        {
            v2f o;
            o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.viewPos = mul (UNITY_MATRIX_MV, v.vertex);
			o.texCoord = v.texcoord;
            return o;
        }

        float4 frag (v2f i) : SV_Target
        {
			float depth =i.pos.z / i.pos.w;
            //return float4(depth,depth,depth,1);
			return tex3Dlod(_FluidDensity, i.texCoord);
        }

		ENDCG
	}

	} 
}
