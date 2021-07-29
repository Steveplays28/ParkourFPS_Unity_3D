using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerManager : EntityManager
{
    #region Variables
    [Header("References")]
    public PlayerController playerController;
    public CameraController cameraController;

    [Header("Player stats")]
    private float cameraRotateSpeed = 0.5f;
    private float cameraRotationAmount = 15f;
    #endregion

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        currentHealth = maxHealth;

        nameTag.text = username;
        camera.GetUniversalAdditionalCameraData().cameraStack.Add(UIManager.instance.UICamera);
    }

    #region Health
    public override void Hit(GameObject hitBy, float amount)
    {
        base.Hit(hitBy, amount);

        if (id == Client.instance.myId)
        {
            UIManager.instance.healthBar.value = currentHealth;
            UIManager.instance.UpdateHealthEffect();
        }
    }

    public override void Heal(GameObject hitBy, float amount)
    {
        base.Heal(hitBy, amount);

        if (id == Client.instance.myId)
        {
            UIManager.instance.healthBar.value = currentHealth;
            UIManager.instance.UpdateHealthEffect();
        }
    }
    #endregion

    public void EquipWeapon(int weaponId)
    {
        if (weaponManager.isReloading)
        {
            weaponManager.StopReload();
        }
        weaponManager.Disable();

        weaponManager = weaponManagers[weaponId];
        weaponManager.Enable();
        currentWeaponId = weaponId;

        if (id == Client.instance.myId)
        {
            UIManager.instance.UpdateWeapon();
            UIManager.instance.UpdateAmmo();
        }
    }

    public void StartWallrun(bool leftSide)
    {
        float side;
        if (leftSide)
        {
            side = -1;
        }
        else
        {
            side = 1;
        }

        float angle = 0;
        DOTween.To(() => angle, x => angle = x, cameraRotationAmount * side, cameraRotateSpeed)
            .OnUpdate(() =>
            {
                camera.transform.eulerAngles = new Vector3(camera.transform.eulerAngles.x, camera.transform.eulerAngles.y, angle);
            });
    }

    public void StopWallrun()
    {
        float angle = camera.transform.eulerAngles.z;
        float zero;
        if (angle > 180)
        {
            zero = 360f;
        }
        else
        {
            zero = 0f;
        }
        DOTween.To(() => angle, x => angle = x, zero, cameraRotateSpeed)
            .OnUpdate(() =>
            {
                camera.transform.eulerAngles = new Vector3(camera.transform.eulerAngles.x, camera.transform.eulerAngles.y, angle);
            });
    }
}
