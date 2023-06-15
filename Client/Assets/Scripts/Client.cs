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
    public class Client
    {
        public string ServerIp = "127.0.0.1";
        public int serverPort = 8888;


        StreamReader streamReader;
        StreamWriter streamWriter;

        public Dictionary<uint, string> TypePOST { get; } = new Dictionary<uint, string>()
        {
            { 0,"DISCONNECT"},
            { 1,"CREATE_PROFILE"},
            { 2,"LOGIN_PROFILE"},
            { 3,"CHANGE_DATA_PROFILE"},
            { 4,"GET_LIST_ACTIVE_LOBBY"},
            { 5,"CREATE_LOBBY"},
            { 6,"CONNECT_LOBBY"},
        };
        public Dictionary<int, string> TypeGET { get; } = new Dictionary<int, string>()
        {
            { -1,"Incorrect request"},
            { 1,"Account created successfully!"},
            { 2,"An account with this login already exists!"},
            { 3,"Account information is correct!"},
            { 4,"Account information is not correct!"},
            { 5,"Profile has been update!"},
            { 6,"Profile not update, please try again!"},
            { 7,"Received a list from the lobby!"},
            { 8,"Failed to get lobby list data!"},
            { 9,"New lobby successfully created!"},
            { 10,"Failed to create lobby!"},
        };

        static Client_INSPECTOR _ThisClient;
        TcpClient tcpClient;


        public Queue<Request> queueMethods = new Queue<Request>();

        public async void Start(Client_INSPECTOR client_INSPECTOR)
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
                message = JsonUtility.ToJson(Object[0]);
                await streamWriter.WriteLineAsync(message);
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

                    
                    switch (Convert.ToInt32(message))
                    {
                        case 1:
                            //Пока пусто, тк тут результат регистрации
                            break;
                        case 3:
                            //Получаем информацию о корректных данных пользователя которым логинимся
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.isCorrectData, data = data };
                            queueMethods.Enqueue(_request);
                            break;
                        case 5:
                            //Получаем информацию об успешном обновлении данных (пока что только аватар) 
                            _request = new Request { method = _ThisClient.isSuccessfullChangeAvatar, data = "" }; //Посмотреть где у меня беруться данные, оттуда и будем забирать
                            queueMethods.Enqueue(_request);
                            break;
                        case 7:
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
                        case 9:
                            //Получаем успешность создания лобби, если успешно создано то запускаем у пользователя открытия лобби
                            data = await streamReader.ReadLineAsync();
                            _request = new Request { method = _ThisClient.AddLobbyInList, data = data };
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