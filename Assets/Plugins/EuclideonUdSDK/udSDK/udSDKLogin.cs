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
        public static udContext uContext = new udContext();
        public static udRenderContext renderer = new udRenderContext();
        public static UDSessionThreadManager sessionKeeper = new UDSessionThreadManager();
        public static string vaultServer = "https://udstream.euclideon.com";

        public static string vaultUsername = ""; // Add credentials here for build
        
        public static string vaultPassword = ""; // Add credentials here for build

        // These strings exist to ensure during development no typo or error is ever set regarding the saving/loading/reading of 
        // .. usernames and passwords.
        public static string SavedUsernameKey = "Username";
        public static string SavedPasswordKey = "Password";
        public static void Login()
        {
            //For builds, set in login page
            vaultUsername = GlobalUDContext.SavedUsernameKey;
            vaultPassword = GlobalUDContext.SavedPasswordKey;

            // No longer using player prefs as they save to disk persistantly
          #if UNITY_EDITOR

            vaultUsername = EditorPrefs.GetString(SavedUsernameKey);
            vaultPassword = EditorPrefs.GetString(SavedPasswordKey);
          #endif
          
            Debug.Log("udSDK Trying to Login: " + vaultUsername);
            if (!GlobalUDContext.isCreated)
            {
                if (Application.platform == RuntimePlatform.Android)
                    uContext.IgnoreCertificateVerification(true);
                try
                {
                    Debug.Log("Attempting to resume Euclideon udSDK session");
                    uContext.Try_Resume(vaultServer, "Unity", vaultUsername, false);
                    //uContext.RequestLicense(LicenseType.Render);
                    isCreated = true;
                    Debug.Log("Resume Succeeded");
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.ToString() + "Logging in to Euclideon udSDK server");
                  try
                  {
                    GlobalUDContext.uContext.Connect(vaultServer, "Unity", vaultUsername, vaultPassword);
                    GlobalUDContext.isCreated = true;
                    Debug.Log("udSDK Logged in!");
                  }
                  catch(System.Exception f) {
                    Debug.Log("Login Failed: " + f.ToString());
                    return;
                  }
                    //uContext.RequestLicense(LicenseType.Render);
                }
            }
            else
            {
              Debug.Log("udSDK Skipping Login: already logged in");
            }
            GlobalUDContext.renderer.Create(uContext);
        }
    }
}

