using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class LevelEditorCameraController : MonoBehaviour
{
    [Header("References")]
    public new Camera camera;
    [HideInInspector]
    public bool isPaused;

    public GameObject levelEditorObjectPrefab;

    [Header("Camera settings")]
    public float currentSpeed = 1;
    public float normalSpeed = 1;
    public float fastSpeed = 2;

    private void Update()
    {
        // Pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.instance.currentOpenMenu == UIManager.instance.pauseMenu)
            {
                UIManager.instance.ClosePauseMenu();
            }
            else if (UIManager.instance.currentOpenMenu == UIManager.instance.optionsMenu)
            {
                UIManager.instance.CloseOptionsMenu();
            }
            else if (UIManager.instance.currentOpenMenu == null)
            {
                UIManager.instance.OpenPauseMenu();
            }
        }

        // Fast speed
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            currentSpeed = fastSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            currentSpeed = normalSpeed;
        }

        if (isPaused)
        {
            return;
        }

        // Movement
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(transform.forward * currentSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-transform.forward * currentSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(transform.right * currentSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(-transform.right * currentSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(-Vector3.up * currentSpeed * Time.deltaTime);
        }

        // Equipping objects
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {

        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {

        }

        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Forward scroll
        {
            if (camera.fieldOfView == 90)
            {
                DOTween.To(() => camera.fieldOfView, x => camera.fieldOfView = x, 45, 1);
            }
            if (camera.fieldOfView == 120)
            {
                DOTween.To(() => camera.fieldOfView, x => camera.fieldOfView = x, 90, 1);
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Backwards scroll
        {
            if (camera.fieldOfView == 45)
            {
                DOTween.To(() => camera.fieldOfView, x => camera.fieldOfView = x, 90, 1);
            }
            if (camera.fieldOfView == 90)
            {
                DOTween.To(() => camera.fieldOfView, x => camera.fieldOfView = x, 120, 1);
            }
        }

        // Placing objects
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject instantiatedObject = Instantiate(levelEditorObjectPrefab);
            instantiatedObject.GetComponent<LevelEditorObject>().Initialize(camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z + 10)), Vector3.zero, LevelEditorObject.ObjectType.Cube);
        }
    }
}
