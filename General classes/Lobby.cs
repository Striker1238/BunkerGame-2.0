using System.Collections;
using System.Collections.Generic;
using BunkerGame.ClassHero;
using BunkerGame.ClassPlayer;

namespace BunkerGame.ClassLobby
{
    
    public struct InfoAboutPlayer
    {
        public Hero hero;
        public Player player;
    }
    [System.Serializable]
    public class Lobby
    {
        public string? IndexLobby;
        //public List<InfoAboutPlayer> AllHero = new List<InfoAboutPlayer>();
        public bool IsEnd;
    }
}
