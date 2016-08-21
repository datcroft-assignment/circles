using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ClientUiBhv : MonoBehaviour
{
    public Text PointsText;
    public Text TimeText;

    void FixedUpdate()
    {
        PointsText.text = GameClient.I.Points.ToString();
        TimeText.text = GameClient.I.Time.ToString();
    }
}
