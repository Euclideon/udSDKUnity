using System;
using System.Runtime.InteropServices;

//! The udCloudScene object provide an interface for accessing data of Scenes hosted in udCloud

namespace udSDK
{
  /// <summary>
  /// This represents a udCloud Scene
  /// </summary>
  public struct udCloudScene
  {
    public string pName; //!< The name of the Scene
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] ID; //!< The sceneid from udCloud
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] orgID; //!< The workspace it belongs to from udCloud
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] projID; //!< The project it belongs to from udCloud
    public string pRegion; //!< The region the data is hosted on udCloud
    public string pCreated; //!< The time this scene was created
    public string pLastUpdated; //!< The time this scene was last updated
    public string pDeletedTime; //!< The time this scene was deleted
    public UInt32 isShared; //!< Is the scene shared
    public string pShortcode; //!< The short code for this scene
  };
}