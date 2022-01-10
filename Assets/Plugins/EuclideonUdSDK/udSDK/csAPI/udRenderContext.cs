using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
    public enum udRenderContextPointMode
    {
        udRCPM_Rectangles,
        udRCPM_Cubes,
        udRCPM_Points,
        udRCPM_Count
    }

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
    /// Contains information returned by the picking system.
    /// </summary>
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

    /// <summary>
    /// Unity serializable equivalent of udRenderPicking.
    /// </summary>
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

    /// <summary>
    /// Contains the settings of the render.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udRenderSettings
    {
        public udRenderContextFlags flags; // optional flags providing information on how to perform the render
        public IntPtr pPick;
        public udRenderContextPointMode pointMode;
        public IntPtr pFilter; // pointer to a udQueryFilter
    }

    /// <summary>
    /// Contains the instance of the point cloud within the renderer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udRenderInstance
    {
        public IntPtr pointCloud;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public double[] worldMatrix;

        public IntPtr filter;
        public IntPtr voxelShader;
        public IntPtr voxelUserData;

        public double opacity;
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
                throw new Exception("Context not instantiatiated.");

            udError error = udRenderContext_Create(context.pContext, ref pRenderer);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udRenderContext.Create failed: " + error.ToString());

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
                throw new Exception("udRenderContext.Destroy failed: " + error.ToString());

            pRenderer = IntPtr.Zero;
        }

        public void Render(udRenderTarget renderView, udRenderInstance[] pModels, int modelCount, RenderOptions options = null )
        {
            if (modelCount == 0)
            {
                Debug.Log("Model count is zero!");
                return;
            }
                
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
                Debug.Log("udRenderContext.Render failed: " + error.ToString());
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

    public class RenderOptions
    {
        // this is an interface to the udRenderSettings struct
        // it provides a safe udPick option for accessing the pick results inside unity component scripts 

        public udRenderSettings options;
        public bool pickSet = false;
        public bool pickRendered = false;

        public udRenderSettings Options {
            get
            {
                return options;
            }
        }

        public RenderOptions(udRenderContextPointMode pointMode, udRenderContextFlags flags)
        {
            options.pointMode = pointMode;

            //this will need to change once support for multiple picks is introduced
            options.pPick = Marshal.AllocHGlobal(Marshal.SizeOf<udRenderPicking>());            

            options.flags = flags;
        }

        public RenderOptions() : this(udRenderContextPointMode.udRCPM_Rectangles, udRenderContextFlags.udRCF_None)
        {
        }

        ~RenderOptions()
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

        public udPick getPick()
        {
            //UnityEngine.Debug.Log("Getting pick");

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
    }
}