using System.Collections;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    public PlayerManager entity;

    public Mesh model;
    public Mesh barrel;
    public Mesh grip;
    public Mesh optic;

    public Transform bulletOrigin;

    [Header("Weapon stats")]
    public string weaponName;
    public int damage;
    public float range;
    public int currentAmmo;
    public int maxAmmo;
    public int ammoPerShot;
    public float reloadTime;
    public bool isReloading;
    public float timeBetweenShots;
    public bool shooting;
    public bool isAutomatic;

    [Header("Animations")]
    public Animator animator;

    public void Shoot()
    {
        currentAmmo -= ammoPerShot;
        if (entity.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }
        //rb.
        animator.ResetTrigger("Shoot");
        animator.SetTrigger("Shoot");
        Debug.Log("shoot");
    }

    public IEnumerator Reload()
    {
        animator.ResetTrigger("Reload");
        animator.SetTrigger("Reload");
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        if (entity.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }
    }

    public void StopReload()
    {
        StopCoroutine(Reload());
    }

    public void ResetWeapon()
    {
        currentAmmo = maxAmmo;
        if (entity.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }
    }

    public void Enable()
    {
        GetComponent<MeshRenderer>().enabled = true;
        animator.enabled = true;
        animator.Play("Idle");
    }

    public void Disable()
    {
        GetComponent<MeshRenderer>().enabled = false;
        animator.enabled = false;
    }
}
