using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public MeshRenderer model;
    public WeaponManager weaponManager;
    public new Camera camera;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
    }

    public void SetHealth(float _health)
    {
        health = _health;
        if (id == Client.instance.myId)
        {
            UIManager.instance.healthBar.value = health;
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        weaponManager.CancelReload();
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }

    public void EquipWeapon(int _weaponId)
    {
        if (weaponManager.isReloading)
        {
            weaponManager.CancelReload();
        }

        WeaponManager[] _weapons = GetComponentsInChildren<WeaponManager>(true);

        foreach (WeaponManager _weapon in _weapons)
        {
            _weapon.gameObject.SetActive(false);
        }

        weaponManager = _weapons[_weaponId];
        _weapons[_weaponId].gameObject.SetActive(true);

        if (id == Client.instance.myId)
        {
            UIManager.instance.UpdateWeapon();
            UIManager.instance.UpdateAmmo();
        }
    }
}
