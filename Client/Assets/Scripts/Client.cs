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



        public async void ClientDissconnect()
        {
            queue.Enqueue(0);
            await SendMessageAsync();
        }
        public async void CreateProfile(User _newUser)
        {
            newUser = _newUser;
            queue.Enqueue(1);
            await SendMessageAsync();
        }
        public async void LoginProfile(User _newUser)
        {
            newUser = _newUser;
            queue.Enqueue(2);
            await SendMessageAsync();
        }
        public async void ChangeAvatarOnClient(User _newUser, string Path)
        {
            newUser = _newUser;
            _Path = Path;
            queue.Enqueue(3);
            await SendMessageAsync();
        }
        public async void GetListLobby ()
        {
            queue.Enqueue(4);
            await SendMessageAsync();
        }
        public async void CreateLobby(Lobby _newLobby)
        {
            newLobby = _newLobby;
            queue.Enqueue(5);
            await SendMessageAsync();
        }






        public void ServerMessage(string? message) => Debug.Log($"Server answer: {TypeGET[Convert.ToInt32(message)]}");

        User? newUser = new User();
        Lobby? newLobby = new Lobby();
        string? _Path = null;
        Queue<byte> queue = new Queue<byte>();
        async Task SendMessageAsync()
        {
            string? message = null;
            await streamWriter.WriteLineAsync(TypePOST[queue.Peek()]);
            switch (queue.Peek())
            {
                case 0:
                    await streamWriter.FlushAsync();
                    tcpClient.Client.Close();
                    return;
                case 1:
                    message = JsonUtility.ToJson(newUser);
                    await streamWriter.WriteLineAsync(message);
                    break;
                case 2:
                    message = JsonUtility.ToJson(newUser);
                    await streamWriter.WriteLineAsync(message);
                    break;
                case 3:
                    newUser.AvatarBase64 = Convert.ToBase64String(File.ReadAllBytes(_Path));
                    message = JsonUtility.ToJson(newUser);
                    await streamWriter.WriteLineAsync(message);
                    break;
                case 5:
                    message = JsonUtility.ToJson(newLobby);
                    await streamWriter.WriteLineAsync(message);
                    break;
                default:
                    break;
            }
            await streamWriter.FlushAsync();
            queue.Dequeue();
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
                            _request = new Request { method = _ThisClient.isSuccessfullChangeAvatar, data = Convert.ToBase64String(File.ReadAllBytes(_Path)) };
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
                            _request = new Request { method = _ThisClient.isSuccessfullCreateLobby, data = JsonUtility.ToJson(newLobby) };
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