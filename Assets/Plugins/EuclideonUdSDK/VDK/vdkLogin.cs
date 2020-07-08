using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Vault;
namespace Vault
{

    public static class GlobalVDKContext
    {
        public static bool isCreated = false;
        public static vdkContext vContext = new vdkContext();
        public static vdkRenderContext renderer = new vdkRenderContext();
        public static Dictionary<Camera, vdkRenderOptions> optionList = new Dictionary<Camera, vdkRenderOptions>();
        public static VDKSessionThreadManager sessionKeeper = new VDKSessionThreadManager();
        public static string vaultServer = "https://earth.vault.euclideon.com";
        public static string vaultUsername = ""; // Add credentials here for build
        
        public static string vaultPassword = ""; // Add credentials here for build

        // These strings exist to ensure during development no typo or error is ever set regarding the saving/loading/reading of 
        // .. usernames and passwords.
        public static string SavedUsernameKey = "VDKUsername";
        public static string SavedPasswordKey = "VDKPassword";
        public static void Login()
        {
             //For builds, set in login page
             vaultUsername = GlobalVDKContext.SavedUsernameKey;
             vaultPassword = GlobalVDKContext.SavedPasswordKey;

            // No longer using player prefs as they save to disk persistantly
          #if UNITY_EDITOR

            vaultUsername = EditorPrefs.GetString(SavedUsernameKey);
            vaultPassword = EditorPrefs.GetString(SavedPasswordKey);
          #endif
//            Debug.Log("Attempting to login with: " + vaultUsername + " / " + vaultPassword);
            if (!GlobalVDKContext.isCreated)
            {
#if UNITY_ANDROID
        vContext.IgnoreCertificateVerification(true);
#endif

        try
                {
                    Debug.Log("Attempting to resume Euclideon Vault session");
                    vContext.Try_Resume(vaultServer, "Unity", vaultUsername, true);
                    //vContext.RequestLicense(LicenseType.Render);
                    isCreated = true;
                    Debug.Log("Resume Succeeded");
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.ToString() + "Logging in to Euclideon Vault server");
                  try
                  {
                    GlobalVDKContext.vContext.Connect(vaultServer, "Unity", vaultUsername, vaultPassword);
                  }
                  catch(System.Exception f) {
                    Debug.Log("Login Failed: " + f.ToString());
                  }
                    vContext.RequestLicense(LicenseType.Render);
                    GlobalVDKContext.isCreated = true;
                    Debug.Log("Logged in!");
                }
            }
            renderer.Create(vContext); // Maybe not call here? Throws errors in editor
        }
    }
}

