using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;
    public float sensitivity = 100f;
    public float clampAngle = 90f;

    private void LateUpdate()
    {
        if (Cursor.lockState == CursorLockMode.Locked && !Cursor.visible)
        {
            Look();
        }
    }

    private void Look()
    {
        transform.rotation *= Quaternion.Euler(Mathf.Clamp(-Input.GetAxis("Mouse Y") * sensitivity, -clampAngle, clampAngle), 0f, 0f);
        player.transform.rotation *= Quaternion.Euler(0f, Mathf.Clamp(Input.GetAxis("Mouse X") * sensitivity, -clampAngle, clampAngle), 0f);
    }

    public void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
