Shader "Hidden/VDK/VDKShader"
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

    float2 udUV = float2(i.texcoord.x, 1 - i.texcoord.y);

    float4 ud = SAMPLE_TEXTURE2D(_udCol, my_point_clamp_sampler, udUV).bgra;
    float4 color = SAMPLE_TEXTURE2D(_MainTex, my_point_clamp_sampler, i.texcoord);

    float depthCam = tex2D(_CameraDepthTexture, i.texcoord).r;
    float depthVDK = 1.0 - (SAMPLE_TEXTURE2D(_udDep, my_point_clamp_sampler, udUV).r * 0.5 + 0.5);

    if (depthVDK == 0.0 || depthCam > depthVDK)
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
