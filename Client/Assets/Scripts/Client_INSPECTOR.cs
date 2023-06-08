using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using System.IO;


using BunkerGame.ClassClient;
using BunkerGame.ClassPlayer;
using BunkerGame.ClassLobby;



/// <summary>
/// Не хранить данны о лобби и игроке
/// Только содержать ссылки на объекты с такими данными 
/// Обращаться к серверу с запросами и получать ответы от сервера с помощью скрипта Client
/// </summary>
public class Client_INSPECTOR : MonoBehaviour
{
    [Header("Client stats")]
    public bool isConnectToServer;

    [Header("Create Profile Objects")]
    public TMP_InputField username_IF;
    public TMP_InputField login_Reg_IF;
    public TMP_InputField password_Reg_IF;

    [Header("Login In Profile")]
    public TMP_InputField login_Log_IF;
    public TMP_InputField password_Log_IF;

    [Header("Player prefab")]
    //public GameObject PlayerPrefab;
    /// <summary>
    /// Игрок - игровой объект, который управляет основными скриптами
    /// </summary>
    [HideInInspector] public Player ThisPlayer = null;
    //
    [Header("Scenes Controller")]
    public ScenesControll scenesControll;

    //[Header("Lobby Controller")]
    //public Lobby_INSPECTOR LobbyController;

    /// <summary>
    /// Клиент, который связан с сервером
    /// </summary>
    static Client ThisClient;

    void Start()
    {
        ThisClient = new Client();
        scenesControll = GetComponent<ScenesControll>();
        ThisPlayer = null;

        ThisClient.Start(this);

        DontDestroyOnLoad(this);
    }
    public void FixedUpdate()
    {
        if(ThisClient.queueMethods.Count > 0)
        {
            var MethodAndData = ThisClient.queueMethods.Dequeue();
            RequestMethod requestMethod = MethodAndData.method;
            requestMethod(MethodAndData.data);
        }
        //Тут что то другое делаем
    }


    #region POST
    public void CreateNewProfile() => ThisClient.CreateProfile(new User { UserName = username_IF.text, Login = login_Reg_IF.text, Password = password_Reg_IF.text, AvatarBase64 = Convert.ToBase64String(File.ReadAllBytes($"{Application.dataPath}/StreamingAssets/TESTIMG.jpg"))});
    public void LoginInProfile() => ThisClient.LoginProfile(new User { Login = login_Log_IF.text, Password = password_Log_IF.text });
    public void ChangeAvatarProfile(string Path) => ThisClient.ChangeAvatarOnClient(ThisPlayer.UserInfo, Path);
    public void GetListLobby() => ThisClient.GetListLobby();
    #endregion


    #region GET
    public void isCorrectData(string data) => CreateClientObj(data);
    public void isSuccsessfullChangeAvatar(string data)
    {
        ThisPlayer.UserInfo.AvatarBase64 = data;

        if (FindObjectsOfType<ProfileControll>().Length > 0)
            FindObjectOfType<ProfileControll>().LoadingProfileData(ThisPlayer.UserInfo);
    }
    public void AddLobbyInList(string data) => FindObjectOfType<Lobby_INSPECTOR>().AddLobbyInList(data);
    #endregion






    public async void CreateClientObj(string data)
    {
        //if (ThisPlayer != null) return; <- эта хрень не работает =\
        ThisPlayer = new Player();
        ThisPlayer.UserInfo = JsonUtility.FromJson<User>(data);
        isConnectToServer = true;
        scenesControll.ChangeScene(2);
    }
}
