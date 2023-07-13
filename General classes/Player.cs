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
        public User UserInfo;
        public Lobby ActiveLobby;
        public bool isReady;
    }
}

