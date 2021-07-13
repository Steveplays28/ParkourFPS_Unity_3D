using DG.Tweening;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        // Now that we have the client's id, connect UDP
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void Ping(Packet _packet)
    {
        UIManager.instance.UpdatePingCounter();
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();

            player.transform.DOMove(position, player.lerpTime);
            //_player.transform.position = _position;
        }
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion playerRotation = packet.ReadQuaternion();
        Quaternion cameraRotation = packet.ReadQuaternion();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();

            player.transform.DORotate(playerRotation.eulerAngles, player.lerpTime);
            player.camera.transform.DORotate(cameraRotation.eulerAngles, player.lerpTime);
        }
    }

    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();
        string reason = packet.ReadString();

        GameManager.instance.DisconnectPlayer(id, reason);
    }

    public static void EntityHit(Packet packet)
    {
        int id = packet.ReadInt();
        int hitBy = packet.ReadInt();
        float amount = packet.ReadFloat();

        GameManager.gameObjects[id].GetComponent<EntityManager>().Hit(GameManager.gameObjects[hitBy], amount);
    }
    public static void EntityHeal (Packet packet)
    {
        int id = packet.ReadInt();
        int hitBy = packet.ReadInt();
        float amount = packet.ReadFloat();

        GameManager.gameObjects[id].GetComponent<EntityManager>().Heal(GameManager.gameObjects[hitBy], amount);
    }

    public static void PlayerRespawned(Packet packet)
    {
        int id = packet.ReadInt();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();

            player.Respawn();
        }
    }

    public static void CreateItemSpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();

        GameManager.instance.CreateItemSpawner(_spawnerId, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.itemSpawners[_spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        int byPlayer = packet.ReadInt();

        GameManager.itemSpawners[spawnerId].ItemPickedUp();
        if (GameManager.gameObjects.TryGetValue(byPlayer, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();
            player.itemCount++;
        }
    }

    public static void SpawnProjectile(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        int _thrownByPlayer = _packet.ReadInt();

        GameManager.instance.SpawnProjectile(_projectileId, _position);
        if (GameManager.gameObjects.TryGetValue(_thrownByPlayer, out GameObject gameObject))
        {
            PlayerManager player = gameObject.GetComponent<PlayerManager>();
            player.itemCount--;
        }
    }

    public static void ProjectilePosition(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.projectiles.TryGetValue(_projectileId, out ProjectileManager _projectile))
        {
            _projectile.transform.position = _position;
        }
    }

    public static void ProjectileExploded(Packet _packet)
    {
        int _projectileId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.projectiles[_projectileId].Explode(_position);
    }

    public static void SpawnEnemy(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        GameManager.instance.SpawnEnemy(_enemyId, _position);
    }

    public static void EnemyPosition(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.enemies.TryGetValue(_enemyId, out EnemyManager _enemy))
        {
            _enemy.transform.position = _position;
        }
    }

    public static void EnemyHealth(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.enemies[_enemyId].SetHealth(_health);
    }

    public static void PlayerShoot(Packet packet)
    {
        int id = packet.ReadInt();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            player.weaponManager.Shoot();
        }
    }

    public static void PlayerEquipWeapon(Packet packet)
    {
        int id = packet.ReadInt();
        int weaponId = packet.ReadInt();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            player.EquipWeapon(weaponId);
        }
    }

    public static void PlayerReloadWeapon(Packet packet)
    {
        int id = packet.ReadInt();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            player.weaponManager.StartCoroutine(player.weaponManager.Reload());
        }
    }

    public static void EntityStartWallrun(Packet packet)
    {
        int id = packet.ReadInt();
        bool leftSide = packet.ReadBool();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            player.StartWallrun(leftSide);
        }
    }

    public static void EntityStopWallrun(Packet packet)
    {
        int id = packet.ReadInt();

        if (GameManager.gameObjects.TryGetValue(id, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            player.StopWallrun();
        }
    }
}
