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
        public static Dictionary<Camera, udRenderOptions> optionList = new Dictionary<Camera, udRenderOptions>();
        public static UDSessionThreadManager sessionKeeper = new UDSessionThreadManager();
        public static string vaultServer = "https://udstream.euclideon.com";

        public static string vaultUsername = ""; // Add credentials here for build
        
        public static string vaultPassword = ""; // Add credentials here for build

        // These strings exist to ensure during development no typo or error is ever set regarding the saving/loading/reading of 
        // .. usernames and passwords.
        public static string SavedUsernameKey = "VDKUsername";
        public static string SavedPasswordKey = "VDKPassword";
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
//            Debug.Log("Attempting to login with: " + vaultUsername + " / " + vaultPassword);
            if (!GlobalUDContext.isCreated)
            {
                if (Application.platform == RuntimePlatform.Android)
                    uContext.IgnoreCertificateVerification(true);
                try
                {
                    Debug.Log("Attempting to resume Euclideon udSDK session");
                    uContext.Try_Resume(vaultServer, "Unity", vaultUsername, true);
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
                  }
                  catch(System.Exception f) {
                    Debug.Log("Login Failed: " + f.ToString());
                    GlobalUDContext.isCreated = true;
                    Debug.Log("Logged in!");
                  }
                    //uContext.RequestLicense(LicenseType.Render);
                }
            }
            renderer.Create(uContext); 
        }
    }
}

