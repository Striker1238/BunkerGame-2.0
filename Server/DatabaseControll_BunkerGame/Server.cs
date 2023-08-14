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
using BunkerGame.Server;
using BunkerGame.GameRules;
using BunkerGame.ClassLobby;

namespace BunkerGame.Server
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
                BodyType = DatabaseControll.GetListDocumentsAsync<characteristic>("BodyType_Hero").Result;
                FurtherInformation = DatabaseControll.GetListDocumentsAsync<characteristic>("FurtherInformation_Hero").Result;
                Hobbies = DatabaseControll.GetListDocumentsAsync<characteristic>("Hobbies_Hero").Result;
                HealthCondition = DatabaseControll.GetListDocumentsAsync<characteristic>("HealthCondition_Hero").Result;
                HumanTrait = DatabaseControll.GetListDocumentsAsync<characteristic>("HumanTrait_Hero").Result;
                Luggage = DatabaseControll.GetListDocumentsAsync<characteristic>("Luggage_Hero").Result;
                Phobia = DatabaseControll.GetListDocumentsAsync<characteristic>("Phobia_Hero").Result;
                Profession = DatabaseControll.GetListDocumentsAsync<characteristic>("Profession_Hero").Result;
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
            ClientObject? client = AllActiveClients.FirstOrDefault(x => x.client_id == id);
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
        protected internal async Task BroadcastMessageForAllClientsInLobbyAsync<T>(string indexLobby, int indexMessage, string dropUserName = null, params T[] Object)
        {
            foreach (var player in AllActiveLobby.Find(x => x.Index == indexLobby).AllHero)
            {
                string? message = null;
                if(player.user.UserName != dropUserName)
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


                Player? newPlayerInLobby = new Player()
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




        protected internal async Task<bool> ChangeReadiness(string indexLobby, string client_id, bool state)
        {
            if (indexLobby is null || client_id is null) return false;

            var lobby = AllActiveLobby.Find(x => x.Index == indexLobby);

            lobby.AllHero.Find(x => x.client.client_id == client_id).isReady = state;
            foreach (var player in lobby.AllHero)
            {
                if (player.isReady) continue;
                return true;
            }
            //Здесь выполняется если все игроки в комнате будут готовы.
            //if (lobby.AllHero.Count == lobby.Settings.MaxPlayers)
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
        
        ///Пересоздание характеристики по индексу, список игроков

        private async Task StartGame(string indexLobby)
        {
            Lobby? lobby = AllActiveLobby.Find(x => x.Index == indexLobby);
            if (lobby is null) return;


            lobby.WorldEvent = "Падение метеорита. " +
                "Крупный метеорит попал в Землю, " +
                "что приводит к глобальным разрушениям и смене климата," +
                " что вызвало массовые вымирание среди животных и растений," +
                " не способных приспособиться к новым условиям. " +
                "После выхода из бункера на планете вечная зима.";
            lobby.NewBunker = new Lobby.BunkerInfo()
            {
                Contry = "Авганистан",
                Items = new string[] { "Видеопроигрыватель", "Книги по психологии", "Успокоительные препараты" },
                Equipment = new string[] { "Библиотека", "Кухня-столовая" },
                InBunkerLive = "Крысы",
            };



            lobby.StartTime = $"{DateTime.Now}";
            lobby.IsStart = true;

            foreach (var player in lobby.AllHero)
            {
                player.hero = player.client.GenerateHero().Result;
                SendMessageAsync<string>(player.client, 100, JsonSerializer.Serialize(player.hero) , lobby.WorldEvent, JsonSerializer.Serialize(lobby.NewBunker));
            }
            Console.WriteLine($"Lobby {lobby.Index} is started");
            ///Здесь стоит показать различные заставки/начала [-]
            ///Также нужно сгенерировать ситуации, бункер, подзадачи [-]
            ///Запустить таймер на ознакомление со всей имеющейся информацией [-]
            ///Запуск раундов по игровым правилам(GameRules) [-]
            GameRules.GameRules test = new GameRules.GameRules();
            test.CountRounds = (byte)MathF.Ceiling(lobby.Settings.MaxPlayers / 2);
            test.round = new Round(1, 2, 60, lobby.Settings.MaxPlayers, 13);
            while (test.CountRounds > test.round.RoundNumber)
            {
                await Timer(test.round,lobby);
                test.round = new Round((byte)(test.round.RoundNumber+1), 2, 60, lobby.Settings.MaxPlayers, 13);
            }
            
        }
        private async Task Timer(Round round, Lobby lobby)
        {
            byte time = round.TimeInSecondsPerStep;
            round.StepNumber = 0;

            while (true)
            {
                await Task.Delay(1000);
                time++;

                if (time < round.TimeInSecondsPerStep) continue;

                round.StepNumber++;
                time = 0;
                //Здесь меняется очередь игрока
                Console.WriteLine($"Player #{round.StepNumber} start step");
                BroadcastMessageForAllClientsInLobbyAsync(lobby.Index, 105, null, lobby.AllHero[round.StepNumber-1].user.Login);


                if (round.StepNumber > round.CountStep)
                {
                    //Здесь завершается раунд и должен начинаться новый
                    Console.WriteLine($"Round #{round.RoundNumber} end");
                    return;
                }

            }
        }
        /// <summary>
        /// Изменить видимость характеристики
        /// </summary>
        /// <param name="indexLobby">Лобби, в котором происходят действия</param>
        /// <param name="client_id">Имя пользователя, который открывает характеристику</param>
        /// <param name="indexCharacteristics">Индекс характеристики</param>
        /// <returns>true - характеристика успешно показана, false - ошибка в ходных данных, либо данная характеристика уже открыта</returns>
        protected internal async Task<bool> ChangeVisibleInfo(string indexLobby, string client_id, byte[] indexCharacteristics)
        {
            if (indexCharacteristics is null || indexLobby is null || client_id is null) return false;
            var lobby = AllActiveLobby.Find(x => x.Index == indexLobby);

            string[] characteristic = lobby.AllHero.Find(x => x.client.client_id == client_id).ReturnCharacteristics(indexCharacteristics).Result;
            for (int i = 0; i < indexCharacteristics.Length; i++)
            {
                await BroadcastMessageForAllClientsInLobbyAsync<string>(indexLobby, 110, null /*client_id*/,lobby.AllHero.Find(x => x.client.client_id == client_id).user.Login, indexCharacteristics[i].ToString(), characteristic[i]);
            }
            


            return true;
        }

        private async Task EndGame(string message)
        {
            Lobby? newLobby = JsonSerializer.Deserialize<Lobby>(message);
            newLobby.EndTime = $"{DateTime.Now}";
        }
    }
    public class ClientObject
    {
        public string client_id { get; } = Guid.NewGuid().ToString();
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
                //Отправляем клиенту его id
                await server.SendMessageAsync(this, 1, client_id);

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



                        case "VISIBLE_INFO":
                            //Индекс лобби
                            data_1 = await streamReader.ReadLineAsync();
                            //Индекс игрока
                            data_2 = await streamReader.ReadLineAsync();
                            //Индекс информации, у которой меняется видимость
                            data_3 = await streamReader.ReadLineAsync();
                            if(!server.ChangeVisibleInfo(data_1, data_2, new byte[] { Convert.ToByte(data_3) } ).Result)
                                //await server.SendMessageAsync<string>(this, 111);
                            //else
                                await server.SendMessageAsync<string>(this, 112);
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
                server.RemoveConnection(client_id);
            }
            finally
            {
                Console.WriteLine($"Клиент {ip} отключен");
                client.Close();
                //Отключаем клиент от сервера
                server.RemoveConnection(client_id);
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


            hero.Profession_Hero = ServerObject.GetRandomIndex(ServerObject.Profession).Result.translation[1].Profession;
                hero.ExperienceProfession_Hero = (byte)ServerObject.GetRandomData<int>
                ((5, 1, 5),     //5% от 1 месяца до 5 месяцев
                (10, 6, 11),    //10% от 6 месяцев до 11 месяцев
                (40, 12, 35),   //40% от 1 года до 2 лет и 11 месяцев
                (30, 36, 95),   //30% от 3 лет до 7 лет и 11 месяцев
                (10, 96, 179),  //10% от 8 лет  до 14 лет и 11 месяцев
                (5, 180, 480)   //5% от 15 лет до 40 лет
                ).Result;

            hero.Sex_Hero = ServerObject.GetRandomData<bool>
            ((50, true, 0), //50% мужчина - true
            (50, false, 0)  //50% женщина - false
            ).Result;


            hero.Hobbies_Hero = ServerObject.GetRandomIndex(ServerObject.Hobbies).Result.translation[1].Profession;
                hero.ExperienceHobbies_Hero = (byte)ServerObject.GetRandomData<int>
                ((5, 1, 5),     //5% от 1 месяца до 5 месяцев
                (10, 6, 11),    //10% от 6 месяцев до 11 месяцев
                (40, 12, 35),   //40% от 1 года до 2 лет и 11 месяцев
                (30, 36, 95),   //30% от 3 лет до 7 лет и 11 месяцев
                (10, 96, 179),  //10% от 8 лет  до 14 лет и 11 месяцев
                (5, 180, 480)   //5% от 15 лет до 40 лет
                ).Result;

            hero.Luggage_Hero = ServerObject.GetRandomIndex(ServerObject.Luggage).Result.translation[1].Profession;

            hero.HealthCondition_Hero = ServerObject.GetRandomIndex(ServerObject.HealthCondition).Result.translation[1].Profession;
            hero.HealthPoint_Hero = (byte)ServerObject.GetRandomData<int>
            ((15, 0, 0),      //15% 0 => болезни нет
            (5, 1, 15),       //5% тяжесть от 1% до 15%  
            (35, 16, 35),     //35% тяжесть от 16% до 35% 
            (20, 36, 60),     //20% тяжесть от 36% до 60%
            (15, 61, 75),     //15% тяжесть от 61% до 75%
            (7, 76, 90),      //7% тяжесть от 76% до 90%
            (3, 91, 99)       //3% тяжесть от 91% до 99%
            ).Result;

            hero.Phobia_Hero = ServerObject.GetRandomIndex(ServerObject.Phobia).Result.translation[1].Profession;
            hero.PhobiaPercentage_Hero = (byte)ServerObject.GetRandomData<int>
            ((35, 0, 0),      //35% 0 => фобии нет
            (5, 1, 15),       //5% тяжесть от 1% до 15%  
            (25, 16, 35),     //25% тяжесть от 16% до 35% 
            (15, 36, 60),     //15% тяжесть от 36% до 60%
            (10, 61, 75),     //10% тяжесть от 61% до 75%
            (7, 76, 90),      //7% тяжесть от 76% до 90%
            (3, 91, 99)       //3% тяжесть от 91% до 99%
            ).Result;


            hero.HumanTrait_Hero = ServerObject.GetRandomIndex(ServerObject.HumanTrait).Result.translation[1].Profession;


            hero.FurtherInformation_Hero = ServerObject.GetRandomIndex(ServerObject.FurtherInformation).Result.translation[1].Profession;


            hero.BodyType_Hero = ServerObject.GetRandomIndex(ServerObject.BodyType).Result.translation[1].Profession;
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

}
/// <summary>
/// [BsonIgnoreExtraElements] для фикса ошибки, если поля не принципиальны, подробнее по ссылке
/// https://metanit.com/sharp/mongodb/1.7.php
/// </summary>
/// <summary>
/// Возможно переименовать и переписать класс
/// </summary>
public class characteristic
{
    public ObjectId Id { get; set; }
    public Text[] translation { get; set; }
    public bool Whether_Measured { get; set; }


    public class Text
    {
        public string language { get; set; }
        public string Profession { get; set; }
    }
}