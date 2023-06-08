using System.Collections;
using System.Collections.Generic;
using BunkerGame.ClassHero;
using BunkerGame.ClassPlayer;

namespace BunkerGame.ClassLobby
{
    [System.Serializable]
    public class Lobby
    {
        public string Index;
        public struct InfoAboutPlayer
        {
            public Hero hero;
            public Player player;
        }
        public List<InfoAboutPlayer> AllHero = new List<InfoAboutPlayer>();
        public bool IsEnd;
    }
}
