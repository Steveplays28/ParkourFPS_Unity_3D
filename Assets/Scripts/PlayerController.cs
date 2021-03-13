using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public PlayerManager playerManager;
    public new Camera camera;
    public bool isPaused;

    private Vector3 wallRunDirection;
    private bool isWallrunning;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                UIManager.instance.ClosePauseMenu();
            }
            else
            {
                UIManager.instance.OpenPauseMenu();
            }
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

        if (isPaused)
        {
            return;
        }

        //Shooting weapons
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot();
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

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ClientSend.PlayerCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            ClientSend.PlayerCrouch();
        }

        //Equipping weapons
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ClientSend.PlayerEquipWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ClientSend.PlayerEquipWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ClientSend.PlayerEquipWeapon(2);
        }

        //Reloading weapons
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClientSend.PlayerReloadWeapon();
        }

        ////Wallrunning
        //if (Physics.Raycast(transform.position, transform.right, out RaycastHit hitR, 1))
        //{
        //    wallRunDirection = Quaternion.AngleAxis(90, Vector3.up) * hitR.normal;

        //    Debug.Log(wallRunDirection);
        //    Debug.DrawLine(transform.position, transform.position + wallRunDirection + transform.right, Color.white, 1);

        //    transform.DOLookAt(transform.position + wallRunDirection + transform.right, 0.5f);
        //    //transform.forward = transform.position + wallRunDirection + transform.right;
        //    //transform.rotation = Quaternion.LookRotation(transform.position + wallRunDirection + transform.right, Vector3.up);

        //    if (!isWallrunning)
        //    {
        //        isWallrunning = true;
        //    }
        //}
        //else if (Physics.Raycast(transform.position, -transform.right, out RaycastHit hitL, 1))
        //{
        //    wallRunDirection = Quaternion.AngleAxis(-90, Vector3.up) * hitL.normal;

        //    Debug.DrawLine(transform.position, transform.position + wallRunDirection + -transform.right, Color.white, 1);
        //    Debug.Log(wallRunDirection);

        //    transform.DOLookAt(transform.position + wallRunDirection + -transform.right, 0.5f);
        //    //transform.forward = transform.position + wallRunDirection + -transform.right;
        //    //transform.rotation = Quaternion.LookRotation(transform.position + wallRunDirection + -transform.right, Vector3.up);

        //    if (!isWallrunning)
        //    {
        //        isWallrunning = true;
        //    }
        //}
        //else
        //{
        //    if (isWallrunning)
        //    {
        //        isWallrunning = false;
        //    }
        //}
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


        if (isPaused)
        {
            _inputs = new bool[4];
        }

        ClientSend.PlayerMovement(_inputs, transform.rotation, camera.transform.rotation);
    }
}
