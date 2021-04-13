using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image), typeof(Button))]
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Alpha Treshold")]
    public float alphaTreshold = 0.5f;

    [Header("Animation")]
    public bool animated;
    public float duration = 0.5f;
    public float smallScale = 1f;
    public float bigScale = 1.5f;

    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = alphaTreshold;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAnimation();
    }

    public void StartAnimation()
    {
        if (animated)
        {
            gameObject.transform.DOScale(bigScale, duration);
        }
    }

    public void StopAnimation()
    {
        if (animated)
        {
            gameObject.transform.DOScale(smallScale, duration);
        }
    }
}
