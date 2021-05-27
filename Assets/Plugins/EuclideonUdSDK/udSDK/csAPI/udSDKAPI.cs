
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
  public enum udError
  {
    udE_Success, //!< Indicates the operation was successful

    udE_Failure, //!< A catch-all value that is rarely used, internally the below values are favored
    udE_NothingToDo, //!< The operation didn't specifically fail but it also didn't do anything
    udE_InternalError, //!< There was an internal error that could not be handled

    udE_NotInitialized, //!< The request can't be processed because an object hasn't been configured yet
    udE_InvalidConfiguration, //!< Something in the request is not correctly configured or has conflicting settings
    udE_InvalidParameter, //!< One or more parameters is not of the expected format
    udE_OutstandingReferences, //!< The requested operation failed because there are still references to this object

    udE_MemoryAllocationFailure, //!< udSDK wasn't able to allocate enough memory for the requested feature
    udE_CountExceeded, //!< An internal count was exceeded by the request, generally going beyond the end of a buffer or internal limit

    udE_NotFound, //!< The requested item wasn't found or isn't currently available

    udE_BufferTooSmall, //!< Either the provided buffer or an internal one wasn't big enough to fufill the request
    udE_FormatVariationNotSupported, //!< The supplied item is an unsupported variant of a supported format

    udE_ObjectTypeMismatch, //!< There was a mismatch between what was expected and what was found

    udE_CorruptData, //!< The data/file was corrupt

    udE_InputExhausted, //!< The input buffer was exhausted so no more processing can occur
    udE_OutputExhausted, //!< The output buffer was exhausted so no more processing can occur

    udE_CompressionError, //!< There was an error in compression or decompression
    udE_Unsupported, //!< This functionality has not yet been implemented (usually some combination of inputs isn't compatible yet)

    udE_Timeout, //!< The requested operation timed out. Trying again later may be successful

    udE_AlignmentRequired, //!< Memory alignment was required for the operation

    udE_DecryptionKeyRequired, //!< A decryption key is required and wasn't provided
    udE_DecryptionKeyMismatch, //!< The provided decryption key wasn't the required one

    udE_SignatureMismatch, //!< The digital signature did not match the expected signature

    udE_ObjectExpired, //!< The supplied object has expired

    udE_ParseError, //!< A requested resource or input was unable to be parsed

    udE_InternalCryptoError, //!< There was a low level cryptography issue

    udE_OutOfOrder, //!< There were inputs that were provided out of order
    udE_OutOfRange, //!< The inputs were outside the expected range

    udE_CalledMoreThanOnce, //!< This function was already called

    udE_ImageLoadFailure, //!< An image was unable to be parsed. This is usually an indication of either a corrupt or unsupported image format

    udE_StreamerNotInitialised, //!<  The streamer needs to be initialised before this function can be called

    udE_OpenFailure, //!< The requested resource was unable to be opened
    udE_CloseFailure, //!< The resource was unable to be closed
    udE_ReadFailure, //!< A requested resource was unable to be read
    udE_WriteFailure, //!< A requested resource was unable to be written
    udE_SocketError, //!< There was an issue with a socket problem

    udE_DatabaseError, //!< A database error occurred
    udE_ServerError, //!< The server reported an error trying to complete the request
    udE_AuthError, //!< The provided credentials were declined (usually email or password issue)
    udE_NotAllowed, //!< The requested operation is not allowed (usually this is because the operation isn't allowed in the current state)
    udE_InvalidLicense, //!< The required license isn't available or has expired

    udE_Pending, //!< A requested operation is pending.
    udE_Cancelled, //!< The requested operation was cancelled (usually by the user)
    udE_OutOfSync, //!< There is an inconsistency between the internal udSDK state and something external. This is usually because of a time difference between the local machine and a remote server
    udE_SessionExpired, //!< The udServer has terminated your session

    udE_ProxyError, //!< There was some issue with the provided proxy information (either a proxy is in the way or the provided proxy info wasn't correct)
    udE_ProxyAuthRequired, //!< A proxy has requested authentication
    udE_ExceededAllowedLimit, //!< The requested operation failed because it would exceed the allowed limits (generally used for exceeding server limits like number of projects)

    udE_RateLimited, //!< This functionality is currently being rate limited or has exhausted a shared resource. Trying again later may be successful
    udE_PremiumOnly, //!< The requested operation failed because the current session is not for a premium user

    udE_Count //!< Internally used to verify return values
  };

  public enum udRenderTargetMatrix
  {
    Camera,     // The local to world-space transform of the camera (View is implicitly set as the inverse)
    View,       // The view-space transform for the model (does not need to be set explicitly)
    Projection, // The projection matrix (default is 60 degree LH)
    Viewport,   // Viewport scaling matrix (default width and height of viewport)

    Count,
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
  public struct udRenderInstance
  {
    public IntPtr pointCloud;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public double[] worldMatrix;


    public IntPtr filter;
    public IntPtr voxelShader;
    public IntPtr voxelUserData;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct udAttributeSet
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
    private udAttributeSet set;

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
    [DllImport(UDSDKLibrary.name)]
    private static extern udError vdkAttributeSet_GetOffsetOfStandardAttribute(ref udAttributeSet pAttributeSet, StandardAttribute attribute, IntPtr pOffset);
    [DllImport(UDSDKLibrary.name)]
    private static extern udError vdkAttributeSet_GetOffsetOfNamedAttribute(ref udAttributeSet pAttributeSet, string pName, IntPtr pOffset);
    
  }

    [StructLayout(LayoutKind.Sequential)]
    public struct udPointCloudHeader
    {
        public double scaledRange; //!< The point cloud's range multiplied by the voxel size
        public double unitMeterScale; //!< The scale to apply to convert to/from metres (after scaledRange is applied to the unitCube)
        public uint totalLODLayers; //!< The total number of LOD layers in this octree
        public double convertedResolution; //!< The resolution this model was converted at
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] storedMatrix; //!< This matrix is the 'default' internal matrix to go from a unit cube to the full size

        public udAttributeSet attributes;   //!< The attributes contained in this pointcloud

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
    unsafe public struct udRenderPicking
    {
        public UInt32 x;//view space mouse x
        public UInt32 y;//view space mouse y
        public UInt32 hit;//true if voxel was hit by this pick
        public UInt32 isHighestLOD;//true if hit was as accurate as possible
        public UInt32 modelIndex; //index of the model in the array hit by this pick
        public fixed double pointCenter[3]; //location of the point hit by the pick
        public udVoxelID voxelID; //ID of the hit voxel
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct udVoxelID
    {
        public UInt64 index; 
        public IntPtr pTrav; 
        public IntPtr pRenderInfo; 
    }

    public enum udRenderContextFlags
    {
        udRF_None = (0),
        udRF_Preserve_Buffers = 1 << 0,
        udRF_ComplexIntersections = 1 << 1,
        udRF_BlockingStreaming = 1 << 2,
        udRCF_LogarithmicDepth = 1 << 3, //!< Calculate the depth as a logarithmic distribution.
        udRCF_ManualStreamerUpdate = 1 << 4, //!< The streamer won't be updated internally but a render call without this flag or a manual streamer update will be required
    }

    public enum udRenderContextPointMode
    {
        udRCPM_Rectangles,
        udRCPM_Cubes,
        udRCPM_Points,
        udRCPM_Count
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct udRenderSettings
    {
        public udRenderContextFlags flags; // optional flags providing information on how to perform the render
        public IntPtr pPick;
        public udRenderContextPointMode pointMode;
        public IntPtr pFilter; // pointer to a vdkQueryFilter
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct udSessionInfo
    {
      public int isOffline; //!< Is not 0 if this is an offline session (dongle or other offline license)

      public double expiresTimestamp; //!< The timestamp in UTC when the session will automatically end
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
      public char[] displayName; //!< The null terminated display name of the user
    };

    public class udContext
    {

      public udContext()
      {
        if (Application.platform == RuntimePlatform.Android)
        {
          AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
          AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
          AndroidJavaClass jcvdk = new AndroidJavaClass("com.euclideon.udSDK");
          jcvdk.CallStatic("setupJNI", jo);
        }
      }

        ~udContext()
        {
            if (pContext != IntPtr.Zero) {
                //this does not need to be called currently:
                //Disconnect();

            }

        }

        public void IgnoreCertificateVerification(bool ignore) 
        {
          if (ignore)
            Debug.LogWarning("WARNING: Certificate verification disabled");
            udConfig_IgnoreCertificateVerification(ignore);
        }

        public void Connect(string pURL, string pApplicationName, string pUsername, string pPassword)
        {
            udError error = udContext.udContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, true);

            if (error != udError.udE_Success)
                error = udContext_Connect(ref pContext, pURL, pApplicationName, pUsername, pPassword);

            if (error == udSDK.udError.udE_ServerError)
                throw new Exception("Could not connect to server.");
            else if (error == udSDK.udError.udE_AuthError)
                throw new Exception("Username or Password incorrect.");
            else if (error == udSDK.udError.udE_OutOfSync)
                throw new Exception("Your clock doesn't match the remote server clock.");
            else if (error == udSDK.udError.udE_DecryptionKeyRequired || error == udSDK.udError.udE_DecryptionKeyMismatch )
                throw new Exception("A decryption key is required, or not matching");    
            else if (error == udSDK.udError.udE_SignatureMismatch )
                throw new Exception("Server not accepting digital signature.");
            else if (error != udSDK.udError.udE_Success)
                throw new Exception("Unknown error occurred: " + error.ToString() + ", please try again later.");
        }


        public void Try_Resume(string pURL, string pApplicationName, string pUsername, bool tryDongle)
        {

            udError error = udContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, tryDongle);
            if (error != udError.udE_Success)
            {
                throw new Exception("Unable to keep session alive: " + error.ToString());
            }
        }
        public void Disconnect(bool endSession = false)
        {
            udError error = udContext_Disconnect(ref pContext, endSession);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udContext.Disconnect failed.");
        }

        public void GetSessionInfo(ref udSessionInfo info)
        {
            udError error = udContext_GetSessionInfo(pContext, ref info);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udContext.Disconnect failed.");
        }
        public IntPtr pContext = IntPtr.Zero;

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_TryResume(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, bool tryDongle);
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_Connect(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, string pPassword);
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_Disconnect(ref IntPtr ppContext, bool endSession);
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_GetSessionInfo(IntPtr pContext, ref udSessionInfo pInfo);
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConfig_IgnoreCertificateVerification(bool ignore);
    }

    public class udRenderContext
    {
        public IntPtr pRenderer = IntPtr.Zero;

        private udContext context;
        ~udRenderContext()
        {
            Destroy();
        }
        public void Create(udContext context)
        {
            //ensure we destroy the existing context if we are creating a new one:
            if (pRenderer != IntPtr.Zero)
                Destroy();

            if (context.pContext == IntPtr.Zero)
                throw new Exception("context not instantiatiated");

            udError error = udRenderContext_Create(context.pContext, ref pRenderer);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkRenderContext.Create failed: " + error.ToString());

            this.context = context;
        }

        public void Destroy()
        {
            if (pRenderer == IntPtr.Zero)
            {
                return;
            }
            udError error = udRenderContext_Destroy(ref pRenderer);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkRenderContext.Destroy failed: " + error.ToString());

            pRenderer = IntPtr.Zero;
        }

        public void Render(udRenderTarget renderView, udRenderInstance[] pModels, int modelCount, RenderOptions options = null )
        {
            if (modelCount == 0)
                return;

            if (options == null)
                options = new RenderOptions();

            if (renderView == null)
                throw new Exception("renderView is null");

            if (renderView.pRenderView == IntPtr.Zero)
                throw new Exception("RenderView not initialised");

            if (pRenderer == IntPtr.Zero)
                throw new Exception("renderContext not initialised");

            udError error = udRenderContext_Render(pRenderer, renderView.pRenderView, pModels, modelCount, options.options);

            if (error != udSDK.udError.udE_Success)
            {
                Debug.Log("vdkRenderContext.Render failed: " + error.ToString());
            }
            options.pickRendered = true;
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderContext_Create(IntPtr pContext, ref IntPtr ppRenderer);
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderContext_Destroy(ref IntPtr ppRenderer);
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderContext_Render(IntPtr pRenderer, IntPtr pRenderView, udRenderInstance[] pModels, int modelCount, [In, Out] udRenderSettings options);
    }

    public class udRenderTarget
    {

        ~udRenderTarget()
        {
            Destroy();
        }
        public void Create(udContext context, udRenderContext renderer, UInt32 width, UInt32 height)
        {
            if (context.pContext == IntPtr.Zero)
                throw new Exception("context not instantiated");

            if (renderer.pRenderer == IntPtr.Zero)
                throw new Exception("renderer not instantiated");

            udError error = udRenderTarget_Create(context.pContext, ref pRenderView, renderer.pRenderer, width, height);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkRenderView.Create failed: " + error.ToString());

            this.context = context;
        }

        public void Destroy()
        {
            if (colorBufferHandle.IsAllocated)
                colorBufferHandle.Free();

            if (depthBufferHandle.IsAllocated)
                depthBufferHandle.Free();

            udError error = udRenderTarget_Destroy(ref pRenderView);
            if (error != udSDK.udError.udE_Success)
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

            udError error = udRenderTarget_SetTargets(pRenderView, colorBufferHandle.AddrOfPinnedObject(), clearColor, depthBufferHandle.AddrOfPinnedObject());
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkRenderView.SetTargets failed.");
        }

        public void GetMatrix(udRenderTargetMatrix matrixType, double[] cameraMatrix)
        {
            udError error = udRenderTarget_GetMatrix(pRenderView, matrixType, cameraMatrix);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkRenderView.GetMatrix failed.");
        }

        public void SetMatrix(udRenderTargetMatrix matrixType, double[] cameraMatrix)
        {
            if (pRenderView == IntPtr.Zero)
                throw new Exception("view not instantiated");

            udError error = udRenderTarget_SetMatrix(pRenderView, matrixType, cameraMatrix);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkRenderView.SetMatrix failed: " + error.ToString());
        }

        public IntPtr pRenderView = IntPtr.Zero;
        private udContext context;

        private GCHandle colorBufferHandle;
        private GCHandle depthBufferHandle;

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderTarget_Create(IntPtr pContext, ref IntPtr ppRenderView, IntPtr pRenderer, UInt32 width, UInt32 height);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderTarget_Destroy(ref IntPtr ppRenderView);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderTarget_SetTargets(IntPtr pRenderView, IntPtr pColorBuffer, UInt32 colorClearValue, IntPtr pDepthBuffer);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderTarget_GetMatrix(IntPtr pRenderView, udRenderTargetMatrix matrixType, double[] cameraMatrix);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udRenderTarget_SetMatrix(IntPtr pRenderView, udRenderTargetMatrix matrixType, double[] cameraMatrix);
    }

    public struct udPick 
    {
        public int x; // view space mouse x
        public int y; // view space mouse y
        public int hit; // true if voxel was hit by this pick
        public int isHighestLOD; // true if his was as accurate as possible
        public int modelIndex; // the index of the model in the array hit by this pick
        public UnityEngine.Vector3 pointCenter; // the position of the point hit by the pick 
        public udVoxelID voxelID; // ID of the hit voxel 
    }

    public class RenderOptions
    {
        // this is an interface to the udRenderSettings struct
        // it provides a safe udPick option for accessing the pick results inside unity component scripts 

        public udRenderSettings options;
        public bool pickSet = false;
        public bool pickRendered = false;

        public RenderOptions(udRenderContextPointMode pointMode, udRenderContextFlags flags)
        {
            options.pointMode = pointMode;

            //this will need to change once support for multiple picks is introduced
            options.pPick = Marshal.AllocHGlobal(Marshal.SizeOf<udRenderPicking>());            

            options.flags = flags;
        }

        public RenderOptions() : this(udRenderContextPointMode.udRCPM_Rectangles, udRenderContextFlags.udRF_None)
        {

        }

        public void setPick(uint x, uint y)
        {
            udRenderPicking pick = new udRenderPicking();
            pick.x = x;
            pick.y = y;

            Marshal.StructureToPtr(pick, options.pPick, false);

            pickRendered = false;
            pickSet = true;
        }

        public udPick getPick()
        {
            UnityEngine.Debug.Log("Getting pick");

            if(!pickSet)
                return new udPick();

            udRenderPicking targetPick = this.Pick;
            udPick newPick = new udPick();
            newPick.hit = (int)targetPick.hit;
            newPick.x   = (int)targetPick.x;
            newPick.y   = (int)targetPick.y;

            unsafe
            {
                newPick.pointCenter = new UnityEngine.Vector3((float)targetPick.pointCenter[0], (float)targetPick.pointCenter[1], (float)targetPick.pointCenter[2]);
            }
            
            newPick.isHighestLOD = (int)targetPick.isHighestLOD;

            newPick.voxelID = targetPick.voxelID;

            return newPick; 
        }

        public udRenderPicking Pick
        {
            get
            {
                if (!pickSet)
                    return new udRenderPicking();

                if (!pickRendered)
                    throw new Exception("Render must be called before pick can be read");

                return (udRenderPicking)Marshal.PtrToStructure(options.pPick, typeof(udRenderPicking)); 
            }
        }

        public udRenderSettings Options {
            get
            {
                return options;
            }
        }

        ~RenderOptions()
        { 
            Marshal.DestroyStructure<udRenderPicking>(options.pPick);
            Marshal.FreeHGlobal(options.pPick);
        }
    }
  public class udPointCloud
  {
    public void Load(udContext context, string modelLocation, ref udPointCloudHeader header)
    {
      if (context.pContext == IntPtr.Zero) 
        throw new Exception("Point cloud load failed: udContext is not initialised");

      udError error = udPointCloud_Load(context.pContext, ref pModel, modelLocation, ref header);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udPointCloud.Load " +modelLocation + " failed: " + error.ToString());

      this.context = context;
    }

    public void Unload()
    {
      udError error = udPointCloud_Unload(ref pModel);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udPointCloud.Unload failed.");
    }


    public void GetMetadata(ref string ppJSONMetadata)
    {
      udError error = udPointCloud_GetMetadata(pModel, ref ppJSONMetadata);
      if (error != udSDK.udError.udE_Success)
        throw new Exception("udPointCloud.GetMetadata failed.");
    }

    public udError GetStreamingStatus()
    {
      return udPointCloud_GetStreamingStatus(this.pModel);
    }


    public IntPtr pModel = IntPtr.Zero;
    private udContext context;

    [DllImport(UDSDKLibrary.name)]
    private static extern udError udPointCloud_Load(IntPtr pContext, ref IntPtr ppModel, string modelLocation, ref udPointCloudHeader header);

    [DllImport(UDSDKLibrary.name)]
    private static extern udError udPointCloud_Unload(ref IntPtr ppModel);

    [DllImport(UDSDKLibrary.name)]
    private static extern udError udPointCloud_GetMetadata(IntPtr pModel, ref string ppJSONMetadata);
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udPointCloud_GetNodeColour(IntPtr pModel, UInt64 voxelID, IntPtr pColour);
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udPointCloud_GetAttributeAddress(IntPtr pModel, ulong voxelID, uint attributeOffset, ref IntPtr ppAttributeAddress);
    [DllImport(UDSDKLibrary.name)]
    private static extern udError udPointCloud_GetStreamingStatus(IntPtr pModel);
  }

    class vdkConvertContext
    {
        public void Create(udContext context)
        {
            udError error = vdkConvert_CreateContext(context.pContext, ref pConvertContext);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkConvertContext.Create failed.");
        }

        public void Destroy()
        {
            udError error = vdkConvert_DestroyContext(ref pConvertContext);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkConvertContext.Destroy failed.");
        }


        public void AddFile(string fileName)
        {
            udError error = vdkConvert_AddItem(pConvertContext, fileName);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkConvertContext.AddItem failed.");
        }
        public void SetFileName(string fileName)
        {
            udError error = vdkConvert_SetOutputFilename(pConvertContext, fileName);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkConvertContext.SetOutputFilename failed.");
        }

        public void DoConvert()
        {
            udError error = vdkConvert_DoConvert(pConvertContext);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("vdkConvertContext.DoConvert failed.");
        }

        public IntPtr pConvertContext;

        [DllImport(UDSDKLibrary.name)]
        private static extern udError vdkConvert_CreateContext(IntPtr pContext, ref IntPtr ppConvertContext);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError vdkConvert_DestroyContext(ref IntPtr ppConvertContext);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError vdkConvert_AddItem(IntPtr pConvertContext, string fileName);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError vdkConvert_SetOutputFilename(IntPtr pConvertContext, string fileName);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError vdkConvert_DoConvert(IntPtr pConvertContext);
    }

    public class VDKException: Exception
    {
        public udError value;

    }
}

