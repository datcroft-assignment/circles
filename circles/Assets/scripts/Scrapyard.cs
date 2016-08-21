using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scrapyard : MonoBehaviour
{
    public GameObject LoadingPlug;
    public GameObject MenuPanel;

    public static Scrapyard I { get; private set; }
    public Dictionary<string, GameObject> Prefabs { get; private set; }
    public Dictionary<string, Material> Materials { get; private set; }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    IEnumerator Start ()
    {
        if (I != null) throw new Exception("more than one Scrapyard");
        I = this;
        Prefabs = new Dictionary<string, GameObject>();
        Materials = new Dictionary<string, Material>();

        string URL="";
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            URL = "ftp://syslina:a5f96b25@ftp.drivehq.com/windows/circles_bundle";
        else if(Application.platform == RuntimePlatform.Android)
            URL = "ftp://syslina:a5f96b25@ftp.drivehq.com/android/circles_bundle";
        using (WWW www = new WWW(URL))
        {
            yield return www;         
            if (www.error != null)
                throw new Exception("WWW download had an error:" + www.error);
            AssetBundle bundle = www.assetBundle;

            Prefabs.Add("ClientCircle", (GameObject)bundle.LoadAsset("ClientCircle"));
            Prefabs.Add("Circle", (GameObject)bundle.LoadAsset("Circle"));
            Materials.Add("GradientMat", (Material)bundle.LoadAsset("GradientMat"));

            bundle.Unload(false);
            MenuPanel.SetActive(true);
            LoadingPlug.SetActive(false);
        }

        //var clientCirclePrefab = Resources.Load("ClientCircle", typeof(GameObject)) as GameObject;
        //if (clientCirclePrefab == null) throw new Exception("не удалось загрузить ClientCircle");
        //Prefabs.Add("ClientCircle", clientCirclePrefab);

        //var circlePrefab = Resources.Load("Circle", typeof(GameObject)) as GameObject;
        //if (circlePrefab == null) throw new Exception("не удалось загрузить Circle");
        //Prefabs.Add("Circle", circlePrefab);

        //var gradientMat = Resources.Load("GradientMat", typeof(Material)) as Material;
        //if (gradientMat == null) throw new Exception("не удалось загрузить GradientMat");
        //Materials.Add("GradientMat", gradientMat);
    }
}
