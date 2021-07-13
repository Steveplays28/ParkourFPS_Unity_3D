using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;

    [Space]
    public float entityHeadHeight = 0.75f;
    private GameObject gameObjectToFollow;

    #region Singleton pattern
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }
    #endregion

    private void LateUpdate()
    {
        if (!IsFollowingGameObject())
        {
            return;
        }

        if (gameObjectToFollow.TryGetComponent(out EntityManager entityManager))
        {
            transform.position = gameObjectToFollow.transform.position + new Vector3(0, 0, entityHeadHeight);
            transform.rotation = gameObjectToFollow.transform.rotation;
        }
    }

    public bool IsFollowingGameObject()
    {
        if (gameObjectToFollow == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public GameObject GetGameObjectToFollow()
    {
        return gameObjectToFollow;
    }

    public void SetGameObjectToFollow(GameObject gameObject)
    {
        gameObjectToFollow = gameObject;
    }
}
