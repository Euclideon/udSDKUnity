
using System;
using System.Runtime.InteropServices;
using UnityEngine;


namespace Vault
{
  static class VaultSDKLibrary
  {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public const string name = "vaultSDK";
#else
        public const string name = "libvaultSDK";
#endif
  }
  public enum vdkError
  {
    vE_Success, //!< Indicates the operation was successful

    vE_Failure, //!< A catch-all value that is rarely used, internally the below values are favored
    vE_InvalidParameter, //!< One or more parameters is not of the expected format
    vE_InvalidConfiguration, //!< Something in the request is not correctly configured or has conflicting settings
    vE_InvalidLicense, //!< The required license isn't available or has expired
    vE_SessionExpired, //!< The Vault Server has terminated your session

    vE_NotAllowed, //!< The requested operation is not allowed (usually this is because the operation isn't allowed in the current state)
    vE_NotSupported, //!< This functionality has not yet been implemented (usually some combination of inputs isn't compatible yet)
    vE_NotFound, //!< The requested item wasn't found or isn't currently available
    vE_NotInitialized, //!< The request can't be processed because an object hasn't been configured yet

    vE_ConnectionFailure, //!< There was a connection failure
    vE_MemoryAllocationFailure, //!< VDK wasn't able to allocate enough memory for the requested feature
    vE_ServerFailure, //!< The server reported an error trying to fufil the request
    vE_AuthFailure, //!< The provided credentials were declined (usually username or password issue)
    vE_SecurityFailure, //!< There was an issue somewhere in the security system- usually creating or verifying of digital signatures or cryptographic key pairs
    vE_OutOfSync, //!< There is an inconsistency between the internal VDK state and something external. This is usually because of a time difference between the local machine and a remote server

    vE_ProxyError, //!< There was some issue with the provided proxy information (either a proxy is in the way or the provided proxy info wasn't correct)
    vE_ProxyAuthRequired, //!< A proxy has requested authentication

    vE_OpenFailure, //!< A requested resource was unable to be opened
    vE_ReadFailure, //!< A requested resourse was unable to be read
    vE_WriteFailure, //!< A requested resource was unable to be written
    vE_ParseError, //!< A requested resource or input was unable to be parsed
    vE_ImageParseError, //!< An image was unable to be parsed. This is usually an indication of either a corrupt or unsupported image format

    vE_Pending, //!< A requested operation is pending.
    vE_TooManyRequests, //!< This functionality is currently being rate limited or has exhausted a shared resource. Trying again later may be successful
    vE_Cancelled, //!< The requested operation was cancelled (usually by the user)

    vE_Count //!< Internally used to verify return values
  };

  public enum RenderViewMatrix
  {
    Camera,     // The local to world-space transform of the camera (View is implicitly set as the inverse)
    View,       // The view-space transform for the model (does not need to be set explicitly)
    Projection, // The projection matrix (default is 60 degree LH)
    Viewport,   // Viewport scaling matrix (default width and height of viewport)

    Count,
  };

  public enum LicenseType
  {
    Render,
    Convert,

    Count
  };

  public enum StandardAttribute
  {
    GPSTime,
    ARGB,
    Normal,
    Intensity,
    NIR,
    ScanAngle,
    PointSourceID,
    Classification,
    ReturnNumber,
    NumberOfReturns,
    ClassificationFlags,
    ScannerChannel,
    ScanDirection,
    EdgeOfFlightLine,
    ScanAngleRank,
    LASUserData,

    Count
  };

  public enum StandardAttributeContent
  {
    None = (0),
    GPSTime = (1 << StandardAttribute.GPSTime),
    ARGB = (1 << StandardAttribute.ARGB),
    Normal = (1 << StandardAttribute.Normal),
    Intensity = (1 << StandardAttribute.Intensity),
    NIR = (1 << StandardAttribute.NIR),
    ScanAngle = (1 << StandardAttribute.ScanAngle),
    PointSourceID = (1 << StandardAttribute.PointSourceID),
    Classification = (1 << StandardAttribute.Classification),
    ReturnNumber = (1 << StandardAttribute.ReturnNumber),
    NumberOfReturns = (1 << StandardAttribute.NumberOfReturns),
    ClassificationFlags = (1 << StandardAttribute.ClassificationFlags),
    ScannerChannel = (1 << StandardAttribute.ScannerChannel),
    ScanDirection = (1 << StandardAttribute.ScanDirection),
    EdgeOfFlightLine = (1 << StandardAttribute.EdgeOfFlightLine),
    ScanAngleRank = (1 << StandardAttribute.ScanAngleRank),
    LasUserData = (1 << StandardAttribute.LASUserData),
  };

  [StructLayout(LayoutKind.Sequential)]
  public struct vdkRenderInstance
  {
    public IntPtr pointCloud;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public double[] worldMatrix;

    public int modelFlags;

    public IntPtr filter;
    public IntPtr voxelShader;
    public IntPtr voxelUserData;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct vdkAttributeSet
  {
    public StandardAttributeContent standardContent; //!< Which standard attributes are available (used to optimize lookups internally), they still appear in the descriptors
    public uint count; //!< How many vdkAttributeDescriptor objects are used in pDescriptors
    public uint allocated; //!< How many vdkAttributeDescriptor objects are allocated to be used in pDescriptors
    public IntPtr pDescriptors; //!< this contains the actual information on the attributes
  };
  [StructLayout(LayoutKind.Sequential)]
  public struct vdkAttributeDescriptor
  {

  };

  public class AttributeSet {
    private vdkAttributeSet set;

    int GetStandardOffset(StandardAttribute attribute) 
    {
      IntPtr pOffset = new IntPtr();
      vdkAttributeSet_GetOffsetOfStandardAttribute(ref set, attribute, pOffset);
      unsafe { 
        return *((int*)pOffset.ToPointer());
      }
    }

    int GetNamedOffset(string name) 
    {
      IntPtr pOffset = new IntPtr();
      vdkAttributeSet_GetOffsetOfNamedAttribute(ref set, name, pOffset);
      unsafe 
      { 
        return *((int*)pOffset.ToPointer());
      }
    }
    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkAttributeSet_GetOffsetOfStandardAttribute(ref vdkAttributeSet pAttributeSet, StandardAttribute attribute, IntPtr pOffset);
    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkAttributeSet_GetOffsetOfNamedAttribute(ref vdkAttributeSet pAttributeSet, string pName, IntPtr pOffset);
    
  }

    [StructLayout(LayoutKind.Sequential)]
    public struct vdkPointCloudHeader
    {
        public double scaledRange; //!< The point cloud's range multiplied by the voxel size
        public double unitMeterScale; //!< The scale to apply to convert to/from metres (after scaledRange is applied to the unitCube)
        public uint totalLODLayers; //!< The total number of LOD layers in this octree
        public double convertedResolution; //!< The resolution this model was converted at
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] storedMatrix; //!< This matrix is the 'default' internal matrix to go from a unit cube to the full size

        public vdkAttributeSet attributes;   //!< The attributes contained in this pointcloud

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] baseOffset; //!< The offset to the root of the pointcloud in unit cube space
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] pivot; //!< The pivot point of the model, in unit cube space
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] boundingBoxCenter; //!< The center of the bounding volume, in unit cube space
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] boundingBoxExtents; //!< The extents of the bounding volume, in unit cube space  }
    }

    /*Contains information returned by the picking system
     */
    [StructLayout(LayoutKind.Sequential)]
    unsafe public struct vdkRenderPicking
    {
        public UInt32 x;//view space mouse x
        public UInt32 y;//view space mouse y
        public Byte hit;//true if voxel was hit by this pick
        public Byte isHighestLOD;//true if hit was as accurate as possible
        public UInt32 modelIndex; //index of the model in the array hit by this pick
        public fixed double pointCenter[3]; //location of the point hit by the pick
        public UInt64 voxelID; //ID of the hit voxel
    }

    public enum vdkRenderFlags
    {
        vdkRF_None = (0),
        vdkRF_Preserve_Buffers = 1 << 0,
        vdkRF_ComplexIntersections = 1 << 1,
        vdkRF_BlockingStreaming = 1 << 2
    }

    public enum vdkRenderContextPointMode
    {
        vdkRCPM_Rectangles,
        vdkRCPM_Cubes,
        vdkRCPM_Points,
        vdkRCPM_Count
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct vdkRenderOptions
    {
        public vdkRenderFlags flags; //optional flags providing information on how to perform the render
        public IntPtr pPick;
        //public vdkRenderPicking pPick;
        public vdkRenderContextPointMode pointMode;
        public IntPtr pFilter;//pointer to a vdkQueryFilter
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct vdkLicenseInfo
    {
        public Int64 queuePosition;
        public UInt64 expiresTimestamp;
    }

    public class vdkContext
    {
        ~vdkContext()
        {
            if (pContext != IntPtr.Zero) {
                //this does not need to be called currently:
                //Disconnect();

            }

        }

        public void Connect(string pURL, string pApplicationName, string pUsername, string pPassword)
        {
            vdkError error = vdkContext.vdkContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, true);
            if (error != vdkError.vE_Success)
                error = vdkContext_Connect(ref pContext, pURL, pApplicationName, pUsername, pPassword);
            if (error == Vault.vdkError.vE_ConnectionFailure)
                throw new Exception("Could not connect to server.");
            else if (error == Vault.vdkError.vE_AuthFailure)
                throw new Exception("Username or Password incorrect.");
            else if (error == Vault.vdkError.vE_OutOfSync)
                throw new Exception("Your clock doesn't match the remote server clock.");
            else if (error == Vault.vdkError.vE_SecurityFailure)
                throw new Exception("Could not open a secure channel to the server.");
            else if (error == Vault.vdkError.vE_ServerFailure)
                throw new Exception("Unable to negotiate with server, please confirm the server address");
            else if (error != Vault.vdkError.vE_Success)
                throw new Exception("Unknown error occurred: " + error.ToString() + ", please try again later.");
        }

        public void KeepAlive()
        {
            vdkError error = vdkContext_KeepAlive(pContext);
            if (error != vdkError.vE_Success)
            {
                throw new Exception("Unable to keep session alive: " + error.ToString());
            }
        }

        public void Try_Resume(string pURL, string pApplicationName, string pUsername, bool tryDongle)
        {

            vdkError error = vdkContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, tryDongle);
            if (error != vdkError.vE_Success)
            {
                throw new Exception("Unable to keep session alive: " + error.ToString());
            }
        }
        public void Disconnect(bool endSession = false)
        {
            vdkError error = vdkContext_Disconnect(ref pContext, endSession);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkContext.Disconnect failed.");
        }

        public void GetLicenseInfo(LicenseType type, ref vdkLicenseInfo info)
        {
            vdkError error = vdkContext_GetLicenseInfo(pContext, type, ref info);
            if (error != Vault.vdkError.vE_Success && error != Vault.vdkError.vE_InvalidLicense)
                throw new Exception("vdkContext.GetLicenseInfo failed: " + error.ToString());
        }

        public void RequestLicense(LicenseType type)
        {
            vdkError error = vdkContext_RequestLicense(pContext, type);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("VDK License Error: " + error.ToString());
        }

        public void RenewLicense(LicenseType type)
        {
            vdkError error = vdkContext_RenewLicense(pContext, type);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("VDK License Error: " + error.ToString());
        }

        public IntPtr pContext = IntPtr.Zero;

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_RenewLicense(IntPtr pContext, LicenseType type);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_TryResume(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, bool tryDongle);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_Connect(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, string pPassword);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_Disconnect(ref IntPtr ppContext, bool endSession);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_RequestLicense(IntPtr pContext, LicenseType licenseType);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_GetLicenseInfo(IntPtr pContext, LicenseType licenseType, ref vdkLicenseInfo info);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkContext_KeepAlive(IntPtr pContext);
    }

    public class vdkRenderContext
    {
        public IntPtr pRenderer = IntPtr.Zero;

        private vdkContext context;
        ~vdkRenderContext()
        {
            Destroy();
        }
        public void Create(vdkContext context)
        {
            //ensure we destroy the existing context if we are creating a new one:
            if (pRenderer != IntPtr.Zero)
                Destroy();

            if (context.pContext == IntPtr.Zero)
                throw new Exception("context not instantiatiated");

            vdkError error = vdkRenderContext_Create(context.pContext, ref pRenderer);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderContext.Create failed: " + error.ToString());

            this.context = context;
        }

        public void Destroy()
        {
            if (pRenderer == IntPtr.Zero)
            {
                return;
            }
            vdkError error = vdkRenderContext_Destroy(ref pRenderer);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderContext.Destroy failed: " + error.ToString());

            pRenderer = IntPtr.Zero;
        }

        public void Render(vdkRenderView renderView, vdkRenderInstance[] pModels, int modelCount, RenderOptions options)
        {
            if (modelCount == 0)
                return;

            if (renderView == null)
                throw new Exception("renderView is null");

            if (renderView.pRenderView == IntPtr.Zero)
                throw new Exception("RenderView not initialised");

            if (pRenderer == IntPtr.Zero)
                throw new Exception("renderContext not initialised");

            vdkError error = vdkRenderContext_Render(pRenderer, renderView.pRenderView, pModels, modelCount, options.options);

            if (error != Vault.vdkError.vE_Success)
            {
                Debug.Log("vdkRenderContext.Render failed: " + error.ToString());
            }
            options.pickRendered = true;
        }

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderContext_Create(IntPtr pContext, ref IntPtr ppRenderer);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderContext_Destroy(ref IntPtr ppRenderer);
        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderContext_Render(IntPtr pRenderer, IntPtr pRenderView, vdkRenderInstance[] pModels, int modelCount, [In, Out] vdkRenderOptions options);
    }

    public class vdkRenderView
    {

        ~vdkRenderView()
        {
            Destroy();
        }
        public void Create(vdkContext context, vdkRenderContext renderer, UInt32 width, UInt32 height)
        {
            if (context.pContext == IntPtr.Zero)
                throw new Exception("context not instantiated");

            if (renderer.pRenderer == IntPtr.Zero)
                throw new Exception("renderer not instantiated");

            vdkError error = vdkRenderView_Create(context.pContext, ref pRenderView, renderer.pRenderer, width, height);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderView.Create failed: " + error.ToString());

            this.context = context;
        }

        public void Destroy()
        {
            if (colorBufferHandle.IsAllocated)
                colorBufferHandle.Free();

            if (depthBufferHandle.IsAllocated)
                depthBufferHandle.Free();

            vdkError error = vdkRenderView_Destroy(ref pRenderView);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderView.Destroy failed.");

            pRenderView = IntPtr.Zero;
        }

        public void SetTargets(ref UnityEngine.Color32[] colorBuffer, UInt32 clearColor, ref float[] depthBuffer)
        {
            if (colorBufferHandle.IsAllocated)
                colorBufferHandle.Free();

            if (depthBufferHandle.IsAllocated)
                depthBufferHandle.Free();

            colorBufferHandle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
            depthBufferHandle = GCHandle.Alloc(depthBuffer, GCHandleType.Pinned);

            vdkError error = vdkRenderView_SetTargets(pRenderView, colorBufferHandle.AddrOfPinnedObject(), clearColor, depthBufferHandle.AddrOfPinnedObject());
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderView.SetTargets failed.");
        }

        public void GetMatrix(RenderViewMatrix matrixType, double[] cameraMatrix)
        {
            vdkError error = vdkRenderView_GetMatrix(pRenderView, matrixType, cameraMatrix);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderView.GetMatrix failed.");
        }

        public void SetMatrix(RenderViewMatrix matrixType, double[] cameraMatrix)
        {
            if (pRenderView == IntPtr.Zero)
                throw new Exception("view not instantiated");

            vdkError error = vdkRenderView_SetMatrix(pRenderView, matrixType, cameraMatrix);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkRenderView.SetMatrix failed: " + error.ToString());
        }

        public IntPtr pRenderView = IntPtr.Zero;
        private vdkContext context;

        private GCHandle colorBufferHandle;
        private GCHandle depthBufferHandle;

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderView_Create(IntPtr pContext, ref IntPtr ppRenderView, IntPtr pRenderer, UInt32 width, UInt32 height);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderView_Destroy(ref IntPtr ppRenderView);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderView_SetTargets(IntPtr pRenderView, IntPtr pColorBuffer, UInt32 colorClearValue, IntPtr pDepthBuffer);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderView_GetMatrix(IntPtr pRenderView, RenderViewMatrix matrixType, double[] cameraMatrix);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkRenderView_SetMatrix(IntPtr pRenderView, RenderViewMatrix matrixType, double[] cameraMatrix);
    }

    public class RenderOptions
    {
        private unsafe vdkRenderPicking pick;
        public vdkRenderOptions options;
        public bool pickSet = false;
        public bool pickRendered = false;

        public RenderOptions(vdkRenderContextPointMode pointMode, vdkRenderFlags flags)
        {
            options.pointMode = pointMode;
            //this will need to change once support for multiple picks is introduced:
            options.pPick = Marshal.AllocHGlobal(Marshal.SizeOf(pick));
            options.flags = flags;
        }

        public RenderOptions() : this(vdkRenderContextPointMode.vdkRCPM_Rectangles, vdkRenderFlags.vdkRF_None)
        {

        }

        public void setPick(uint x, uint y)
        {
            pick = new vdkRenderPicking();
            pick.x = x;
            pick.y = y;
            Marshal.StructureToPtr(pick, options.pPick, true);

            pickRendered = false;
            pickSet = true;

        }

        public unsafe vdkRenderPicking Pick
        {
            get
            {
                if (!pickSet)
                    return new vdkRenderPicking();

                if (!pickRendered)
                    throw new Exception("Render must be called before pick can be read");

                pick = *((vdkRenderPicking*)options.pPick.ToPointer());
                return pick;
            }
        }

        unsafe public UnityEngine.Vector3 PickLocation()
        {
            vdkRenderPicking pick = this.Pick;
            return new UnityEngine.Vector3((float)pick.pointCenter[0], (float)pick.pointCenter[1], (float)pick.pointCenter[2]);
        }

        public vdkRenderOptions Options {
            get
            {
                return options;
            }
        }

        ~RenderOptions()
        {
            Marshal.FreeHGlobal(options.pPick);
        }
    }
  public class vdkPointCloud
  {
    public void Load(vdkContext context, string modelLocation, ref vdkPointCloudHeader header)
    {
      vdkError error = vdkPointCloud_Load(context.pContext, ref pModel, modelLocation, ref header);
      if (error != Vault.vdkError.vE_Success)
        throw new Exception("vdkPointCloud.Load failed: " + error.ToString());

      this.context = context;
    }

    public void Unload()
    {
      vdkError error = vdkPointCloud_Unload(ref pModel);
      if (error != Vault.vdkError.vE_Success)
        throw new Exception("vdkPointCloud.Unload failed.");
    }


    public void GetMetadata(ref string ppJSONMetadata)
    {
      vdkError error = vdkPointCloud_GetMetadata(pModel, ref ppJSONMetadata);
      if (error != Vault.vdkError.vE_Success)
        throw new Exception("vdkPointCloud.GetMetadata failed.");
    }


    public IntPtr pModel = IntPtr.Zero;
    private vdkContext context;

    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkPointCloud_Load(IntPtr pContext, ref IntPtr ppModel, string modelLocation, ref vdkPointCloudHeader header);

    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkPointCloud_Unload(ref IntPtr ppModel);

    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkPointCloud_GetMetadata(IntPtr pModel, ref string ppJSONMetadata);
    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkPointCloud_GetNodeColour(IntPtr pModel, UInt64 voxelID, IntPtr pColour);
    [DllImport(VaultSDKLibrary.name)]
    private static extern vdkError vdkPointCloud_GetAttributeAddress(ref vdkPointCloud pModel, ulong voxelID, uint attributeOffset, ref IntPtr ppAttributeAddress);
  }

    class vdkConvertContext
    {
        public void Create(vdkContext context)
        {
            vdkError error = vdkConvert_CreateContext(context.pContext, ref pConvertContext);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkConvertContext.Create failed.");
        }

        public void Destroy()
        {
            vdkError error = vdkConvert_DestroyContext(ref pConvertContext);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkConvertContext.Destroy failed.");
        }


        public void AddFile(string fileName)
        {
            vdkError error = vdkConvert_AddItem(pConvertContext, fileName);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkConvertContext.AddItem failed.");
        }
        public void SetFileName(string fileName)
        {
            vdkError error = vdkConvert_SetOutputFilename(pConvertContext, fileName);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkConvertContext.SetOutputFilename failed.");
        }

        public void DoConvert()
        {
            vdkError error = vdkConvert_DoConvert(pConvertContext);
            if (error != Vault.vdkError.vE_Success)
                throw new Exception("vdkConvertContext.DoConvert failed.");
        }

        public IntPtr pConvertContext;

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkConvert_CreateContext(IntPtr pContext, ref IntPtr ppConvertContext);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkConvert_DestroyContext(ref IntPtr ppConvertContext);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkConvert_AddItem(IntPtr pConvertContext, string fileName);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkConvert_SetOutputFilename(IntPtr pConvertContext, string fileName);

        [DllImport(VaultSDKLibrary.name)]
        private static extern vdkError vdkConvert_DoConvert(IntPtr pConvertContext);
    }

    public class VDKException: Exception
    {
        public vdkError value;

    }
}

