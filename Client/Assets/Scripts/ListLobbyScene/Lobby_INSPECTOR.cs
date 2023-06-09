using BunkerGame.ClassLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby_INSPECTOR : MonoBehaviour
{
    public List<Lobby> AllLobby = new List<Lobby>();

    /// <summary>
    /// ������ ������ � ���� �����
    /// </summary>
    public void GetRequestListLobby()
    {
        FindObjectOfType<Client_INSPECTOR>().GetListLobby();
    }

    /// <summary>
    /// ��������� ������ � �������
    /// </summary>
    /// <param name="data"> ������ ���������� ��������</param>
    public void AddLobbyInList(string data)
    {
        Lobby? lobby = JsonUtility.FromJson<Lobby>(data);
        if (lobby == null) return;

        AllLobby.Add(lobby);
    }


    public void CreateLobby()
    {
        var test = new Lobby() { IndexLobby = "Test" + Random.Range(100000, 999999), IsEnd = false };
        FindObjectOfType<Client_INSPECTOR>().CreateLobby(test);
    }
    public void ConnectToLobby()
    {

    }
}