using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public TMP_Text fullscreenModeText;
    public TMP_Text resolutionText;
    public TMP_Text qualityPresetText;

    [Header("Fullscreen modes")]
    public int currentFullScreenModeId;
    public Dictionary<int, FullScreenMode> supportedFullscreenModes = new Dictionary<int, FullScreenMode>();
    private Dictionary<FullScreenMode, string> fullscreenModeNames = new Dictionary<FullScreenMode, string>()
    {
        { FullScreenMode.ExclusiveFullScreen, "Exclusive fullscreen" },
        { FullScreenMode.FullScreenWindow, "Windowed fullscreen" },
        { FullScreenMode.MaximizedWindow, "Maximized window" },
        { FullScreenMode.Windowed, "Windowed" }
    };

    [Header("Resolutions")]
    public int currentResolutionId;
    public Dictionary<int, Resolution> supportedResolutions = new Dictionary<int, Resolution>();

    [Header("Quality presets")]
    public int currentQualityPresetId;
    public Dictionary<int, string> supportedQualityPresets = new Dictionary<int, string>();

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
        int index = 0;

        for (int i = Enum.GetValues(typeof(FullScreenMode)).Length - 1; i >= 0; i--)
        {
            supportedFullscreenModes.Add(index, (FullScreenMode)i);
            index++;
        }

        currentFullScreenModeId = supportedFullscreenModes.FirstOrDefault(x => x.Value.Equals(Screen.fullScreenMode)).Key;
        string currentFullScreenMode = supportedFullscreenModes[currentFullScreenModeId].ToString();
        fullscreenModeText.text = char.ToUpper(currentFullScreenMode.First()) + currentFullScreenMode.Substring(1).ToLower();
    }

    public void SetFullscreenMode(bool lower)
    {
        if (lower && supportedFullscreenModes.ContainsKey(currentFullScreenModeId - 1))
        {
            Screen.fullScreenMode = supportedFullscreenModes[currentFullScreenModeId - 1];
            currentFullScreenModeId -= 1;
        }
        else if (lower == false && supportedFullscreenModes.ContainsKey(currentFullScreenModeId + 1))
        {
            Screen.fullScreenMode = supportedFullscreenModes[currentFullScreenModeId + 1];
            currentFullScreenModeId += 1;
        }
        else
        {
            return;
        }

        fullscreenModeText.text = fullscreenModeNames[supportedFullscreenModes[currentFullScreenModeId]];
    }
    #endregion

    #region Resolutions
    private void GetSupportedResolutions()
    {
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            supportedResolutions[i] = (Resolution)Screen.resolutions.GetValue(i);
        }

        currentResolutionId = supportedResolutions.FirstOrDefault(x => x.Value.Equals(Screen.currentResolution)).Key;
        resolutionText.text = string.Concat(supportedResolutions[currentResolutionId].width.ToString(), "x", supportedResolutions[currentResolutionId].height.ToString(), " ", supportedResolutions[currentResolutionId].refreshRate.ToString(), "Hz");
    }

    public void SetResolution(bool lower)
    {
        if (lower && supportedResolutions.ContainsKey(currentResolutionId - 1))
        {
            Screen.SetResolution(supportedResolutions[currentResolutionId - 1].width, supportedResolutions[currentResolutionId - 1].height, Screen.fullScreen);
            currentResolutionId -= 1;

        }
        else if (lower == false && supportedResolutions.ContainsKey(currentResolutionId + 1))
        {
            Screen.SetResolution(supportedResolutions[currentResolutionId + 1].width, supportedResolutions[currentResolutionId + 1].height, Screen.fullScreen);
            currentResolutionId += 1;
        }
        else
        {
            return;
        }

        resolutionText.text = string.Concat(supportedResolutions[currentResolutionId].width.ToString(), "x", supportedResolutions[currentResolutionId].height.ToString(), " ", supportedResolutions[currentResolutionId].refreshRate.ToString(), "Hz");
    }
    #endregion

    #region Quality preset
    private void GetSupportedQualityPresets()
    {
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            supportedQualityPresets.Add(i, QualitySettings.names[i]);
        }

        currentQualityPresetId = QualitySettings.GetQualityLevel();
        qualityPresetText.text = supportedQualityPresets[currentQualityPresetId];
    }

    public void SetQualityPreset(bool lower)
    {
        if (lower && supportedQualityPresets.ContainsKey(currentQualityPresetId - 1))
        {
            QualitySettings.DecreaseLevel();
            currentQualityPresetId -= 1;
        }
        else if (lower == false && supportedQualityPresets.ContainsKey(currentQualityPresetId + 1))
        {
            QualitySettings.IncreaseLevel();
            currentQualityPresetId += 1;
        }
        else
        {
            return;
        }

        qualityPresetText.text = supportedQualityPresets[currentQualityPresetId];
    }
    #endregion
}