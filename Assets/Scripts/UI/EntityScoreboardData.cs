using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class EntityScoreboardData : MonoBehaviour
{
    [Foldout("References")]
    public TMP_Text entityName;
    [Foldout("References")]
    public TMP_Text entityPoints;

    [Space]
    [Foldout("References")]
    public TMP_Text entityKills;
    [Foldout("References")]
    public TMP_Text entityDeaths;
    [Foldout("References")]
    public TMP_Text entityAssists;

    public void Initialize(EntityManager entity)
    {
        entityName.text = entity.username;
        //entityPoints.text = entity.points; // Coming soon :)
        entityKills.text = entity.kills.ToString();
        entityDeaths.text = entity.deaths.ToString();
        entityAssists.text = entity.assists.ToString();
    }
}
