using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public FromToFloat SpawnRate; // частота появления кружков
    public int[] Levels; // границы смены уровня сложности
    public float Difficulty { get; private set; } // сложность, (0, 1)
    public event Action DifficultyChanged;
    public int Points { get; private set; } // очки

    public int Time
    {
        get { return (int) UnityEngine.Time.time; }
    }
    public static GameManager I { get; private set; }
    
    void Awake()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one GameManager");
        I = this;
    }

    void Start ()
    {
        StartCoroutine("SpawnCoroutine");// запуск корутины для создания кружков
    }

    void FixedUpdate()
    {
        SendSetValuesRequest();
    }

    // корутина для создания кружков
    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            Instantiate(Scrapyard.I.Prefabs["Circle"]);
            yield return new WaitForSeconds(SpawnRate.Lerp(Difficulty)); // частота появления кружков зависит от сложности
        }
    }

    public void AddPoints(int inp)
    {
        for(int i = 0; i < Levels.Length; i++)
            if (Levels[i] > Points && Levels[i] <= Points + inp) // true если уровень сложности изменился
            {
                Difficulty = Mathf.Lerp(0, 1, (float) i/Levels.Count());
                if (DifficultyChanged != null) DifficultyChanged();
                break;
            }
        Points += inp;
    }

    // отправка клиентам запроса на установку очков и времени
    void SendSetValuesRequest(int clientId = -1)
    {
        Envelope env = new Envelope();
        env.Addressee = AddresseeType.GameClient;
        env.SenderInstanceId = this.GetInstanceID();
        using (var ms = new MemoryStream())
        {
            var msg = new GameClient.SetValuesRequest();
            msg.Time = Time;
            msg.Points = Points;
            (new BinaryFormatter()).Serialize(ms, msg);
            env.Data = ms.ToArray();
        }
        GameServer.I.Send(env, clientId);
    }
}
