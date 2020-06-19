using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Vault;

public class LoginSDK : MonoBehaviour
{
    public GameObject username;
    public GameObject password;
    private string Username;
    private string Password;
    public void Login()
    {
        GlobalVDKContext.SavedUsernameKey = username.GetComponent<InputField>().text;
        GlobalVDKContext.SavedPasswordKey = password.GetComponent<InputField>().text;
        GlobalVDKContext.Login();
    }

    public void LoadByIndex(int sceneIndex)
    {
        Login();
        SceneManager.LoadScene(sceneIndex);
    }
}
