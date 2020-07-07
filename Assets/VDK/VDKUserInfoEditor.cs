using UnityEngine;
using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif
using Vault;




namespace vdk
{

#if UNITY_EDITOR

    public class VDKUserInfoEditor : EditorWindow
    {
        [MenuItem("VDK/Login")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(VDKUserInput));
        }
    }

    public class VDKUserInput : EditorWindow
    {
        // Show window logic
        private static void ShowWindow()
        {
            var window = GetWindow<VDKUserInput>();
            window.titleContent = new GUIContent("newWindow");
            window.Show();

        }

        // Load username
        private void Awake()
        {
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            usernameEntry = EditorPrefs.GetString(GlobalVDKContext.SavedUsernameKey);
            passwordEntry = EditorPrefs.GetString(GlobalVDKContext.SavedPasswordKey);

            CommitUserInfo();
        }

        private string PrefsUsername()
        {
            return EditorPrefs.GetString(GlobalVDKContext.SavedUsernameKey);
        }

        // Strings used to store the entered info
        private string usernameEntry;
        private string passwordEntry;
        //private bool saveToDisk = false;

        private void OnGUI()
        {
            // Current input related event 
            Event e = Event.current;

            // Centered layer style for headers
            var CenteredBold = GUI.skin.GetStyle("Label");
            CenteredBold.alignment = TextAnchor.MiddleCenter;
            CenteredBold.fontStyle = FontStyle.Bold;

            // Centered bottom layerstyle for clear button
            var BottomButton = GUI.skin.GetStyle("Button");
            BottomButton.alignment = TextAnchor.LowerCenter;

            // Drawing the GUI elements
            EditorGUILayout.LabelField("VDK Login", CenteredBold);

            // Username and password
            EditorGUILayout.LabelField("Username:");
            usernameEntry = EditorGUILayout.TextField(usernameEntry);
            EditorGUILayout.LabelField("Password:");
            passwordEntry = EditorGUILayout.PasswordField(passwordEntry);

            bool pressed = GUILayout.Button("Confirm");

            EditorGUILayout.LabelField("*WARNING* these details are saved to your computer in plaintext");

            // Radio button for saving to the hard disk or not
            // saveToDisk = GUILayout.Toggle(saveToDisk, "Remember my details", RadioButtonCentered);

            bool clearInfo = GUILayout.Button("Remove saved info", BottomButton);

            // Commit the user info in several ways.
            if (pressed || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                // Save user info
                CommitUserInfo();
                this.Close();
            }

            // wipe the users info
            if (clearInfo == true)
            {
                RemoveUserInfo();
            }
        }

        // Called from ONGUI when the user commits info
        private void CommitUserInfo()
        {
            EditorPrefs.SetString(GlobalVDKContext.SavedUsernameKey, usernameEntry);
            EditorPrefs.SetString(GlobalVDKContext.SavedPasswordKey, passwordEntry);
        }

        // Totally wipe user information from the system. Called from ONGUI when button is pressed
        private void RemoveUserInfo()
        {
            usernameEntry = "";
            passwordEntry = "";

            // Clear editor prefs 
            EditorPrefs.SetString(GlobalVDKContext.SavedUsernameKey, "");
            EditorPrefs.SetString(GlobalVDKContext.SavedPasswordKey, "");

            // Player prefs isn't saved, but on the off chance it was during development make sure to clear it
            PlayerPrefs.SetString(GlobalVDKContext.SavedUsernameKey, "");
            PlayerPrefs.SetString(GlobalVDKContext.SavedPasswordKey, "");
            
        }
    }
#endif
}
