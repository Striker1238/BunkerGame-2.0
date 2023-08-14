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
        /// <summary>
        /// Информация о данных пользователя
        /// </summary>
        public User UserInfo;
        
        /// <summary>
        /// Id клиента на сервере
        /// </summary>
        public string client_id;
        /// <summary>
        /// Статус подключения клиента к серверу
        /// </summary>
        public bool isConnectToServer;


        /// <summary>
        /// Информация об активном лобби
        /// </summary>
        public Lobby ActiveLobby;
        /// <summary>
        /// Готовность игрока к игре в лобби
        /// </summary>
        public bool isReady;

    }
}

