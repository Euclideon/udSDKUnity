using System;
using System.Runtime.InteropServices;

//! udCompare.h provides an interface to compare Unlimited Detail models.

namespace udSDK
{
  public static class udCompare_f
  {
    /// <summary>
    /// Compares input models using the Ball-Pivot Algorithm (BPA).
    /// This function does not start the conversion process, this allows the user to make additional changes to the convert job.
    /// </summary>
    /// <param name="pConvertContext">The convert context to be used to compare the models.</param>
    /// <param name="pBaseModelPath">The location of the base model to perform the comparison against.</param>
    /// <param name="pComparisonModelPath">The location of the model to compare against the base model.</param>
    /// <param name="ballRadius">The radius of the ball to use in the algorithm.</param>
    /// <param name="gridSize">The size of the grid to use in the algorithm.</param>
    /// <param name="pName">The output name for the comparison results. This is a UDS containing the points from pComparisonModelPath with additional channels for the comparison information.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udRenderContext_LockBlock")]
    public static extern udError BPA(IntPtr pConvertContext, string pBaseModelPath, string pComparisonModelPath, double ballRadius, double gridSize, string pName);
  }
}