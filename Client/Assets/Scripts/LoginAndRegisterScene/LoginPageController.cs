using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SFB;
public class LoginPageController : MonoBehaviour
{
    [Header("Page object")]
    public GameObject LoginPage;
    public GameObject RegistrationPage;

    [Header("Create Profile Objects")]
    public TMP_InputField username_IF;
    public TMP_InputField login_Reg_IF;
    public TMP_InputField password_Reg_IF;

    [Header("Login In Profile Objects")]
    public TMP_InputField login_Log_IF;
    public TMP_InputField password_Log_IF;

    [Header("Common Objects")]
    public Button ChangePage_Button;
    public GameObject NotificationPref;

    [Header("Path on file")]
    public string AvatarPath;

    private bool isActiveLoginPage = true;

    public void ChangePage()
    {
        RegistrationPage.SetActive(LoginPage.activeSelf);
        LoginPage.SetActive(!RegistrationPage.activeSelf);

        isActiveLoginPage = LoginPage.activeSelf;

        ChangePage_Button.GetComponentInChildren<TextMeshProUGUI>().text = (isActiveLoginPage) ? "Sing Up" : "Log In";
    }

    public void ChangeVisiblePassword()
    {
        password_Reg_IF.contentType = (password_Reg_IF.contentType == TMP_InputField.ContentType.Password) ? TMP_InputField.ContentType.Standard :TMP_InputField.ContentType.Password;
        password_Log_IF.contentType = (password_Reg_IF.contentType == TMP_InputField.ContentType.Password) ? TMP_InputField.ContentType.Standard :TMP_InputField.ContentType.Password;
        password_Reg_IF.ActivateInputField();
        password_Log_IF.ActivateInputField();
    }

    public void NewNotificationOnLoginPage(string? message)
    {
        var notification = Instantiate(NotificationPref);
        notification.transform.localPosition = new Vector3(0,0,0);
        notification.GetComponent<TextMeshProUGUI>().text = $"Server: {message}";

        notification.GetComponent<Notification>().NotificationFromServer(new Vector3(0,0,0));
    }

    /// <summary>
    /// Показывает диалоговое окно для выбора файла
    /// </summary>
    public void ShowFileDialog()
    {
        //какие файлы вообще можно открыть
        var extensions = new[] {  
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            new ExtensionFilter("All Files", "*" ),
        };
        AvatarPath = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false)[0];
        Debug.Log("Select: " + AvatarPath);
    }
}
