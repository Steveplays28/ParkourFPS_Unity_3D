using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerManager playerManager;
    public new Camera camera;

    private void Update()
    {
        //Shooting weapons
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot();
            playerManager.weaponManager.Shoot();
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

        //Equipping weapons
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ClientSend.PlayerEquipWeapon(0);
            playerManager.EquipWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ClientSend.PlayerEquipWeapon(1);
            playerManager.EquipWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ClientSend.PlayerEquipWeapon(2);
            playerManager.EquipWeapon(2);
        }

        //Reloading weapons
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClientSend.PlayerReloadWeapon();
            playerManager.weaponManager.Reload();
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
