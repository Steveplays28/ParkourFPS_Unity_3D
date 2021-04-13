using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public TMP_Text fullscreenModeDropdown;
    public TMP_Text resolutionText;
    public Dropdown qualityPresetDropdown;

    [Header("Fullscreen modes")]
    public int currentFullScreenModeId;
    public Dictionary<int, FullScreenMode> supportedFullscreenModes = new Dictionary<int, FullScreenMode>();

    [Header("Resolutions")]
    public int currentResolutionId;
    public Dictionary<int, Resolution> supportedResolutions = new Dictionary<int, Resolution>();

    [Header("Quality presets")]
    public int currentQualityPresetId;

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

    private void Start()
    {
        GetSupportedResolutions();
        GetSupportedFullscreenModes();
        GetSupportedQualityPresets();
    }

    #region Fullscreen mode
    private void GetSupportedFullscreenModes()
    {
        foreach (FullScreenMode fullScreenMode in Enum.GetValues(typeof(FullScreenMode)))
        {
            supportedFullscreenModes.Add((int)fullScreenMode, fullScreenMode);
        }
    }

    public void SetFullscreenMode(bool lower)
    {
        if (lower)
        {
            Screen.fullScreenMode = supportedFullscreenModes[currentFullScreenModeId - 1];
            currentFullScreenModeId -= 1;
        }
        else
        {
            Screen.fullScreenMode = supportedFullscreenModes[currentFullScreenModeId + 1];
            currentFullScreenModeId += 1;
        }

        string currentFullScreenMode = supportedFullscreenModes[currentFullScreenModeId].ToString();
        fullscreenModeDropdown.text = char.ToUpper(currentFullScreenMode.First()) + currentFullScreenMode.Substring(1).ToLower();
    }
    #endregion

    #region Resolutions
    private void GetSupportedResolutions()
    {
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            supportedResolutions[i] = (Resolution)Screen.resolutions.GetValue(i);
        }
    }

    public void SetResolution(bool lower)
    {
        if (lower)
        {
            Screen.SetResolution(supportedResolutions[currentResolutionId - 1].width, supportedResolutions[currentResolutionId - 1].height, Screen.fullScreen);
            currentResolutionId -= 1;

        }
        else
        {
            Screen.SetResolution(supportedResolutions[currentResolutionId + 1].width, supportedResolutions[currentResolutionId + 1].height, Screen.fullScreen);
            currentResolutionId += 1;
        }

        resolutionText.text = string.Concat(supportedResolutions[currentResolutionId].width.ToString(), "x", supportedResolutions[currentResolutionId].height.ToString(), " ", supportedResolutions[currentResolutionId].refreshRate.ToString());
    }
    #endregion

    #region Quality preset
    private void GetSupportedQualityPresets()
    {
        currentQualityPresetId = QualitySettings.GetQualityLevel();
    }

    public void SetQualityPreset(bool lower)
    {
        if (lower)
        {
            QualitySettings.DecreaseLevel();
            currentQualityPresetId -= 1;
        }
        else
        {
            QualitySettings.IncreaseLevel();
            currentQualityPresetId += 1;
        }
    }
    #endregion
}