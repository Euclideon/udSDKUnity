using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using udSDK;

public class LoginUDSDK : MonoBehaviour
{
    public GameObject username;
    public GameObject password;
    private string Username;
    private string Password;
    public void Login()
    {
        GlobalUDContext.SavedUsernameKey = username.GetComponent<InputField>().text;
        GlobalUDContext.SavedPasswordKey = password.GetComponent<InputField>().text;
        GlobalUDContext.Login();
    }

    public void LoadByIndex(int sceneIndex)
    {
        Login();
        SceneManager.LoadScene(sceneIndex);
    }
}
