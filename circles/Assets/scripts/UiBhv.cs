using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiBhv : MonoBehaviour
{
    public Text PointsText;
    public Text TimeText;
	
	void FixedUpdate ()
	{
	    PointsText.text = GameManager.I.Points.ToString();
        TimeText.text = GameManager.I.Time.ToString();
    }
}
