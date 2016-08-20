using UnityEngine;
using System.Collections;

public class CircleBhv : MonoBehaviour
{
    public FromToFloat Speed;
    public FromToFloat Size;

    private float _speed;
    private float _size;

    void Start ()
    {
        float rndFloat = Random.value;
        _size = Size.Lerp(rndFloat); // получаем случайное значение размера
        transform.localScale *= _size;
        _speed = Speed.Lerp(1 - rndFloat); // значение скорости, соответствующее размеру
        // случайное расположение кружка вдоль верхней границы экрана
        float randPosAcrossScreen = 2 * (Random.value - 0.5f) * (Camera.main.aspect * Camera.main.orthographicSize - _size);
        transform.position = new Vector3(randPosAcrossScreen, Camera.main.orthographicSize - _size, 0);
    }

	void Update ()
    {
        transform.position -= new Vector3(0, _speed * Time.deltaTime, 0); // движение вниз
        if (transform.position.y < -Camera.main.orthographicSize + _size)
            Destroy(gameObject); // проверка на достижение нижней границы экрана
    }
}
