using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

public class ClientTextureManager : MonoBehaviour, IClientObject
{
    public static ClientTextureManager I { get; private set; }
    private Dictionary<int, Texture> _textures;

    void Awake ()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one ClientTextureManager");
        I = this;
        _textures = new Dictionary<int, Texture>();       
    }

    public Texture GetTexture(int Id)
    {
        return _textures[Id];
    }

    //void OnGUI()
    //{
    //    // показываем число текстур в _textures
    //    GUILayout.BeginArea(new Rect(0, 0, 200, 40));
    //    GUILayout.TextArea("Объектов в _textures: " + _textures.Count);
    //    GUILayout.EndArea();
    //}

    public void ReceiveMessage(Envelope message)
    {
        // сравниваем тип сообщения с поддерживаемыми получателем
        var body = (new BinaryFormatter()).Deserialize(new MemoryStream(message.Data));
        if (body is CreateTexRequest)
        {
            var request = (CreateTexRequest)body;
            var newTex = new Texture2D(request.Width, request.Height);
            newTex.LoadRawTextureData(request.Data);
            newTex.Apply();
            _textures.Add(request.Id, newTex);
        }
        else if (body is RealeseTexRequest)
        {
            var request = (RealeseTexRequest) body;
            Destroy(_textures[request.Id]);
            _textures.Remove(request.Id);
        }
        else throw new Exception("Неизвестный тип запроса");
    }

    // запрос на создание новой текстуры
    [Serializable]
    public class CreateTexRequest
    {
        public int Id;
        public int Width;
        public int Height;
        public byte[] Data;
    }

    // запрос на освобождение текстуры
    [Serializable]
    public class RealeseTexRequest
    {
        public int Id;

        public RealeseTexRequest(int id)
        {
            Id = id;
        }
    }
}
