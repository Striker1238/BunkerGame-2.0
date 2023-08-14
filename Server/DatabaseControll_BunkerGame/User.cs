using MongoDB.Bson;
public class User
{
    public ObjectId Id { get; set; }
    public string? UserName { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? AvatarBase64 { get; set; }
}