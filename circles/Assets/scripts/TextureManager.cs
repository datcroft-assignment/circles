using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TextureManager : MonoBehaviour
{ 
    public int Count; // максимальное число текстур каждого размера

    private Dictionary<RenderTexture, int> _textures; // ассоциативный массив с текстурами для подсчета ссылок
    private Material _gradientMat; // материал для генерации текстур

    public static TextureManager I { get; private set; }
    void Awake()
    {
        // эрзац-синглтон
        if (I != null) throw new Exception("more than one TextureManager");
        I = this;
        _textures = new Dictionary<RenderTexture, int>();
        _gradientMat = Resources.Load("GradientMat", typeof(Material)) as Material; // загрузка материала
        if (_gradientMat == null) throw new Exception(" не удалось загрузить GradientMat");
    }

    void OnGUI()
    {
        // показываем число текстур в _textures
        GUILayout.BeginArea(new Rect(0, 0, 200, 40));
        GUILayout.TextArea("Объектов в _textures: " + _textures.Count);
        GUILayout.EndArea();
    }

    // для получения ссылок на текстуры клиентскими классами
    public RenderTexture GetRandomTexture(TextureSize size)
    {
        // получаем подмножество текстур подходящего размера
        var texturesOfSize = _textures.Where(cur => cur.Key.width == (int) size); 
        RenderTexture res = null;

        // если текстур меньше максимального, создаем новую
        if (texturesOfSize.Count() < Count) 
        {
            res = GenTexture(size);
            _textures.Add(res, 1);
            return res;
        }
        // иначе берем ту, у которой наименьшее число ссылок
        res = texturesOfSize.OrderBy(cur => cur.Value).First().Key;
        ++_textures[res];
        return res;
    }

    // для освобождения текстур
    public void ReleaseTexture(RenderTexture inp)
    {
        // уменьшаем счетчик. Если не осталось ссылок на текстуру - удаляем её.
        if (--_textures[inp] == 0)
        {
            _textures.Remove(inp);
            Destroy(inp);
        };
    }

    RenderTexture GenTexture(TextureSize size)
    {
        RandomValuesToGenMaterial(); // передаем в шейдер случайные значения
        var res = new RenderTexture((int) size, (int)size, 0); // создаем текстуру
        Graphics.Blit(null, res, _gradientMat); // рендерим в неё квад с градиентом
        return res;
    }

    void RandomValuesToGenMaterial()
    {
        _gradientMat.SetVector("_Dir", new Vector4(UnityEngine.Random.value-0.5f,
                                                   UnityEngine.Random.value-0.5f));
        _gradientMat.SetColor("_ColorA", new Color(UnityEngine.Random.value,
                                                  UnityEngine.Random.value,
                                                  UnityEngine.Random.value));
        _gradientMat.SetColor("_ColorB", new Color(UnityEngine.Random.value,
                                                  UnityEngine.Random.value,
                                                  UnityEngine.Random.value));
    }
}

public enum TextureSize {Big = 256, Mid = 128, Small = 64}
