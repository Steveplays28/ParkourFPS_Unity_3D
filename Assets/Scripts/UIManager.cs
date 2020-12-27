using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Connect")]
    public GameObject startMenu;
    public InputField usernameField;
    public InputField IpField;

    [Header("Disconnect")]
    public Button disconnectButton;

    [Header("In-game")]
    public Image crosshair;
    public Slider healthBar;

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
        string _ip;
        int _port;

        if (string.IsNullOrEmpty(IpField.text))
        {
            _ip = "127.0.0.1";
            _port = 26950;
        }
        else
        {
            if (IpField.text.Contains(":"))
            {
                string[] splitString = IpField.text.Split(new string[] { ":" }, StringSplitOptions.None);

                _ip = splitString[0];
                _port = int.Parse(splitString[1]);
            }
            else
            {
                return;
            }
        }

        if (_ip.Length == 9 && _port.ToString().Length == 5)
        {
            Client.instance.ConnectToServer(_ip, _port);
        }
    }

    public void OnConnected()
    {
        //Disable main menu
        startMenu.SetActive(false);
        usernameField.interactable = false;
        IpField.interactable = false;
        Camera.main.gameObject.SetActive(false);

        //Enable in-game menus
        disconnectButton.gameObject.SetActive(true);
        crosshair.gameObject.SetActive(true);
        healthBar.gameObject.SetActive(true);
        healthBar.maxValue = GameManager.players[Client.instance.myId].maxHealth;
        healthBar.value = GameManager.players[Client.instance.myId].health;
    }

    public void DisconnectFromServer()
    {
        Client.instance.Disconnect();
    }
}
