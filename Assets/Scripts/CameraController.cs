using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Player
    [Header("Player")]
    public new Camera camera;

    private Rigidbody rigidBody;

    //Movement
    [Header("Movement")]
    public int movementSpeed;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        rigidBody = gameObject.GetComponent<Rigidbody>();

        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rigidBody.isKinematic = true;
    }

    void OnEnable()
    {
        Start();
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += camera.transform.forward * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += -camera.transform.forward * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += -camera.transform.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += camera.transform.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += Vector3.down * Time.deltaTime * movementSpeed;
        }
    }
}