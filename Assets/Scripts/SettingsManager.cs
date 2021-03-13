using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public Dropdown fullscreenModeDropdown;
    public Dropdown resolutionDropdown;
    public Dropdown qualityPresetDropdown;
    public bool fullscreen;

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
        GetQualityPreset();
    }

    public void GetSupportedResolutions()
    {
        List<string> _resolutions = new List<string>();
        int _currentResolutionindex = 0;

        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution _resolution = (Resolution)Screen.resolutions.GetValue(i);

            _resolutions.Add(string.Concat(_resolution.width.ToString(), "x", _resolution.height.ToString()));

            if (_resolution.width == Screen.currentResolution.width && _resolution.height == Screen.currentResolution.height)
            {
                _currentResolutionindex = i;
            }
        }

        resolutionDropdown.AddOptions(_resolutions);
        resolutionDropdown.value = _currentResolutionindex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution()
    {
        Screen.SetResolution(Screen.resolutions[resolutionDropdown.value].width, Screen.resolutions[resolutionDropdown.value].height, fullscreen);
    }

    public void SetFullscreenMode()
    {
        Screen.fullScreenMode = (FullScreenMode)fullscreenModeDropdown.value;
    }

    public void GetQualityPreset()
    {
        qualityPresetDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void SetQualityPreset()
    {
        QualitySettings.SetQualityLevel(qualityPresetDropdown.value, true);
    }
}
