using BunkerGame.ClassHero;
using BunkerGame.ClassLobby;
using BunkerGame.ClassPlayer;
using BunkerGame.ClassUser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [System.Serializable]
    public struct PlayerPosition
    {
        public byte CountPlayers;
        public Vector3[] AllPosForPlayer;
    }

    public PlayerPosition[] AllPosition;
    static CreatingNewPlayerObject creatingPlayer;
    public Lobby thisLobby;
    
    void Start()
    {
        thisLobby = FindObjectOfType<Client_INSPECTOR>().ThisPlayer.ActiveLobby;
        creatingPlayer = GetComponent<CreatingNewPlayerObject>();
        for (int i = 0; i < thisLobby.AllHero.Count; i++)
        {
            var player = thisLobby.AllHero[i];
            creatingPlayer.CreatePlayerObject(player.user, thisLobby.Settings.MaxPlayers, (byte)i);
        }
        
    }
    /// <summary>
    /// При подключении нового пользователя
    /// </summary>
    /// <param name="connectUser">данные нового пользователя</param>
    public void OnNewConnectToLobby(User connectUser)
    {
        InfoAboutPlayer newPlayerInLobby = new InfoAboutPlayer()
        {
            user = connectUser,
            hero = new Hero()
        };
        thisLobby.AllHero.Add(newPlayerInLobby);
        creatingPlayer.CreatePlayerObject(connectUser, thisLobby.Settings.MaxPlayers, (byte)(thisLobby.AllHero.Count-1));
    }
}
