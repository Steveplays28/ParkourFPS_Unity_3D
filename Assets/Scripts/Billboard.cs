using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.forward = GameManager.players[Client.instance.myId].camera.transform.forward;
    }
}
