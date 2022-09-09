using System;
using System.Runtime.InteropServices;

//! The **udServerAPI** module provides an interface to communicate with a Euclideon udServer API directly in a simple fashion.

namespace udSDK
{
  public static class udServerAPI_f
  {
    /// <summary>
    /// Queries provided API on the specified Euclideon udServer.
    /// The application should call **udServerAPI_ReleaseResult** with `ppResult` to destroy the data once it's no longer needed.
    /// </summary>
    /// <param name="pContext">The context to execute the query with.</param>
    /// <param name="pAPIAddress">The API address to query, this is the part of the address *after* `/api/`. The rest of the address is constructed from the context provided.</param>
    /// <param name="pJSON">The JSON text to POST to the API address.</param>
    /// <param name="ppResult">A pointer to a location in which the result data is to be stored.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udServerAPI_Query")]
    public static extern udError Query(IntPtr pContext, string pAPIAddress, string pJSON, ref IntPtr ppResult);
      
    /// <summary>
    /// Destroys the result that was allocated.
    /// The value of `ppResult` will be set to `NULL`.
    /// </summary>
    /// <param name="ppResult"> A pointer to a location in which the result data is stored.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udServerAPI_ReleaseResult")]
    public static extern udError ReleaseResult(ref IntPtr ppResult);
  }
}