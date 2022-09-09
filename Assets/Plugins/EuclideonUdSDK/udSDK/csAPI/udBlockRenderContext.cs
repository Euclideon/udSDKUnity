using System;
using System.Runtime.InteropServices;

//! The **udBlockRenderContext** object provides an interface to render Euclideon Unlimited Detail models, with callbacks to enable GPU renderering.
//! It provides the ability to render by colour, intensity or classification; additionally allowing the user to query a specific pixel for information about the pointcloud data.

namespace udSDK
{
  /// <summary>
  /// Stores the vertex data needed to render a block 
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udBlockRenderVertexData
  {
    public IntPtr pBlock;                       //!< Pointer to internal block memory
    public IntPtr pPointBuffer;           //!< Pointer to points data
    public double unitOctreeMultiplier;        //!< Multiplier to take from integer octee space to 0..1 octree space
    public double nodeSize;                    //!< Generally the w component for the size of the node (1.0 == root, 0.5 = level 1, 0.25, etc)
    public double childSize;                   //!< nodeSize * 0.5 Currently legacy, with intent to switch everything to nodeSize
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
    public UInt32[] divisionVertexCounts;   //!< Each block is divided into at most 9 pieces, sum of counts equals pPointBuffer->pointCount
    public UInt32 divisionCount;             //!< How many divisions used in this block, referencing divisionVertexCounts above (at most 9)

    public udMathDouble4x4 modelToBlock;   //!< Matrix to take a block from model space, to block space
    public double distEye;                 //!< Distance of this block to the camera (in eye space)
  };
  
  /// <summary>
  /// Exposes data required to render a model 
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udBlockRenderModel
  {
    public IntPtr pRenderModel; //!< Pointer to internal model
  
    public udMathDouble4x4 world;        //!< World Matrix
    public udMathDouble4x4 worldView;    //!< World-View Matrix
    public udMathDouble4x4 projection;   //!< Projection Matrix
    public udMathDouble4x4 wvps;         //!< World-View-Projection-Screen Matrix
  
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public udMathDouble4[] frustumPlanes; //!< Cached view frustum planes
    public udMathDouble3 eyePosMS;         //!< Position of the eye in model space
    public udMathDouble3 cameraForward;    //!< Forward vector of the camera in model space
    public udMathDouble3 cameraFacingDiag; //!< Vector that when transformed will be a diagonal line in screen space
  };

  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Called when vertex buffer is ready to be created
  /// </summary>
  public delegate udError udBlockRenderGPUInterface_CreateVertexBuffer(IntPtr pGPUContext, udBlockRenderModel pModel, udBlockRenderVertexData vertexData, ref IntPtr ppVertexBuffer);
  
  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Called when vertex buffer is ready to uploaded to GPU
  /// </summary>
  public delegate udError udBlockRenderGPUInterface_UploadVertexBuffer(IntPtr pGPUContext, udBlockRenderModel pModel, IntPtr pVertexBuffer);
  
  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Called when vertex buffer is to be rendered
  /// </summary>
  public delegate udError udBlockRenderGPUInterface_RenderVertexBuffer(IntPtr pGPUContext, udBlockRenderModel pModel, IntPtr pVertexBuffer, UInt16 divisionsMask, double blockPriority);

  /// Note: not in the header, provided to get around lack of paramaterized function pointer in C#
  /// <summary>
  /// Called when vertex buffer is to be destroyed
  /// </summary>
  public delegate udError udBlockRenderGPUInterface_DestroyVertexBuffer(IntPtr pGPUContext, ref IntPtr pVertexBuffer);

  /// <summary>
  /// Structure that stores user-defined rendering callbacks (optional and required) 
  /// </summary>
  public struct udBlockRenderGPUInterface
  {
    public IntPtr pBeginRender;  //!< Called when rendering begins
    public IntPtr pEndRender;  //!< Called when rendering ends

    public udBlockRenderGPUInterface_CreateVertexBuffer pCreateVertexBuffer;  //!< Called when vertex buffer is ready to be created
    public udBlockRenderGPUInterface_UploadVertexBuffer pUploadVertexBuffer;  //!< Called when vertex buffer is ready to uploaded to GPU
    public udBlockRenderGPUInterface_RenderVertexBuffer pRenderVertexBuffer;  //!< Called when vertex buffer is to be rendered
    public udBlockRenderGPUInterface_DestroyVertexBuffer pDestroyVertexBuffer;  //!< Called when vertex buffer is to be destroyed

    public IntPtr pGPUContext; //!< Internal pointer to user-defined storage
  };

  public static class udBlockRenderVertexData_f
  {
    /// <summary>
    /// Retrieves a voxel position in model space.
    /// </summary>
    /// <param name="pData">The block vertex data to query.</param>
    /// <param name="index">The voxel index.</param>
    /// <returns>The voxel position in model space.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udBlockRenderVertexData_GetFloatPosition")]
    public static extern udMathDouble3 GetFloatPosition(udBlockRenderVertexData pData, UInt32 index);
  }

  public static class udBlockRenderContext_f
  {
    /// <summary>
    /// Create an instance of `udBlockRenderContext` for rendering.
    /// </summary>
    /// <param name="pGPUInterface">The pointer of the user-defined callbacks interface.</param>
    /// <returns>A udError value based on the result of the render context creation.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udBlockRenderContext_Init")]
    public static extern udError Init(ref udBlockRenderGPUInterface pGPUInterface);
    
    /// <summary>
    /// Destroy the instance of the renderer.
    /// </summary>
    /// <param name="ppRenderer">The pointer pointer of the udRenderContext. This will deallocate the instance of udRenderContext.</param>
    /// <returns>A udError value based on the result of the render context destruction.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udBlockRenderContext_Deinit")]
    public static extern udError Deinit();
    
    /// <summary>
    /// Render the models from the perspective of `pRenderView`.
    /// </summary>
    /// <param name="pRenderer">The renderer to render the scene.</param>
    /// <param name="pRenderView">The view to render from with the render buffers associated with it being filled out.</param>
    /// <param name="pModels">The array of models to use in render.</param>
    /// <param name="modelCount">The amount of models in pModels.</param>
    /// <param name="pRenderOptions">Additional render options.</param>
    /// <returns>A udError value based on the result of the render.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udBlockRenderContext_Render")]
    public static extern udError Render(IntPtr pRenderer, IntPtr pRenderView, udRenderInstance[] pModels, int modelCount, udRenderSettings pRenderOptions);
  }

  public static partial class udRenderContext_f
  {
    /// <summary>
    /// Get the unique user-defined id of the supplied model.
    /// </summary>
    /// <param name="pRenderModel">The model whose id will be returned.</param>
    /// <returns>The unique id of the model.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_GetModelId")]
    public static extern UInt32 GetModelId(udBlockRenderModel pBlock);
    
    /// <summary>
    /// Locks an Unlimited Detail block, preventing it from being freed by the streamer.
    /// </summary>
    /// <param name="pBlock">A pointer to an internal Unlimited Detail block.</param>
    /// <returns>A udError value indicating the result of the lock update.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_LockBlock")]
    public static extern udError LockBlock(ref IntPtr pBlock);
    
    /// <summary>
    /// Unlocks an Unlimited Detail block, allowing the streamer to free it.
    /// </summary>
    /// <param name="pBlock">A pointer to an internal Unlimited Detail block.</param>
    /// <returns>A udError value indicating the result of the lock update.</returns>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_UnlockBlock")]
    public static extern udError UnlockBlock(ref IntPtr pBlock);
  }
}