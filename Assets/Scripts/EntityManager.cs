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
    public float LerpTime = 0.20f;
    public float currentHealth;
    public float maxHealth = 100f;
    public int itemCount = 0;
}
