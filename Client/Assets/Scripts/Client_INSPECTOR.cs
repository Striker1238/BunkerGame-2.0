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



/// <summary>
/// �� ������� ����� � ����� � ������
/// ������ ��������� ������ �� ������� � ������ ������� 
/// ���������� � ������� � ��������� � �������� ������ �� ������� � ������� ������� Client
/// </summary>
public class Client_INSPECTOR : MonoBehaviour
{
    [Header("Client stats")]
    public bool isConnectToServer;


    [Header("Player prefab")]
    //public GameObject PlayerPrefab;
    /// <summary>
    /// ����� - ������� ������, ������� ��������� ��������� ���������
    /// </summary>
    [HideInInspector] public Player ThisPlayer = null;
    //
    [Header("Scenes Controller")]
    public ScenesControll scenesControll;

    //[Header("Lobby Controller")]
    //public Lobby_INSPECTOR LobbyController;

    /// <summary>
    /// ������, ������� ������ � ��������
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
        //��� ��� �� ������ ������
    }


    #region POST
    public void CreateNewProfile(User newUser) => ThisClient.CreateMessageForServer(1,newUser);
    public void LoginInProfile(User user) => ThisClient.CreateMessageForServer(2, user);
    public void ChangeAvatarProfile(User newData) => ThisClient.CreateMessageForServer(3, newData);
    public void GetListLobby() => ThisClient.CreateMessageForServer<Lobby>(4);
    public void CreateLobby(Lobby newLobby) => ThisClient.CreateMessageForServer(5, newLobby);
    #endregion


    #region GET
    public void isCorrectData(string data) => CreateClientObj(data);
    public void isSuccessfullChangeAvatar(string data)
    {
        ThisPlayer.UserInfo.AvatarBase64 = data;

        if (FindObjectsOfType<ProfileControll>().Length > 0)
            FindObjectOfType<ProfileControll>().LoadingProfileData(ThisPlayer.UserInfo);
    }
    public void AddLobbyInList(string data) => FindObjectOfType<Lobby_INSPECTOR>().AddLobbyInList(data);
    public void isSuccessfullCreateLobby(string data) 
    {
        FindObjectOfType<Lobby_INSPECTOR>().AllLobby.Add(JsonUtility.FromJson<Lobby>(data));
        //����� ��� �� ��� �����
    }
    #endregion






    public async void CreateClientObj(string data)
    {
        //if (ThisPlayer != null) return; <- ��� ����� �� �������� =\
        ThisPlayer = new Player();
        ThisPlayer.UserInfo = JsonUtility.FromJson<User>(data);
        isConnectToServer = true;
        scenesControll.ChangeScene(2);
    }

    
}
