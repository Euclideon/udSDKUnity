using System;
using System.Runtime.InteropServices;

//! The **udRenderContext** object provides an interface to render Euclideon Unlimited Detail models.
//! It provides the ability to render by colour, intensity or classification; additionally allowing the user to query a specific pixel for information about the pointcloud data.

namespace udSDK
{
    /// <summary>
    /// These are the various point modes available in udSDK
    /// </summary>
    public enum udRenderContextPointMode
    {
        udRCPM_Rectangles, //!< This is the default, renders the voxels expanded as screen space rectangles
        udRCPM_Cubes, //!< Renders the voxels as cubes
        udRCPM_Points, //!< Renders voxels as a single point (Note: does not accurately reflect the 'size' of voxels)
        
        udRCPM_Count //!< Total number of point modes. Used internally but can be used as an iterator max when displaying different point modes.
    }

    /// <summary>
    /// These are the various render flags available in udSDK
    /// </summary>
    public enum udRenderContextFlags
    {
        udRCF_None = 0, //!< Render the points using the default configuration.

        udRCF_PreserveBuffers = 1 << 0, //!< The colour and depth buffers won't be cleared before drawing and existing depth will be respected
        udRCF_ComplexIntersections = 1 << 1, //!< This flag is required in some scenes where there is a very large amount of intersecting point clouds
                                             //!< It will internally batch rendering with the udRCF_PreserveBuffers flag after the first render.
        udRCF_BlockingStreaming = 1 << 2, //!< This forces the streamer to load as much of the pointcloud as required to give an accurate representation in the current view. A small amount of further refinement may still occur.
        udRCF_LogarithmicDepth = 1 << 3, //!< Calculate the depth as a logarithmic distribution.
        udRCF_ManualStreamerUpdate = 1 << 4, //!< The streamer won't be updated internally but a render call without this flag or a manual streamer update will be required
        udRCF_ZeroAlphaSkip = 1 << 5, //!< If the voxel has 0 alpha (upper 8 bits) after the voxel shader then the voxel will not be drawn to the buffers (effectively skipped)

        udRCF_2PixelOpt = 1 << 6, //!< Optimisation that allows the renderer to resolve the last 2 pixels simulataneously, this can result in slight inaccuracies (generally a few pixels) in the final image for a huge performance improvement.
        udRCF_DisableOrthographic = 1 << 7, //!< Disables the renderer entering high-performance orthographic mode
    }

    /// <summary>
    /// Stores both the input and output of the udSDK picking system
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udRenderPicking
    {
        public UInt32 x; //!< Mouse X position in udRenderTarget space
        public UInt32 y; //!< Mouse Y position in udRenderTarget space
        
        public UInt32 hit; //!< Not 0 if a voxel was hit by this pick
        public UInt32 isHighestLOD; //!< Not 0 if this voxel that was hit is as precise as possible
        // todo is this the correct size for "unsized int"
        public UInt16 modelIndex; //!< Index of the model in the ppModels list
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public double[] pointCenter; //!< The center of the hit voxel in world space
        public udVoxelID voxelID; //!< The ID of the voxel that was hit by the pick; this ID is only valid for a very short period of time- Do any additional work using this ID this frame.
    }
    
    /// <summary>
    /// Stores the render settings used per render
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udRenderSettings
    {
        public udRenderContextFlags flags; //!< Optional flags providing information about how to perform this render
        public IntPtr pPick; //!< Optional This provides information about the voxel under the mouse
        public udRenderContextPointMode pointMode; //!< The point mode for this render
        public IntPtr pFilter; //!< Optional This filter is applied to all models in the scene

        public UInt32 pointCount; //!< Optional (GPU Renderer) A hint to the renderer at the upper limit of voxels that are to be rendered.
        public float pointThreshold; //!< Optional (GPU Renderer) A hint of the minimum size (in screen space) of a voxel that the renderer will produce.
    }
    
    /// <summary>
    /// Stores the instance settings of a model to be rendered
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udRenderInstance
    {
        public IntPtr pointCloud; //!< This is the point cloud to display
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] matrix; //!< The world space matrix for this point cloud instance (this does not to be the default matrix)
        //!< @note The default matrix for a model can be accessed from the associated udPointCloudHeader

        public IntPtr pFilter; //!< Filter to override for this model, this one will be used instead of the global one applied in udRenderSettings
        
        public IntPtr pVoxelShader; //!< When the renderer goes to select a colour, it calls this function instead
        public IntPtr pVoxelUserData; //!< If pVoxelShader is set, this parameter is passed to that function

        public double opacity; //!< If this is a value between 0 and 1 this model will be rendered blended with the rest of the scene. If the alpha from pVoxelShader is 0, the alpha provided will be written to the colourBuffer otherwise it will be calculated using this opacity value
        public UInt32 skipRender; //!< If set not 0 the model will not be rendered
    }
    

    public static partial class udRenderContext_f
    {
        /// <summary>
        /// Create an instance of `udRenderContext` for rendering.
        /// </summary>
        /// <param name="pContext">The context to be used to create the render context.</param>
        /// <param name="ppRenderer">The pointer pointer of the udRenderContext. This will allocate an instance of udRenderContext into `ppRenderer`.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_Create")]
        public static extern udError Create(IntPtr pContext, ref IntPtr ppRenderer);

        /// <summary>
        /// Destroy the instance of the renderer.
        /// </summary>
        /// <param name="ppRenderer">The pointer pointer of the udRenderContext. This will deallocate the instance of udRenderContext.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_Destroy")]
        public static extern udError Destroy(ref IntPtr ppRenderer);

        /// <summary>
        /// Render the models from the perspective of `pRenderView`.
        /// </summary>
        /// <param name="pRenderer">The renderer to render the scene.</param>
        /// <param name="pRenderView">The view to render from with the render buffers associated with it being filled out.</param>
        /// <param name="pModels">The array of models to use in render.</param>
        /// <param name="modelCount">The amount of models in pModels.</param>
        /// <param name="options">Additional render options.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_Render")]
        public static extern udError Render(IntPtr pRenderer, IntPtr pRenderView, udRenderInstance[] pModels, int modelCount, [In, Out] udRenderSettings options);
    }
    
}