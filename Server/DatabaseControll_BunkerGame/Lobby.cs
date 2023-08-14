using MongoDB.Bson;
public class Lobby
{
    [System.Serializable]
    public class BunkerInfo
    {
        public string Contry { get; set; }
        public string[] Items { get; set; }
        public string[] Equipment { get; set; }
        public string InBunkerLive { get; set; }
    }
    [System.Serializable]
    public class SettingsLobby
    {
        public string Name { get; set; }
        public byte MaxPlayers { get; set; }
        public bool isPrivate { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Индекс лобби в базе данных
    /// </summary>
    public ObjectId Id { get; set; }
    /// <summary>
    /// Индекс лобби
    /// </summary>
    public string? Index { get; set; }
    /// <summary>
    /// Все игроки, их данные + характеристики персонажей
    /// </summary>
    public List<Player> AllHero { get; set; }
    /// <summary>
    /// Настройки лобби
    /// </summary>
    public SettingsLobby Settings { get; set; }
    /// <summary>
    /// Описание события в мире
    /// </summary>
    public string WorldEvent { get; set; }
    /// <summary>
    /// Характеристики бункера
    /// </summary>
    public BunkerInfo NewBunker { get; set; }

    /// <summary>
    /// Статус старта лобби
    /// </summary>
    public bool IsStart { get; set; }
    /// <summary>
    /// Статус завершения лобби
    /// </summary>
    public bool IsEnd { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }




    public Lobby() { }
    public Lobby(Lobby? oldLobby)
    {
        this.Id = oldLobby.Id;
        this.Index = oldLobby.Index;
        this.AllHero = oldLobby.AllHero;
        this.Settings = oldLobby.Settings;
        this.IsStart = oldLobby.IsStart;
        this.IsEnd = oldLobby.IsEnd;
        this.StartTime = oldLobby.StartTime;
        this.EndTime = oldLobby.EndTime;
    }
    /// <summary>
    /// Генерирует строку
    /// </summary>
    /// <param name="length">длина сгенерированной строки</param>
    /// <returns>Сгенерированная строка</returns>
    public string GeneratingIndex(byte length = 16)
    {
        Random random = new Random();
        string RandomString = string.Empty;

        while (length-- > 0) RandomString += (char)random.Next(33, 123);

        return RandomString;
    }
}