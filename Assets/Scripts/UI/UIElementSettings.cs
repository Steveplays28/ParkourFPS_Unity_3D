using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementSettings : MonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerEnterHandler
{
    public enum HoverAudio
    {
        Hover1,
        Hover2,
        Hover3
    }

    public enum ClickAudio
    {
        Click1,
        Click2,
        Click3
    }

    [Header("Audio")]
    public bool playHoverAudio = true;
    public HoverAudio hoverAudio;

    [Space]
    public bool playClickAudio = true;
    public ClickAudio clickAudio;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (playClickAudio)
        {
            if (clickAudio == ClickAudio.Click1)
            {
                AudioSource.PlayClipAtPoint(UISoundManager.instance.click1, UIManager.instance.normalCamera.transform.position, UISoundManager.instance.audioVolume);
            }
            else if (clickAudio == ClickAudio.Click2)
            {
                AudioSource.PlayClipAtPoint(UISoundManager.instance.click2, UIManager.instance.normalCamera.transform.position, UISoundManager.instance.audioVolume);
            }
            else if (clickAudio == ClickAudio.Click3)
            {
                AudioSource.PlayClipAtPoint(UISoundManager.instance.click3, UIManager.instance.normalCamera.transform.position, UISoundManager.instance.audioVolume);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverAudio)
        {
            if (hoverAudio == HoverAudio.Hover1)
            {
                AudioSource.PlayClipAtPoint(UISoundManager.instance.hover1, UIManager.instance.normalCamera.transform.position, UISoundManager.instance.audioVolume);
            }
            else if (hoverAudio == HoverAudio.Hover2)
            {
                AudioSource.PlayClipAtPoint(UISoundManager.instance.hover2, UIManager.instance.normalCamera.transform.position, UISoundManager.instance.audioVolume);
            }
            else if (hoverAudio == HoverAudio.Hover2)
            {
                AudioSource.PlayClipAtPoint(UISoundManager.instance.hover3, UIManager.instance.normalCamera.transform.position, UISoundManager.instance.audioVolume);
            }
        }
    }
}
