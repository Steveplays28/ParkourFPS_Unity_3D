using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class InputFieldSettings : MonoBehaviour
{
    public bool disablePlaceholderOnSelect = true;
    [Tooltip("WARNING: setting this value below 0.05 may cause it not to work.")]
    public float placeholderDisableDelay = 0.05f;

    private TMP_InputField inputField;
    private TMP_Text placeholderText;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        placeholderText = inputField.placeholder.GetComponent<TMP_Text>();

        inputField.onSelect.AddListener(delegate { OnSelect(); });
        inputField.onDeselect.AddListener(delegate { OnDeSelect(); });
    }

    private void OnSelect()
    {
        StartCoroutine(DisablePlaceholder());
    }

    private void OnDeSelect()
    {
        if (disablePlaceholderOnSelect && inputField.text == "")
        {
            StopCoroutine(DisablePlaceholder());
            placeholderText.color = new Color(placeholderText.color.r, placeholderText.color.g, placeholderText.color.b, 255);
        }
    }

    private IEnumerator DisablePlaceholder()
    {
        yield return new WaitForSeconds(placeholderDisableDelay);
        if (disablePlaceholderOnSelect && inputField.text == "")
        {
            placeholderText.color = new Color(placeholderText.color.r, placeholderText.color.g, placeholderText.color.b, 0);
        }
    }
}
