using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;

    [Header("Alpha Treshold")]
    public float alphaTreshold = 0.5f;

    [Header("Settings")]
    public bool animated;
    public float duration = 0.5f;
    public float smallScale = 1f;
    public float bigScale = 1.5f;

    [Header("")]
    public bool canBeHeldDown;
    public float waitTime = 0.5f;
    public float holdDownTime = 0.1f;

    private bool heldDown;

    #region On mouse down settings
    [Serializable]
    public class ButtonMouseDownEvent : UnityEvent { }

    // Event delegates triggered on mouse down.
    [FormerlySerializedAs("onMouseDown")]
    [SerializeField]
    [Header("")]
    private ButtonMouseDownEvent m_OnMouseDown = new ButtonMouseDownEvent();
    #endregion

    #region On mouse down
    public ButtonMouseDownEvent onMouseDown
    {
        get { return m_OnMouseDown; }
        set { m_OnMouseDown = value; }
    }

    private void Press()
    {
        if (!button.IsActive() || !button.IsInteractable())
            return;

        m_OnMouseDown.Invoke();
    }
    #endregion

    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = alphaTreshold;
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable)
        {
            StartAnimation();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button.interactable)
        {
            StopAnimation();
        }
    }

    #region On mouse down invoke
    public void OnPointerDown(PointerEventData eventData)
    {
        Press();

        if (canBeHeldDown)
        {
            heldDown = true;
            StartCoroutine(Wait());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (canBeHeldDown)
        {
            heldDown = false;
            StopAllCoroutines();
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSecondsRealtime(waitTime);

        if (!heldDown)
        {
            yield break;
        }
        StartCoroutine(HoldButtonDown());
    }

    private IEnumerator HoldButtonDown()
    {
        if (!heldDown)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(holdDownTime);
        m_OnMouseDown.Invoke();

        StartCoroutine(HoldButtonDown());
    }
    #endregion

    private void StartAnimation()
    {
        if (animated)
        {
            gameObject.transform.DOScale(bigScale, duration);
        }
    }

    private void StopAnimation()
    {
        if (animated)
        {
            gameObject.transform.DOScale(smallScale, duration);
        }
    }
}
