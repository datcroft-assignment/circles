using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClientConnectButtonBhv : MonoBehaviour
{
    public InputField IpField;
    public InputField PortField;
    private Button _button;
    private int _port;

    public void Awake()
    {
        _button = GetComponent<Button>();
    }

    void FixedUpdate()
    {
        try
        {
            _port = Convert.ToInt16(PortField.text);
            _button.interactable = true;
        }
        catch (FormatException)
        {
            _button.interactable = false;
        }
    }

    public void OnConnectClick()
    {
        GameClient.I.Connect(IpField.text, _port);
    }
}
