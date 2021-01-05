﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

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
        startMenu.SetActive(false);
        usernameField.interactable = false;
        IpField.interactable = false;
        Camera.main.gameObject.SetActive(false);

        //Enable in-game menus
        inGameUI.SetActive(true);
        for (int i = 0; i < inGameUI.transform.childCount; i++)
        {
            inGameUI.transform.GetChild(i).gameObject.SetActive(true);
        }
        healthBar.maxValue = GameManager.players[Client.instance.myId].maxHealth;
        healthBar.value = GameManager.players[Client.instance.myId].health;

        //Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DisconnectFromServer()
    {
        Client.instance.Disconnect();
    }

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

        DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 1 - GameManager.players[Client.instance.myId].health / 100, 0.5f);
    }
}
