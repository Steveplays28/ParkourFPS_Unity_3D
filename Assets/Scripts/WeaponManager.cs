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

    public AnimationClip idleAnim;
    public AnimationClip shootAnim;
    public AnimationClip reloadAnim;

    public void Shoot()
    {
        currentAmmo -= ammoPerShot;
        if (entity.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }
        animator.Play("Shoot", -1, 0f);
    }

    public IEnumerator Reload()
    {
        animator.Play("Reload", -1, 0f);
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
        animator.enabled = false;
        animator.enabled = true;
    }

    public void ResetWeapon()
    {
        currentAmmo = maxAmmo;
        if (entity.id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }
    }
}
