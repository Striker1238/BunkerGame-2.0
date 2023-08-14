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
    

    /// <summary>
    /// Игрок - игровой объект, который управляет основными скриптами
    /// </summary>
     public Player ThisPlayer;

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
    public void ChangeReadiness(string indexLobby, bool state) => ThisClient.CreateMessageForServer(30, indexLobby, ThisPlayer.client_id, state?"1":"0");
    public void ShowCharacteristic(string indexLobby, string characteristicIndex) => ThisClient.CreateMessageForServer(100, indexLobby, ThisPlayer.client_id, characteristicIndex);
    public void ClientDisconnect() => ThisClient.CreateMessageForServer<string>(0);
    #endregion


    #region GET
    public void isCorrectData(string[] data)
    {
        //if (ThisPlayer != null) return; <- эта хрень не работает =\
        ThisPlayer = new Player();
        ThisPlayer.UserInfo = JsonUtility.FromJson<User>(data[0]);
        ThisPlayer.isConnectToServer = true;


        ThisPlayer.client_id = ThisClient.client_id;

        scenesControll.ChangeScene("ListLobbyScene");
    }
    public void isChangeAvatar(string[] data)
    {
        ThisPlayer.UserInfo.AvatarBase64 = data[0];

        if (FindObjectsOfType<ProfileControll>().Length > 0)
            FindObjectOfType<ProfileControll>().LoadingProfileData(ThisPlayer.UserInfo);
    }
    public void isCreatedNewLobby(string[] data)
    {
        FindObjectOfType<LobbyController>().IndexSelectLobby = JsonUtility.FromJson<Lobby>(data[0]).Index;

        ConnectToLobby(FindObjectOfType<LobbyController>().IndexSelectLobby, FindObjectOfType<LobbyController>().PasswordLobby_IF.text);
    }
    public void AddLobbyInList(string[] data) => FindObjectOfType<LobbyController>().AddLobbyInList(data[0]);
    /// <summary>
    /// Вызывается если к лобби подключился новый игрок
    /// </summary>
    /// <param name="data"></param>
    public void ThisPlayerConnectToLobby(string[] data)
    {
        ThisPlayer.ActiveLobby = JsonUtility.FromJson<Lobby>(data[0]);
        //Меняем сцену
        FindObjectOfType<ScenesControll>().ChangeScene(3);
    }
    public void ConnectNewPlayerToLobby(string[] data)
    {
        var newUser = JsonUtility.FromJson<User>(data[0]);
        FindObjectOfType<GameController>().OnNewConnectToLobby(newUser);
    }

    public void StartGame(string[] data)
    {
        ThisPlayer.ActiveLobby.AllHero.Find(x => x.user.UserName == ThisPlayer.UserInfo.UserName).hero = JsonUtility.FromJson<Hero>(data[0]);
        ThisPlayer.ActiveLobby.WorldEvent = data[1];
        ThisPlayer.ActiveLobby.NewBunker = JsonUtility.FromJson<BunkerInfo>(data[2]);
        FindObjectOfType<GameController>().StartGame();
    }
    public void AnotherPlayerShowCharacteristic(string[] data) => FindObjectOfType<GameController>().AnotherPlayerShowCharacteristic(data[0], data[1], data[2]);
    public void NewPlayerTurn(string[] data) => FindObjectOfType<GameController>().NewPlayerTurn(data[0]);
    #endregion
}
