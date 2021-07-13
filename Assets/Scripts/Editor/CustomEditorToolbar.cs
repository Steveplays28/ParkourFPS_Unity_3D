#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class CustomEditorToolbar
{
    static float minTimeScale = 0f;
    static float maxTimeScale = 2f;

    static int minFps = 30;
    static int maxFps = 120;
    static bool lowFpsMode = false;
    static readonly int normalMinFps = 30;
    static readonly int lowFpsModeMinFps = 5;

    static bool isRestarting = false;

    static CustomEditorToolbar()
    {
        if (Application.targetFrameRate == -1)
        {
            Application.targetFrameRate = 120;
        }

        ToolbarExtender.LeftToolbarGUI.Add(DrawLeftGUI);
        ToolbarExtender.RightToolbarGUI.Add(DrawRightGUI);

        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        EditorApplication.quitting += EditorQuit;
    }

    static void DrawLeftGUI()
    {
        #region Space between custom toolbar elements and default toolbar elements
        GUILayout.Space(105);
        #endregion

        #region Space
        GUILayout.Space(10);
        #endregion

        #region Reload scene and domain
        if (!EditorApplication.isPlaying && (EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload) || EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableSceneReload)))
        {
            if (GUILayout.Button(new GUIContent("Reset states", "Reloads the domain and scene."), GUILayout.Width(105)))
            {
                EditorPrefs.SetBool("isResetting", true);
                EditorPrefs.SetBool("DisableDomainReload", EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableDomainReload));
                EditorPrefs.SetBool("DisableSceneReload", EditorSettings.enterPlayModeOptions.HasFlag(EnterPlayModeOptions.DisableSceneReload));
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.None;
                EditorApplication.EnterPlaymode();

            }
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button(new GUIContent("Reset states", "Reloads the domain and scene."), GUILayout.Width(105));
            GUI.enabled = true;
        }
        #endregion

        #region Space
        GUILayout.Space(10);
        #endregion

        #region Restart game
        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button(new GUIContent("Restart game", "Restarts the game."), GUILayout.Width(105)))
            {
                isRestarting = true;
                EditorApplication.ExitPlaymode();
            }
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button(new GUIContent("Restart game", "Restarts the game."), GUILayout.Width(105));
            GUI.enabled = true;
        }
        #endregion

        #region Space
        GUILayout.Space(10);
        #endregion

        #region Clear saved data
        if (GUILayout.Button(new GUIContent("Clear saved data", "Clears the saved data."), GUILayout.Width(105)))
        {
            try
            {
                if (File.Exists(Application.persistentDataPath + "/Data.txt"))
                {
                    File.Delete(Application.persistentDataPath + "/Data.txt");
                    Debug.Log("Successfully deleted saved data.");
                }
                else
                {
                    Debug.Log("No saved data found to delete.");
                }
            }
            catch (IOException e)
            {
                Debug.LogError("Caught exception: " + e);
            }
        }
        #endregion

        #region Space between custom toolbar elements and default toolbar elements
        GUILayout.Space(10);
        #endregion
    }

    static void DrawRightGUI()
    {
        #region Space between custom toolbar elements and default toolbar elements
        GUILayout.Space(10);
        #endregion

        #region Time scale
        GUILayout.Label(new GUIContent("Time scale:", "Use this slider to change the time scale."));

        int newTimeScale = Mathf.RoundToInt(GUILayout.HorizontalSlider(Time.timeScale, minTimeScale, maxTimeScale, GUILayout.Width(100)));
        if (Time.timeScale != newTimeScale)
        {
            Time.timeScale = newTimeScale;
        }

        if (int.TryParse(GUILayout.TextField(Time.timeScale.ToString(), GUILayout.Width(30)), out newTimeScale))
        {
            if (Time.timeScale != newTimeScale)
            {
                if (newTimeScale > maxTimeScale)
                {
                    Time.timeScale = maxFps;
                }
                else if (newTimeScale < minTimeScale)
                {
                    Time.timeScale = minTimeScale;
                }
                else
                {
                    Time.timeScale = newTimeScale;
                }
            }
        }
        #endregion

        #region Space
        GUILayout.Space(10);
        #endregion

        #region Framerate cap
        GUILayout.Label(new GUIContent("FPS:", "Use this slider to change the framerate limit."));

        int newFpsCap = Mathf.RoundToInt(GUILayout.HorizontalSlider(Application.targetFrameRate, minFps, maxFps, GUILayout.Width(100)));
        if (Application.targetFrameRate != newFpsCap)
        {
            Application.targetFrameRate = newFpsCap;
        }

        if (int.TryParse(GUILayout.TextField(Application.targetFrameRate.ToString(), GUILayout.Width(30)), out newFpsCap))
        {
            if (Application.targetFrameRate != newFpsCap)
            {
                if (newFpsCap > maxFps)
                {
                    Application.targetFrameRate = maxFps;
                }
                else if (newFpsCap < minFps)
                {
                    Application.targetFrameRate = minFps;
                }
                else
                {
                    Application.targetFrameRate = newFpsCap;
                }
            }
        }

        #region Space
        GUILayout.Space(10);
        #endregion

        if (lowFpsMode == false)
        {
            if (GUILayout.Button(new GUIContent("Low fps mode - Off", "Low fps mode is currently off. Warning: changing this setting can be dangerous."), GUILayout.Width(125)))
            {
                lowFpsMode = true;
                minFps = lowFpsModeMinFps;
                Debug.LogWarning("Low fps mode is now on, use at your own risk.");
            }
        }
        else
        {
            if (GUILayout.Button(new GUIContent("Low fps mode - On", "Low fps mode is currently on. Warning: this setting can be dangerous."), GUILayout.Width(125)))
            {
                lowFpsMode = false;
                minFps = normalMinFps;
                if (Application.targetFrameRate == lowFpsModeMinFps)
                {
                    Application.targetFrameRate = normalMinFps;
                }
                Debug.Log("Low fps mode is now off.");
            }
        }
        #endregion

        #region Space between custom toolbar elements and default toolbar elements
        GUILayout.Space(10);
        #endregion
    }

    #region Play mode state changed
    static void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && isRestarting)
        {
            EditorApplication.EnterPlaymode();
            isRestarting = false;
        }

        if (state == PlayModeStateChange.EnteredPlayMode && EditorPrefs.GetBool("isResetting"))
        {
            EditorApplication.ExitPlaymode();
            if (EditorPrefs.GetBool("DisableDomainReload") && EditorPrefs.GetBool("DisableSceneReload"))
            {
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
            }
            else if (EditorPrefs.GetBool("DisableDomainReload"))
            {
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            }
            else if (EditorPrefs.GetBool("DisableSceneReload"))
            {
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableSceneReload;
            }
            EditorPrefs.SetBool("isResetting", false);
        }
    }
    #endregion

    static void EditorQuit()
    {
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        EditorApplication.quitting -= EditorQuit;
    }
}
#endif