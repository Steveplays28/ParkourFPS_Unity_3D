using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (GameManager.gameObjects.TryGetValue(Client.instance.myId, out GameObject fetchedGameObject))
        {
            PlayerManager player = fetchedGameObject.GetComponent<PlayerManager>();

            transform.forward = player.camera.transform.forward;
        }
    }
}
