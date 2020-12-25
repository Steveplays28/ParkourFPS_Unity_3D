using System.Collections;
using System.Collections.Generic;
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

    public void DestroyPlayer(int _id)
    {
        Destroy(players[_id].gameObject);
    }

    public void SetPlayerPosition(int _id, Vector3 _position)
    {
        StartCoroutine(MoveOverSeconds(players[_id].gameObject, _position, Time.deltaTime * 2));
    }

    private IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public void SetPlayerRotation(int _id, Quaternion _playerRotation, Quaternion _cameraRotation)
    {
        StartCoroutine(RotateOverSeconds(players[_id].gameObject, _playerRotation, Time.deltaTime * 2));
        StartCoroutine(RotateOverSeconds(players[_id].gameObject.GetComponent<PlayerManager>().camera.gameObject, _cameraRotation, Time.deltaTime * 2));
    }

    private IEnumerator RotateOverSeconds(GameObject objectToMove, Quaternion end, float seconds)
    {
        float elapsedTime = 0;
        Quaternion startingRot = objectToMove.transform.rotation;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.rotation = Quaternion.Lerp(startingRot, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
