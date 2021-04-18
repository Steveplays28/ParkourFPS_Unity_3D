using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public TMP_Text fullscreenModeText;
    public TMP_Text resolutionText;
    public TMP_Text qualityPresetText;
    public TMP_Text fpsText;
    public TMP_Text vSyncText;

    public Button VSyncButton;
    public Button[] fpsButtons;

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

    [Header("FPS Cap")]
    public int currentFpsCap = 60;
    public int minFps = 1;
    public int maxFps = 360;

    [Header("VSync")]
    public int currentVSyncCount;
    public Dictionary<int, string> supportedVSyncCounts = new Dictionary<int, string>()
    {
        { 0, "Off" },
        { 1, "Every V blank" },
        { 2, "Every second V blank" },
        { 3, "Every third V blank" },
        { 4, "Every fourth V blank" }
    };

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

        GetSupportedResolutions();
        GetSupportedFullscreenModes();
        GetSupportedQualityPresets();
        SetCurrentFps();
        SetCurrentVSyncCount();
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

        currentFullScreenModeId = supportedFullscreenModes.FirstOrDefault(x => x.Value == Screen.fullScreenMode).Key;
        fullscreenModeText.text = fullscreenModeNames[supportedFullscreenModes[currentFullScreenModeId]];
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

    #region Frames per second
    private void SetCurrentFps()
    {
        if (Application.targetFrameRate > 0)
        {
            currentFpsCap = Application.targetFrameRate;
            fpsText.text = string.Concat(currentFpsCap.ToString(), " fps");
        }
        else
        {
            fpsText.text = string.Concat(currentFpsCap, " fps");
        }
    }

    //public void ToggleFpsCap(bool enable)
    //{
    //    if (enable)
    //    {
    //        Application.targetFrameRate = currentFpsCap;

    //        QualitySettings.vSyncCount = 0;
    //        VSyncButton.enabled = false;
    //    }
    //    else
    //    {
    //        Application.targetFrameRate = -1;

    //        QualitySettings.vSyncCount = currentVSyncCount;
    //        VSyncButton.enabled = true;
    //    }
    //}

    public void SetFps(bool lower)
    {
        if (lower && currentFpsCap - 1 >= minFps)
        {
            Application.targetFrameRate = currentFpsCap - 1;
            currentFpsCap -= 1;
        }
        else if (lower == false && currentFpsCap + 1 <= maxFps)
        {
            Application.targetFrameRate = currentFpsCap + 1;
            currentFpsCap += 1;
        }
        else
        {
            return;
        }

        fpsText.text = string.Concat(currentFpsCap.ToString(), " fps");
    }
    #endregion

    #region VSync
    private void SetCurrentVSyncCount()
    {
        currentVSyncCount = QualitySettings.vSyncCount;
        vSyncText.text = supportedVSyncCounts[currentVSyncCount];
    }

    public void SetVSyncCount(bool lower)
    {
        if (lower && supportedVSyncCounts.ContainsKey(currentVSyncCount - 1))
        {
            QualitySettings.vSyncCount = currentVSyncCount - 1;
            currentVSyncCount -= 1;
        }
        else if (lower == false && supportedVSyncCounts.ContainsKey(currentVSyncCount + 1))
        {
            QualitySettings.vSyncCount = currentVSyncCount + 1;
            currentVSyncCount += 1;
        }
        else
        {
            return;
        }

        if (currentVSyncCount == 0)
        {
            for (int i = 0; i < fpsButtons.Length; i++)
            {
                fpsButtons[i].interactable = true;
            }
        }
        else
        {
            for (int i = 0; i < fpsButtons.Length; i++)
            {
                fpsButtons[i].interactable = false;
            }

            currentFpsCap = Screen.currentResolution.refreshRate;
            Application.targetFrameRate = currentFpsCap;
            fpsText.text = string.Concat(currentFpsCap.ToString(), " fps");
        }

        vSyncText.text = supportedVSyncCounts[currentVSyncCount];
    }
    #endregion
}