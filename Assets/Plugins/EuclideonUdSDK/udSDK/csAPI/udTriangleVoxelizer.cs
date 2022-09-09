using System;
using System.Runtime.InteropServices;

//! The **udTriangleVoxelizer** object provides an interface to convert triangles to voxels to be added to a convert job.

namespace udSDK
{
  public static class udTriangleVoxelizer_f
  {
    /// <summary>
    /// Creates a udTriangleVoxelizer object at the specified grid resolution.
    /// The application should call **udTriangleVoxelizer_Destroy** with `ppTriRaster` to destroy the object once it's no longer needed.
    /// </summary>
    /// <param name="ppVoxelizer">A pointer to a location in which the new udTriangleVoxelizer object is stored.</param>
    /// <param name="gridRes">The intended grid resolution of the convert job.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udTriangleVoxelizer_Create")]
    public static extern udError Create(ref IntPtr ppVoxelizer, double gridRes); 
    
    /// <summary>
    /// Destroys a udTriangleVoxelizer object that was created using **udTriangleVoxelizer_Create**.
    /// The value of `ppTriRaster` will be set to `NULL`.
    /// </summary>
    /// <param name="ppVoxelizer">A pointer to a udTriangleVoxelizer object which is to be destroyed.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udTriangleVoxelizer_Destroy")]
    public static extern udError Destroy(ref IntPtr ppVoxelizer); 
    
    /// <summary>
    /// Set the vertices of the triangle, this primes a new triangle.
    /// Triangle will be treated as a line if v1 == v2 (v2 is degenerate)
    /// </summary>
    /// <param name="pVoxelizer"></param>
    /// <param name="pV0">The first vertex in the triangle.</param>
    /// <param name="pV1">The second vertex in the triangle.</param>
    /// <param name="pV2">The third vertex in the triangle.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udTriangleVoxelizer_SetTriangle")]
    public static extern udError SetTriangle(IntPtr pVoxelizer, double pV0, double pV1, double pV2); 
    
    /// <summary>
    /// Get the voxelized points from the current triangle.
    /// Returns a portion of voxelized points from current triangle, call repeatedly until *pPointCount is zero meaning the triangle is fully voxelized.
    /// The udTriangleVoxelizer object owns the arrays returned via ppPositions and ppBarycentricWeights.
    /// </summary>
    /// <param name="pVoxelizer">The voxelizer to be used to get the points.</param>
    /// <param name="ppPositions">A pointer to be populated with an array of positions.</param>
    /// <param name="ppBarycentricWeights">A pointer to be populated with an array of Barycentric weights.</param>
    /// <param name="pPointCount">A pointer to be populated with the number of positions and weights.</param>
    /// <param name="maxPoints">The maximum number of points to be returned.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udTriangleVoxelizer_GetPoints")]
    public static extern udError GetPoints(IntPtr pVoxelizer, ref IntPtr ppPositions, ref IntPtr ppBarycentricWeights, ref UInt32 pPointCount, UInt32 maxPoints);
  }
  
}