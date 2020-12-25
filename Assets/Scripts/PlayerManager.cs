using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id = 0;
    public string username = "";

    public new Camera camera;
    public float horizontalSensitivity = 1;
    public float verticalSensitivity = 1;

    public float maxHealth = 100;
    private float health;

    private bool[] inputs = new bool[11];
    private Quaternion mouseRotation;

    private bool spaceDown;
    private bool spaceUp;
    private bool leftShiftDown;
    private bool leftShiftUp;
    private bool lmbDown;
    private bool lmbUp;

    private void Start()
    {
        health = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spaceDown = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            spaceUp = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            leftShiftDown = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            leftShiftUp = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            lmbDown = true;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            lmbUp = true;
        }
    }

    private void FixedUpdate()
    {
        // inputs[0]
        inputs[0] = Input.GetKey(KeyCode.W);
        // inputs[1]
        inputs[1] = Input.GetKey(KeyCode.S);
        // inputs[2]
        inputs[2] = Input.GetKey(KeyCode.A);
        // inputs[3]
        inputs[3] = Input.GetKey(KeyCode.D);
        // inputs[4]
        if (spaceDown)
        {
            inputs[4] = true;
            spaceDown = false;
        }
        else
        {
            inputs[4] = false;
        }
        // inputs[5]
        if (spaceUp)
        {
            inputs[5] = true;
            spaceUp = false;
        }
        else
        {
            inputs[5] = false;
        }

        // inputs[6]
        if (leftShiftDown)
        {
            inputs[6] = true;
            leftShiftDown = false;
        }
        else
        {
            inputs[6] = false;
        }
        // inputs[7]
        if (leftShiftUp)
        {
            inputs[7] = true;
            leftShiftUp = false;
        }
        else
        {
            inputs[7] = false;
        }
        // inputs[8]
        inputs[8] = Input.GetKey(KeyCode.Mouse0);
        // inputs[9]
        if (lmbDown)
        {
            inputs[9] = true;
            lmbDown = false;
        }
        else
        {
            inputs[9] = false;
        }
        // inputs[10]
        if (lmbUp)
        {
            inputs[10] = true;
            lmbUp = false;
        }
        else
        {
            inputs[10] = false;
        }

        //Get mouse rotation inputs
        mouseRotation.y += Input.GetAxis("Mouse X") * horizontalSensitivity;
        mouseRotation.x += -Input.GetAxis("Mouse Y") * verticalSensitivity;

        ClientSend.PlayerInput(inputs, mouseRotation);
    }
}
