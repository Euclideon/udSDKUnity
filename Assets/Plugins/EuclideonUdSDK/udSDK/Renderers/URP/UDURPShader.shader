Shader "UDSDK/UDSDKShader URP"
{
  Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Color ("Color", Color) = (1,1,1,1)
  }

  SubShader
  {
    Cull Off ZWrite On ZTest LEqual

    Pass
    {
      HLSLPROGRAM
      #pragma vertex vert
      #pragma fragment Frag
      #pragma multi_compile_fog
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      
      Texture2D _MainTex;
      sampler2D _CameraDepthTexture;

      Texture2D _udCol;
      Texture2D _udDep;

      SamplerState my_point_clamp_sampler;

      struct PS_OUTPUT
      {
          float4 Color0 : SV_Target;
          float Depth0 : SV_Depth;
      };

      struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      v2f vert (appdata v) {
        v2f o;
        o.vertex = TransformObjectToHClip(v.vertex);
        o.uv = v.uv;
        return o;
      }

      PS_OUTPUT Frag(v2f i)
      {
        PS_OUTPUT output;

#if 1 //UNITY_UV_STARTS_AT_TOP
        float2 udUV = float2(i.uv.x, 1 - i.uv.y);
#else
        float2 udUV = float2(i.uv.x, i.uv.y);
#endif

        float4 ud = _udCol.Sample(my_point_clamp_sampler, udUV).bgra;
        float4 color = _MainTex.Sample(my_point_clamp_sampler, i.uv);

        float depthCam = tex2D(_CameraDepthTexture, i.uv).r;
        float depthVDK = (_udDep.Sample(my_point_clamp_sampler, udUV).r * 0.5 + 0.5);

#if UNITY_REVERSED_Z
        depthVDK = 1.0 - depthVDK;
        if (depthVDK == 0.0 || depthCam > depthVDK /*&& 0*/)
#else
        if (depthVDK == 1.0 || depthCam < depthVDK )
#endif
          {
            output.Color0 = color;
            output.Depth0 = depthCam;
          }
          else
          {
            output.Color0 = ud;
            output.Depth0 = depthVDK;
          }
          
          return output;
      }
      ENDHLSL
    }
  }
}