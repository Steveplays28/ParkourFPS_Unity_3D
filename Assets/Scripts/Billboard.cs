using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(transform.position + GameManager.players[Client.instance.myId].camera.transform.forward);
    }
}
