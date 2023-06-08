namespace BunkerGame.ClassUser
{
    /// <summary>
    /// Объект сохранения в базу данных(вид данных польлзователя)
    /// </summary>
    [System.Serializable]
    public class User
    {
        public string? UserName;
        public string? Login;
        public string? Password;
        public string? AvatarBase64;
    }
}