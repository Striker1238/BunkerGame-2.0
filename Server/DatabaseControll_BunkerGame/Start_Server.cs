using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Transactions;
using System.Text.Json;
using MongoDB.Driver.Builders;
using BunkerGame.ClassClient;
using BunkerGame.ClassPlayer;
using BunkerGame.ClassLobby;
using static System.Net.WebRequestMethods;


namespace BunkerGame.Database
{
    class StartConnection
    {
        static Database? DatabaseControll;
        static void Main(string[] args)
        {
            StartConnection CreateServer = new StartConnection();
            Console.WriteLine("Start server...");


            CreateServer.StartServerForDatabaseControll("127.0.0.1",8888);


            DatabaseControll = new Database("BunkerGame");


            Console.WriteLine("Connect to " + "BunkerGame" + " database...");
            
            if(DatabaseControll.database == null)
            {
                Console.WriteLine("Error connecting to database!");
                return;
            }
            else 
            {
                Console.WriteLine("Connection successfull!");
            }
            Console.ReadLine();
        }

        public async void StartServerForDatabaseControll(string ipAddress, int port)
        {
            IPAddress listeningIpAddres = IPAddress.Parse(ipAddress);
            IPEndPoint ipPoint = new IPEndPoint(listeningIpAddres, port);

            TcpListener server = new TcpListener(ipPoint);
            server.Start(1000); //1000 - максимальная очередь

            while (true)
            {
                var tcpClient = await server.AcceptTcpClientAsync();

                Console.WriteLine($"Client: {tcpClient.Client.RemoteEndPoint} successfully connected");

                new Thread(async () => await ProcessClientAsync(tcpClient)).Start();
            }

        }
        async Task ProcessClientAsync(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            using var streamReader = new StreamReader(stream);
            using var streamWriter = new StreamWriter(stream);



            while (true)
            {
                try
                {
                    var CommandForServerDatabase = await streamReader.ReadLineAsync();
                    if (CommandForServerDatabase == "DISCONNECT") break;


                    string? message;
                    switch (CommandForServerDatabase)
                    {
                        //Создание нового пользователя
                        case "CREATE_PROFILE":
                            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} - Send a request to create a profile");
                            message = await streamReader.ReadLineAsync();

                            var isSuccessfull_Creating = await CreateNewProfile(message);

                            if(isSuccessfull_Creating)
                                await streamWriter.WriteLineAsync("1");
                            else
                                await streamWriter.WriteLineAsync("2");
                            break;

                        //Вход в аккаунт
                        case "LOGIN_PROFILE":
                            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} - Send a request to login in profile");
                            message = await streamReader.ReadLineAsync();

                            var isSuccessfull_Login = await LoginInProfile(message);

                            if (isSuccessfull_Login != null)
                            {
                                await streamWriter.WriteLineAsync("3");
                                await streamWriter.WriteLineAsync(JsonSerializer.Serialize(isSuccessfull_Login));
                            }
                            else
                                await streamWriter.WriteLineAsync("4");
                            break;

                        //Смена аватара
                        case "CHANGE_DATA_PROFILE":
                            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} - Send a request to change data profile");
                            message = await streamReader.ReadLineAsync();

                            var isSuccessfull_ChangeData = await ChangeDataInProfile(message);

                            if (isSuccessfull_ChangeData)
                                await streamWriter.WriteLineAsync("5");
                            else
                                await streamWriter.WriteLineAsync("6");
                            break;
                        case "GET_LIST_ACTIVE_LOBBY":
                            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} - Send a request list with all active lobby");
                            message = await streamReader.ReadLineAsync();

                            var allActiveLobby = await GetListActiveLobby();

                            if (allActiveLobby.Count > 0)
                            {
                                await streamWriter.WriteLineAsync("7");
                                await streamWriter.WriteLineAsync(allActiveLobby.Count.ToString());
                                foreach (var lobby in allActiveLobby)
                                {
                                    await streamWriter.WriteLineAsync(JsonSerializer.Serialize(lobby));
                                }
                            }
                            else
                                await streamWriter.WriteLineAsync("8");

                            break;


                        default:
                            message = await streamReader.ReadLineAsync();
                            Console.WriteLine($"{tcpClient.Client.RemoteEndPoint} - Send a unknow request");

                            await streamWriter.WriteLineAsync("-1");
                            break;
                    }
                    await streamWriter.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }

            Console.WriteLine($"Клиент {tcpClient.Client.RemoteEndPoint} отключен");
            tcpClient.Close();
        }

        private async Task<bool> CreateNewProfile(string message)
        {
            User? newUser = JsonSerializer.Deserialize<User>(message);

            var filter = new BsonDocument { { "Login", newUser.Login } };

            if (message != null && !DatabaseControll.IsDocumentExistsAsync<User>("Users", filter).Result)
            {
                await DatabaseControll.AddNewDocumentAsync("Users", newUser);
                return true;
            }
            else return false;
        }
        private async Task<User?> LoginInProfile(string message)
        {
            User? newUser = JsonSerializer.Deserialize<User>(message);

            var filter = new BsonDocument { { "Login", newUser.Login } };

            if (message != null && DatabaseControll.IsDocumentExistsAsync<User>("Users", filter).Result)
            {
                var user = await DatabaseControll.GetListDocumentsAsync<User>("Users", filter);

                if (user[0].Password == newUser.Password)
                    return user[0];
                else return null;
            }
            else return null;
        }
        private async Task<bool> ChangeDataInProfile(string message)
        {
            User? newUser = JsonSerializer.Deserialize<User>(message);

            var filter = new BsonDocument { { "Login", newUser.Login } };

            var updateSettings = new BsonDocument ("$set", new BsonDocument{
                    { "UserName", newUser.UserName },
                    { "Password", newUser.Password },
                    { "AvatarBase64", newUser.AvatarBase64 },
                });

            if (message != null && DatabaseControll.UpdateDocumentAsync<User>("Users", filter, updateSettings).Result)
                return true;
            else return false;
        }
        private async Task<List<Lobby>> GetListActiveLobby()
        {
            var filter = new BsonDocument { { "IsEnd", false } };

            if (DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result)
                return await DatabaseControll.GetListDocumentsAsync<Lobby>("Lobby", filter);
            else return new List<Lobby>();

            
        }
        private async Task<bool> CreateNewLobby(string message)
        {
            Lobby? newLobby = JsonSerializer.Deserialize<Lobby>(message);

            var filter = new BsonDocument { { "Index", newLobby.Index } };

            if (newLobby != null && DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result)
            {
                await DatabaseControll.AddNewDocumentAsync<Lobby>("Lobby", newLobby);
                return true;
            }
            else return false;
        }
    }
    

    public class Database
    {

        internal protected MongoClient client;
        internal IMongoDatabase database;
        internal string dbName;
        internal string usersCollection = "Users";

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

            return cursor.ToList().Count != 0;
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
}
///[BsonIgnoreExtraElements] для фикса ошибки, если поля не принципиальны
public class User
{
    public ObjectId Id { get; set; }
    public string? UserName { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? AvatarBase64 { get; set; }
}