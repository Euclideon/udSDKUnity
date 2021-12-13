using System;
using System.Runtime.InteropServices;

namespace udSDK
{
    [StructLayout(LayoutKind.Sequential)]
    public struct udSessionInfo
    {
        public uint apiVersion; //!< The version of the API of the remote system (0 is offine, 1 is legacy udServer, 2 is udCloud)
        public uint isDomain;  //!< If this is not 0 then this is a domain session (0 is non-domain session)
        public uint isPremium; //!< If this session will have access to premium features
        public double expiresTimestamp; //!< The timestamp in UTC when the session will automatically end
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public char[] displayName; //!< The null terminated display name of the user
    };

    /* Unity serializable version of udSessionInfo */
    public struct UDSessionInfo
    {
        public uint apiVersion; //!< The version of the API of the remote system (0 is offine, 1 is legacy udServer, 2 is udCloud)
        public bool isDomain;  //!< If this is not 0 then this is a domain session (0 is non-domain session)
        public bool isPremium; //!< If this session will have access to premium features
        public float expiresTimestamp; //!< The timestamp in UTC when the session will automatically end
        public string displayName; //!< The null terminated display name of the user
    };


    /// <summary>
    /// Contains the information return from starting a connection with ud<b><i>Cloud</i></b>.
    /// </summary>
    public struct UDCloudApproval
    {
        public string path;
        public string code;
    }

    public class udContext
    {
        public IntPtr pContext = IntPtr.Zero;
        public IntPtr pPartialContext = IntPtr.Zero;
        public udSessionInfo info = new udSessionInfo();
        public IntPtr pApprovePath = IntPtr.Zero;
        public IntPtr pApproveCode = IntPtr.Zero;

        public udContext()
        {
            //if (Application.platform == RuntimePlatform.Android)
            //{
            //    AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //    AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            //    AndroidJavaClass jcsdk = new AndroidJavaClass("com.euclideon.udSDK");
            //    jcsdk.CallStatic("setupJNI", jo);
            //}
        }

        ~udContext()
        {
            if (pContext != IntPtr.Zero)
                Disconnect(false);
        }

        /// <summary>
        /// Should we ignore the certification authentication?
        /// </summary>
        /// <param name="ignore"></param>
        public void IgnoreCertificateVerification(bool ignore)
        {
            udConfig_IgnoreCertificateVerification(ignore);
        }

        /// <summary>
        /// Starts the ud<b><i>Cloud</i></b> connection process. This will create a partial context, that will close after ConnectComplete is called.
        /// </summary>
        /// <param name="pURL">The ud<b><i>Cloud</i></b> server URL to use.</param>
        /// <param name="pApplicationName">The name of the application statrting the connection.</param>
        /// <param name="pApplicationVersion">The version of the application starting the connection.</param>
        /// <returns> A UDCloudApproval struct containing strings to the approval path to open, and an optional approval code for interaction with ud<b><i>Cloud</i></b> API.</returns>
        public UDCloudApproval ConnectStart(string pURL, string pApplicationName, string pApplicationVersion)
        {
            udError error = udContext_ConnectStart(ref pPartialContext, pURL, pApplicationName, pApplicationVersion, ref pApprovePath, ref pApproveCode);
                            
            if (error != udError.udE_Success)
                throw new Exception("Failed to start udCloud connection with error: " + Enum.GetName(typeof(udError), error));

            UDCloudApproval cloudApproval = new UDCloudApproval();
            cloudApproval.path = Marshal.PtrToStringAnsi(pApprovePath);
            cloudApproval.code = Marshal.PtrToStringAnsi(pApproveCode);
            Marshal.FreeHGlobal(pApprovePath);
            Marshal.FreeHGlobal(pApproveCode);

            return cloudApproval;
        }

        /// <summary>
        /// Completes the ud<b><i>Cloud</i></b> connection process. This will destroy the partial context created by ConnectStart.
        /// </summary>
        public void ConnectComplete()
        {
            udError error = udContext_ConnectComplete(ref pContext, ref pPartialContext);

            if (error != udError.udE_Success)
                throw new Exception("Failed to complete udCloud connection with error: " + Enum.GetName(typeof(udError), error));
        }

        /// <summary>
        /// Cancels the connection attempt to ud<b><i>Cloud</i></b>, and destroys the partial context.
        /// </summary>
        public void ConnectCancel()
        {
            udError error = udContext_ConnectCancel(ref pPartialContext);

            if (error != udError.udE_Success)
                throw new Exception("Failed to cancel udCloud connection with error: " + Enum.GetName(typeof(udError), error));
        }

        /// <summary>
        /// Connects to a specified udServer with a key.
        /// </summary>
        /// <param name="pServerURL">The udserver URL to use.</param>
        /// <param name="pApplicationName"></param>
        /// <param name="pApplicationVersion"></param>
        /// <param name="pKey"></param>
        public void ConnectWithKey(string pServerURL, string pApplicationName, string pApplicationVersion, string pKey)
        {
            udError error = udContext_ConnectWithKey(ref pContext, pServerURL, pApplicationName, pApplicationVersion, pKey);

            if (error != udError.udE_Success)
                throw new Exception("Failed to connect to udCloud with error: " + Enum.GetName(typeof(udError), error));
        }

        /// <summary>
        /// Connects to a legacy udServer with a username and password.
        /// </summary>
        /// <param name="pURL">The legacy udServer URL to use.</param>
        /// <param name="pApplicationName">The name of the application attempting to connect.</param>
        /// <param name="pUsername">The username of the user trying to establish a context.</param>
        /// <param name="pPassword">The password of the user trying to establish a context.</param>
        public void Connect(string pURL, string pApplicationName, string pUsername, string pPassword)
        {
            udError error = udContext.udContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, true);

            if (error != udError.udE_Success)
                error = udContext_ConnectLegacy(ref pContext, pURL, pApplicationName, pUsername, pPassword);

            if (error == udError.udE_ServerError)
                throw new Exception("Could not connect to server.");
            else if (error == udError.udE_AuthError)
                throw new Exception("Username or Password incorrect.");
            else if (error == udError.udE_OutOfSync)
                throw new Exception("Your clock doesn't match the remote server clock.");
            else if (error == udError.udE_DecryptionKeyRequired || error == udError.udE_DecryptionKeyMismatch)
                throw new Exception("A decryption key is required, or not matching");
            else if (error == udError.udE_SignatureMismatch)
                throw new Exception("Server not accepting digital signature.");
            else if (error != udError.udE_Success)
                throw new Exception("Unknown error occurred: " + Enum.GetName(typeof(udError), error) + ", please try again later.");
        }

        /// <summary>
        /// Tries to resume a udContext.
        /// </summary>
        /// <param name="pURL">The server URL to attempt the reconnection.</param>
        /// <param name="pApplicationName">The application name attempting to reconnect.</param>
        /// <param name="pUsername">The username of the user trying to re-establish a context.</param>
        /// <param name="tryDongle">Should the function try a dongle?</param>
        public void Try_Resume(string pURL, string pApplicationName, string pUsername, bool tryDongle)
        {
            udError error = udContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, tryDongle);

            if (error != udError.udE_Success)
                throw new Exception("Unable to keep session alive with error: " + Enum.GetName(typeof(udError), error));
        }

        /// <summary>
        /// Attempts to disconnect the context. All references to the context must be desstroyed beforehand.
        /// </summary>
        /// <param name="endSession">Should the function end the session? Try_Resume can not re-establish a context if so.</param>
        public void Disconnect(bool endSession)
        {
            Marshal.FreeHGlobal(pContext);
            udError error = udContext_Disconnect(ref pContext, endSession);

            if (error != udSDK.udError.udE_Success)
                throw new Exception("Disconnect failed with error: " + Enum.GetName(typeof(udError), error));
        }

        /// <summary>
        /// Gets the information about the current udSession.
        /// </summary>
        /// <returns>A UDSessionInfo struct containing information about the session.</returns>
        public UDSessionInfo GetSessionInfo()
        {
            udError error = udContext_GetSessionInfo(pContext, ref info);

            if (error != udSDK.udError.udE_Success)
                throw new Exception("GetSessionInfo failed with error: " + Enum.GetName(typeof(udError), error));

            udSessionInfo sessionInfo = info;
            UDSessionInfo formattedInfo = new UDSessionInfo();

            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            ulong cur_time = (ulong)(System.DateTime.UtcNow - epochStart).TotalSeconds;
            formattedInfo.expiresTimestamp = (float)(sessionInfo.expiresTimestamp - cur_time);

            formattedInfo.displayName = new string(sessionInfo.displayName);
            formattedInfo.displayName = formattedInfo.displayName.Trim('\0');

            formattedInfo.isDomain = sessionInfo.isDomain != 0 ? true : false;
            formattedInfo.apiVersion = sessionInfo.apiVersion;
            formattedInfo.isPremium = sessionInfo.isPremium != 0 ? true : false;

            return formattedInfo;
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_TryResume(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, bool tryDongle);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_ConnectLegacy(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, string pPassword);

        [DllImport(UDSDKLibrary.name)]
        private unsafe static extern udError udContext_ConnectStart(ref IntPtr ppPartialContext, string pURL, string pApplicationName, string pApplicationVersion, ref IntPtr ppApprovePath, ref IntPtr ppApproveCode);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_ConnectComplete(ref IntPtr ppContext, ref IntPtr ppPartialContext);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_ConnectCancel(ref IntPtr ppPartialContextIntPtr);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_ConnectWithKey(ref IntPtr ppContext, string pServerURL, string pApplicationName, string pApplicationVersion, string pKey);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_ConnectFromDomain(ref IntPtr ppContext, string pURL, string pApplicationName);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_Disconnect(ref IntPtr ppContext, bool endSession); //c++ handles bool to int conversion

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_GetSessionInfo(IntPtr pContext, ref udSessionInfo info);
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConfig_IgnoreCertificateVerification(bool ignore); // this ought to be in udConfig - but we don't implement any other part of that 
    }
}