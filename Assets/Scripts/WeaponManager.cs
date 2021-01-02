using System.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public Animator animator;

    [Header("Weapon stats")]
    public string weaponName;
    public float damage;
    public float range;

    [Header("Reloadable weapons only")]
    public bool usesAmmo = true;
    public int currentAmmo;
    public int maxAmmo;
    public int ammoPerShot;
    public float reloadTime;
    public bool isReloading;

    [Header("Automatic weapons only")]
    public bool isAutomatic;
    public float timeBetweenShots;
    public bool isShooting;
    public bool isWaiting;

    private void Start()
    {
        currentAmmo = maxAmmo;

        if (playerManager.id == Client.instance.myId)
        {
            UIManager.instance.UpdateWeapon();
            UIManager.instance.UpdateAmmo();
        }
    }

    public void Shoot()
    {
        if (playerManager.health <= 0f)
        {
            return;
        }
        else
        {
            if (isAutomatic && isShooting)
            {
                isShooting = false;
                return;
            }
        }

        if (isReloading)
        {
            return;
        }
        else if (currentAmmo <= 0 && usesAmmo)
        {
            Reload();
            return;
        }

        if (isAutomatic)
        {
            if (isShooting)
            {
                isShooting = false;
                return;
            }
            else
            {
                isShooting = true;
                ShootAutomatic();
                return;
            }
        }

        if (usesAmmo)
        {
            currentAmmo -= ammoPerShot;
        }

        animator.Play("Shoot", -1, 0f);

        if (playerManager.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }

        if (isAutomatic)
        {
            StartCoroutine(ShootAfterDelay());
        }
    }

    public void ShootAutomatic()
    {
        if (playerManager.health <= 0f || isReloading || isShooting == false || isWaiting)
        {
            return;
        }
        else if (currentAmmo <= 0 && usesAmmo)
        {
            Reload();
            return;
        }

        if (usesAmmo)
        {
            currentAmmo -= ammoPerShot;
        }

        animator.Play("Shoot", -1, 0f);

        if (playerManager.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }

        StartCoroutine(ShootAfterDelay());
    }

    private IEnumerator ShootAfterDelay()
    {
        isWaiting = true;
        yield return new WaitForSeconds(timeBetweenShots);
        isWaiting = false;
        ShootAutomatic();

        yield break;
    }

    public void Reload()
    {
        if (currentAmmo == maxAmmo || playerManager.health <= 0f || isReloading || usesAmmo == false)
        {
            return;
        }

        isReloading = true;
        animator.Play("Reload");
        StartCoroutine(ReloadAfterDelay());
    }

    private IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;

        if (playerManager.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }

        if (isAutomatic)
        {
            ShootAutomatic();
        }

        yield break;
    }

    public void CancelReload()
    {
        if (isReloading)
        {
            isReloading = false;
            StopCoroutine(ReloadAfterDelay());
        }
    }

    public void ResetWeapon()
    {
        CancelReload();
        currentAmmo = maxAmmo;
    }
}
