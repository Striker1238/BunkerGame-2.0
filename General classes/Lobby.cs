using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BunkerGame.ClassHero;
using BunkerGame.ClassUser;

namespace BunkerGame.ClassLobby
{
    [System.Serializable]
    public class InfoAboutPlayer
    {
        public Hero hero;
        public User user;
    }
    [System.Serializable]
    public class SettingsLobby
    {
        public string Name;
        public byte MaxPlayers;
        public bool isPrivate;
        public string Password;
    }
    [System.Serializable]
    public class Lobby
    {
        public string? Index;
        public List<InfoAboutPlayer> AllHero = new List<InfoAboutPlayer>();
        public SettingsLobby Settings;

        public string WorldEvent;
        public BunkerInfo NewBunker;


        public bool IsStart;
        public bool IsEnd;
        public string? StartTime;
        public string? EndTime;
    }
    [System.Serializable]
    public class BunkerInfo
    {
        public string Contry;
        public string[] Items;
        public string[] Equipment;
        public string InBunkerLive;
    }
}
