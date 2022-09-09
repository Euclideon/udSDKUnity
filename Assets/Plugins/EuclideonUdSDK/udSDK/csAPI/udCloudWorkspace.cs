using System;
using System.Runtime.InteropServices;

//! The udCloudWorkspace object provides an interface for accessing data of workspace hosted in udCloud

namespace udSDK
{
  /// <summary>
  /// This represents a udCloud Workspace
  /// </summary>
  struct udCloudWorkspace
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public char[] ID; //!< The workspace id from udCloud
    public string pName; //!< The name of the workspace
    public UInt64 permissions; //!< The permissions of this workspace
    public udCloudStorageVolume volume; //!< Hot And Archives volume stored in udCloud
    public UInt32 isPAYG; //!< A flag for PAYG workspace
    public UInt32 isPendingDelete; //!< A flag to know if the workspace will be deleted shortly
  };
  
  public static class udCloudWorkspace_f
  {
    /// <summary>
    /// Get a list of available Workspaces hosted in udCloud
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use.</param>
    /// <param name="ppCloudWorkspaces">The pointer pointer of the udCloudWorkspace.</param>
    /// <param name="pCount">The number of Workspace in ppCloudWorkspaces.</param>
    /// <returns>A udError value based on the result of the query on udCloud.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudWorkspace_GetWorkspaceList")]
    public static extern udError GetWorkspaceList(IntPtr pContext, IntPtr ppCloudWorkspaces, ref int pCount);
    
    /// <summary>
    /// Destroys the list of Workspaces that was created by udCloudWorkspace_GetWorkspaceList
    /// </summary>
    /// <param name="ppCloudWorkspaces">The list of Workspaces to be destroyed.</param>
    /// <param name="count">The number of udCloudWorkspace in ppCloudWorkspaces.</param>
    /// <returns>A udError value based on the memory been freed.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudWorkspace_ReleaseWorkspaceList")]
    public static extern udError ReleaseWorkspaceList(IntPtr ppCloudWorkspaces, int count);
    
    /// <summary>
    /// Get a list of available Projects owned by a specific Workspace in udCloud
    /// </summary>
    /// <param name="pContext">The pointer to the udContext of the session to use.</param>
    /// <param name="pCloudWorkspace">The pointer of the udCouldWorkspace.</param>
    /// <param name="ppCloudProjects">A list of Projects returned.</param>
    /// <param name="pCount">The number of projects in ppCloudProject.</param>
    /// <returns>A udError value based on the result of the query on udCloud.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudWorkspace_GetProjectList")]
    public static extern udError GetProjectList(IntPtr pContext, IntPtr pCloudWorkspace, IntPtr ppCloudProjects, ref int pCount);
    
    /// <summary>
    /// Destroys the list of Projects that was created by udCloudWorkspace_GetProjectList
    /// </summary>
    /// <param name="ppCloudProjects">The list of Projects to be destroyed.</param>
    /// <param name="count">The number of udCloudProject in ppCloudProjects.</param>
    /// <returns>A udError value based on the memory been freed.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udCloudWorkspace_ReleaseProjectList")]
    public static extern udError ReleaseProjectList(IntPtr ppCloudProjects, int count);
  }
}