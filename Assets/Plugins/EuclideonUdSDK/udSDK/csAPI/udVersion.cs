using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
  [StructLayout(LayoutKind.Sequential)]
  public struct udVersion
  {
    public byte major; 
    public byte minor; 
    public byte patch; 
  }

  public class UDVersion
  {
	// with no arguments, this class gets the current sdk version
	// alternatively, you can pass in version numbers to create a version object

    udVersion versionStruct;
    public int major, minor, patch;
    
    public UDVersion()
    { 
      IntPtr pVersion = Marshal.AllocHGlobal(Marshal.SizeOf(versionStruct));
            
      udError error = udVersion_GetVersion(pVersion);
      if (error != udError.udE_Success)
        throw new Exception("Failed to get udVersion.");
            
      versionStruct = (udVersion)Marshal.PtrToStructure<udVersion>(pVersion); 
            
      Marshal.FreeHGlobal(pVersion);
      
      this.major = (int)versionStruct.major;
      this.minor = (int)versionStruct.minor;
      this.patch = (int)versionStruct.patch;
    }
    
    public UDVersion(int majorVersion, int minorVersion, int patchVersion)
    {
      this.major = majorVersion;
      this.minor = minorVersion;
      this.patch = patchVersion;

      versionStruct = new udVersion
      {
        major = (byte)this.major,
        minor = (byte)this.minor,
        patch = (byte)this.patch,
      };
    }

    // Formats a udVersion into a sensible looking string
    public string Format()
    {
      return (string)(major+"."+minor+"."+patch); 
    }

    // Checks for equality
    public bool Equals(UDVersion otherVersion)
    {
      if(major == otherVersion.major && minor == otherVersion.minor && patch == otherVersion.patch)
        return true; 
      return false; 
    }

    // Returns whether or not a udVersion is greater than another version
    // Considers "depth", so you can limit the check to major, major and minor, or major minor and patch
    // Default behaviour is to check against everything : major minor and patch 
    public bool GreaterThan(UDVersion otherVersion, int depth=2)
    {
      if(major > otherVersion.major) 
        return true; 
      if(major < otherVersion.major || depth == 0) 
        return false; 
      if(minor > otherVersion.minor)
        return true; 
      if(minor < otherVersion.minor || depth == 1)
        return false;
      if(patch > otherVersion.patch)
        return true; 
      return false; 
    }

    [DllImport(UDSDKLibrary.name)]
    private static extern udError udVersion_GetVersion(IntPtr pVersion);
  }
}
