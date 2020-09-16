Shader "Hidden/UDSDK/UDSDKShader"
{
  HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

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

  PS_OUTPUT Frag(VaryingsDefault i)
  {
    PS_OUTPUT output;

#if 1//UNITY_UV_STARTS_AT_TOP
    float2 udUV = float2(i.texcoord.x, 1 - i.texcoord.y);
#else
    float2 udUV = float2(i.texcoord.x, i.texcoord.y);
#endif

    float4 ud = _udCol.Sample(my_point_clamp_sampler, udUV).bgra;
    float4 color = _MainTex.Sample(my_point_clamp_sampler, i.texcoord);

    float depthCam = tex2D(_CameraDepthTexture, i.texcoord).r;
    float depthVDK = (_udDep.Sample(my_point_clamp_sampler, udUV).r * 0.5 + 0.5);

#if UNITY_REVERSED_Z
    //depthCam = 1.0 - depthCam;
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
//#if UNITY_REVERSED_Z
      output.Depth0 = depthVDK;
//#else
      //output.Depth0 = -depthVDK - 1;
//#endif
    }

    return output;
  }

  ENDHLSL

  SubShader
  {
    Cull Off ZWrite On ZTest Always

      Pass
    {
        HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

        ENDHLSL
    }
  }
}