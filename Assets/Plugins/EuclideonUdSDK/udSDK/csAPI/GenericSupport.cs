using System;
using System.Runtime.InteropServices;

/*
 * This file contains any helper classes from the wrapper that are compatible with the ordinary C# platform
 * The comments will identify if they are compatible with Unity or not 
 */
namespace udSDK
{
  /// <summary>
  /// Helper class for udAttributeSet
  /// </summary>
  public class UDAttributeSet {
    private udAttributeSet set;
    
    int GetStandardOffset(udStdAttribute attribute) 
    {
      IntPtr pOffset = new IntPtr();
      udAttributeSet_f.GetOffsetOfStandardAttribute(ref set, attribute, pOffset);
      unsafe { 
        return *((int*)pOffset.ToPointer());
      }
    }
    
    int GetNamedOffset(string name) 
    {
      IntPtr pOffset = new IntPtr();
      udAttributeSet_f.GetOffsetOfNamedAttribute(ref set, name, pOffset);
      unsafe 
      { 
        return *((int*)pOffset.ToPointer());
      }
    }
  }
  
  /// <summary>
  /// Helper class for udConvert/udConvertContext
  /// </summary>
  public class UDConvertContext
  {
    public IntPtr pConvertContext;

    public void Create(UDContext context)
    {
      udError error = udConvert_f.CreateContext(context.pContext, ref pConvertContext);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udConvertContext.Create failed.");
    }

    public void Destroy()
    {
      udError error = udConvert_f.DestroyContext(ref pConvertContext);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udConvertContext.Destroy failed.");
    }

    public void AddFile(string fileName)
    {
      udError error = udConvert_f.AddItem(pConvertContext, fileName);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udConvertContext.AddItem failed.");
    }
    public void SetFileName(string fileName)
    {
      udError error = udConvert_f.SetOutputFilename(pConvertContext, fileName);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udConvertContext.SetOutputFilename failed.");
    }

    public void DoConvert()
    {
      udError error = udConvert_f.DoConvert(pConvertContext);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udConvertContext.DoConvert failed.");
    }
  }
  
  /// <summary>
  /// Helper class for udPointCloud
  /// </summary>
  public class UDPointCloud
  {
    public IntPtr pModel = IntPtr.Zero;
    private UDContext context;

    public void Load(UDContext context, string modelLocation, ref udPointCloudHeader header)
    {
      if (context.pContext == IntPtr.Zero) 
        throw new Exception("Point cloud load failed: udContext is not initialised");

      udError error = udPointCloud_f.Load(context.pContext, ref pModel, modelLocation, ref header);
      if (error != udError.udE_Success)
        throw new Exception("udPointCloud.Load " +modelLocation + " failed: " + error.ToString());

      this.context = context;
    }

    public void Unload()
    {
      udError error = udPointCloud_f.Unload(ref pModel);
      if (error != udError.udE_Success)
        throw new Exception("udPointCloud.Unload failed.");
    }

    public void GetMetadata(ref string ppJSONMetadata)
    {
      udError error = udPointCloud_f.GetMetadata(pModel, ref ppJSONMetadata);
      if (error != udError.udE_Success)
        throw new Exception("udPointCloud.GetMetadata failed.");
    }

    public udError GetStreamingStatus()
    {
      return udPointCloud_f.GetStreamingStatus(this.pModel);
    }
  }
  
  /// <summary>
  /// Helper class for udSceneNode
  /// Note: udScene implementation is currently coupled closely with Unity
  /// Compatible with Unity
  /// </summary>
  public class UDSceneNode
  {
    public IntPtr pNode;
    public udSceneNode nodeData;

    public UDSceneNode(IntPtr nodeAddr)
    {
      pNode = nodeAddr;
      this.nodeData = (udSceneNode) Marshal.PtrToStructure(nodeAddr, typeof(udSceneNode));
    }
  }
  
  /// <summary>
  /// Helper struct for udSessionInfo
  /// Changes:
  /// - uint > bool
  /// - char[] > string
  /// Compatible with Unity
  /// </summary>
  public struct UDSessionInfo
  {
    public bool isOffline; //!< Is not 0 if this is an offline session (dongle or other offline license)
    public bool isDomain;  //!< If this is not 0 then this is a domain session (0 is non-domain session)
    public bool isPremium; //!< If this session will have access to premium features

    public float expiresTimestamp; //!< The timestamp in UTC when the session will automatically end

    public string displayName; //!< The null terminated display name of the user
  };
  
  /// <summary>
  /// Helper class for udVersion
  /// Compatible with Unity 
  /// </summary>
  public class UDVersion
  {
    // with no arguments, this class gets the current sdk version
    // alternatively, you can pass in version numbers to create a version object

    udVersion versionStruct;
    public int major, minor, patch;
    
    public UDVersion()
    { 
      IntPtr pVersion = Marshal.AllocHGlobal(Marshal.SizeOf(versionStruct));
            
      udError error = udVersion_f.GetVersion(pVersion);
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
  }
}