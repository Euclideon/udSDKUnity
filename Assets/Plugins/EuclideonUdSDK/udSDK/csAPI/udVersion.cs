using System;
using System.Runtime.InteropServices;

//! The **udVersion** object provides an interface to query the version of the loaded udSDK library.

namespace udSDK
{
  /// <summary>
  /// Stores the version information for the loaded udSDK library.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udVersion
  {
    public byte major; //!< The major version part of the library version
    public byte minor; //!< The minor version part of the library version
    public byte patch; //!< The patch version part of the library version
  }

  public static class udVersion_f
  {
    /// <summary>
    /// Populates the provided structure with the version information for the loaded udSDK library.
    /// </summary>
    /// <param name="pVersion">The version structure which will be populated with the version information.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udVersion_GetVersion")]
    public static extern udError GetVersion(IntPtr pVersion);
  }
}
