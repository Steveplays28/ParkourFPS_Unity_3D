using NaughtyAttributes;
using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager instance;

    [Header("Audio")]
    public float audioVolume = 1f;

    [Foldout("Hover")]
    [InfoBox("These audio clips will be played on hover by any UI elements implementing it.", EInfoBoxType.Normal)]
    public AudioClip hover1;
    [Foldout("Hover")]
    [Label("Hover 2 | Disabled")]
    [ReadOnly]
    public AudioClip hover2;
    [Foldout("Hover")]
    [Label("Hover 3 | Disabled")]
    [ReadOnly]
    public AudioClip hover3;

    [Foldout("Click")]
    [InfoBox("These audio clips will be played on click by any UI elements implementing it.", EInfoBoxType.Normal)]
    public AudioClip click1;
    [Foldout("Click")]
    [Label("Click 2 | Disabled")]
    [ReadOnly]
    public AudioClip click2;
    [Foldout("Click")]
    [Label("Click 3 | Disabled")]
    [ReadOnly]
    public AudioClip click3;

    #region Singleton pattern
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
    #endregion
}
