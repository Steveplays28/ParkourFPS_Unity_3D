using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public InputField usernameField;
    public InputField IpField;

    public GameObject crosshair;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer()
    {
        string[] splitString = IpField.text.Split(new string[] { ":" }, StringSplitOptions.None);

        string _ip = splitString[0];
        int _port = int.Parse(splitString[1]);

        if (_ip.Length == 9 && _port.ToString().Length == 5)
        {
            startMenu.SetActive(false);
            usernameField.interactable = false;
            IpField.interactable = false;

            Client.instance.ConnectToServer(_ip, _port);
        }
    }
}
