using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using UnityEditor;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Main Menu")]
    public GameObject mainMenu;
    public InputField usernameField;
    public InputField IpField;

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

    #region Menus
    /// <summary>Closes a menu.</summary>
    /// <param name="_menu">The menu to close.</param>
    public void CloseMenu(GameObject _menu)
    {
        _menu.SetActive(false);
    }

    /// <summary>Opens a menu.</summary>
    /// <param name="_menu">The menu to open.</param>
    public void OpenMenu(GameObject _menu)
    {
        _menu.SetActive(true);
    }

    /// <summary>Opens the pause menu.</summary>
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        GameManager.players[Client.instance.myId].gameObject.GetComponent<PlayerController>().isPaused = true;
    }

    /// <summary>Closes the pause menu.</summary>
    public void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        GameManager.players[Client.instance.myId].gameObject.GetComponent<PlayerController>().isPaused = false;
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

        Client.instance.ConnectToServer(_ip, _port);
    }

    public void OnConnected()
    {
        //Disable main menu
        mainMenu.SetActive(false);
        usernameField.interactable = false;
        IpField.interactable = false;
        Camera.main.gameObject.SetActive(false);

        //Enable in-game UI
        inGameUI.SetActive(true);
        healthBar.maxValue = GameManager.players[Client.instance.myId].maxHealth;
        healthBar.value = GameManager.players[Client.instance.myId].currentHealth;

        //Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DisconnectFromServer()
    {
        Client.instance.Disconnect();
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
