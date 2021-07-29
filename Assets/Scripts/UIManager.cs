using DG.Tweening;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Variables
    public static UIManager instance;

    [Space]
    public string dataPath;
    public Camera normalCamera;
    public Camera UICamera;
    public GameObject currentOpenMenu;

    [Foldout("Debug Menu")]
    public TMP_Text fpsCounter;

    [Space]
    [Foldout("Debug Menu")]
    public TMP_Text pingCounter;
    [Foldout("Debug Menu")]
    public float ping;

    [Space]
    [SerializeField]
    [Foldout("Debug Menu")]
    private int frames = 0;
    [Foldout("Debug Menu")]
    public int updateInterval = 30;

    [Foldout("Main Menu")]
    public GameObject mainMenu;

    [Space]
    [Foldout("Main Menu")]
    public GameObject title;
    [Foldout("Main Menu")]
    public Vector2 titleDefaultPosition = new Vector3(0, -180);
    [Foldout("Main Menu")]
    public Vector2 titlePositionInOptionsMenu = new Vector3(250, -180);
    [Foldout("Main Menu")]
    public float titleMoveTime = 0.5f;

    [Space]
    [Foldout("Main Menu")]
    public GameObject background;

    [Space]
    [Foldout("Main Menu")]
    public TMP_InputField usernameField;
    [Foldout("Main Menu")]
    public TMP_Text usernameCharactersLeftText;

    [Space]
    [Foldout("Main Menu")]
    public TMP_InputField ipField;

    [Space]
    [Foldout("Main Menu")]
    public Button quitGameButton;

    [Foldout("Main Menu")]
    public float menuPopOutTime = 0.5f;
    [Foldout("Main Menu")]
    public float menuPopInTime = 0.5f;

    [Foldout("Pause Menu")]
    public GameObject pauseMenu;
    [Foldout("Pause Menu")]
    public Button disconnectButton;

    [Foldout("Options Menu")]
    public GameObject optionsMenu;

    [Foldout("HUD")]
    public GameObject inGameUI;
    [Foldout("HUD")]
    public Slider healthBar;
    [Foldout("HUD")]
    public TMP_Text weaponName;
    [Foldout("HUD")]
    public TMP_Text ammoCounter;
    [Foldout("HUD")]
    public Volume volume;
    [Foldout("HUD")]
    public GameObject scoreboard;
    [Foldout("HUD")]
    public bool isScoreboardActive;

    [Foldout("Notifications")]
    public GameObject notificationPrefab;
    [Foldout("Notifications")]
    public int maxNotifications = 10;
    public Dictionary<int, Notification> notifications = new Dictionary<int, Notification>();
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Application.targetFrameRate = SettingsManager.instance.currentFpsCap;

        GameManager.instance.MainMenuLoaded += OnMainMenuLoaded;
        GameManager.instance.PlayerConnected += OnConnected;
        GameManager.instance.PlayerDisconnected += OnDisconnected;

        dataPath = Application.persistentDataPath + "/Data.txt";
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

    private void OnDestroy()
    {
        GameManager.instance.MainMenuLoaded -= OnMainMenuLoaded;
        GameManager.instance.PlayerConnected -= OnConnected;

        ResetPostProcessingVolume();
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

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            DisplayNotification(@"you pressed \");
        }
    }

    public void OnMainMenuLoaded(AsyncOperation asyncOperation)
    {
        normalCamera.GetUniversalAdditionalCameraData().cameraStack.Add(UICamera);
        volume = FindObjectOfType<Volume>();
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
            if (!File.Exists(dataPath))
            {
                File.Create(dataPath);
            }

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
        WriteLine(dataPath, 1, usernameField.text);
    }

    public void SaveIp()
    {
        WriteLine(dataPath, 2, ipField.text);
    }

    /// <summary>Closes a menu.</summary>
    /// <param name="menu">The menu to close.</param>
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
        menu.GetComponent<RectTransform>().DOScale(Vector3.zero, menuPopOutTime).onComplete = () =>
        {
            if (menu.activeInHierarchy)
            {
                menu.SetActive(false);
            }
        };
    }

    /// <summary>Opens a menu.</summary>
    /// <param name="menu">The menu to open.</param>
    public void OpenMenu(GameObject menu)
    {
        currentOpenMenu = menu;
        menu.SetActive(true);
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(menu.GetComponentInChildren<Button>().gameObject);
        menu.GetComponent<RectTransform>().DOScale(Vector3.one, menuPopInTime).onComplete = () =>
        {
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
        };
    }

    /// <summary>Opens the pause menu.</summary>
    public void OpenPauseMenu()
    {
        OpenMenu(pauseMenu);
        background.GetComponentInChildren<Image>().DOFade(1, menuPopInTime);

        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();

            player.playerController.isPaused = true;
            player.cameraController.ToggleCursorMode();
        }
    }

    /// <summary>Closes the pause menu.</summary>
    public void ClosePauseMenu()
    {
        CloseMenu(pauseMenu);
        background.GetComponentInChildren<Image>().DOFade(0, menuPopOutTime);

        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();

            player.playerController.isPaused = false;
            player.cameraController.ToggleCursorMode();
        }
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
        EditorApplication.ExitPlaymode();
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
        DiscordController.instance.currentPartySize = GameManager.gameObjects.Count;
        DiscordController.instance.UpdateActivity();

        if (GameManager.gameObjects.TryGetValue(playerId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            DisplayNotification(player.username + " has joined the game");
        }

        if (playerId != Client.instance.myId)
        {
            return;
        }

        // Disable main menu
        normalCamera.gameObject.SetActive(false);
        CloseMenu(mainMenu);
        CloseMenu(title);
        CloseMenu(quitGameButton.gameObject);
        background.GetComponentInChildren<Image>().DOFade(0, menuPopOutTime);

        // Enable in-game UI
        inGameUI.SetActive(true);
        pingCounter.gameObject.SetActive(true);
        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject fetchedGameObjectTwo))
        {
            PlayerManager player = fetchedGameObjectTwo.GetComponent<PlayerManager>();

            healthBar.maxValue = player.maxHealth;
            healthBar.value = player.currentHealth;
        }
        UpdateAmmo();
        UpdateWeapon();

        // Hide cursor
        CustomMouseCursor.instance.HideCursor();

        // Discord RPC
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

    public void OnDisconnected(int playerId, string reason)
    {

        if (GameManager.gameObjects.TryGetValue(playerId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            DisplayNotification(player.username + " has left the game. Reason: " + reason);
        }

        if (Client.instance.myId != playerId)
        {
            return;
        }

        // Disable in-game UI
        background.GetComponentInChildren<Image>().DOFade(1, menuPopInTime);
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
    public void DisplayNotification(string text)
    {
        Notification notification = Instantiate(notificationPrefab, gameObject.transform).GetComponent<Notification>();
        notifications.Add(notifications.Count, notification);
        notification.Initialise(text);

        for (int i = 0; i < notifications.Count; i++)
        {
            GameObject fetchedNotification = notifications[i].gameObject;
            Notification fetchedNotifComp = fetchedNotification.GetComponent<Notification>();

            fetchedNotification.GetComponent<RectTransform>().DOAnchorPosY(fetchedNotifComp.currentPosition.y - fetchedNotifComp.moveDownAmount, fetchedNotifComp.lerpDuration);
            fetchedNotifComp.currentPosition -= new Vector2(0, fetchedNotifComp.moveDownAmount);

            if (i > maxNotifications)
            {
                fetchedNotification.GetComponent<TMP_Text>().DOFade(0, fetchedNotifComp.lerpDuration);
            }
        }
    }

    public void ToggleScoreboard(bool enable)
    {
        volume.sharedProfile.TryGet(out DepthOfField depthOfField);

        if (enable)
        {
            scoreboard.GetComponent<RectTransform>().localScale = Vector3.one;
            depthOfField.focusDistance.value = 0.1f;

            isScoreboardActive = true;
        }
        else
        {
            scoreboard.GetComponent<RectTransform>().localScale = Vector3.zero;
            depthOfField.focusDistance.value = 15;

            isScoreboardActive = false;
        }
    }

    public void ResetPostProcessingVolume()
    {
        volume.sharedProfile.TryGet(out DepthOfField depthOfField);
        depthOfField.focusDistance.value = 15;

        volume.profile.TryGet(out Vignette vignette);
        vignette.intensity.value = 0;
    }

    public void UpdateWeapon()
    {
        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            weaponName.text = player.weaponManager.weaponName;
        }
    }

    public void UpdateAmmo()
    {
        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            ammoCounter.text = string.Concat(player.weaponManager.currentAmmo.ToString(), "/", player.weaponManager.maxAmmo.ToString());
        }
    }

    public void UpdateHealthEffect()
    {
        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            volume.profile.TryGet(out Vignette vignette);
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1 - player.currentHealth / 100, 0.5f);
        }
    }
    #endregion
}
