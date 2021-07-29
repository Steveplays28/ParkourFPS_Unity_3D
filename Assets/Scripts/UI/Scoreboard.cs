using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    #region Variables
    public GameObject scoreboardContent;

    public GameObject entityScorePrefab;
    #endregion

    private void Start()
    {
        GameManager.instance.MainMenuLoaded += OnMainMenuLoaded;
    }

    private void OnMainMenuLoaded(AsyncOperation asyncOperation)
    {
        GameManager.instance.PlayerConnected += OnPlayerConnected;
    }

    private void OnDestroy()
    {
        GameManager.instance.MainMenuLoaded -= OnMainMenuLoaded;
        GameManager.instance.PlayerConnected -= OnPlayerConnected;
    }

    private void OnPlayerConnected(int playerId)
    {
        GameObject instantiatedEntityScore = Instantiate(entityScorePrefab, scoreboardContent.transform);

        if (GameManager.gameObjects.TryGetValue(playerId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            instantiatedEntityScore.GetComponent<EntityScoreboardData>().Initialize(player);
        }
    }
}
