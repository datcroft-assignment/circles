using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class ClientCircleBhv : MonoBehaviour, IClientObject
{
    private RenderTexture _rt;
    private bool _сoordsReceived; // были ли получены от сервера координаты
    void Start ()
    {
        _rt = TextureManager.I.GetRandomTexture(TextureSize.Mid);
        GetComponent<Renderer>().material.mainTexture = _rt;
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    void OnDestroy()
    {
        TextureManager.I.ReleaseTexture(_rt);
    }

    public void ReceiveMessage(Envelope message)
    {
        // сравниваем тип сообщения с поддерживаемыми получателем
        var body = (new BinaryFormatter()).Deserialize(new MemoryStream(message.Data));
        if (body is SetPosAndScaleRequest)
        {
            // костыль для избавления от мерцания кружков в центре экрана
            // кружок начинает отображаться после того, как впервые получены координаты
            if (_сoordsReceived) gameObject.GetComponent<Renderer>().enabled = true;
            _сoordsReceived = true;
            // устанавливаем размер и координаты
            var request = (SetPosAndScaleRequest)body;
            transform.position = new Vector3(request.Pos[0], request.Pos[1], request.Pos[2]);
            transform.localScale = new Vector3(request.Scale[0], request.Scale[1], request.Scale[2]);
            
        }
        else if (body is DestroyRequest)
        {
            Destroy(gameObject);
        }
        else throw new Exception("Неизвестный тип запроса");
    }

    [Serializable]
    public class SetPosAndScaleRequest
    {
        // Vector3 не сериализуемый, так что передаем вместо него массивы
        public float[] Pos;
        public float[] Scale;
    }

    [Serializable]
    public class DestroyRequest
    {
    }
}
