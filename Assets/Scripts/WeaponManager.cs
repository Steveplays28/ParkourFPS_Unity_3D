using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager playerManager;
    public Animator animator;

    [Header("Weapon stats")]
    public string weaponName;
    public float damage;
    public bool usesAmmo = true;
    public int currentAmmo;
    public int maxAmmo;
    public int ammoPerShot;
    public float range;
    public float reloadTime;
    public bool isReloading;

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
        if (playerManager.health <= 0f || isReloading)
        {
            return;
        }
        else if (currentAmmo <= 0 && usesAmmo)
        {
            Reload();
        }
        else
        {
            if (usesAmmo)
            {
                currentAmmo -= ammoPerShot;
            }

            animator.SetTrigger("Shoot");

            if (playerManager.id == Client.instance.myId)
            {
                UIManager.instance.UpdateAmmo();
            }
        }
    }

    public void Reload()
    {
        if (currentAmmo == maxAmmo || playerManager.health <= 0f || isReloading || usesAmmo == false)
        {
            return;
        }

        isReloading = true;
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
}
