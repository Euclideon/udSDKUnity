using System;
using System.Runtime.InteropServices;

//! The **udConfig** functions all set global configuration options for the entire loaded shared library.

namespace udSDK
{
  public static class udConfig_f
  {
    /// <summary>
    /// This function can be used to override the internal proxy auto-detection used by cURL.
    /// </summary>
    /// <param name="pProxyAddress">This is a null terminated string, can include port number and protocol. `192.168.0.1`, `169.123.123.1:80`, `https://10.4.0.1:8081` or `https://proxy.example.com`. Setting this to either `NULL` or (empty string) `""` will reset to attempting auto-detection.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConfig_ForceProxy")]
    public static extern udError ForceProxy(string pProxyAddress);
    
    /// <summary>
    /// This function is used in conjunction with `udConfig_ForceProxy` or the auto-detect proxy system to forward info from the user for their proxy details.
    /// </summary>
    /// <param name="pProxyUsername">This is a null terminated string of the username of the user for the proxy.</param>
    /// <param name="pProxyPassword">This is a null terminated string of the password of the user for the proxy.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConfig_SetProxyAuth")]
    public static extern udError SetProxyAuth(string pProxyUsername, string pProxyPassword);

    /// <summary>
    /// Allows udSDK to connect to server with an unrecognized certificate authority, sometimes required for self-signed certificates or poorly configured proxies.
    /// By default certificate verification is run (not ignored).
    /// </summary>
    /// <param name="ignore">`0` if verification is to be processed, all other values if certificate verification should be skipped.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConfig_IgnoreCertificateVerification")]
    public static extern udError IgnoreCertificateVerification(bool ignore);

    /// <summary>
    /// This function can be used to override the user agent used by cURL.
    /// </summary>
    /// <param name="pUserAgent">This is a null terminated string of the user agent.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConfig_SetUserAgent")]
    public static extern udError SetUserAgent(string pUserAgent);

    /// <summary>
    /// This function can be used to override the location that udSDK will save it's configuration files.
    /// By default, udSDK will attempt to use some known platform locations, in some cases failing to save entirely.
    /// This location will have `/euclideon/udsdk` appending to the end.
    /// </summary>
    /// <param name="pLocation">This is a null terminated string specifying a directory to save configurations files.</param>
    [DllImport(UDSDKLibrary.name, EntryPoint = "udConfig_SetConfigLocation")]
    public static extern udError SetConfigLocation(string pLocation);
  }
}