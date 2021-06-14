using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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

        inputField.onSelect.AddListener(delegate
        {
            StopCoroutine(DisablePlaceholder());
            StartCoroutine(DisablePlaceholder());
        });
        inputField.onEndEdit.AddListener(delegate { EnablePlaceholder(); });
    }

    private IEnumerator DisablePlaceholder()
    {
        yield return new WaitForSeconds(placeholderDisableDelay);
        if (disablePlaceholderOnSelect && inputField.text == "")
        {
            placeholderText.color = new Color(placeholderText.color.r, placeholderText.color.g, placeholderText.color.b, 0);
        }
    }

    private void EnablePlaceholder()
    {
        if (disablePlaceholderOnSelect && inputField.text == "")
        {
            StopCoroutine(DisablePlaceholder());
            placeholderText.color = new Color(placeholderText.color.r, placeholderText.color.g, placeholderText.color.b, 255);
        }
    }
}
