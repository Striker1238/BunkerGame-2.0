using BunkerGame.ClassHero;
using BunkerGame.ClassLobby;
using BunkerGame.ClassUser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    static CreatingNewPlayerObject creatingPlayer;
    public Lobby thisLobby;
    
    void Start()
    {
        thisLobby = FindObjectOfType<Client_INSPECTOR>().ThisPlayer.ActiveLobby;
        creatingPlayer = GetComponent<CreatingNewPlayerObject>();
        foreach (var player in thisLobby.AllHero)
        {
            creatingPlayer.CreatePlayerObject(player.user);
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
    }
}
