using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine.Networking;

public class GameServer : MonoBehaviour
{
    public static GameServer I { get; private set; }

    // события инициируются после подключения нового клиента. Аргумент обработчика - ID клиента.
    public event Action<int> NewClientConnected;
    public event Action<int> AfterNewClientConnected;

    private List<TcpClient> _clients; // список подключенных клиентов
    private Queue<TcpClient> _newClients; // список подключенных клиентов
    private Thread _listenerThread; // поток приема приема новых клиентов
    private volatile bool _listenerThreadStopRequest;
    
    void Awake()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one GameServer");
        I = this;
        _clients = new List<TcpClient>();
        _newClients = new Queue<TcpClient>();
        Application.runInBackground = true;
    }

    void Start ()
    {
        // начинаем прием клиентов в новом потоке
        TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 2016);
        listener.Start();
        _listenerThread = new Thread(() => AcceptClients(listener));  
        _listenerThread.Start();      
    }

    void FixedUpdate()
    {
        // новые клиенты добавляются через очередь для того, чтобы можно было инициировать события в главном потоке.
        lock (_newClients) while(_newClients.Count != 0)
        {
            _clients.Add(_newClients.Dequeue());
            if (NewClientConnected != null) NewClientConnected(_clients.Count - 1);
            if (AfterNewClientConnected != null) AfterNewClientConnected(_clients.Count - 1);
        }
    }

    // метод, выполняющийся в потоке приема клиентов
    void AcceptClients(TcpListener listener)
    {
        while (!_listenerThreadStopRequest)
        {
            // добавляем нового клиента в очередь
            TcpClient client = listener.AcceptTcpClient();
            lock (_newClients) _newClients.Enqueue(client);
        }
        listener.Stop();
    }

    // отправка сообщения клиентам. С аргументом по умолчанию сообщение рассылается всем клиентам.
    public void Send(Envelope message, int clientId = -1)
    {
        // сериализуем Envelope
        byte[] data;
        using (var ms = new MemoryStream())
        {
            (new BinaryFormatter()).Serialize(ms, message);
            data = ms.ToArray();
        }
        var crashedClients = new List<TcpClient>(); // список отвалившихся клиентов

        // посылка либо конкретному клиенту, либо всем подключенным, в зависимости от clientId
        int minI = (clientId == -1) ? 0 : clientId;
        int maxI = (clientId == -1) ? _clients.Count - 1 : clientId;
        for (int i = minI; i <= maxI; i++)
        {
            try
            {
                // отправляем длину сообщения и само сериализованное сообщение
                var bw = new BinaryWriter(_clients[i].GetStream());
                bw.Write(data.Length);
                bw.Write(data);
                bw.Flush();
            }
            catch (Exception ex)
            {
                // в случае ошибки добавляем клиента в список на удаление
                crashedClients.Add(_clients[i]);
            }
        }
        // удаляем отвалившихся клиентов
        foreach (var cur in crashedClients)
        {
            cur.Close();
            _clients.Remove(cur);
        }
        
    }
}

// сообщение для передачи клиентам
[Serializable]
public class Envelope
{
    public AddresseeType Addressee; // тип адресата
    public int SenderInstanceId; // Id объекта-отправителя
    public byte[] Data; // тело сообщения
}

public enum AddresseeType {GameObject, GameClient, ClientTextureManager }

