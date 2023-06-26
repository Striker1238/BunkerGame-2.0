﻿using System;
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
using BunkerGame.ClassHero;
using BunkerGame.ClassPlayer;
using MongoDB.Bson.Serialization.Attributes;
using BunkerGame.ClassLobby;
using SharpCompress.Writers;
using BunkerGame.Database;
using static System.Net.WebRequestMethods;
//using BunkerGame.ClassLobby;

namespace BunkerGame.Database
{
    public class ServerObject
    {
        TcpListener server;

        public List<Lobby> AllActiveLobby = new List<Lobby>();
        private List<ClientObject> AllActiveClients = new List<ClientObject>();

        static Database? DatabaseControll;
        static void Main(string[] args)
        {
            ServerObject CreateServer = new ServerObject();
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

            server = new TcpListener(ipPoint);
            server.Start(1000); //1000 - максимальная очередь

            while (true)
            {
                var tcpClient = await server.AcceptTcpClientAsync();

                Console.WriteLine($"Client: {tcpClient.Client.RemoteEndPoint} successfully connected");

                ClientObject clientObject = new ClientObject(tcpClient, this);
                AllActiveClients.Add(clientObject);
                new Thread(async () => await clientObject.ProcessClientAsync()).Start();

            }

        }

        /// <summary>
        /// Удаление подключения
        /// </summary>
        /// <param name="id">Id подключения</param>
        protected internal void RemoveConnection(string id)
        {
            ClientObject? client = AllActiveClients.FirstOrDefault(x => x.id == id);
            if (client != null) AllActiveClients.Remove(client);
            client?.Close();
        }
        /// <summary>
        /// Отключение всех подключений и выключение сервера
        /// </summary>
        protected internal void Disconnect()
        {
            foreach (var client in AllActiveClients)
                client.Close();

            server.Stop();
        }
        protected internal async Task<bool> CreateNewProfile(string message)
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
        protected internal async Task<User?> LoginInProfile(string message)
        {
            User? newUser = JsonSerializer.Deserialize<User>(message);

            var filter = new BsonDocument { { "Login", newUser.Login } };

            if (message != null & DatabaseControll.IsDocumentExistsAsync<User>("Users", filter).Result)
            {
                var user = await DatabaseControll.GetListDocumentsAsync<User>("Users", filter);

                if (user[0].Password == newUser.Password)
                    return user[0];
                else return null;
            }
            else return null;
        }
        protected internal async Task<User> GetDataAboutUser(string userLogin)
        {
            var filter = new BsonDocument { { "Login", userLogin } };

            if (userLogin != null & DatabaseControll.IsDocumentExistsAsync<User>("Users", filter).Result)
            {
                var user = await DatabaseControll.GetListDocumentsAsync<User>("Users", filter);
                user[0].Password = "";
                return user[0];
            }
            else return null;
        }
        protected internal async Task<bool> ChangeDataInProfile(string message)
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






        protected internal async Task<List<Lobby>> GetListActiveLobby()
        {
            var listLobby = AllActiveLobby.FindAll(x => x.IsEnd == false && x.IsStart == false);
            return listLobby;
            //if (listLobby.Count > 0) 
            //    return listLobby;
            //else return null;
            /*
            var filter = new BsonDocument("$and",
                new BsonArray{
                    new BsonDocument("IsEnd", false),
                    new BsonDocument("IsStart", false)
                });
            if (DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result)
                return await DatabaseControll.GetListDocumentsAsync<Lobby>("Lobby");
            else return new List<Lobby>();
            */
        }
        protected internal async Task<Lobby> GetDataAboutLobby(string indexLobby)
        {
            var lobby = AllActiveLobby.Find(x => x.Index == indexLobby);
            return lobby;
            //if (lobby is not null)
            //    return lobby;
            //else return null;

            /*
            var filter = new BsonDocument("Index", indexLobby);

            if (DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result)
            {
                var lobby = await DatabaseControll.GetListDocumentsAsync<Lobby>("Lobby", filter);
                return lobby[0];
            }
            else return null;
            */
        }
        protected internal async Task<Lobby> CreateNewLobby(string message)
        {
            try
            {
                Lobby? newLobby = JsonSerializer.Deserialize<Lobby>(message);
                BsonDocument? filter;

                do
                {
                    newLobby.Index = newLobby.GeneratingIndex();
                    filter = new BsonDocument { { "Index", newLobby.Index } };
                } while (AllActiveLobby.Find(x => x.Index == newLobby.Index) is not null || DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result);

                AllActiveLobby.Add(newLobby);
                return newLobby;
            }
            catch (Exception)
            {

                return null;
            }
/*
            var filter = new BsonDocument { { "Index", newLobby.Index } };
            if (newLobby != null && DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result)
            {
                DatabaseControll.AddNewDocumentAsync<Lobby>("Lobby", newLobby);
                
                AllActiveLobby.Add(newLobby);

                return newLobby;
            }
            return null;
*/
        }
        protected internal async Task<bool> SaveLobbyInDatabase(string indexLobby)
        {
            Lobby? findLobby = AllActiveLobby.Find(x => x.Index == indexLobby);
            if (findLobby == null) return false;


            var filter = new BsonDocument { { "Index", indexLobby } };

            if (DatabaseControll.IsDocumentExistsAsync<Lobby>("Lobby", filter).Result)
            {
                var updateSettings = new BsonDocument("$set", new BsonDocument{
                    { "AllHero", JsonSerializer.Serialize(findLobby.AllHero) },
                    { "Settings", JsonSerializer.Serialize(findLobby.Settings) },
                    { "IsStart", findLobby.IsStart },
                    { "IsEnd", findLobby.IsEnd },
                    { "StartTime", findLobby.StartTime },
                    { "EndTime", findLobby.EndTime },
                });
                await DatabaseControll.UpdateDocumentAsync<Lobby>("Lobby", filter, updateSettings);
            }
            else DatabaseControll.AddNewDocumentAsync<Lobby>("Lobby", findLobby);

            return true;
        }
        protected internal async Task<bool> FirstConnectLobby(string indexLobby, string userLogin)
        {
            var filterLobby = new BsonDocument { { "Index", indexLobby } };
            var filterUser = new BsonDocument { { "Login", userLogin } };
            if (indexLobby != null && userLogin != null 
                && DatabaseControll.IsDocumentExistsAsync<User>("Users", filterUser).Result)
            {
                var newUser = GetDataAboutUser(userLogin).Result;
                if (newUser is null) return false;

                InfoAboutPlayer? newPlayerInLobby = new InfoAboutPlayer()
                {
                    user = newUser,
                    hero = new Hero()
                };

                Lobby? findLobby = AllActiveLobby.Find(x => x.Index == indexLobby);
                //Добавляем текущего игрока к списку
                findLobby.AllHero.Add(newPlayerInLobby);
                //У всех, кто подключен к выбранному лоби, вызываем подключение нового игрока
                foreach (var hero in findLobby.AllHero)
                {
                    if (hero == newPlayerInLobby || hero.client is null) continue;
                    hero.client.OnNewConnectToLobby(newUser);
                }
                return true;
            }
            else return false;
        }







        //Временно написал, пока не реализован функцианал других компонентов 
        private async Task StartGame(string message)
        {
            Lobby? newLobby = JsonSerializer.Deserialize<Lobby>(message);
            newLobby.StartTime = $"{DateTime.Now}";
        }
        private async Task EndGame(string message)
        {
            Lobby? newLobby = JsonSerializer.Deserialize<Lobby>(message);
            newLobby.EndTime = $"{DateTime.Now}";
        }
    }
    public class ClientObject
    {
        public string id { get; } = Guid.NewGuid().ToString();
        protected internal string ip { get; }
        protected internal StreamWriter streamWriter { get; }
        protected internal StreamReader streamReader { get; }

        private TcpClient client;
        private ServerObject server;
        public ClientObject(TcpClient tcpClient, ServerObject tcpServer)
        {
            client = tcpClient;
            server = tcpServer;
            var stream = client.GetStream();
            streamReader = new StreamReader(stream);
            streamWriter = new StreamWriter(stream);
            ip = client.Client.RemoteEndPoint.ToString();
        }

        public async Task ProcessClientAsync()
        {
            try
            {
                while (true)
                {
                    var CommandForServer = await streamReader.ReadLineAsync();
                    if (CommandForServer == "DISCONNECT") break;


                    string? message, data;
                    switch (CommandForServer)
                    {
                        //Создание нового пользователя
                        case "CREATE_PROFILE":
                            Console.WriteLine($"{ip} - Send a request to create a profile");
                            message = await streamReader.ReadLineAsync();

                            var isSuccessfull_Creating = await server.CreateNewProfile(message);

                            if (isSuccessfull_Creating)
                                await streamWriter.WriteLineAsync("1");
                            else
                                await streamWriter.WriteLineAsync("2");
                            break;

                        //Вход в аккаунт
                        case "LOGIN_PROFILE":
                            Console.WriteLine($"{ip} - Send a request to login in profile");
                            message = await streamReader.ReadLineAsync();

                            var isSuccessfull_Login = await server.LoginInProfile(message);

                            if (isSuccessfull_Login != null)
                            {
                                isSuccessfull_Login.Password = "";
                                await streamWriter.WriteLineAsync("3");
                                await streamWriter.WriteLineAsync(JsonSerializer.Serialize(isSuccessfull_Login));
                            }
                            else
                                await streamWriter.WriteLineAsync("4");
                            break;

                        //Смена аватара
                        case "CHANGE_DATA_PROFILE":
                            Console.WriteLine($"{ip} - Send a request to change data profile");
                            message = await streamReader.ReadLineAsync();

                            var isSuccessfull_ChangeData = await server.ChangeDataInProfile(message);

                            if (isSuccessfull_ChangeData)
                                await streamWriter.WriteLineAsync("5");
                            else
                                await streamWriter.WriteLineAsync("6");
                            break;

                        //Получение списка всех активных лобби
                        case "GET_LIST_ACTIVE_LOBBY":
                            Console.WriteLine($"{ip} - Send a request list with all active lobby");
                            var allActiveLobby = await server.GetListActiveLobby();

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

                        //Создание нового лобби
                        case "CREATE_LOBBY":
                            Console.WriteLine($"{ip} - Send a request on create new lobby");
                            message = await streamReader.ReadLineAsync();
                            var createLobby = await server.CreateNewLobby(message);

                            if (createLobby != null)
                            {
                                await streamWriter.WriteLineAsync("9");
                                await streamWriter.WriteLineAsync(JsonSerializer.Serialize(createLobby));
                            }
                            else
                                await streamWriter.WriteLineAsync("10");
                            break;

                        //Подключение к существующему лобби
                        case "CONNECT_LOBBY":
                            Console.WriteLine($"{ip} - Send a request on connect to lobby");
                            //Здесь должен быть индекс лобби к которому делаем подключение
                            message = await streamReader.ReadLineAsync();
                            //Здесь получаем логин пользователя, которого подключаем
                            data = await streamReader.ReadLineAsync();

                            if (await server.FirstConnectLobby(message, data))
                            {
                                await streamWriter.WriteLineAsync("11");

                                await streamWriter.WriteLineAsync(JsonSerializer.Serialize(server.AllActiveLobby.Find(x => x.Index == message)));
                                //await streamWriter.WriteLineAsync(JsonSerializer.Serialize(server.GetDataAboutUser(data).Result));
                            }
                            else
                                await streamWriter.WriteLineAsync("12");

                            break;

                        //Если клиент прислал неизвестный запрос
                        default:
                            message = await streamReader.ReadLineAsync();
                            Console.WriteLine($"{ip} - Send a unknow request");

                            await streamWriter.WriteLineAsync("-1");
                            break;
                    }
                    await streamWriter.FlushAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                server.RemoveConnection(id);
            }
            finally
            {
                Console.WriteLine($"Клиент {ip} отключен");
                client.Close();
                //Отключаем клиент от сервера
                server.RemoveConnection(id);

            }
            
        }

        /// <summary>
        /// Вызываем когда к лобби, в котором есть данный игрок, подключается новый игрок
        /// </summary>
        /// <returns></returns>
        protected internal async Task OnNewConnectToLobby(User message)
        {
            await streamWriter.WriteLineAsync("13");
            await streamWriter.WriteLineAsync(JsonSerializer.Serialize(message));
        }


        /// <summary>
        /// Отключение подключения и закрытие клиента на сервере
        /// </summary>
        protected internal void Close()
        {
            streamWriter.Close();
            streamReader.Close();
            client.Close();
        }
    }

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
}
/// <summary>
/// [BsonIgnoreExtraElements] для фикса ошибки, если поля не принципиальны, подробнее по ссылке
/// https://metanit.com/sharp/mongodb/1.7.php
/// </summary>

public class User
{
    public ObjectId Id { get; set; }
    public string? UserName { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? AvatarBase64 { get; set; }
}
public class Lobby
{
    public ObjectId Id { get; set; }
    public string? Index { get; set; }
    public List<InfoAboutPlayer> AllHero { get; set; }
    public SettingsLobby Settings { get; set; }
    public bool IsStart { get; set; }
    public bool IsEnd { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }

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
[System.Serializable]
public class SettingsLobby
{
    public string Name { get; set; }
    public byte MaxPlayers { get; set; }
    public bool isPrivate { get; set; }
    public string Password { get; set; }
}
[System.Serializable]
[BsonIgnoreExtraElements]
public class InfoAboutPlayer
{
    public Hero hero { get; set; }
    public User user { get; set; }
    public ClientObject client;
}