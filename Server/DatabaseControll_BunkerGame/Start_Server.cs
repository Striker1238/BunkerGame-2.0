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
using MongoDB.Bson.Serialization.Attributes;
using SharpCompress.Writers;
using BunkerGame.Database;

namespace BunkerGame.Database
{
    public class ServerObject
    {
        TcpListener server;

        public List<Lobby> AllActiveLobby = new List<Lobby>();
        private List<ClientObject> AllActiveClients = new List<ClientObject>();

        static Database? DatabaseControll;

        #region Characteristics_With_Database
        protected internal static List<characteristic> BodyType = new List<characteristic>();
        protected internal static List<characteristic> FurtherInformation = new List<characteristic>();
        protected internal static List<characteristic> Hobbies = new List<characteristic>();
        protected internal static List<characteristic> HealthCondition = new List<characteristic>();
        protected internal static List<characteristic> HumanTrait = new List<characteristic>();
        protected internal static List<characteristic> Luggage = new List<characteristic>();
        protected internal static List<characteristic> Phobia = new List<characteristic>();
        protected internal static List<characteristic> Profession = new List<characteristic>();
        #endregion



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
                BodyType = DatabaseControll.GetListDocumentsAsync<characteristic>("BodyType_Hero").Result;
                FurtherInformation = DatabaseControll.GetListDocumentsAsync<characteristic>("FurtherInformation_Hero").Result;
                Hobbies = DatabaseControll.GetListDocumentsAsync<characteristic>("Hobbies_Hero").Result;
                HealthCondition = DatabaseControll.GetListDocumentsAsync<characteristic>("HealthCondition_Hero").Result;
                HumanTrait = DatabaseControll.GetListDocumentsAsync<characteristic>("HumanTrait_Hero").Result;
                Luggage = DatabaseControll.GetListDocumentsAsync<characteristic>("Luggage_Hero").Result;
                Phobia = DatabaseControll.GetListDocumentsAsync<characteristic>("Phobia_Hero").Result;
                Profession = DatabaseControll.GetListDocumentsAsync<characteristic>("Profession_Hero").Result;
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

        /// <summary>
        /// Отправляем сообщение ОДНОМУ клиенту
        /// </summary>
        /// <returns></returns>
        protected internal async Task SendMessageAsync<T>(ClientObject client, int index, params T[] Object)
        {
            string? message = null;

            await client.streamWriter.WriteLineAsync(index.ToString());

            if (Object.Length > 0)
            {

                foreach (var item in Object)
                {
                    message = (typeof(T) != typeof(string)) ? JsonSerializer.Serialize(item) : item.ToString();
                    await client.streamWriter.WriteLineAsync(message);
                }
            }
            await client.streamWriter.FlushAsync();
        }
        /// <summary>
        /// Отправляем сообщение ВСЕМ клиентам подключенных к серверу
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected internal async Task BroadcastMessageForAllClientsOnServerAsync(string message)
        {
            foreach (var client in AllActiveClients)
            {
                await client.streamWriter.WriteLineAsync(message);
                await client.streamWriter.FlushAsync();
            }
        }
        /// <summary>
        /// Отправляем сообщение всем клиентам в ОПРЕДЕЛЕННОМ лобби
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected internal async Task BroadcastMessageForAllClientsInLobbyAsync<T>(string indexLobby, int indexMessage, params T[] Object)
        {
            foreach (var player in AllActiveLobby.Find(x => x.Index == indexLobby).AllHero)
            {
                string? message = null;

                await player.client.streamWriter.WriteLineAsync(indexMessage.ToString());

                if (Object.Length > 0)
                {
                    foreach (var item in Object)
                    {
                        message = (typeof(T) != typeof(string)) ? JsonSerializer.Serialize(item) : item.ToString();
                        await player.client.streamWriter.WriteLineAsync(message);
                    }
                }
                await player.client.streamWriter.FlushAsync();
            }
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
            var listLobby = new List<Lobby>(AllActiveLobby.FindAll(x => x.IsEnd == false && x.IsStart == false));
            //foreach (var lobby in listLobby)
               // lobby.Settings.Password = "";
            return listLobby;

        }
        protected internal async Task<Lobby> GetDataAboutLobby(string indexLobby)
        {
            var lobby = new Lobby(AllActiveLobby.Find(x => x.Index == indexLobby));
            //lobby.Settings.Password = "";
            return lobby;
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
            Lobby? findLobby = new Lobby(AllActiveLobby.Find(x => x.Index == indexLobby));
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
        protected internal async Task<bool> FirstConnectLobby(string indexLobby, string userLogin, string passwordLobby , ClientObject newClientObj)
        {
            var filterLobby = new BsonDocument { { "Index", indexLobby } };
            var filterUser = new BsonDocument { { "Login", userLogin } };
            if (indexLobby != null && userLogin != null 
                && DatabaseControll.IsDocumentExistsAsync<User>("Users", filterUser).Result)
            {

                Lobby? findLobby = new Lobby(AllActiveLobby.Find(x => x.Index == indexLobby));
                var newUser = GetDataAboutUser(userLogin).Result;

                if (newUser is null || findLobby.Settings.Password != passwordLobby) return false;


                InfoAboutPlayer? newPlayerInLobby = new InfoAboutPlayer()
                {
                    user = newUser,
                    hero = new Hero(),
                    client = newClientObj
                };

                //Добавляем текущего игрока к списку
                findLobby.AllHero.Add(newPlayerInLobby);
                //У всех, кто подключен к выбранному лобби, вызываем подключение нового игрока
                foreach (var hero in findLobby.AllHero)
                {
                    if (hero == newPlayerInLobby || hero.client is null) continue;
                    await hero.client.OnNewConnectToLobby(newUser);
                }
                return true;
            }
            else return false;
        }




        protected internal async Task<bool> ChangeReadiness(string indexLobby, string userLogin, bool state)
        {
            if (indexLobby is null || userLogin is null) return false;

            var lobby = AllActiveLobby.Find(x => x.Index == indexLobby);

            lobby.AllHero.Find(x => x.user.Login == userLogin).isReady = state;
            foreach (var player in lobby.AllHero)
            {
                if (player.isReady) continue;
                return true;
            }
            //Здесь выполняется если все игроки в комнате будут готовы.
            if (lobby.AllHero.Count == lobby.Settings.MaxPlayers)
                StartGame(indexLobby);

            return true;
        }
        protected internal async static Task<T> GetRandomIndex<T>(List<T> newList) => newList[new Random().Next(newList.Count)];
        protected internal async static Task<T> GetRandomData<T>(params (float weight, T meaning, float max)[] chance)
        {
            if (chance.Length == 0) return (T)new object();

            float TotalWeight = 0;
            foreach (var item in chance)
                TotalWeight += item.weight;

            float WeightCost = 100 / TotalWeight;
            var currentNumber = 0f;
            int rundomNumber = (new Random().Next(100000000, 1000000000)) % 100;
            foreach (var item in chance)
            {
                currentNumber += item.weight * WeightCost;
                if (rundomNumber >= currentNumber) continue;

                if (typeof(T) == typeof(int)) return (T)(object) new Random().Next((int)(object)item.meaning, (int)item.max+1);
                else return item.meaning;
            }
            return (T)new object();
        }


        private async Task StartGame(string indexLobby)
        {
            Lobby? lobby = AllActiveLobby.Find(x => x.Index == indexLobby);
            lobby.StartTime = $"{DateTime.Now}";
            lobby.IsStart = true;

            foreach (var player in lobby.AllHero)
            {
                player.hero = player.client.GenerateHero().Result;
                SendMessageAsync<Hero>(player.client, 100, player.hero);
            }

            Console.WriteLine(SaveLobbyInDatabase(indexLobby));
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


                    string? data_1, data_2, data_3;
                    switch (CommandForServer)
                    {
                        //Создание нового пользователя
                        case "CREATE_PROFILE":
                            Console.WriteLine($"{ip} - Send a request to create a profile");
                            data_1 = await streamReader.ReadLineAsync();

                            var isSuccessfull_Creating = await server.CreateNewProfile(data_1);

                            if (isSuccessfull_Creating)
                                await server.SendMessageAsync<string>(this,10);
                            else
                                await server.SendMessageAsync<string>(this,11);
                            break;

                        //Вход в аккаунт
                        case "LOGIN_PROFILE":
                            Console.WriteLine($"{ip} - Send a request to login in profile");
                            data_1 = await streamReader.ReadLineAsync();

                            var isSuccessfull_Login = await server.LoginInProfile(data_1);

                            if (isSuccessfull_Login != null)
                            {
                                isSuccessfull_Login.Password = "";
                                await server.SendMessageAsync(this, 12, isSuccessfull_Login);
                            }
                            else
                                await server.SendMessageAsync<string>(this, 13);
                            break;

                        //Смена аватара
                        case "CHANGE_DATA_PROFILE":
                            Console.WriteLine($"{ip} - Send a request to change data profile");
                            data_1 = await streamReader.ReadLineAsync();

                            var isSuccessfull_ChangeData = await server.ChangeDataInProfile(data_1);

                            if (isSuccessfull_ChangeData)
                                await server.SendMessageAsync<string>(this, 14);
                            else
                                await server.SendMessageAsync<string>(this, 15);
                            break;

                        //Получение списка всех активных лобби
                        case "GET_LIST_ACTIVE_LOBBY":
                            Console.WriteLine($"{ip} - Send a request list with all active lobby");
                            var allActiveLobby = await server.GetListActiveLobby();

                            if (allActiveLobby.Count > 0)
                            {
                                await streamWriter.WriteLineAsync("20");
                                await streamWriter.WriteLineAsync(allActiveLobby.Count.ToString());
                                foreach (var lobby in allActiveLobby)
                                {
                                    await streamWriter.WriteLineAsync(JsonSerializer.Serialize(lobby));
                                }
                            }
                            else
                                await server.SendMessageAsync<string>(this, 21);

                            await streamWriter.FlushAsync();
                            break;

                        //Создание нового лобби
                        case "CREATE_LOBBY":
                            Console.WriteLine($"{ip} - Send a request on create new lobby");
                            data_1 = await streamReader.ReadLineAsync();
                            var createLobby = new Lobby(await server.CreateNewLobby(data_1));

                            if (createLobby != null)
                                await server.SendMessageAsync(this, 22, createLobby);
                                //createLobby.Settings.Password = "";
                            else
                                await server.SendMessageAsync<string>(this, 23);
                            break;

                        //Подключение к существующему лобби
                        case "CONNECT_LOBBY":
                            Console.WriteLine($"{ip} - Send a request on connect to lobby");
                            //Здесь должен быть индекс лобби к которому делаем подключение
                            data_1 = await streamReader.ReadLineAsync();
                            //Здесь получаем логин пользователя, которого подключаем
                            data_2 = await streamReader.ReadLineAsync();
                            //Здесь получаем пароль, который ввел пользователь, для подключения к лобби
                            data_3 = await streamReader.ReadLineAsync();

                            Console.WriteLine(data_3);
                            if (await server.FirstConnectLobby(data_1, data_2, data_3, this))
                            {
                                var lobby = new Lobby(server.AllActiveLobby.Find(x => x.Index == data_1));
                                //lobby.Settings.Password = "";
                                await server.SendMessageAsync(this, 24, lobby);
                                //await streamWriter.WriteLineAsync(JsonSerializer.Serialize(lobby));
                            }
                            else
                                await server.SendMessageAsync<string>(this, 25);

                            break;


                        case "CHANGE_READINESS":
                            //Здесь должен быть индекс лобби
                            data_1 = await streamReader.ReadLineAsync();
                            //Здесь получаем логин пользователя
                            data_2 = await streamReader.ReadLineAsync();
                            //Здесь получаем статус (готов/не готов)
                            data_3 = await streamReader.ReadLineAsync();
                            if(server.ChangeReadiness(data_1, data_2, data_3 == "1").Result)
                                await server.SendMessageAsync<string>(this, 50);
                            else
                                await server.SendMessageAsync<string>(this, 51);

                            break;


                        //Если клиент прислал неизвестный запрос
                        default:
                            data_1 = await streamReader.ReadLineAsync();
                            Console.WriteLine($"{ip} - Send a unknow request");
                            await server.SendMessageAsync<string>(this, -1);
                            break;
                    }
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
        protected internal async Task OnNewConnectToLobby(User message) => await server.SendMessageAsync(this, 26, message);
        protected internal async Task<Hero> GenerateHero()
        {
            var hero = new Hero();


            hero.Age_Hero = (byte)ServerObject.GetRandomData<int>
            ((3, 14, 20),   //3% от 14 лет до 20 лет
            (40, 21, 35),   //40% от 21 года до 35 лет
            (30, 36, 45),   //30% от 36 до 45 лет
            (10, 46, 50),   //10% от 46 до 50 лет
            (12, 51, 60),   //12% от 51 до 60 лет
            (5, 61, 95)     //5% от 61 до 95 лет
            ).Result;


            hero.Profession_Hero = ServerObject.GetRandomIndex(ServerObject.Profession).Result.translation[0].Profession;
                hero.ExperienceProfession_Hero = (byte)ServerObject.GetRandomData<int>
                ((5, 1, 5),     //5% от 1 месяца до 5 месяцев
                (10, 6, 11),    //10% от 6 месяцев до 11 месяцев
                (40, 12, 35),   //40% от 1 года до 2 лет и 11 месяцев
                (30, 36, 95),   //30% от 3 лет до 7 лет и 11 месяцев
                (10, 96, 179),  //10% от 8 лет  до 14 лет и 11 месяцев
                (5, 180, 480)   //5% от 15 лет до 40 лет
                ).Result;

            hero.Sex_Hero = ServerObject.GetRandomData<bool>
            ((50, true, 0), //50% мужчина
            (50, false, 0)  //50% женщина
            ).Result;


            hero.Hobbies_Hero = ServerObject.GetRandomIndex(ServerObject.Hobbies).Result.translation[0].Profession;
                hero.ExperienceHobbies_Hero = (byte)ServerObject.GetRandomData<int>
                ((5, 1, 5),     //5% от 1 месяца до 5 месяцев
                (10, 6, 11),    //10% от 6 месяцев до 11 месяцев
                (40, 12, 35),   //40% от 1 года до 2 лет и 11 месяцев
                (30, 36, 95),   //30% от 3 лет до 7 лет и 11 месяцев
                (10, 96, 179),  //10% от 8 лет  до 14 лет и 11 месяцев
                (5, 180, 480)   //5% от 15 лет до 40 лет
                ).Result;

            hero.Luggage_Hero = ServerObject.GetRandomIndex(ServerObject.Luggage).Result.translation[0].Profession;

            hero.HealthCondition_Hero = ServerObject.GetRandomIndex(ServerObject.HealthCondition).Result.translation[0].Profession;
            hero.HealthPoint_Hero = (byte)ServerObject.GetRandomData<int>
            ((15, 0, 0),      //15% 0 => болезни нет
            (5, 1, 15),       //5% тяжесть от 1% до 15%  
            (35, 16, 35),     //35% тяжесть от 16% до 35% 
            (20, 36, 60),     //20% тяжесть от 36% до 60%
            (15, 61, 75),     //15% тяжесть от 61% до 75%
            (7, 76, 90),      //7% тяжесть от 76% до 90%
            (3, 91, 99)       //3% тяжесть от 91% до 99%
            ).Result;

            hero.Phobia_Hero = ServerObject.GetRandomIndex(ServerObject.Phobia).Result.translation[0].Profession;
            hero.PhobiaPercentage_Hero = (byte)ServerObject.GetRandomData<int>
            ((35, 0, 0),      //35% 0 => фобии нет
            (5, 1, 15),       //5% тяжесть от 1% до 15%  
            (25, 16, 35),     //25% тяжесть от 16% до 35% 
            (15, 36, 60),     //15% тяжесть от 36% до 60%
            (10, 61, 75),     //10% тяжесть от 61% до 75%
            (7, 76, 90),      //7% тяжесть от 76% до 90%
            (3, 91, 99)       //3% тяжесть от 91% до 99%
            ).Result;


            hero.HumanTrait_Hero = ServerObject.GetRandomIndex(ServerObject.HumanTrait).Result.translation[0].Profession;


            hero.FurtherInformation_Hero = ServerObject.GetRandomIndex(ServerObject.FurtherInformation).Result.translation[0].Profession;


            hero.BodyType_Hero = ServerObject.GetRandomIndex(ServerObject.BodyType).Result.translation[0].Profession;
            hero.BodyPrecentage_Hero = 3;
            return hero;
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
    public Lobby(){}
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
public class Hero
{
    public byte? Age_Hero { get; set; }

    public string? Profession_Hero { get; set; }
    private byte? experienceProfession_Hero { get; set; }
    public byte? ExperienceProfession_Hero { 
        get => experienceProfession_Hero; 
        set { experienceProfession_Hero = ((Age_Hero-16) >= MathF.Ceiling((float)value/12f)) ? value : (byte)Math.Min((int)(Age_Hero - 16),1); } 
    }


    public bool? Sex_Hero { get; set; }


    public string? Hobbies_Hero { get; set; }
    private byte? experienceHobbies_Hero { get; set; }
    public byte? ExperienceHobbies_Hero {
        get => experienceHobbies_Hero;
        set { experienceHobbies_Hero = ((Age_Hero - 16) >= MathF.Ceiling((float)value / 12f)) ? value : (byte)Math.Min((int)(Age_Hero - 16), 1); }
    }


    public string? Luggage_Hero { get; set; }

    public string? HealthCondition_Hero { get; set; }
    private byte? healthPoint_Hero { get; set; }
    public byte? HealthPoint_Hero 
    { 
        get => healthPoint_Hero;
        set {

            healthPoint_Hero = (!ServerObject.HealthCondition.Find(x => x.translation[0].Profession == HealthCondition_Hero).Whether_Measured)
                ? 0
                : value;
            HealthCondition_Hero = (healthPoint_Hero == 0) ? "Нет болезней" : HealthCondition_Hero;
        }
    }

    public string? Phobia_Hero { get; set; }
    private byte? phobiaPercentage_Hero { get; set; }
    public byte? PhobiaPercentage_Hero
    {
        get => phobiaPercentage_Hero;
        set
        {

            phobiaPercentage_Hero = (!ServerObject.Phobia.Find(x => x.translation[0].Profession == Phobia_Hero).Whether_Measured)
                ? 0
                : value;
            Phobia_Hero = (phobiaPercentage_Hero == 0) ? "Нет фобии" : Phobia_Hero;
        }
    }

    public string? HumanTrait_Hero { get; set; }

    public string? FurtherInformation_Hero { get; set; }

    public string? BodyType_Hero { get; set; }
    public byte? BodyPrecentage_Hero { get; set; }
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
    public bool isReady { get; set; }
}



public class translation   
{
    public string language { get; set; }
    public string Profession { get; set; }
}
public class characteristic
{
    public ObjectId Id { get; set; }
    public translation[] translation { get; set; }
    public bool Whether_Measured { get; set; }
}