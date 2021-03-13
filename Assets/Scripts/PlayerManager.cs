using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public new Camera camera;
    public WeaponManager weaponManager;
    public WeaponManager[] weaponManagers;
    public Text playerNameTag;
    public MeshRenderer playerModel;
    public MeshRenderer glassesModel;

    [Header("Player stats")]
    public int id;
    public string username;
    public float currentHealth;
    public float maxHealth = 100f;
    public int itemCount = 0;

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        currentHealth = maxHealth;

        playerNameTag.text = username;
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
        playerModel.enabled = false;
        glassesModel.enabled = false;
        weaponManager.GetComponent<MeshRenderer>().enabled = false;
        playerNameTag.transform.parent.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        SetHealth(maxHealth);

        // Reset weapons
        foreach(WeaponManager _weaponManager in weaponManagers)
        {
            weaponManager.ResetWeapon();
        }
        if (id == Client.instance.myId)
        {
            UIManager.instance.UpdateAmmo();
        }

        // Enable meshes
        playerModel.enabled = true;
        glassesModel.enabled = true;
        weaponManager.GetComponent<MeshRenderer>().enabled = true;
        playerNameTag.transform.parent.gameObject.SetActive(true);
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
