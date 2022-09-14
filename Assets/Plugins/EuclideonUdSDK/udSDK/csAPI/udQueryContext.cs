using System;
using System.Runtime.InteropServices;

//! The udQueryContext object provides an interface to query or filter pointclouds.

namespace udSDK 
{
  public static class udQueryContext_f
  {
    /// <summary>
    /// Create an instance of a udQueryContext with a specific model.
    /// A future release will add multiple model support and non-storedMatrix locations.
    /// </summary>
    /// <param name="pContext">The context to be used to create the query context.</param>
    /// <param name="ppQueryCtx">The pointer pointer of the udQueryContext. This will allocate an instance of udQueryContext into `ppQuery`.</param>
    /// <param name="pPointCloud">The point cloud to run the query on, it is located at its storedMatrix location (this can be changed using udQueryContext_ChangePointCloud).</param>
    /// <param name="pFilter">The filter to use in this query (this can be changed using udQueryContext_ChangeFilter).</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQueryContext_Create")]
    public static extern udError Create(IntPtr pContext, ref IntPtr ppQueryCtx, IntPtr pPointCloud,IntPtr pFilter);

    /// <summary>
    /// Resets the udQueryContext and uses a new filter.
    /// This will reset the query, any existing progress will be lost.
    /// </summary>
    /// <param name="pQueryCtx">The udQueryContext item previously created using udQueryContext_Create.</param>
    /// <param name="pFilter">The new filter to use in this query.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQueryContext_ChangeFilter")]
    public static extern udError ChangeFilter(IntPtr pQueryCtx, IntPtr pFilter);

    /// <summary>
    /// Resets the udQueryContext and uses a different model.
    /// </summary>
    /// <param name="pQueryCtx">The udQueryContext item previously created using udQueryContext_Create.</param>
    /// <param name="pPointCloud">The new model to use in this query.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQueryContext_ChangePointCloud")]
    public static extern udError ChangePointCloud(IntPtr pQueryCtx, IntPtr pPointCloud);

    /// <summary>
    /// Gets the next set of points from an existing udQueryContext.
    /// This should continue to be called until it returns udE_NotFound indicating the query has completed.
    /// </summary>
    /// <param name="pQueryCtx">The udQueryContext to execute.</param>
    /// <param name="pPointBuffer">The point buffer to write found points to.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQueryContext_ExecuteF64")]
    public static extern udError ExecuteF64(IntPtr pQueryCtx, IntPtr pPointBuffer);

    /// <summary>
    /// Gets the next set of points from an existing udQueryContext.
    /// This should continue to be called until it returns udE_NotFound indicating the query has completed.
    /// </summary>
    /// <param name="pQueryCtx">The udQueryContext to execute.</param>
    /// <param name="pPointBuffer">The point buffer to write found points to.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQueryContext_ExecuteI64")]
    public static extern udError ExecuteI64(IntPtr pQueryCtx, IntPtr pPointBuffer);

    /// <summary>
    /// Destroy the instance of udQueryContext.
    /// </summary>
    /// <param name="ppQueryCtx">The pointer pointer of the udQueryContext. This will destroy the instance of udQueryContext in `ppQuery` and set it to NULL.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQueryContext_Destroy")]
    public static extern udError Destroy(ref IntPtr ppQueryCtx);
      
    /// <summary>
    /// Test the specified voxel against the given udQueryFilter.
    /// </summary>
    /// <param name="pQueryFilter">The udQueryFilter to test.</param>
    /// <param name="matrix">The scene matrix for the model that contains the voxel being rendered.</param>
    /// <param name="pRenderInfo">The render info of the voxel being rendered.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udQuery_TestRenderVoxel")]
    public static extern udError TestRenderVoxel(IntPtr pQueryFilter, double[] matrix, IntPtr pRenderInfo);
  }
}