using MongoDB.Bson;
using MongoDB.Driver;

public class Database
{
    internal protected MongoClient client;
    internal IMongoDatabase database;
    internal string dbName;
    public Database(string _dbName, string _connectString = "mongodb://localhost:27017")
    {

        client = new MongoClient(_connectString);
        database = client.GetDatabase(_dbName);
        dbName = _dbName;
    }
    public async Task AddNewDocumentAsync<T>(string _collectionName, T _collectionData)
    {
        var collection = database.GetCollection<T>(_collectionName);
        await collection.InsertOneAsync(_collectionData);
    }
    public async Task<List<T>> GetListDocumentsAsync<T>(string _collectionName)
    {
        var collection = database.GetCollection<T>(_collectionName);
        using var cursor = await collection.FindAsync(new BsonDocument());
        List<T> elements = cursor.ToList();
        return elements;
    }
    public async Task<List<T>> GetListDocumentsAsync<T>(string _collectionName, BsonDocument filter)
    {
        var collection = database.GetCollection<T>(_collectionName);
        using var cursor = await collection.FindAsync(filter);
        List<T> elements = cursor.ToList();
        return elements;
    }
    public async Task<bool> IsDocumentExistsAsync<T>(string _collectionName, BsonDocument filter)
    {
        var collection = database.GetCollection<T>(_collectionName);
        using var cursor = await collection.FindAsync(filter);
        return cursor.ToList().Count > 0;
    }
    public async Task<long> GetCountDocumentAsync(string _collectionName) => await database.GetCollection<BsonDocument>(_collectionName).CountDocumentsAsync(new BsonDocument());
    public async Task<DeleteResult> DeleteDocumentAsync<T>(string _collectionName, BsonDocument filter)
    {
        var collection = database.GetCollection<T>(_collectionName);
        return await collection.DeleteOneAsync(filter);
    }
    public async Task<bool> UpdateDocumentAsync<T>(string _collectionName, BsonDocument filter, BsonDocument update)
    {
        var collection = database.GetCollection<T>(_collectionName);
        var result = await collection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }
    /*
    public async Task<string> UploadFile<T>(string _collectionName, string path)
    {
        var collection = database.GetCollection<T>(_collectionName);
        var picture = File.ReadAllBytes(path);

        //var fB = File.ReadAllBytes(@"C:\rab\kot.jpg");
        string encodedFile = Convert.ToBase64String(picture);

    }
    */
}
