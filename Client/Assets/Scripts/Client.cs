using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using BunkerGame.ClassUser;
using BunkerGame.ClassLobby;



namespace BunkerGame.ClassClient
{
    public delegate void RequestMethod(string val);
    public struct Request
    {
        public RequestMethod method;
        public string data;
    }
    /// <summary>
    /// Client хранит адрес сервера, 
    /// соединение, 
    /// типы запросов/ответов
    /// Самого клиента, 
    /// очередь методов,
    /// очередь сообщений
    /// </summary>
    public class Client
    {
        public string ServerIp = "127.0.0.1";
        public int serverPort = 8888;


        StreamReader streamReader;
        StreamWriter streamWriter;

        public Dictionary<uint, string> TypePOST { get; } = new Dictionary<uint, string>()
        {
            { 0,"DISCONNECT"},
            { 10,"CREATE_PROFILE"},
            { 11,"LOGIN_PROFILE"},
            { 12,"CHANGE_DATA_PROFILE"},
            { 20,"GET_LIST_ACTIVE_LOBBY"},
            { 21,"CREATE_LOBBY"},
            { 22,"CONNECT_LOBBY"},
            { 23,"DISCONNECT_LOBBY"},
            { 30,"CHANGE_READINESS"},
        };
        public Dictionary<int, string> TypeGET { get; } = new Dictionary<int, string>()
        {
            { -1,"Incorrect request"},
            { 10,"Account created successfully!"},
            { 11,"An account with this login already exists!"},
            { 12,"Account information is correct!"},
            { 13,"Account information is not correct!"},
            { 14,"Profile has been update!"},
            { 15,"Profile not update, please try again!"},

            { 20,"Received a list from the lobby!"},
            { 21,"Failed to get lobby list data!"},
            { 22,"New lobby successfully created!"},
            { 23,"Failed to create lobby!"},
            { 24,"Successfully connect to lobby!"},
            { 25,"Error on connect to lobby!"},
            { 26,"New player has been connected to lobby!"},

            { 50,"State has been changed"},
            { 51,"Error"},
            { 100,"Start game"},
        };

        static Client_To_Scripts_Bridge _ThisClient;
        TcpClient tcpClient;


        public Queue<Request> queueMethods = new Queue<Request>();

        public async void Start(Client_To_Scripts_Bridge client_INSPECTOR)
        {
            _ThisClient = client_INSPECTOR;

            tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(ServerIp, serverPort);
                var stream = tcpClient.GetStream();

                streamReader = new StreamReader(stream);
                streamWriter = new StreamWriter(stream);

                if (tcpClient.Connected)
                    Debug.Log($"Подключение с {tcpClient.Client.RemoteEndPoint} установлено");
                else
                    Debug.Log("Не удалось подключиться");

                
                new Thread(async () => await ReceiveMessageAsync()).Start();
            }
            catch (Exception ex) 
            {
                tcpClient.Close();
                Debug.Log("Не удалось подключиться к серверу, проверьте подключение к интернету");
            }

        }

        public async void CreateMessageForServer<T>(UInt16 POSTIndex, params T[] _Object)
        {
            queue.Enqueue(POSTIndex);
            await SendMessageAsync(_Object);
        }


        public void ServerMessage(string? message) => Debug.Log($"Server send message: {TypeGET[Convert.ToInt32(message)]}");

        Queue<UInt16> queue = new Queue<UInt16>();
        async Task SendMessageAsync<T>(params T[]? Object)
        {
            string? message = null;

            var index = queue.Dequeue();
            await streamWriter.WriteLineAsync(TypePOST[index]);
            if (index == 0)
            {
                await streamWriter.FlushAsync();
                tcpClient.Client.Close();
            }

            if (Object.Length > 0)
            {
                foreach (var item in Object)
                {
                    message = (typeof(T) != typeof(string)) ? JsonUtility.ToJson(item) : item.ToString();
                    await streamWriter.WriteLineAsync(message);
                }
            }
            await streamWriter.FlushAsync();
            
        }
        async Task ReceiveMessageAsync()
        {
            while (true)
            {
                try
                {   
                    string? message = await streamReader.ReadLineAsync();
                    string? data = null;
                    Request _request = new Request();
                    Debug.Log("Server send message");
                    
                    switch (Convert.ToInt32(message))
                    {
                        case 10:
                            //Пока пусто, тк тут результат регистрации
                            break;
                        case 12:
                            //Получаем информацию о корректных данных пользователя которым логинимся
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.isCorrectData, data = data };
                            queueMethods.Enqueue(_request);
                            break;
                        case 14:
                            //Получаем информацию об успешном обновлении данных (пока что только аватар) 
                            _request = new Request { method = _ThisClient.isChangeAvatar, data = "" }; //Посмотреть где у меня беруться данные, оттуда и будем забирать
                            queueMethods.Enqueue(_request);
                            break;
                        case 20:
                            //Получаем количество активных лобби
                            var countActiveLobby = await streamReader.ReadLineAsync();
                            //Начинаем получать информацию о каждом активном лобби
                            for (int i = 0; i < Convert.ToUInt32(countActiveLobby); i++)
                            {
                                data = await streamReader.ReadLineAsync();
                                _request = new Request { method = _ThisClient.AddLobbyInList, data = data };
                                queueMethods.Enqueue(_request);
                            }
                            break;
                        case 22:
                            //Получаем успешность создания лобби, если успешно создано то запускаем у пользователя открытия лобби
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.isCreatedNewLobby, data = data };
                            queueMethods.Enqueue(_request);

                            break;


                        //возможно объеденить методы 11 и 13
                        case 24:
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.ThisPlayerConnectToLobby, data = data };
                            queueMethods.Enqueue(_request);
                            break;
                        case 26: ///<- данный метод может сработать только тогда, когда игрок находится в каком то лобби
                            //Сервер сообщает что подключился новый пользователь к лобби
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.ConnectNewPlayerToLobby, data = data };
                            queueMethods.Enqueue(_request);
                            break;


                        case 100:
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.StartGame, data = data };
                            queueMethods.Enqueue(_request);
                            break;
                        default:
                            break;
                    }
                    ServerMessage(message);
                }
                catch
                {
                    break;
                }
            
            }
            return;
        }
    }
}