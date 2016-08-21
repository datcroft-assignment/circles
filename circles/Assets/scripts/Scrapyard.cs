using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scrapyard : MonoBehaviour
{
    public static Scrapyard I { get; private set; }
    public Dictionary<string, GameObject> Prefabs { get; private set; }

    void Awake ()
    {
        if (I != null) throw new Exception("more than one Scrapyard");
        I = this;
        Prefabs = new Dictionary<string, GameObject>();

        var clientCirclePrefab = Resources.Load("ClientCircle", typeof(GameObject)) as GameObject;
        if (clientCirclePrefab == null) throw new Exception(" не удалось загрузить ClientCircle");
        Prefabs.Add("ClientCircle", clientCirclePrefab);

        var circlePrefab = Resources.Load("Circle", typeof(GameObject)) as GameObject;
        if (circlePrefab == null) throw new Exception(" не удалось загрузить Circle");
        Prefabs.Add("Circle", circlePrefab);
    }
}
