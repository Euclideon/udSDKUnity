using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using udSDK;

namespace udSDK
{
    public static class GlobalUDContext
    {
        public static bool isCreated = false;
        public static UDContext uContext = new UDContext();
        public static UDRenderContext renderer = new UDRenderContext();
        public static UDSessionThreadManager sessionKeeper = new UDSessionThreadManager();
        
        // Add credentials below for build
        public static string vaultServer = "https://udstream.euclideon.com";
        public static string vaultUsername = "";
        public static string vaultPassword = "";
        public static bool ignoreCertificate = false;

        // These strings exist to ensure during development no typo or error is ever set
        // regarding the saving/loading/reading of usernames and passwords.
        public static string SavedServerKey = "Server";
        public static string SavedUsernameKey = "Username";
        public static string SavedPasswordKey = "Password";

        // For validating the version 
        public static UDVersion sdkVersion; // the version of the sdk currently linked to the project
        public static UDVersion wrapperVersion = new UDVersion(2, 3, 1); // the latest version of the sdk supported by the sdk

        public static void Login()
        {
            //For builds, set in login page
            vaultUsername = GlobalUDContext.SavedUsernameKey;
            vaultPassword = GlobalUDContext.SavedPasswordKey;

            // No longer using player prefs as they save to disk persistantly
#if UNITY_EDITOR
            vaultServer = EditorPrefs.GetString(SavedServerKey);
            vaultUsername = EditorPrefs.GetString(SavedUsernameKey);
            vaultPassword = EditorPrefs.GetString(SavedPasswordKey);
#endif
          
            Debug.Log("udSDK Trying to Login: " + vaultUsername);
            if (!GlobalUDContext.isCreated)
            {
              if (ignoreCertificate)
              {
                if (Application.platform == RuntimePlatform.Android)
                  AttemptLogin(LoginMethod.IgnoreCertificate);
                else 
                  AttemptLogin(LoginMethod.Domain);
              }
              else
              {
                AttemptLogin(LoginMethod.Legacy);
              }

              UDSessionInfo info = GlobalUDContext.uContext.GetSessionInfo();
              Debug.Log("UDSessionInfo.isDomain = " + info.isDomain);
              CheckSDKVersion();
            }
            else
            {
              Debug.Log("udSDK Skipping Login: already logged in");
            }

            renderer.Create(uContext);
        }

        public enum LoginMethod
        {
          Standard, 
          Legacy, 
          Domain, 
          Key, 
          IgnoreCertificate
        }

        public static void AttemptLogin(LoginMethod loginMethod)
        {
          try
          {
            switch (loginMethod)
            {
              // Note: standard and legacy the same for now
              case (LoginMethod.Standard):
                Debug.Log("Attempting to resume Euclideon udSDK session...");
                GlobalUDContext.uContext.Connect(vaultServer, "Unity", vaultUsername, vaultPassword);
                GlobalUDContext.isCreated = true;
                Debug.Log("Resume Succeeded");
                break;
              
              case (LoginMethod.Legacy):
                Debug.Log("Attempting to resume Euclideon udSDK session...");
                GlobalUDContext.uContext.Connect(vaultServer, "Unity", vaultUsername, vaultPassword);
                GlobalUDContext.isCreated = true;
                Debug.Log("Resume Succeeded");
                break;
              
              case (LoginMethod.Domain):
                Debug.Log("Attempting domain login...");
                GlobalUDContext.uContext.ConnectFromDomain(vaultServer, "globalpool");
                GlobalUDContext.isCreated = true;
                Debug.Log("Login Succeeded");
                break;
              
              case (LoginMethod.IgnoreCertificate):
                Debug.Log("Attempting login ignoring certificate...");
                GlobalUDContext.uContext.IgnoreCertificateVerification(true);
                GlobalUDContext.uContext.ConnectFromDomain(vaultServer, "globalpool");
                GlobalUDContext.isCreated = true;
                Debug.Log("Login Succeeded");
                break;
            }
          }
          catch (System.Exception e)
          {
            Debug.LogError("Login Failed: " + e.ToString());
            return;
          }
        }
        
        public static void CheckSDKVersion()
        {
          if(GlobalUDContext.sdkVersion == null)
            GlobalUDContext.sdkVersion = new UDVersion(); 

          if (GlobalUDContext.sdkVersion.Equals(wrapperVersion))
            return ; 

          // identify mismatches that will likely affect the functionality (major, and minor versions, ignoring patch)
          if(wrapperVersion.GreaterThan(sdkVersion, 1) )
          {
            Debug.LogWarning("udSDK "+sdkVersion.Format()+" is deprecated, and does not match the integration. "+
                             "Please update to the supported version "+wrapperVersion.Format()+".");
          }
          else if(sdkVersion.GreaterThan(wrapperVersion, 1) )
          {
            Debug.LogWarning("udSDK "+sdkVersion.Format()+" is newer than the integration. "+
                             "The integration should be updated, as certain features may not be working correctly.");
          }
        }
    }
}

