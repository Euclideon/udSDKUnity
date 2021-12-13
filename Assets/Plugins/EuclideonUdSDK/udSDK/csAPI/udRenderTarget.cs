using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
    public enum udRenderTargetMatrix
    {
        Camera,     // The local to world-space transform of the camera (View is implicitly set as the inverse)
        View,       // The view-space transform for the model (does not need to be set explicitly)
        Projection, // The projection matrix (default is 60 degree LH)
        Viewport,   // Viewport scaling matrix (default width and height of viewport)
        Count
    };

    public class udRenderTarget
    {
        public IntPtr pRenderView = IntPtr.Zero;
        private udContext context;

        private GCHandle colorBufferHandle;
        private GCHandle depthBufferHandle;
        
        ~udRenderTarget()
        {
            Destroy();
        }

        public void Destroy()
        {
            if (colorBufferHandle.IsAllocated)
                colorBufferHandle.Free();

            if (depthBufferHandle.IsAllocated)
                depthBufferHandle.Free();

            udError error = udRenderTarget_Destroy(ref pRenderView);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udRenderView.Destroy failed.");

            pRenderView = IntPtr.Zero;
        }

        public void Create(udContext context, udRenderContext renderer, UInt32 width, UInt32 height)
        {
            if (context.pContext == IntPtr.Zero)
                throw new Exception("context not instantiated");

            if (renderer.pRenderer == IntPtr.Zero)
                throw new Exception("renderer not instantiated");

            udError error = udRenderTarget_Create(context.pContext, ref pRenderView, renderer.pRenderer, width, height);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udRenderView.Create failed: " + error.ToString());

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

            udError error = udRenderTarget_SetTargets(pRenderView, colorBufferHandle.AddrOfPinnedObject(), clearColor, depthBufferHandle.AddrOfPinnedObject());
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udRenderView.SetTargets failed.");
        }

        public void GetMatrix(udRenderTargetMatrix matrixType, double[] cameraMatrix)
        {
            udError error = udRenderTarget_GetMatrix(pRenderView, matrixType, cameraMatrix);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udRenderView.GetMatrix failed.");
        }

        public void SetMatrix(udRenderTargetMatrix matrixType, double[] cameraMatrix)
        {
            if (pRenderView == IntPtr.Zero)
                throw new Exception("view not instantiated");

            udError error = udRenderTarget_SetMatrix(pRenderView, matrixType, cameraMatrix);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udRenderView.SetMatrix failed: " + error.ToString());
        }

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
}