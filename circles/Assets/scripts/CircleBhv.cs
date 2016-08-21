using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class CircleBhv : MonoBehaviour
{
    public FromToFloat Speed;
    public FromToFloat Size;

    private float _speed;
    private float _size;
    private RenderTexture _rt;

    void Start ()
    {
        float rndFloat = Random.value;
        _size = Size.Lerp(rndFloat); // получаем случайное значение размера
        transform.localScale *= _size;
        _speed = Speed.Lerp(1 - rndFloat); // значение скорости, соответствующее размеру
        // случайное расположение кружка вдоль верхней границы экрана
        float randPosAcrossScreen = 2 * (Random.value - 0.5f) * (Camera.main.aspect * Camera.main.orthographicSize - _size);
        transform.position = new Vector3(randPosAcrossScreen, Camera.main.orthographicSize - _size, 0);

        // определяем размер текстуры
        var texSize = TextureSize.Small;
        if (rndFloat > 0.33)
            if (rndFloat > 0.66) texSize = TextureSize.Big;
            else texSize = TextureSize.Mid;
        _rt = TextureManager.I.GetRandomTexture(texSize); // получем текстуру из менеджера
        GetComponent<Renderer>().material.mainTexture = _rt;
        SendInstantiateRequest(); // просим всех клиентов создать экземпляры ClientCircle 
        GameServer.I.AfterNewClientConnected += OnAfterNewClientConnected; // подписываемся на подключение клиентов
    }

	void Update ()
    {
        transform.position -= new Vector3(0, _speed * Time.deltaTime, 0); // движение вниз

        if (Input.GetMouseButtonDown(0))// обработка клика мышкой
        {
            Vector3 cursorOnScene = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 diff = cursorOnScene - transform.position; // расстояние между центром кружка и курсором
            diff.z = 0;
            cursorOnScene.z = 0;
            if (diff.magnitude < _size) Destroy(gameObject);
        }

        if (transform.position.y < -Camera.main.orthographicSize + _size)
            Destroy(gameObject); // проверка на достижение нижней границы экрана
    }

    void FixedUpdate()
    {
        SendScaleAndPos(); // отправка координат и размера клиентам
    }

    void OnDestroy()
    {
        TextureManager.I.ReleaseTexture(_rt);
        SendDestroyRequest();
        GameServer.I.AfterNewClientConnected -= OnAfterNewClientConnected;
    }

    // запрос на создание экземпляра ClientCircle. С параметром по умолчанию рассылается всем клиентам.
    void SendInstantiateRequest(int clientId = -1)
    {
        Envelope env = new Envelope();
        env.Addressee = AddresseeType.GameClient;
        env.SenderInstanceId = this.GetInstanceID();
        using (var ms = new MemoryStream())
        {
            (new BinaryFormatter()).Serialize(ms, new GameClient.InstantiateRequest("ClientCircle"));
            env.Data = ms.ToArray();
        }
        GameServer.I.Send(env, clientId);
    }

    // отправка клиентам запроса на уничтожение объекта
    void SendDestroyRequest(int clientId = -1)
    {
        Envelope env = new Envelope();
        env.Addressee = AddresseeType.GameObject;
        env.SenderInstanceId = this.GetInstanceID();
        using (var ms = new MemoryStream())
        {
            (new BinaryFormatter()).Serialize(ms, new ClientCircleBhv.DestroyRequest());
            env.Data = ms.ToArray();
        }
        GameServer.I.Send(env, clientId);
    }

    // отправка координат и размера клиентам
    void SendScaleAndPos(int clientId = -1)
    {
        Envelope env = new Envelope();
        env.Addressee = AddresseeType.GameObject;
        env.SenderInstanceId = this.GetInstanceID();
        using (var ms = new MemoryStream())
        {
            var request = new ClientCircleBhv.SetPosAndScaleRequest();
            request.Scale = transform.localScale.ToArray();
            request.Pos = transform.position.ToArray();
            (new BinaryFormatter()).Serialize(ms, request);
            env.Data = ms.ToArray();
        }
        GameServer.I.Send(env, clientId);
    }

    void OnAfterNewClientConnected(int clientId)
    {
        SendInstantiateRequest(clientId);
        SendScaleAndPos(clientId);
    }
}
