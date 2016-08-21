using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float SpawnRate;
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

    // корутина для создания кружков
    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            Instantiate(Scrapyard.I.Prefabs["Circle"]);
            yield return new WaitForSeconds(SpawnRate);
        }
    }
}
