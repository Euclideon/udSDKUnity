using System;
using System.Runtime.InteropServices;

namespace udSDK
{
  static class UDSDKLibrary
  {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public const string name = "udSDK";
#else
    public const string name = "udSDK";
#endif
  }

  // Note: not in the header, but invented to handle the repeated pattern for throwing errors when not successful
  public static class udGuard
  {
    public static void Error(udError error, string functionCall)
    {
      if (error != udError.udE_Success)
        throw new Exception(functionCall + " failed. : " + error);
    }
    
    public static void Null(object guardVariable, string variableName)
    {
      if (guardVariable == null)
        throw new Exception(variableName + " is null.");
    }
    
    public static void NullPtr(IntPtr guardPointer, string pointerName)
    {
      if (guardPointer == IntPtr.Zero)
        throw new Exception(pointerName + " is not initialized.");
    }
  }
}
