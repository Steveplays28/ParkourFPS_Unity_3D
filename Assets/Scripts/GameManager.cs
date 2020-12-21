using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject playerPrefab;
    public Material localPlayerMat;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _spawnPosition, Quaternion _spawnRotation)
    {
        GameObject _player;

        _player = Instantiate(playerPrefab, _spawnPosition, _spawnRotation);

        if (_id == Client.instance.myId)
        {
            _player.GetComponent<Renderer>().material = localPlayerMat;
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;

        players.Add(_id, _player.GetComponent<PlayerManager>());

        if (_id != Client.instance.myId)
        {
            _player.GetComponentInChildren<Camera>().gameObject.SetActive(false);
        }
    }

    public void SetPlayerPosition(int _id, Vector3 _position)
    {
        Vector3.Lerp(players[_id].transform.position, _position, Time.fixedDeltaTime);
        players[_id].transform.position = _position;
    }

    public void SetPlayerRotation(int _id, Quaternion _rotation)
    {
        players[_id].transform.rotation = _rotation;
    }
}
