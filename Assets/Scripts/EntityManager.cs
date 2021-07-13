using UnityEngine;
using UnityEngine.UI;

public class EntityManager : MonoBehaviour
{
    [Header("References")]
    public new Camera camera;

    public Text nameTag;
    public MeshRenderer model;
    public MeshRenderer glassesModel;

    public WeaponManager weaponManager;
    public WeaponManager[] weaponManagers;
    public int currentWeaponId;

    [Header("Entity stats")]
    public int id;
    public float lerpTime = 0.20f;
    public float currentHealth;
    public float maxHealth = 100f;
    public int itemCount = 0;

    [Space]
    public int kills;
    public int deaths;
    public int assists;

    #region Health
    public virtual void Hit(GameObject hitBy, float amount)
    {
        currentHealth -= Mathf.Clamp(amount, 0, maxHealth - currentHealth);
        if (currentHealth <= 0f)
        {
            Die(hitBy);
        }
    }

    public virtual void Heal(GameObject hitBy, float amount)
    {
        currentHealth += Mathf.Clamp(amount, 0, maxHealth);
    }

    public virtual void Die(GameObject killedBy)
    {
        IncrementDeaths();

        EntityManager entity = killedBy.GetComponent<EntityManager>();
        if (entity != null)
        {
            entity.IncrementKills();
        }

        // Disable meshes
        model.enabled = false;
        glassesModel.enabled = false;
        weaponManager.Disable();
        nameTag.transform.parent.gameObject.SetActive(false);
    }

    public virtual void Respawn()
    {
        currentHealth = maxHealth;

        // Reset weapons
        foreach (WeaponManager weaponManager in weaponManagers)
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
        weaponManager.Enable();
        nameTag.transform.parent.gameObject.SetActive(true);
    }
    #endregion

    #region Kills, deaths and assists
    public virtual void IncrementKills()
    {
        kills++;
    }

    public virtual void IncrementDeaths()
    {
        deaths++;
    }

    public virtual void IncrementAssists()
    {
        assists++;
    }
    #endregion
}
