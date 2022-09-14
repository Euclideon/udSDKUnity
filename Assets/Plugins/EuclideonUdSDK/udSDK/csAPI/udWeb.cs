using System;
using System.Runtime.InteropServices;

//! This is an optional helper module to assist with doing web requests. Internally it uses libcURL with the global settings (proxy and certificates information) from `udConfig`.
//! As this is not a core module, it does not accept a `udContext` and can be used without having a udServer available.

namespace udSDK
{
  /// <summary>
  /// These are the support HTTP method types in udWeb
  /// </summary>
  public enum udWebMethod
  {
    udWM_HEAD = 0x0, //!< Performs a HEAD request
    udWM_GET  = 0x1, //!< Performs a GET request
    udWM_POST = 0x2, //!< Performs a POST request
  };
  
  /// <summary>
  /// This structure stores the options for a udWeb request
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct udWebOptions
  {
    public udWebMethod method; //!< The HTTP method to use for the request.
    public IntPtr pPostData; //!< The data to send to the server as part of a POST request.
    public UInt64 postLength; //!< The length of the data being sent to the server.
    public UInt64 rangeBegin; //!< The position of the first byte in the requested file to receive data from.
    public UInt64 rangeEnd; //!< The position of the last byte in the requested file to receive data from.
    public IntPtr pAuthUsername; //!< The username to use when authenticating with the server.
    public IntPtr pAuthPassword; //!< The password to use when authenticating with the server.
  };
  
  public static class udWeb_f
  {
    /// <summary>
    /// This sends a GET request to a given URL, the response (if any) is written to `ppResponse`.
    /// </summary>
    /// <param name="pUrl">The URL to request.</param>
    /// <param name="ppResponse">This will be modified with a pointer to internal udSDK memory with the contents of the request. You must call `udWeb_ReleaseResponse` to free this memory.</param>
    /// <param name="pResponseLength">If non-null, the pointer's memory will be set to the length of `ppResponse`.</param>
    /// <param name="pHTTPResponseCode">If non-null this will be set to the HTTP status code. See https ://www.ietf.org/assignments/http-status-codes/http-status-codes.xml for status codes.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udWeb_Request")]
    public static extern udError Request(string pUrl, ref IntPtr ppResponse, ref ulong pResponseLength, ref int pHTTPResponseCode);

    /// <summary>
    /// This sends a request to a given URL using the specified options, the response (if any) is written to `ppResponse`.
    /// </summary>
    /// <param name="pUrl">The URL to request.</param>
    /// <param name="options">The options for the request, see above for details.</param>
    /// <param name="ppResponse">This will be modified with a pointer to internal udSDK memory with the contents of the request. You must call `udWeb_ReleaseResponse` to free this memory.</param>
    /// <param name="pResponseLength">If non-null, the pointer's memory will be set to the length of `ppResponse`.</param>
    /// <param name="pHTTPResponseCode">If non-null this will be set to the HTTP status code. See https ://www.ietf.org/assignments/http-status-codes/http-status-codes.xml for status codes.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udWeb_RequestAdv")]
    public static extern udError RequestAdv(string pUrl, udWebOptions options, ref IntPtr ppResponse, ref UInt64 pResponseLength, ref int pHTTPResponseCode);

    /// <summary>
    /// Frees memory of a prior call to `udWeb_Request` or `udWeb_RequestAdv`.
    /// </summary>
    /// <param name="ppResponse">A pointer to a pointer containing the response from a prior call to `udWeb_Request` or `udWeb_RequestAdv`.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udWeb_ReleaseResponse")]
    public static extern udError ReleaseResponse(ref IntPtr ppResponse);
  }
}