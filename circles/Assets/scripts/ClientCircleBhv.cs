using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class ClientCircleBhv : MonoBehaviour, IClientObject
{
    private bool _сoordsReceived; // были ли получены от сервера координаты
    private Vector3 _speed;
    void Start ()
    {
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        transform.position += _speed * Time.deltaTime; // движение вниз
    }

    public void ReceiveMessage(Envelope message)
    {
        // сравниваем тип сообщения с поддерживаемыми получателем
        var body = (new BinaryFormatter()).Deserialize(new MemoryStream(message.Data));
        if (body is SetVector3Request)
        {
            var request = (SetVector3Request)body;
            switch (request.ValueType)
            {
                case SetVector3Request.Type.Scale:
                    transform.localScale = new Vector3(request.Value[0], request.Value[1], request.Value[2]);
                    break;
                case SetVector3Request.Type.Position:
                    transform.position = new Vector3(request.Value[0], request.Value[1], request.Value[2]);
                    // костыль для избавления от мерцания кружков в центре экрана
                    // кружок начинает отображаться после того, как впервые получены координаты
                    if (_сoordsReceived) gameObject.GetComponent<Renderer>().enabled = true;
                    _сoordsReceived = true;
                    break;
                case SetVector3Request.Type.Speed:
                    _speed = new Vector3(request.Value[0], request.Value[1], request.Value[2]);
                    break;
            }    
        }
        else if (body is SetTextureRequest)
        {
            var request = (SetTextureRequest) body;
            GetComponent<Renderer>().material.mainTexture = ClientTextureManager.I.GetTexture(request.TextureId);
        }
        else if (body is DestroyRequest)
        {
            Destroy(gameObject);
        }
        else throw new Exception("Неизвестный тип запроса");
    }

    // запрос на установку скорости, размера или координат
    [Serializable]
    public class SetVector3Request
    {
        // Vector3 не сериализуемый, так что передаем вместо него массивы
        public float[] Value;
        public Type ValueType;
        public enum Type {Position, Speed, Scale}
    }

    // запрос на установку текстуры
    [Serializable]
    public class SetTextureRequest
    {
        public int TextureId;

        public SetTextureRequest(int textureId)
        {
            TextureId = textureId;
        }
    }

    [Serializable]
    public class DestroyRequest
    {
    }
}
