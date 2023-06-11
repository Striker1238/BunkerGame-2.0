using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BunkerGame.ClassClient;
using System;
using UnityEngine.XR;
using System.IO;
using Unity.VisualScripting;
using BunkerGame.ClassUser;
using SFB;

public class ProfileControll : MonoBehaviour
{
    [Header("Path on file")]
    public string AvatarPath;

    [Header("Other")]
    public Image Avatar;
    public TMP_InputField Username_IF;
    public TextMeshProUGUI CountMatches;
    public TextMeshProUGUI WinMatches;

    public void Start()
    {
        LoadingProfileData(FindObjectOfType<Client_INSPECTOR>().ThisPlayer.UserInfo);
    }

    public void LoadingProfileData(User _User)
    {
        byte[] data = Convert.FromBase64String(_User.AvatarBase64);

        var tex = new Texture2D(1, 1);
        tex.LoadImage(data);
        tex.Apply();
        
        Avatar.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2());

        Username_IF.text = _User.UserName;

        CountMatches.text = "0";
        WinMatches.text = "0";
    }

    /// <summary>
    /// Вызывается с помощью кнопки
    /// </summary>
    public void ChangeAvatar()
    {
        var thisUser = FindObjectOfType<Client_INSPECTOR>().ThisPlayer.UserInfo;

        //Меняем аватар
        thisUser.AvatarBase64 = Convert.ToBase64String(File.ReadAllBytes(GetPathOnFile()));

        FindObjectOfType<Client_INSPECTOR>().ChangeAvatarProfile(thisUser);
    }


    /// <summary>
    /// Показывает диалоговое окно для выбора файла
    /// </summary>
    public string GetPathOnFile()
    {
        //какие файлы вообще можно открыть
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            new ExtensionFilter("All Files", "*" ),
        };
        return StandaloneFileBrowser.OpenFilePanel("Select your avatar", "", extensions, false)[0];
    }

}
