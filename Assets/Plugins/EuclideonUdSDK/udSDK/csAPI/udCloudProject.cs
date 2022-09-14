using System;
using System.Runtime.InteropServices;

//! The udCloudProject object provide an interface for accessing data of projects hosted in udCloud

namespace udSDK
{
  /// <summary>
  /// The values for Hot and Archive data in udCloud
  /// </summary>
  public struct udCloudStorageVolume
  {
    public double hot; //!< The amount of stored data in Hot 
    public double archive; //!< The amount of stored data in Archive 
  };
  
  /// <summary>
  /// This represents a udCloud Project
  /// </summary>
  public struct udCloudProject
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] ID; //!< The project id from udCloud
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] orgID; //!< The workspace it belongs to from udCloud
    public string pName; //!< The name of the project
    public string pRegion; //!< The region the data is hosted on udCloud
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] createdBy; //!< The user id of the user who created this project
    public string pCreated; //!< The time this project was created
    public UInt64 optionalPermissions; //!< The permissions of this project
    public udCloudStorageVolume volume; //!< Hot And Archives volume stored in udCloud
    public UInt64 processingtime; //!< The processing time used to convert
  };
  
  public class udCloudProject_f
  {
    /// <summary>
    /// Get a list of available Files owned by a specific Project in udCloud
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use</param>
    /// <param name="pCloudProject">The pointer of the udCloudProject.</param>
    /// <param name="ppCloudFiles">A list of Files returned.</param>
    /// <param name="pCount">The number of Files in ppCloudFiles.</param>
    /// <returns>A udError value based on the result of the query on udCloud.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudProject_GetFileList")]
    public static extern udError GetFileList(IntPtr pContext, udCloudProject pCloudProject, ref udCloudFile[] ppCloudFiles, ref int pCount);
    
    /// <summary>
    /// Destroys the list of Files that was created by udCloudProject_GetFileList
    /// </summary>
    /// <param name="ppCloudFiles">The list of Files to be destroyed.</param>
    /// <param name="count">The number of Files in ppCloudFiles.</param>
    /// <returns>A udError value based on the memory been freed.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudProject_ReleaseFileList")]
    public static extern udError ReleaseFileList(ref udCloudFile[] ppCloudFiles, int count);
    
    /// <summary>
    /// Get a list of available scenes owned by a specific Project in udCloud
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use</param>
    /// <param name="pCloudProject">The pointer of the udCloudProject.</param>
    /// <param name="ppCloudScenes">A list of Scenes returned.</param>
    /// <param name="pCount">The number of scenes in ppCloudScenes.</param>
    /// <returns>A udError value based on the result of the query on udCLoud.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudProject_GetSceneList")]
    public static extern udError GetSceneList(IntPtr pContext, udCloudProject pCloudProject, ref IntPtr ppCloudScenes, ref int pCount);
    
    /// <summary>
    /// Destroys the list of Scenes that was created by udCloudProject_GetSceneList
    /// </summary>
    /// <param name="ppCloudScenes">The list of Scenes to be destroyed.</param>
    /// <param name="count">The number of Scenes in ppCloudScenes.</param>
    /// <returns>A udError value based on the memory been freed.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudProject_ReleaseSceneList")]
    public static extern udError ReleaseSceneList(ref IntPtr ppCloudScenes, int count);
  }
}