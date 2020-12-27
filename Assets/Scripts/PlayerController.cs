using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public new Camera camera;

    private void Update()
    {
        //Shooting
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(camera.transform.forward);
        }

        //Throwing projectiles
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerThrowItem(camera.transform.forward);
        }

        //Jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClientSend.PlayerJump();
        }

        //Running
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ClientSend.PlayerRun();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            ClientSend.PlayerRun();
        }

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ClientSend.PlayerCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            ClientSend.PlayerCrouch();
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>Sends player input to the server.</summary>
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
        };

        ClientSend.PlayerMovement(_inputs, transform.rotation, camera.transform.rotation);
    }
}
