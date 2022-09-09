using System;
using System.Runtime.InteropServices;

//! The **udContext** object provides an interface to connect and communicate with a Euclideon udServer.
//! Once instantiated, the **udContext** can be passed into many udSDK functions to provide a context to operate within.

namespace udSDK
{
    /// <summary>
    /// This structure stores information about the current session
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct udSessionInfo
    {
        public uint isOffline; //!< Is not 0 if this is an offline session (dongle or other offline license)
        public uint isDomain;  //!< If this is not 0 then this is a domain session (0 is non-domain session)
        public uint isPremium; //!< If this session will have access to premium features

        public double expiresTimestamp; //!< The timestamp in UTC when the session will automatically end
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public char[] displayName; //!< The null terminated display name of the user
    };

    public static class udContext_f
    {  
        /// <summary>
        /// Establishes a connection to a Euclideon udCloud and creates a new udContext object.
        /// The application should call udContext_ConnectComplete or udContext_ConnectCancel with ppPartialContext to destroy the object
        /// </summary>
        /// <param name="ppPartialContext">A pointer to a location in which the new udContextPartial object is stored.</param>
        /// <param name="pServerURL">A Server URL to the Euclideon udCloud instance.</param>
        /// <param name="pApplicationName">The name of the application using udSDK.</param>
        /// <param name="pApplicationVersion">The version of the application using udSDK.</param>
        /// <param name="ppApprovePath">The address that needs to be opened in a browser window (if this is nullptr proceed to udContext_ConnectComplete)</param>
        /// <param name="ppApproveCode">A code that the user can use to verify their session in the udCloud API on another device (can be NULL)</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_ConnectStart")]
        public static extern udError ConnectStart(ref IntPtr ppPartialContext, string pServerURL, string pApplicationName, string pApplicationVersion, ref string ppApprovePath, ref string ppApproveCode);
        
        /// <summary>
        /// Establishes a connection to a Euclideon udCloud server and creates a new udContext object.
        /// The application should call **udContext_Disconnect** with `ppContext` to destroy the object once it's no longer needed.
        /// ppApprovePath from udContext_ConnectStart will be invalid after this call
        /// </summary>
        /// <param name="ppContext">A pointer to a location in which the new udContext object is stored.</param>
        /// <param name="ppPartialContext">A pointer to the udContextPartial created from udContext_ConnectStart (will be freed on a successful login).</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_ConnectComplete")]
        public static extern udError ConnectComplete(ref IntPtr ppContext, ref IntPtr ppPartialContext);
        
        /// <summary>
        /// Cancels a login attempt to a Euclideon udCloud server;
        /// </summary>
        /// <param name="ppPartialContext">A pointer to the udContextPartial created from udContext_ConnectStart (will be freed).</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_ConnectCancel")]
        public static extern udError ConnectCancel(ref IntPtr ppPartialContext);
        
        /// <summary>
        /// Establishes a connection to Euclideon udCloud server and creates a new udContext object.
        /// The application should call udContext_Disconnect with `ppContext` to destroy the object once it's no longer needed.
        /// When used from the Emscripten/WebAssembly builds it will try start a domain session when pKey is NULL
        /// </summary>
        /// <param name="ppContext">A pointer to a location in which the new udContext object is stored.</param>
        /// <param name="pServerURL">A URL to a Euclideon udCloud server to connect to.</param>
        /// <param name="pApplicationName">The name of the application using udSDK.</param>
        /// <param name="pApplicationVersion">The version of the application using udSDK.</param>
        /// <param name="pKey">The provided key that will start the context</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_ConnectWithKey")]
        public static extern udError ConnectWithKey(ref IntPtr ppContext, string pServerURL, string pApplicationName, string pApplicationVersion, string pKey);
        
        /// <summary>
        /// Establishes a (legacy) connection to a Euclideon udServer and creates a new udContext object.
        /// </summary>
        /// <param name="ppContext">A pointer to a location in which the new udContext object is stored.</param>
        /// <param name="pURL">A URL to a Euclideon udServer to connect to.</param>
        /// <param name="pApplicationName">The name of the application connecting to the Euclideon udServer.</param>
        /// <param name="pEmail">The email address of the user connecting to the Euclideon udServer.</param>
        /// <param name="pPassword">The password of the user connecting to the Euclideon udServer.</param>
        /// <returns></returns>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_ConnectLegacy")]
        public static extern udError ConnectLegacy(ref IntPtr ppContext, string pURL, string pApplicationName, string pEmail, string pPassword);
        
        /// <summary>
        /// Establishes a (legacy) connection to a Euclideon udServer and creates a new udContext object.
        /// The application should call **udContext_Disconnect** with `ppContext` to destroy the object once it's no longer needed.
        /// This connect function is specific to the Emscripten/WebAssembly builds and will return udE_Unsupported on all other platforms
        /// </summary>
        /// <param name="ppContext">A pointer to a location in which the new udContext object is stored.</param>
        /// <param name="pServerURL">A URL to a Euclideon udServer to connect to.</param>
        /// <param name="pApplicationName">The name of the application connecting to the Euclideon udServer.</param>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_ConnectFromDomain")]
        public static extern udError ConnectFromDomain(ref IntPtr ppContext, string pServerURL, string pApplicationName);
        
        /// <summary>
        /// Attempts to reestablish a connection to Euclideon udCloud, Euclideon udServer (or run offline with an offline context) and creates a new udContext object.
        /// </summary>
        /// <param name="ppContext">A pointer to a location in which the new udContext object is stored.</param>
        /// <param name="pURL">A URL to a Euclideon udServer to connect to.</param>
        /// <param name="pApplicationName"></param>
        /// <param name="pUsername"></param>
        /// <param name="tryDongle"></param>
        /// <returns></returns>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_TryResume")]
        public static extern udError TryResume(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, bool tryDongle);

        /// <summary>
        /// Disconnects and destroys a udContext object that was created using one of the context creation functions.
        /// </summary>
        /// <param name="ppContext">A pointer to a udContext object which is to be disconnected and destroyed.</param>
        /// <param name="endSession">Not 0 if the session will be ended (cannot be resumed)</param>
        /// <returns></returns>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_Disconnect")]
        public static extern udError Disconnect(ref IntPtr ppContext, bool endSession);

        /// <summary>
        /// Get the session information from an active udContext.
        /// </summary>
        /// <param name="pContext">The udContext to get the session info for.</param>
        /// <param name="pInfo">The preallocated structure to copy the info into.</param>
        /// <returns></returns>
        [DllImport(UDSDKLibrary.name, EntryPoint = "udContext_GetSessionInfo")]
        public static extern udError GetSessionInfo(IntPtr pContext, ref udSessionInfo pInfo);
    }
}