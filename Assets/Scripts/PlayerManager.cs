using UnityEngine;

public class PlayerManager : EntityManager
{
    [Header("Player stats")]
    public string username;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        currentHealth = maxHealth;

        nameTag.text = username;
    }

    public void SetHealth(float _health)
    {
        currentHealth = _health;
        if (id == Client.instance.myId)
        {
            UIManager.instance.healthBar.value = currentHealth;
            UIManager.instance.UpdateHealthEffect();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        weaponManager.ResetWeapon();

        // Disable meshes
        model.enabled = false;
        glassesModel.enabled = false;
        weaponManager.GetComponent<MeshRenderer>().enabled = false;
        nameTag.transform.parent.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        SetHealth(maxHealth);

        // Reset weapons
        foreach(WeaponManager weaponManager in weaponManagers)
        {
            weaponManager.ResetWeapon();
        }
        if (id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }

        // Enable meshes
        model.enabled = true;
        glassesModel.enabled = true;
        weaponManager.GetComponent<MeshRenderer>().enabled = true;
        nameTag.transform.parent.gameObject.SetActive(true);
    }

    public void EquipWeapon(int _weaponId)
    {
        if (weaponManager.isReloading)
        {
            weaponManager.StopReload();
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
