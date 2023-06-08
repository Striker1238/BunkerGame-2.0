using BunkerGame.ClassLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lobby_INSPECTOR : MonoBehaviour
{
    public List<Lobby> AllLobby = new List<Lobby>();

    /// <summary>
    /// Запрос данных о всех лобби
    /// </summary>
    public void GetRequestListLobby()
    {
        FindObjectOfType<Client_INSPECTOR>().GetListLobby();
    }

    /// <summary>
    /// Получение данных с сервера
    /// </summary>
    /// <param name="data"> Данные пресланные сервером</param>
    public void AddLobbyInList(string data)
    {
        Lobby? lobby = JsonUtility.FromJson<Lobby>(data);
        if (lobby == null) return;

        AllLobby.Add(lobby);
    }

}
