using System;
using System.Runtime.InteropServices;
using UnityEngine;

/*
 * This file contains all of the Unity support classes, all beginning with capital "UD"
 * These are friendly to Unity as much as possible.
 * Where possible, serializable and containing pointers such that end-users in Unity scripts needn't be aware of them.
 */
namespace udSDK
{
  /// <summary>
  /// Helper class for udContext/udConfig
  /// Coupled to Unity via Android implementation, and debug logs
  /// </summary>
  public class UDContext
  {
    public IntPtr pContext = IntPtr.Zero;
    public udSessionInfo pInfo = new udSessionInfo();

    public UDContext()
    {
      if (Application.platform == RuntimePlatform.Android)
      {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass jcsdk = new AndroidJavaClass("com.euclideon.udSDK");
        jcsdk.CallStatic("setupJNI", jo);
      }
    }

    ~UDContext()
    {
      if (pContext != IntPtr.Zero)
      {
        //this does not need to be called currently:
        //Disconnect();
      }
    }

    public void IgnoreCertificateVerification(bool ignore)
    {
      if (ignore)
        Debug.LogWarning("WARNING: Certificate verification disabled");

      udError error = udConfig_f.IgnoreCertificateVerification(ignore);
      udGuard.Error(error, "udConfig_IgnoreCertificateVerification");
    }

    public void ConnectFromDomain(string serverUrl, string pool)
    {
      udError error = udContext_f.ConnectFromDomain(ref pContext, serverUrl, pool);
      
      CheckServerError(error);
    }

    public void Connect(string pURL, string pApplicationName, string pUsername, string pPassword)
    {
      udError error = udContext_f.TryResume(ref pContext, pURL, pApplicationName, pUsername, true);

      if (error != udError.udE_Success)
        error = udContext_f.ConnectLegacy(ref pContext, pURL, pApplicationName, pUsername, pPassword);
      
      CheckServerError(error);
    }

    public static void CheckServerError(udError error)
    {
      switch (error)
      {
        case udError.udE_Success:
          break;
        case udError.udE_ServerError:
          throw new Exception("Could not connect to server.");
          break;
        case udError.udE_AuthError:
          throw new Exception("Username or Password incorrect.");
          break;
        case udError.udE_OutOfSync:
          throw new Exception("Your clock doesn't match the remote server clock.");
          break;
        case udError.udE_DecryptionKeyRequired:
          throw new Exception("A decryption key is required, or not matching");
          break;
        case udError.udE_DecryptionKeyMismatch:
          throw new Exception("A decryption key is required, or not matching");
          break;
        case udError.udE_SignatureMismatch:
          throw new Exception("Server not accepting digital signature.");
          break;
        default:
          throw new Exception("Unknown error occurred: " + error + ", please try again later.");
          break;
      }
    }

    public void TryResume(string pURL, string pApplicationName, string pUsername, bool tryDongle)
    {
      udError error = udContext_f.TryResume(ref pContext, pURL, pApplicationName, pUsername, tryDongle);
      udGuard.Error(error, "udContext_TryResume");
    }

    public void Disconnect(bool endSession = false)
    {
      udError error = udContext_f.Disconnect(ref pContext, endSession);
      udGuard.Error(error, "udContext_Disconnect");
    }

    public UDSessionInfo GetSessionInfo()
    {
      udError error = udContext_f.GetSessionInfo(pContext, ref pInfo);
      udGuard.Error(error, "udContext_Disconnect");

      udSessionInfo sessionInfo = pInfo;
      UDSessionInfo formattedInfo = new UDSessionInfo();

      DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
      ulong cur_time = (ulong) (System.DateTime.UtcNow - epochStart).TotalSeconds;
      formattedInfo.expiresTimestamp = (float) (sessionInfo.expiresTimestamp - cur_time);

      formattedInfo.displayName = new string(sessionInfo.displayName);
      formattedInfo.displayName = formattedInfo.displayName.Trim('\0');

      formattedInfo.isDomain = sessionInfo.isDomain != 0 ? true : false;
      formattedInfo.isOffline = sessionInfo.isOffline != 0 ? true : false;
      formattedInfo.isPremium = sessionInfo.isPremium != 0 ? true : false;

      return formattedInfo;
    }
  }

  /// <summary>
  /// Helper struct for udRenderPicking
  /// Changes:
  /// - uint16/32 > int
  /// - double[] > UnityEngine.Vector3
  /// Coupled to Unity via Vector3 usage 
  /// </summary>
  public struct UDPick 
  {
    public int x; // view space mouse x
    public int y; // view space mouse y
    public int hit; // true if voxel was hit by this pick
    public int isHighestLOD; // true if his was as accurate as possible
    public int modelIndex; // the index of the model in the array hit by this pick
    public Vector3 pointCenter; // the position of the point hit by the pick 
    public udVoxelID voxelID; // ID of the hit voxel 
  }
    
  /// <summary>
  /// Helper class for udRenderContext
  /// Coupled to Unity via coupled UDRenderSettings
  /// </summary>
  public class UDRenderContext
  {
    public IntPtr pRenderer = IntPtr.Zero;

    private UDContext context;

    ~UDRenderContext()
    {
      Destroy();
    }

    public void Create(UDContext context)
    {
      //ensure we destroy the existing context if we are creating a new one:
      if (pRenderer != IntPtr.Zero)
        Destroy();

      udGuard.NullPtr(context.pContext, "udContext");

      udError error = udRenderContext_f.Create(context.pContext, ref pRenderer);
      udGuard.Error(error, "udRenderContext_Create");

      this.context = context;
    }

    public void Destroy()
    {
      if (pRenderer == IntPtr.Zero)
        return;
      
      udError error = udRenderContext_f.Destroy(ref pRenderer);
      udGuard.Error(error, "udRenderContext_Destroy");
      
      pRenderer = IntPtr.Zero;
    }

    public void Render(UDRenderTarget renderView, udRenderInstance[] pModels, int modelCount, UDRenderSettings options = null )
    {
      if (modelCount == 0)
        return;

      if (options == null)
        options = new UDRenderSettings();

      udGuard.Null(renderView, "UDRenderTarget");
      udGuard.NullPtr(renderView.pRenderView, "udRenderView");
      udGuard.NullPtr(pRenderer, "udRendererContext");

      udError error = udRenderContext_f.Render(pRenderer, renderView.pRenderView, pModels, modelCount, options.options);
      udGuard.Error(error, "udRenderContext_Render");
      
      options.pickRendered = true;
    }
  }

  /// <summary>
  /// Helper class for udRenderSettings
  /// It provides a safe udPick option for accessing the pick results inside unity component scripts
  /// Coupled to Unity via the Unity serializable UDPick struct
  /// </summary>
  public class UDRenderSettings
  {
    public udRenderSettings options;
    public bool pickSet = false;
    public bool pickRendered = false;

    public udRenderSettings Options {
      get
      {
        return options;
      }
    }

    public UDRenderSettings(udRenderContextPointMode pointMode, udRenderContextFlags flags)
    {
      options.pointMode = pointMode;

      //this will need to change once support for multiple picks is introduced
      options.pPick = Marshal.AllocHGlobal(Marshal.SizeOf<udRenderPicking>());            

      options.flags = flags;
    }

    public UDRenderSettings() : this(udRenderContextPointMode.udRCPM_Rectangles, udRenderContextFlags.udRCF_None)
    {
    }

    ~UDRenderSettings()
    { 
      Marshal.DestroyStructure<udRenderPicking>(options.pPick);
      Marshal.FreeHGlobal(options.pPick);
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

    public UDPick getPick()
    {
      if(!pickSet)
        return new UDPick();

      udRenderPicking targetPick = Pick;
      UDPick newPick = new UDPick();
      newPick.hit = (int)targetPick.hit;
      newPick.x   = (int)targetPick.x;
      newPick.y   = (int)targetPick.y;

      unsafe
      {
        newPick.pointCenter = new Vector3((float)targetPick.pointCenter[0], (float)targetPick.pointCenter[1], (float)targetPick.pointCenter[2]);
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
  }
    
    
  /// <summary>
  /// Helper class for udRenderTarget/udRenderView
  /// Coupling results from the render function connection to Unity specific implementation
  /// </summary>
  public class UDRenderTarget
  {
    public IntPtr pRenderView = IntPtr.Zero;
    private UDContext context;

    private GCHandle colorBufferHandle;
    private GCHandle depthBufferHandle;
        
    ~UDRenderTarget()
    {
      Destroy();
    }

    public void Destroy()
    {
      if (colorBufferHandle.IsAllocated)
        colorBufferHandle.Free();

      if (depthBufferHandle.IsAllocated)
        depthBufferHandle.Free();

      udError error = udRenderTarget_f.Destroy(ref pRenderView);
      udGuard.Error(error, "udRenderTarget_Destroy");
      
      pRenderView = IntPtr.Zero;
    }

    public void Create(UDContext context, UDRenderContext renderer, UInt32 width, UInt32 height)
    {
      udGuard.NullPtr(context.pContext, "udContext");
      udGuard.NullPtr(renderer.pRenderer, "udRendererContext");

      udError error = udRenderTarget_f.Create(context.pContext, ref pRenderView, renderer.pRenderer, width, height);
      udGuard.Error(error, "udRenderTarget_Create");

      this.context = context;
    }

    public void SetTargets(ref UnityEngine.Color32[] colorBuffer, UInt32 clearColor, ref float[] depthBuffer)
    {
      if (colorBufferHandle.IsAllocated)
        colorBufferHandle.Free();

      if (depthBufferHandle.IsAllocated)
        depthBufferHandle.Free();

      colorBufferHandle = GCHandle.Alloc(colorBuffer, GCHandleType.Pinned);
      depthBufferHandle = GCHandle.Alloc(depthBuffer, GCHandleType.Pinned);

      udError error = udRenderTarget_f.SetTargets(pRenderView, colorBufferHandle.AddrOfPinnedObject(), clearColor, depthBufferHandle.AddrOfPinnedObject());
      udGuard.Error(error, "udRenderTarget_SetTargets");
    }

    public void GetMatrix(udRenderTargetMatrix matrixType, double[] cameraMatrix)
    {
      udError error = udRenderTarget_f.GetMatrix(pRenderView, matrixType, cameraMatrix);
      udGuard.Error(error, "udRenderTarget_GetMatrix");
    }

    public void SetMatrix(udRenderTargetMatrix matrixType, double[] cameraMatrix)
    {
      udGuard.NullPtr(pRenderView, "udRenderTarget");

      udError error = udRenderTarget_f.SetMatrix(pRenderView, matrixType, cameraMatrix);
      udGuard.Error(error, "udRenderTarget_SetMatrix");
    }
  }
  
  /// <summary>
  /// Helper class for udScene
  /// Coupled to Unity via the debug log
  /// </summary>
  public class UDScene
  {
    IntPtr pudScene;
    public IntPtr pRootNode;

    public UDScene(string geoJSON)
    {
      if(!GlobalUDContext.isCreated)
        throw new Exception("Global context not loaded, cannot load scene.");

      Debug.Log("Attempting scene load from memory"); 
      udError error = udScene_f.LoadFromMemory(GlobalUDContext.uContext.pContext, ref pudScene, geoJSON);
      udGuard.Error(error, "udScene_LoadFromMemory");
      
      pRootNode = IntPtr.Zero;
      udScene_f.GetProjectRoot(pudScene, ref pRootNode);
      Debug.Log("Scene loaded"); 
    }

    ~UDScene()
    {
      udScene_f.Release(ref pudScene);
    }
  }
}