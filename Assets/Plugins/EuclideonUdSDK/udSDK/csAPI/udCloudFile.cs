using System;
using System.Runtime.InteropServices;

//! The udCloudFile object provide an interface for accessing data of files hosted in udCloud

namespace udSDK
{
  /// <summary>
  /// This represents a udCloud File
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udCloudFile
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] ID; //!< The id from udCloud
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] projID; //!< The project id it belongs to from udCloud
    public string pPath; //!< The path of the udCloud File
    public string pCreated; //!< The time this udCloud File was created
    public string pTier; //!< The tier where this udCloud File is stored in udCloud
    public string pTimeToLive;//!< If the udCloud File will be auto-deleted this is the timestamp for when that will occur
    public UInt64 size; //!< The size of the udCloud File
    public UInt64 isFolder; //!< != 0 if the udCloud File is a folder
    public UInt64 isShared; //!< != 0 if the udCloud File is shared
  };
}