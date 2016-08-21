using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class TextureManager : MonoBehaviour
{ 
    public int Count; // максимальное число текстур каждого размера

    private Dictionary<Texture2D, int> _textures; // ассоциативный массив с текстурами для подсчета ссылок

    public static TextureManager I { get; private set; }
    void Awake()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one TextureManager");
        I = this;
        _textures = new Dictionary<Texture2D, int>();
    }

    void Start()
    {
        GameServer.I.NewClientConnected += OnNewClientConnected; // подписка на подключение новых клиентов
        GameManager.I.DifficultyChanged += OnDifficultyChanged; // подписка на изменение сложности
    }

    //void OnGUI()
    //{
    //    // показываем число текстур в _textures
    //    GUILayout.BeginArea(new Rect(0, 0, 200, 40));
    //    GUILayout.TextArea("Объектов в _textures: " + _textures.Count);
    //    GUILayout.EndArea();
    //}

    void OnDestroy()
    {
        GameServer.I.NewClientConnected -= OnNewClientConnected;
        GameManager.I.DifficultyChanged -= OnDifficultyChanged;
    }

    // для получения ссылок на текстуры клиентскими классами
    public Texture2D GetRandomTexture(TextureSize size)
    {
        // получаем подмножество текстур подходящего размера
        var texturesOfSize = _textures.Where(cur => cur.Key.width == (int) size);
        Texture2D res = null;

        if (texturesOfSize.Count() < Count) // если текстур меньше максимального
        {
            res = GenTexture(size); // создаем новую текстуру
            SendCreateTextureRequest(res); // просим клиентов создать текстуры
            // добавляем в коллекцию с начальным значением счетчика 2, чтобы текстура не удалялась
            // до смены уровня сложности даже если её никто не использует
            _textures.Add(res, 2); 
            return res;
        }
        // иначе берем ту, у которой наименьшее число ссылок
        res = texturesOfSize.OrderBy(cur => cur.Value).First().Key;
        ++_textures[res];
        return res;
    }

    // для освобождения текстур
    public void ReleaseTexture(Texture2D inp)
    {
        // уменьшаем счетчик. Если не осталось ссылок на текстуру - удаляем её.
        if (--_textures[inp] == 0)
        {
            _textures.Remove(inp);
            SendReleaseTextureRequest(inp);
            Destroy(inp);
        };
    }

    Texture2D GenTexture(TextureSize size)
    {
        RandomValuesToGenMaterial(); // передаем в шейдер случайные значения
        var rt = new RenderTexture((int) size, (int)size, 0); // создаем текстуру
        Graphics.Blit(null, rt, Scrapyard.I.Materials["GradientMat"]); // рендерим в неё квад с градиентом
        Texture2D res = new Texture2D(rt.width, rt.height); // копируем в Texture2D и возвращаем
        RenderTexture.active = rt;
        res.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        res.Apply();
        RenderTexture.active = null;
        rt.Release();
        return res;
    }

    // запрос клиентам на создание копий текстуры. Параметр по умолчанию - отправка всем клиентам
    void SendCreateTextureRequest(Texture2D tex, int clientId = -1)
    {
        Envelope env = new Envelope();
        env.Addressee = AddresseeType.ClientTextureManager;
        env.SenderInstanceId = this.GetInstanceID();
        using (var ms = new MemoryStream())
        {
            var request = new ClientTextureManager.CreateTexRequest();
            request.Id = tex.GetInstanceID();
            request.Data = tex.GetRawTextureData(); // передаем содержимое текстуры в виде массива байтов
            request.Width = tex.width;
            request.Height = tex.height;
            (new BinaryFormatter()).Serialize(ms, request);
            env.Data = ms.ToArray();
        }
        GameServer.I.Send(env, clientId);
    }

    // запрос клиентам на удаление текстуры 
    void SendReleaseTextureRequest(Texture2D tex, int clientId = -1)
    {
        Envelope env = new Envelope();
        env.Addressee = AddresseeType.ClientTextureManager;
        env.SenderInstanceId = this.GetInstanceID();
        using (var ms = new MemoryStream())
        {
            (new BinaryFormatter()).Serialize(ms, new ClientTextureManager.RealeseTexRequest(tex.GetInstanceID()));
            env.Data = ms.ToArray();
        }
        GameServer.I.Send(env, clientId);
    }

    void RandomValuesToGenMaterial()
    {
        Scrapyard.I.Materials["GradientMat"].SetVector("_Dir", new Vector4(UnityEngine.Random.value-0.5f,
                                                                           UnityEngine.Random.value-0.5f));
        Scrapyard.I.Materials["GradientMat"].SetColor("_ColorA", new Color(UnityEngine.Random.value,
                                                                           UnityEngine.Random.value,
                                                                           UnityEngine.Random.value));
        Scrapyard.I.Materials["GradientMat"].SetColor("_ColorB", new Color(UnityEngine.Random.value,
                                                                           UnityEngine.Random.value,
                                                                           UnityEngine.Random.value));
    }

    void OnNewClientConnected(int clientId)
    {
        // передаем новому клиенту все текстуры
        foreach (Texture2D cur in _textures.Keys) SendCreateTextureRequest(cur, clientId);
    }

    void OnDifficultyChanged()
    {
        // уменьшаем значение счетчика. Неиспользуемые текстуры будут удалены сразу, используемые останутся
        // пока их не освободят использующие их объекты
        foreach (var key in _textures.Keys.ToList()) ReleaseTexture(key);
    }
}

public enum TextureSize {Big = 256, Mid = 128, Small = 64}
