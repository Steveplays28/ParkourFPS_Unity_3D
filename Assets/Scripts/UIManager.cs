using DG.Tweening;
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Debug Menu")]
    public TMP_Text fpsCounter;
    public TMP_Text pingCounter;
    public int updateInterval = 30;
    private int frames = 0;
    public float ping;

    [Header("Main Menu")]
    public GameObject mainMenu;
    public TMP_InputField usernameField;
    public TMP_Text usernameCharactersLeftText;
    public TMP_InputField ipField;
    public Button quitGameButton;

    public static string lastUsername;
    public static string lastIp;
    public static string lastPort;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button disconnectButton;

    [Header("Options Menu")]
    public GameObject optionsMenu;

    [Header("In-game")]
    public GameObject inGameUI;
    public Slider healthBar;
    public Text weaponName;
    public Text ammoCounter;
    public Volume volume;

    public GameObject currentOpenMenu;

    [Header("Settings")]
    public int maxFps = 60;

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

        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        usernameField.onValueChanged.AddListener(delegate { UsernameFieldOnValueChanged(); });
        quitGameButton.onClick.AddListener(delegate { QuitGame(); });

        usernameField.text = lastUsername;
        ipField.text = string.Concat(lastIp, lastPort);

        //QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = maxFps;
    }

    private void Update()
    {
        frames++;

        if (frames == updateInterval)
        {
            frames = 0;
            UpdateFPSCounter();

            if (Client.instance.isConnected)
            {
                ping = Mathf.RoundToInt(Time.realtimeSinceStartup);
                ClientSend.Ping();
            }
        }
    }

    #region Debug menu
    private void UpdateFPSCounter()
    {
        fpsCounter.text = Mathf.RoundToInt(1 / Time.deltaTime).ToString() + " fps";
    }

    public void UpdatePingCounter()
    {
        ping = Mathf.Round(Time.realtimeSinceStartup) - ping;
        pingCounter.text = ping + " ms ping";
    }
    #endregion

    #region Menus
    private void UsernameFieldOnValueChanged()
    {
        usernameCharactersLeftText.text = (32 - usernameField.text.Length).ToString();
    }

    /// <summary>Closes a menu.</summary>
    /// <param name="_menu">The menu to close.</param>
    public void CloseMenu(GameObject menu)
    {
        menu.SetActive(false);
        currentOpenMenu = null;
    }

    /// <summary>Opens a menu.</summary>
    /// <param name="_menu">The menu to open.</param>
    public void OpenMenu(GameObject menu)
    {
        currentOpenMenu = menu;
        menu.SetActive(true);
    }

    /// <summary>Opens the pause menu.</summary>
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        currentOpenMenu = pauseMenu;
        GameManager.players[Client.instance.myId].gameObject.GetComponent<PlayerController>().isPaused = true;
        GameManager.players[Client.instance.myId].gameObject.GetComponentInChildren<CameraController>().ToggleCursorMode();
    }

    /// <summary>Closes the pause menu.</summary>
    public void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        currentOpenMenu = null;
        GameManager.players[Client.instance.myId].gameObject.GetComponent<PlayerController>().isPaused = false;
        GameManager.players[Client.instance.myId].gameObject.GetComponentInChildren<CameraController>().ToggleCursorMode();
    }

    /// <summary>Closes the options menu and opens the correct menu.</summary>
    public void CloseOptionsMenu()
    {
        CloseMenu(optionsMenu);

        if (Client.instance.isConnected)
        {
            OpenMenu(pauseMenu);
        }
        else
        {
            OpenMenu(mainMenu);
        }
    }

    /// <summary>Quits the game.</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Attempts to connect to the server.</summary>
    public void ConnectToServer()
    {
        string ip;
        int port;

        if (string.IsNullOrEmpty(usernameField.text))
        {
            return;
        }
        lastUsername = usernameField.text;

        if (string.IsNullOrEmpty(ipField.text) || ipField.text.ToLower() == "localhost")
        {
            ip = "127.0.0.1";
            port = 26950;

            lastIp = "localhost";
        }
        else
        {
            if (ipField.text.Contains(":"))
            {
                string[] splitString = ipField.text.Split(new string[] { ":" }, StringSplitOptions.None);

                ip = splitString[0];
                port = int.Parse(splitString[1]);

                if (ip == "" || port.ToString() == "")
                {
                    return;
                }

                lastIp = ip;
                lastPort = string.Concat(":", port.ToString());
            }
            else
            {
                return;
            }
        }

        Client.instance.ConnectToServer(ip, port);
    }

    public void OnConnected(int playerId)
    {
        DiscordController.instance.currentPartySize = GameManager.players.Count;
        DiscordController.instance.UpdateActivity();

        if (playerId != Client.instance.myId)
        {
            return;
        }

        //Disable main menu
        CloseMenu(mainMenu);
        usernameField.interactable = false;
        ipField.interactable = false;
        Camera.main.gameObject.SetActive(false);

        //Enable in-game UI
        inGameUI.SetActive(true);
        pingCounter.gameObject.SetActive(true);
        healthBar.maxValue = GameManager.players[Client.instance.myId].maxHealth;
        healthBar.value = GameManager.players[Client.instance.myId].currentHealth;
        UpdateAmmo();
        UpdateWeapon();

        //Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //Discord RPC
        DiscordController.instance.state = "In a game";
        DiscordController.instance.details = "Team Deathmatch | n - n";
        DiscordController.instance.largeImageTooltip = "Map: " + SceneManager.GetActiveScene().name;
        DiscordController.instance.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DiscordController.instance.UpdateActivity();
    }

    public void DisconnectFromServer()
    {
        Client.instance.Disconnect(false);
    }

    public void OnDisconnected()
    {
        // Disable in-game UI
        inGameUI.SetActive(false);
        pingCounter.gameObject.SetActive(false);
        CloseMenu(pauseMenu);
        OpenMenu(mainMenu);
        usernameField.interactable = true;
        ipField.interactable = true;

        // Discord RPC
        DiscordController.instance.state = "In main menu";
        DiscordController.instance.details = "";
        DiscordController.instance.largeImageTooltip = "";
        DiscordController.instance.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        DiscordController.instance.UpdateActivity();
    }
    #endregion

    #region HUD
    public void UpdateWeapon()
    {
        weaponName.text = GameManager.players[Client.instance.myId].weaponManager.weaponName;
    }

    public void UpdateAmmo()
    {
        ammoCounter.text = string.Concat(GameManager.players[Client.instance.myId].weaponManager.currentAmmo.ToString(), "/", GameManager.players[Client.instance.myId].weaponManager.maxAmmo.ToString());
    }

    public void UpdateHealthEffect()
    {
        Vignette vignette;
        volume.profile.TryGet(out vignette);

        DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1 - GameManager.players[Client.instance.myId].currentHealth / 100, 0.5f);
    }
    #endregion
}
