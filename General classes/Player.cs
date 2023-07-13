using BunkerGame.ClassLobby;
using BunkerGame.ClassUser;
using System.Collections;
using System.Collections.Generic;

namespace BunkerGame.ClassPlayer
{
    /// <summary>
    /// Объект игрока содержит информацию о залогиневшемся игроке
    /// Активном лобби и готовности в этом лобби
    /// </summary>
    [System.Serializable]
    public class Player
    {
        public User UserInfo;
        public Lobby ActiveLobby;
        public bool isReady;
    }
}

