using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerManager playerManager;
    public new Camera camera;
    public bool isPaused;

    private void Update()
    {
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
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            ClientSend.PlayerStopShooting();
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
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Forward scroll
        {
            int newWeaponIndex = playerManager.currentWeaponId + 1;
            if (newWeaponIndex > playerManager.weaponManagers.Length - 1)
            {
                newWeaponIndex = 0;
            }

            ClientSend.PlayerEquipWeapon(newWeaponIndex);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Backwards scroll
        {
            int newWeaponIndex = playerManager.currentWeaponId - 1;
            if (newWeaponIndex < 0)
            {
                newWeaponIndex = playerManager.weaponManagers.Length - 1;
            }

            ClientSend.PlayerEquipWeapon(newWeaponIndex);
        }

        //Reloading weapons
        if (Input.GetKeyDown(KeyCode.R))
        {
            ClientSend.PlayerReloadWeapon();
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


        if (isPaused)
        {
            _inputs = new bool[4];
        }

        ClientSend.PlayerMovement(_inputs, transform.rotation, camera.transform.rotation);
    }
}
