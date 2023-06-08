using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
namespace BunkerGame
{
    using BunkerGame.ClassClient;
    using BunkerGame.ClassPlayer;
    using BunkerGame.ClassLobby;
    using BunkerGame.ClassHero;

    internal class Lobby_Server
    {
        public ObjectId Id { get; set; }
        public string Index { get; set; }
        public struct InfoAboutPlayer
        {
            public Hero hero;
            public Player player;
        }
        public List<InfoAboutPlayer> AllHero = new List<InfoAboutPlayer>();
    }
}
