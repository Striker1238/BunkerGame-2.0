using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BunkerGame.Lobby
{
    using BunkerGame.Hero;
    using BunkerGame.Player;
    public class Lobby
    {
        public string Index;
        public struct InfoAboutPlayer
        {
            public Hero hero;
            public Player player;
        }
        public List<InfoAboutPlayer> AllHero = new List<InfoAboutPlayer>(); 
    }
}
