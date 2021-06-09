using System;
using System.Runtime.InteropServices;
using UnityEngine;

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
}
