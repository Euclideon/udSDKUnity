using System;
using System.Runtime.InteropServices;

//! Helper functions related to user management

namespace udSDK
{
  public static class udUserUtil_f
  {
    /// <summary>
    /// Changes the password of a logged in session
    /// </summary>
    /// <param name="pContext">The context for the user to change the password of</param>
    /// <param name="pNewPassword">The password the user wants to have</param>
    /// <param name="pOldPassword">The current password for the user</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udUserUtil_ChangePassword")]
    public static extern udError ChangePassword(IntPtr pContext, string pNewPassword, string pOldPassword); 
    
    /// <summary>
    /// Attempts to register a user using the simple registration system
    /// </summary>
    /// <param name="pServerURL">The address of the server to register the user on</param>
    /// <param name="pName">The users name</param>
    /// <param name="pEmail">The users email address</param>
    /// <param name="pApplicationName">The name of this application (analytical to help us improve user experience; can be NULL)</param>
    /// <param name="marketingEmailOptIn">Not 0 if the user ACCEPTS being sent marketing material via email</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udUserUtil_Register")]
    public static extern udError Register(string pServerURL, string pName, string pEmail, string pApplicationName, UInt32 marketingEmailOptIn); 
    
    /// <summary>
    /// Sends an email with password reset instructions to the provided email
    /// </summary>
    /// <param name="pServerURL">The address of the server to register the user on</param>
    /// <param name="pEmail">The users email address</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udUserUtil_ForgotPassword")]
    public static extern udError ForgotPassword(string pServerURL, string pEmail); 
  }
}