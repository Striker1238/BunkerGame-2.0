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
using BunkerGame.ClassUser;
using BunkerGame.ClassLobby;
using BunkerGame.ClassHero;

/// <summary>
/// Объект клиента хранит информацию о подключении информации,
/// объект игрока,
/// объект контроллера сцен
/// объект Client
/// Этот скрипт является мостом между объектом Client и другими скриптами проекта
/// </summary>
public class Client_To_Scripts_Bridge : MonoBehaviour
{
    [Header("Client stats")]
    public bool isConnectToServer;

    /// <summary>
    /// Игрок - игровой объект, который управляет основными скриптами
    /// </summary>
    [HideInInspector] public Player ThisPlayer;

    [Header("Scenes Controller")]
    public ScenesControll scenesControll;

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
    public void CreateNewProfile(User newUser) => ThisClient.CreateMessageForServer(10,newUser);
    public void LoginInProfile(User user) => ThisClient.CreateMessageForServer(11, user);
    public void ChangeAvatarProfile(User newData) => ThisClient.CreateMessageForServer(12, newData);
    public void GetListLobby() => ThisClient.CreateMessageForServer<Lobby>(20);
    public void CreateLobby(Lobby newLobby) => ThisClient.CreateMessageForServer(21, newLobby);
    public void ConnectToLobby(string indexLobby, string passwordLobby) => ThisClient.CreateMessageForServer(22, indexLobby, ThisPlayer.UserInfo.Login, passwordLobby);
    public void ChangeReadiness(string indexLobby, bool state) => ThisClient.CreateMessageForServer(30, indexLobby, ThisPlayer.UserInfo.Login, state?"1":"0");


    public void ClientDisconnect() => ThisClient.CreateMessageForServer<string>(0);
    #endregion


    #region GET
    public void isCorrectData(string data) => CreateClientObj(data);
    public void isChangeAvatar(string data)
    {
        ThisPlayer.UserInfo.AvatarBase64 = data;

        if (FindObjectsOfType<ProfileControll>().Length > 0)
            FindObjectOfType<ProfileControll>().LoadingProfileData(ThisPlayer.UserInfo);
    }
    public void isCreatedNewLobby(string data)
    {
        FindObjectOfType<Lobby_INSPECTOR>().IndexSelectLobby = JsonUtility.FromJson<Lobby>(data).Index;

        ConnectToLobby(FindObjectOfType<Lobby_INSPECTOR>().IndexSelectLobby, FindObjectOfType<Lobby_INSPECTOR>().PasswordLobby_IF.text);
    }
    public void AddLobbyInList(string data) => FindObjectOfType<Lobby_INSPECTOR>().AddLobbyInList(data);
    /// <summary>
    /// Вызывается если к лобби подключился новый игрок
    /// </summary>
    /// <param name="data"></param>
    public void ThisPlayerConnectToLobby(string data)
    {
        ThisPlayer.ActiveLobby = JsonUtility.FromJson<Lobby>(data);
        //Меняем сцену
        FindObjectOfType<ScenesControll>().ChangeScene(3);
    }
    public void ConnectNewPlayerToLobby(string data)
    {
        var newUser = JsonUtility.FromJson<User>(data);
        Debug.Log("Aboba2");
        FindObjectOfType<GameController>().OnNewConnectToLobby(newUser);
    }


    public void StartGame(string data)
    {
        ThisPlayer.ActiveLobby.AllHero.Find(x => x.user.UserName == ThisPlayer.UserInfo.UserName).hero = JsonUtility.FromJson<Hero>(data);
        Debug.Log("START GAME");
    }
    #endregion






    public async void CreateClientObj(string data)
    {
        //if (ThisPlayer != null) return; <- эта хрень не работает =\
        ThisPlayer = new Player();
        ThisPlayer.UserInfo = JsonUtility.FromJson<User>(data);
        isConnectToServer = true;
        scenesControll.ChangeScene("ListLobbyScene");
    }
}
