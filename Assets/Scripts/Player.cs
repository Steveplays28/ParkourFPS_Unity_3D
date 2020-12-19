using UnityEngine;

public class Player : MonoBehaviour
{
    //Player
    [Header("Player")]
    public new Camera camera;
    public float horizontalSensitivity;
    public float verticalSensitivity;

    private Vector3 rotation;

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnEnable()
    {
        Start();
    }

    // Update is called once per frame
    void Update()
    {
        //Camera Controller
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameObject.GetComponent<FpsController>().enabled = false;
            gameObject.GetComponent<CameraController>().enabled = true;
        }

        //FPS Controller
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameObject.GetComponent<CameraController>().enabled = false;
            gameObject.GetComponent<FpsController>().enabled = true;
        }

        //Get mouse rotation inputs
        rotation.y += Input.GetAxis("Mouse X") * horizontalSensitivity;
        rotation.x += -Input.GetAxis("Mouse Y") * verticalSensitivity;
    }

    private void LateUpdate()
    {
        //Set rotation
        camera.transform.Rotate(rotation);
        transform.Rotate(new Vector3(transform.rotation.x, rotation.y, transform.rotation.z));
    }
}
