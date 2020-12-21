using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    private bool[] inputs = new bool[5];

    private void FixedUpdate()
    {
        inputs[0] = Input.GetKey(KeyCode.W);
        inputs[1] = Input.GetKey(KeyCode.S);
        inputs[2] = Input.GetKey(KeyCode.A);
        inputs[3] = Input.GetKey(KeyCode.D);
        inputs[4] = Input.GetKey(KeyCode.Space);

        ClientSend.PlayerInput(inputs);
    }
}
