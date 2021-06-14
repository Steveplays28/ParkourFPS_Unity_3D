#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CustomEditorCommands
{
    static CustomEditorCommands()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (state == PlayModeStateChange.EnteredPlayMode && SceneManager.GetActiveScene().buildIndex != 0)
        {
            EditorSceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }

    [MenuItem("Tools/Load Level Editor scene", false, 100000)]
    static void LoadLevelEditorScene()
    {
        SceneManager.LoadScene("Level Editor", LoadSceneMode.Single);
        UIManager.instance.CloseMenu(UIManager.instance.mainMenu);
        UIManager.instance.usernameField.interactable = false;
        UIManager.instance.ipField.interactable = false;
    }

    [MenuItem("Tools/Toggle fps cap", false, 100000)]
    static void ToggleFpsCap()
    {
        if (Application.targetFrameRate == 60)
        {
            Application.targetFrameRate = -1;
        }
        else
        {
            Application.targetFrameRate = 60;
        }
    }
}
#endif