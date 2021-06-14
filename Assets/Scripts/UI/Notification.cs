using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public Vector2 currentPosition = new Vector2(120f, -50f);
    public float hiddenPositionX = 120f;
    public float shownPositionX = -120f;

    [Space]
    public float moveDownAmount = 50f;
    public float lerpDuration = 0.5f;

    [Space]
    public float removeDelay = 5f;

    public void Initialise(string text)
    {
        gameObject.GetComponent<TMP_Text>().text = text;
        GetComponent<RectTransform>().DOAnchorPosX(shownPositionX, lerpDuration);
        currentPosition = new Vector2(shownPositionX, currentPosition.y);

        for (int i = 0; i < UIManager.instance.notifications.Count; i++)
        {
            GameObject otherNotification = UIManager.instance.notifications[i].gameObject;

            otherNotification.GetComponent<RectTransform>().DOAnchorPosY(otherNotification.GetComponent<Notification>().currentPosition.y - moveDownAmount, lerpDuration);
            otherNotification.GetComponent<Notification>().currentPosition -= new Vector2(0, moveDownAmount);

            if (i > UIManager.instance.maxNotifications)
            {
                otherNotification.GetComponent<TMP_Text>().DOFade(0, lerpDuration);
            }
        }

        UIManager.instance.notifications.Add(UIManager.instance.notifications.Count, this);
        StartCoroutine(RemoveNotificationAfterDelay());
    }

    private IEnumerator RemoveNotificationAfterDelay()
    {
        yield return new WaitForSeconds(removeDelay);
        GetComponent<RectTransform>().DOAnchorPosX(hiddenPositionX, lerpDuration);

        yield return new WaitForSeconds(lerpDuration);
        UIManager.instance.notifications.Remove(UIManager.instance.notifications.Count);
        Destroy(gameObject);
    }
}