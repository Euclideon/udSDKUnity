using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace udSDK
{
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

    /* Unity serializable version of udSessionInfo
	*/
    public struct UDSessionInfo
    {
        public bool isOffline; //!< Is not 0 if this is an offline session (dongle or other offline license)
        public bool isDomain;  //!< If this is not 0 then this is a domain session (0 is non-domain session)
        public bool isPremium; //!< If this session will have access to premium features

        public float expiresTimestamp; //!< The timestamp in UTC when the session will automatically end

        public string displayName; //!< The null terminated display name of the user
    };

    public class udContext
    {
        public IntPtr pContext = IntPtr.Zero;
        public udSessionInfo pInfo = new udSessionInfo();

        public udContext()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass jcsdk = new AndroidJavaClass("com.euclideon.udSDK");
                jcsdk.CallStatic("setupJNI", jo);
            }
        }

        ~udContext()
        {
            if (pContext != IntPtr.Zero) {
                //this does not need to be called currently:
                //Disconnect();
            }
        }

        public void IgnoreCertificateVerification(bool ignore) 
        {
            if (ignore)
                Debug.LogWarning("WARNING: Certificate verification disabled");

            udConfig_IgnoreCertificateVerification(ignore);
        }

        public void Connect(string pURL, string pApplicationName, string pUsername, string pPassword)
        {
            udError error = udContext.udContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, true);

            if (error != udError.udE_Success)
                error = udContext_Connect(ref pContext, pURL, pApplicationName, pUsername, pPassword);

            if (error == udError.udE_ServerError)
                throw new Exception("Could not connect to server.");
            else if (error == udError.udE_AuthError)
                throw new Exception("Username or Password incorrect.");
            else if (error == udError.udE_OutOfSync)
                throw new Exception("Your clock doesn't match the remote server clock.");
            else if (error == udError.udE_DecryptionKeyRequired || error == udError.udE_DecryptionKeyMismatch )
                throw new Exception("A decryption key is required, or not matching");    
            else if (error == udError.udE_SignatureMismatch )
                throw new Exception("Server not accepting digital signature.");
            else if (error != udError.udE_Success)
                throw new Exception("Unknown error occurred: " + error.ToString() + ", please try again later.");
        }

        public void Try_Resume(string pURL, string pApplicationName, string pUsername, bool tryDongle)
        {
            udError error = udContext_TryResume(ref pContext, pURL, pApplicationName, pUsername, tryDongle);
            if (error != udError.udE_Success)
            {
                throw new Exception("Unable to keep session alive: " + error.ToString());
            }
        }

        public void Disconnect(bool endSession = false)
        {
            udError error = udContext_Disconnect(ref pContext, endSession);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udContext.Disconnect failed.");
        }

        public UDSessionInfo GetSessionInfo()
        {
            udError error = udContext_GetSessionInfo(pContext, ref pInfo);
            if (error != udSDK.udError.udE_Success)
                throw new Exception("udContext.Disconnect failed.");

            udSessionInfo sessionInfo = pInfo;
            UDSessionInfo formattedInfo = new UDSessionInfo();

            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            ulong cur_time = (ulong)(System.DateTime.UtcNow - epochStart).TotalSeconds;
            formattedInfo.expiresTimestamp = (float)(sessionInfo.expiresTimestamp - cur_time);

            formattedInfo.displayName = new string(sessionInfo.displayName);
            formattedInfo.displayName = formattedInfo.displayName.Trim('\0');

            formattedInfo.isDomain  = sessionInfo.isDomain  != 0 ? true : false;
            formattedInfo.isOffline = sessionInfo.isOffline != 0 ? true : false;
            formattedInfo.isPremium = sessionInfo.isPremium != 0 ? true : false;

            return formattedInfo; 
        }

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_TryResume(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, bool tryDongle);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_Connect(ref IntPtr ppContext, string pURL, string pApplicationName, string pUsername, string pPassword);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_Disconnect(ref IntPtr ppContext, bool endSession);

        [DllImport(UDSDKLibrary.name)]
        private static extern udError udContext_GetSessionInfo(IntPtr pContext, ref udSessionInfo pInfo);
        
        [DllImport(UDSDKLibrary.name)]
        private static extern udError udConfig_IgnoreCertificateVerification(bool ignore); // this ought to be in udConfig - but we don't implement any other part of that 
    }
}