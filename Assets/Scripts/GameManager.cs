using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables
    public static GameManager instance;

    public static Dictionary<int, GameObject> gameObjects = new Dictionary<int, GameObject>();

    //public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();
    public static Dictionary<int, ProjectileManager> projectiles = new Dictionary<int, ProjectileManager>();
    public static Dictionary<int, EnemyManager> enemies = new Dictionary<int, EnemyManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public GameObject itemSpawnerPrefab;
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;

    public event Action<AsyncOperation> MainMenuLoaded;
    public event Action<int> PlayerConnected;
    public event Action<int, string> PlayerDisconnected;
    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AsyncOperation sceneLoadAsyncOperation;
        sceneLoadAsyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        sceneLoadAsyncOperation.completed += OnMainMenuLoaded;
    }

    private void OnMainMenuLoaded(AsyncOperation asyncOperation)
    {
        MainMenuLoaded?.Invoke(asyncOperation);
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="id">The player's ID.</param>
    /// <param name="username">The player's username.</param>
    /// <param name="position">The player's starting position.</param>
    /// <param name="rotation">The player's starting rotation.</param>
    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        if (id == Client.instance.myId)
        {
            player = Instantiate(localPlayerPrefab, position, rotation);
        }
        else
        {
            player = Instantiate(playerPrefab, position, rotation);
        }

        player.GetComponent<PlayerManager>().Initialize(id, username);
        gameObjects.Add(id, player);

        // Trigger PlayerConnected event
        PlayerConnected?.Invoke(id);
    }

    public void DisconnectPlayer(int id, string reason)
    {
        Destroy(gameObjects[id]);
        gameObjects.Remove(id);

        // Trigger PlayerDisconnected event
        PlayerDisconnected?.Invoke(id, reason);
    }

    public void SpawnEnemy(int _id, Vector3 _position)
    {
        GameObject _enemy = Instantiate(enemyPrefab, _position, Quaternion.identity);
        _enemy.GetComponent<EnemyManager>().Initialize(_id);
        enemies.Add(_id, _enemy.GetComponent<EnemyManager>());
    }

    public void CreateItemSpawner(int _spawnerId, Vector3 _position, bool _hasItem)
    {
        GameObject _spawner = Instantiate(itemSpawnerPrefab, _position, itemSpawnerPrefab.transform.rotation);
        _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);
        itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());
    }

    public void SpawnProjectile(int _id, Vector3 _position)
    {
        GameObject _projectile = Instantiate(projectilePrefab, _position, Quaternion.identity);
        _projectile.GetComponent<ProjectileManager>().Initialize(_id);
        projectiles.Add(_id, _projectile.GetComponent<ProjectileManager>());
    }
}
