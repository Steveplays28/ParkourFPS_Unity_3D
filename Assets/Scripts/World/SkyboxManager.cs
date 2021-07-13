using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    [Header("Skybox")]
    public float skyboxRotateSpeed = 5f;

    private void LateUpdate()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxRotateSpeed);
    }
}
