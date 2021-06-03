Shader "udSDK/Demo/SpriteBillboard" {
    Properties {
        _MainTex ("Texture Image", 2D) = "white" {}
        _ZOffset ("Z Offset", float) = 0.0
    }
    SubShader {
        Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
        Pass {   
            ZWrite On AlphaToMask On
            CGPROGRAM

            #include "UnityCG.cginc"
            #pragma vertex vert  
            #pragma fragment frag
            
            // User-specified uniforms            
            uniform sampler2D _MainTex;        
            uniform float _ZOffset;

            struct vertexInput {
                float4 vertex : POSITION;
                float4 tex : TEXCOORD0;
            };
            struct vertexOutput {
                float4 pos : SV_POSITION;
                float4 tex : TEXCOORD0;
            };
 
            vertexOutput vert(vertexInput input) 
            {
                vertexOutput output;

                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );

                output.pos = mul(UNITY_MATRIX_P,
                    float4(UnityObjectToViewPos((float3)0), 1.0+_ZOffset)
                    + float4(input.vertex.x, input.vertex.y, 0.0, 0.0) * 
                    float4(scale.x, scale.y, 1.0, 1.0)
                );

                output.tex = input.tex;

                return output;
            }
 
            float4 frag(vertexOutput input) : COLOR
            {
                return tex2D(_MainTex, float2(input.tex.xy)) * input.tex.a;   
            }
 
            ENDCG
        }
    }
}