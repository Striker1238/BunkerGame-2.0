using BunkerGame.ClassLobby;
using BunkerGame.ClassUser;
using System.Collections;
using System.Collections.Generic;

namespace BunkerGame.ClassPlayer
{
    /// <summary>
    /// ������ ������ �������� ���������� � �������������� ������
    /// �������� ����� � ���������� � ���� �����
    /// </summary>
    [System.Serializable]
    public class Player
    {
        /// <summary>
        /// ���������� � ������ ������������
        /// </summary>
        public User UserInfo;
        
        /// <summary>
        /// Id ������� �� �������
        /// </summary>
        public string client_id;
        /// <summary>
        /// ������ ����������� ������� � �������
        /// </summary>
        public bool isConnectToServer;


        /// <summary>
        /// ���������� �� �������� �����
        /// </summary>
        public Lobby ActiveLobby;
        /// <summary>
        /// ���������� ������ � ���� � �����
        /// </summary>
        public bool isReady;

    }
}

