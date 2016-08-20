using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float SpawnRate;
    public static GameManager I { get; private set; } 

    private GameObject _circlePrefab;
    void Awake()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one GameManager");
        I = this;
    }

    void Start ()
    {
        _circlePrefab = Resources.Load("Circle", typeof(GameObject)) as GameObject; // загрузка префаба кружка
        if (_circlePrefab == null) throw new Exception(" не удалось загрузить Circle");
        StartCoroutine("SpawnCoroutine");// запуск корутины для создания кружков
    }
	

	void Update ()
    {
	
	}

    // корутина для создания кружков
    IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            Instantiate(_circlePrefab);
            yield return new WaitForSeconds(SpawnRate);
        }
    }
}
