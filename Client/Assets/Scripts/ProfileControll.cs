using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BunkerGame.Client;
using System;
using UnityEngine.XR;
using System.IO;
using Unity.VisualScripting;

public class ProfileControll : MonoBehaviour
{
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
    public void ChandeAvatar()
    {
        //Здесь ссылку файла изменить на то, что будет выбирать пользователь
        FindObjectOfType<Client_INSPECTOR>().ChangeAvatarProfile($"{Application.dataPath}/StreamingAssets/TETSIMG2.png");
    }
    
}
