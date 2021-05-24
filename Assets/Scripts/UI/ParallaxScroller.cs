using DG.Tweening;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ParallaxScroller : MonoBehaviour
{
    public float scrollTime = 1f;
    public Vector3[] positions;
    
    private RectTransform rectTransform;
    private int currentPositionIndex = 0;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        StartCoroutine(Parallax());
    }

    private IEnumerator Parallax()
    {
        rectTransform.DOAnchorPos(positions[currentPositionIndex], scrollTime);
        yield return new WaitForSeconds(scrollTime);
        if (currentPositionIndex == positions.Length - 1)
        {
            currentPositionIndex = 0;
        }
        else
        {
            currentPositionIndex++;
        }

        // Repeat
        StartCoroutine(Parallax());
    }
}
