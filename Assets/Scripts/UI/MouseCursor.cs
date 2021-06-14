using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Image))]
public class MouseCursor : MonoBehaviour
{
    public static MouseCursor instance;

    [Space]
    public bool isVisible;

    [Space]
    public float lerpDuration = 0.1f;
    public float fadeDuration = 0.5f;

    private Image cursorImage;
    private TrailRenderer cursorTrail;

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
        SceneManager.sceneLoaded += Initialise;
    }

    private void OnApplicationQuit()
    {
        SceneManager.sceneLoaded -= Initialise;
    }

    private void Initialise(Scene scene, LoadSceneMode loadSceneMode)
    {
        cursorImage = GetComponent<Image>();
        cursorTrail = GetComponent<TrailRenderer>();

        if (scene.buildIndex == 1)
        {
            ShowCursor();
        }
    }

    private void Update()
    {
        transform.DOMove(UIManager.instance.UICamera.ScreenToWorldPoint(Input.mousePosition), lerpDuration);
    }

    public void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            ShowCursor();
        }
        else
        {
            HideCursor();
        }
    }

    public void ShowCursor()
    {
        StopCoroutine(LockCursorAfterDelay());
        cursorTrail.enabled = true;
        Cursor.lockState = CursorLockMode.None;
        transform.position = UIManager.instance.UICamera.ScreenToWorldPoint(Input.mousePosition);
        cursorImage.DOFade(1, fadeDuration);
        cursorTrail.emitting = true;

        isVisible = true;
    }

    public void HideCursor()
    {
        cursorImage.DOFade(0, fadeDuration);
        cursorTrail.emitting = false;

        isVisible = false;
        StartCoroutine(LockCursorAfterDelay());
    }

    private IEnumerator LockCursorAfterDelay()
    {
        yield return new WaitForSeconds(fadeDuration);
        cursorTrail.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}