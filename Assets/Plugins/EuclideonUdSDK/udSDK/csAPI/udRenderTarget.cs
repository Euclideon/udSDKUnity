using System;
using System.Runtime.InteropServices;

//! The **udRenderTarget** object provides an interface to specify a viewport to render to.
//! Once instantiated, the **udRenderTarget** can have its targets set, providing both a colour and depth output of the render which will utilize the matrices provided to the SetMatrix function.

namespace udSDK
{
    /// <summary>
    /// These are the various matrix types used within the render target
    /// </summary>
    public enum udRenderTargetMatrix
    {
        udRTM_Camera,     //!< The local to world-space transform of the camera (View is implicitly set as the inverse)
        udRTM_View,       //!< The view-space transform for the model (does not need to be set explicitly)
        udRTM_Projection, //!< The projection matrix (default is 60 degree LH)
        udRTM_Viewport,   //!< Viewport scaling matrix (default width and height of viewport)

        udRTM_Count       //!< Total number of matrix types. Used internally but can be used as an iterator max when checking matrix information.
    };

    public static class udRenderTarget_f
    {
        /// <summary>
        /// Create a udRenderTarget with a viewport using `width` and `height`.
        /// The application should call **udRenderTarget_Destroy** with `ppRenderTarget` to destroy the object once it's no longer needed.
        /// </summary>
        /// <param name="pContext">The context to be used to create the render target.</param>
        /// <param name="ppRenderTarget">The pointer pointer of the udRenderTarget. This will allocate an instance of udRenderTarget into `ppRenderTarget`.</param>
        /// <param name="pRenderer">The renderer associated with the render target.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_Create")]
        public static extern udError Create(IntPtr pContext, ref IntPtr ppRenderTarget, IntPtr pRenderer, UInt32 width, UInt32 height);

        /// <summary>
        /// Destroys the instance of `ppRenderTarget`.
        /// </summary>
        /// <param name="ppRenderTarget">The pointer pointer of the udRenderTarget. This will deallocate the instance of udRenderTarget.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_Destroy" )]
        public static extern udError Destroy(ref IntPtr ppRenderTarget);

        /// <summary>
        /// Set a memory buffers that a render target will write to.
        /// This internally calls udRenderTarget_SetTargetsWithPitch with both color and depth pitches set to 0.
        /// </summary>
        /// <param name="pRenderTarget">The render target to associate a target buffer with.</param>
        /// <param name="pColorBuffer">The color buffer, if null the buffer will not be rendered to anymore.</param>
        /// <param name="colorClearValue">The clear value to clear the color buffer with.</param>
        /// <param name="pDepthBuffer">The depth buffer, required</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_SetTargets")]
        public static extern udError SetTargets(IntPtr pRenderTarget, IntPtr pColorBuffer, UInt32 colorClearValue, IntPtr pDepthBuffer);

        /// <summary>
        /// Set a memory buffers that a render target will write to (with pitch).
        /// </summary>
        /// <param name="pRenderTarget">The render target to associate a target buffer with.</param>
        /// <param name="pColorBuffer">The color buffer, if null the buffer will not be rendered to anymore.</param>
        /// <param name="colorClearValue">The clear value to clear the color buffer with.</param>
        /// <param name="pDepthBuffer">The depth buffer, required</param>
        /// <param name="colorPitchInBytes">The number of bytes that make up a row of the color buffer.</param>
        /// <param name="depthPitchInBytes">The number of bytes that make up a row of the depth buffer.</param>
        /// <returns></returns>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_SetTargetsWithPitch")]
        public static extern udError SetTargetsWithPitch(IntPtr pRenderTarget, IntPtr pColorBuffer, UInt32 colorClearValue, IntPtr pDepthBuffer, UInt32 colorPitchInBytes, UInt32 depthPitchInBytes);

        /// <summary>
        /// Get the matrix associated with `pRenderTarget` of type `matrixType` and fill it in `cameraMatrix`.
        /// </summary>
        /// <param name="pRenderTarget">The render target to get the matrix from.</param>
        /// <param name="matrixType">The type of matrix to get.</param>
        /// <param name="cameraMatrix">The array of 16 doubles which gets filled out with the matrix.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_GetMatrix")]
        public static extern udError GetMatrix(IntPtr pRenderTarget, udRenderTargetMatrix matrixType, double[] cameraMatrix);

        /// <summary>
        /// Set the matrix associated with `pRenderTarget` of type `matrixType` and get it from `cameraMatrix`.
        /// </summary>
        /// <param name="pRenderTarget">The render target to set the matrix to.</param>
        /// <param name="matrixType">The type of matrix to set.</param>
        /// <param name="cameraMatrix">The array of 16 doubles to fill out the internal matrix with.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_SetMatrix")]
        public static extern udError SetMatrix(IntPtr pRenderTarget, udRenderTargetMatrix matrixType, double[] cameraMatrix);
        
        /// <summary>
        /// Set the logarithmic depth near and far planes that will be used for logarithmic rendering.
        /// Note: These values are only used when the 'udRCF_LogarithmicDepth' rendering flag is set.
        /// </summary>
        /// <param name="pRenderTarget">The render target to set the matrix to.</param>
        /// <param name="nearPlane">The value that the near plane will be set to.</param>
        /// <param name="farPlane">The value that the far plane will be set to.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderTarget_SetLogarithmicDepthPlanes")]
        public static extern udError SetLogarithmicDepthPlanes(IntPtr pRenderTarget, double nearPlane, double farPlane);
    }
}