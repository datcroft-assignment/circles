using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public class GameClient : MonoBehaviour, IClientObject
{
    public static GameClient I { get; private set; }
    public int Points { get; private set; }
    public int Time { get; private set; }

    private Thread _receiverThread; // поток приема сообщений от сервера
    private bool _receiverThreadStopRequest; // отмена
    private Queue<Envelope> _messages; // очередь сообщений
    private Dictionary<int, IClientObject> _clientObjects; // объекты-получатели сообщений

    void Awake()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one GameManager");
        I = this;
        _messages = new Queue<Envelope>();
        _clientObjects = new Dictionary<int, IClientObject>();
        Application.runInBackground = true; // чтобы игра не остонавливалась при потере фокуса
    }

    public void Connect (string ip, int port)
    {
        // подключение
        TcpClient client = new TcpClient();
        client.Connect(ip, port);
        client.ReceiveTimeout = 10;
        // запуск потока для получения сообщений
        _receiverThread = new Thread(() => ReceiveMessages(client));
        _receiverThread.Start();
    }

    // получаем приходящие сообщения и заносим их в очередь
    void ReceiveMessages(TcpClient client)
    {
        using (var br = new BinaryReader(client.GetStream()))
        {
            while (!_receiverThreadStopRequest)
            {
                int length = br.ReadInt32(); // получаем размер сообщения
                byte[] data = br.ReadBytes(length); // получаем сообщение
                Envelope msg = (new BinaryFormatter()).Deserialize(new MemoryStream(data)) as Envelope; // десериализуем
                lock(_messages) _messages.Enqueue(msg); // отправляем в очередь полученных сообщений
            }
            client.Close();
        }
    }

    void OnDestroy()
    {
        _receiverThreadStopRequest = true;
    }
	
	void FixedUpdate ()
    {
        // послыка сообщений адресатам
	    lock (_messages) while (_messages.Count != 0)
	    {
            var msg = _messages.Dequeue();
	        switch (msg.Addressee)
	        {
                case AddresseeType.GameObject:           
                    _clientObjects[msg.SenderInstanceId].ReceiveMessage(msg);  // передаем сообщение объекту-адресату
                    break;
                case AddresseeType.GameClient:
	                ReceiveMessage(msg); // обрабатываем сообщение в этом объекте
                    break;
                case AddresseeType.ClientTextureManager:
                    ClientTextureManager.I.ReceiveMessage(msg); // передаем ClientTextureManager'у
                        break;
                default:
                    throw new Exception("Неизвестный тип адресата");
            }
	    }   
    }

    public void ReceiveMessage(Envelope message)
    {
        // сравниваем тип сообщения с поддерживаемыми получателем
        var body = (new BinaryFormatter()).Deserialize(new MemoryStream(message.Data));
        if (body is InstantiateRequest)
        {
            // инстанцируем префаб. Если объект содержит IClientObject, добавляем его в коллекцию.
            var instantiateRequest = (InstantiateRequest)body;
            var newObj = Instantiate(Scrapyard.I.Prefabs[instantiateRequest.PrefabName]);
            IClientObject ico = newObj.GetComponent<IClientObject>();
            if(ico != null) _clientObjects.Add(message.SenderInstanceId, ico);
        }
        else if (body is SetValuesRequest)
        {
            var request = (SetValuesRequest)body;
            Points = request.Points;
            Time = request.Time;
        }
        else throw new Exception("Неизвестный тип запроса");
    }

    // запрос на создание нового объекта
    [Serializable]
    public class InstantiateRequest
    {
        public string PrefabName;
        public InstantiateRequest(string prefabName)
        {
            PrefabName = prefabName;
        }
    }

    // запрос на установку времени и очков
    [Serializable]
    public class SetValuesRequest
    {
        public int Points;
        public int Time;
    }
}

// интерфейс объекта-получателя сообщений
interface IClientObject
{
    void ReceiveMessage(Envelope message);
}
