using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public string dataPath;

    [Header("Debug Menu")]
    public TMP_Text fpsCounter;
    public TMP_Text pingCounter;
    public int updateInterval = 30;
    private int frames = 0;
    public float ping;

    [Header("Main Menu")]
    public GameObject mainMenu;
    public GameObject title;
    public Vector2 titleDefaultPosition = new Vector3(0, -180);
    public Vector2 titlePositionInOptionsMenu = new Vector3(250, -180);
    public float titleMoveTime = 0.5f;
    public GameObject background;
    public TMP_InputField usernameField;
    public TMP_Text usernameCharactersLeftText;
    public TMP_InputField ipField;
    public Button quitGameButton;

    public float menuPopOutTime = 0.5f;
    public float menuPopInTime = 0.5f;

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

        dataPath = Application.persistentDataPath + "/Data.txt";
    }

    private void Start()
    {
        //QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = SettingsManager.instance.maxFps;

        // Try to read data file from disk
        try
        {
            // Open the data file if it exists
            if (File.Exists(dataPath))
            {
                // Set username and ip to last used
                usernameField.text = ReadLine(dataPath, 1);
                ipField.text = ReadLine(dataPath, 2);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Caught exception: " + e);
        }
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

    #region Files
    private string ReadLine(string filePath, int lineNumber)
    {
        try
        {
            if (lineNumber - 1 < File.ReadLines(filePath).Count())
            {
                return File.ReadLines(dataPath).Skip(lineNumber - 1).Take(1).First();
            }
            else
            {
                return "";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception caught: " + e);
            return "";
        }
    }

    private void WriteLine(string filePath, int lineNumber, string text)
    {
        try
        {
            List<string> lines = File.ReadLines(filePath).ToList();
            if (lines.Count < 1)
            {
                lines.Add("");
                lines.Add("");
            }
            else if (lines.Count < 2)
            {
                lines.Add("");
            }
            lines[lineNumber - 1] = text;

            File.WriteAllLines(dataPath, lines);
            return;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception caught: " + e);
            return;
        }
    }
    #endregion

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
    public void UsernameFieldSetCharactersLeft()
    {
        usernameCharactersLeftText.text = (32 - usernameField.text.Length).ToString();
    }

    public void SaveUsername()
    {
        try
        {
            if (!File.Exists(dataPath))
            {
                File.Create(dataPath);
            }

            WriteLine(dataPath, 1, usernameField.text);
        }
        catch (Exception e)
        {
            Debug.LogError("Caught exception: " + e);
        }
    }

    public void SaveIp()
    {
        try
        {
            if (!File.Exists(dataPath))
            {
                File.Create(dataPath);
            }

            WriteLine(dataPath, 2, ipField.text);
        }
        catch (Exception e)
        {
            Debug.LogError("Caught exception: " + e);
        }
    }

    /// <summary>Closes a menu.</summary>
    /// <param name="_menu">The menu to close.</param>
    public void CloseMenu(GameObject menu)
    {
        foreach (Button button in menu.GetComponentsInChildren<Button>())
        {
            if (button.interactable == true)
            {
                button.interactable = false;
            }
        }
        foreach (TMP_InputField inputField in menu.GetComponentsInChildren<TMP_InputField>())
        {
            if (inputField.interactable == true)
            {
                inputField.interactable = false;
            }
        }
        currentOpenMenu = null;
        menu.GetComponent<RectTransform>().DOScale(Vector3.zero, menuPopOutTime);
        StartCoroutine(WaitAndCloseMenu(menu));
    }

    private IEnumerator WaitAndCloseMenu(GameObject menu)
    {
        yield return new WaitForSeconds(menuPopOutTime);
        if (menu.activeInHierarchy)
        {
            menu.SetActive(false);
        }
    }

    /// <summary>Opens a menu.</summary>
    /// <param name="_menu">The menu to open.</param>
    public void OpenMenu(GameObject menu)
    {
        currentOpenMenu = menu;
        menu.SetActive(true);
        menu.GetComponent<RectTransform>().DOScale(Vector3.one, menuPopInTime);
        StartCoroutine(WaitAndOpenMenu(menu));
    }

    private IEnumerator WaitAndOpenMenu(GameObject menu)
    {
        yield return new WaitForSeconds(menuPopInTime);

        foreach (Button button in menu.GetComponentsInChildren<Button>())
        {
            if (button.interactable == false)
            {
                button.interactable = true;
            }
        }
        foreach (TMP_InputField inputField in menu.GetComponentsInChildren<TMP_InputField>())
        {
            if (inputField.interactable == false)
            {
                inputField.interactable = true;
            }
        }
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

    /// <summary>Opens the options menu.</summary>
    public void OpenOptionsMenu()
    {
        CloseMenu(currentOpenMenu);
        OpenMenu(optionsMenu);

        if (Client.instance.isConnected)
        {
            return;
        }
        else
        {
            title.GetComponent<RectTransform>().DOAnchorPos(titlePositionInOptionsMenu, titleMoveTime);
        }
    }

    /// <summary>Closes the options menu.</summary>
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
            title.GetComponent<RectTransform>().DOAnchorPos(titleDefaultPosition, titleMoveTime);
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

        if (ipField.text.ToLower() == "localhost")
        {
            ip = "127.0.0.1";
            port = 26950;
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
        background.SetActive(false);

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
        background.SetActive(true);
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
