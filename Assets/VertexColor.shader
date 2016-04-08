Shader "Custom/VertexColor" {
	Properties {
		_PointCloud ("PointCloud", 3D) = "" {}
		_Size ("Size", Float) = 10
		_CoordX ("CoordX", Float) = 0
		_CoordY ("CoordY", Float) = 0
		_CoordZ ("CoordZ", Float) = 0
	}
    SubShader {
	
    Pass {
        LOD 200
        //Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		//Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag alpha
 
        struct VertexInput {
			
            float4 v : POSITION;
            float4 color: COLOR;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
        };

		uniform sampler3D _PointCloud;
		uniform float _Size;
		uniform float _CoordX;
		uniform float _CoordY;
		uniform float _CoordZ;
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            o.pos = mul(UNITY_MATRIX_MVP, v.v);
			float4 coords = float4(_CoordX, _CoordY, _CoordZ, 0);
            o.col = tex3Dlod(_PointCloud, coords);
             
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return o.col;
        }
 
        ENDCG
        } 
    }
 
}