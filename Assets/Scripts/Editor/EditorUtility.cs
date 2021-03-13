using UnityEditor;
using UnityEngine;

public static class EditorUtility
{
    [MenuItem("Tools/Editor Utility/Reset material of every mesh in current scene", false, 0)]
    public static void ResetAllMeshMaterialInScene()
    {
        Material _defaultMaterial = null;
        int _amount = 0;

        foreach (Material _material in Resources.FindObjectsOfTypeAll<Material>())
        {
            if (_material.name == "Ground")
            {
                _defaultMaterial = _material;
            }
        }
        if (_defaultMaterial == null)
        {
            Debug.Log("Could not find the Ground material!");
            return;
        }

        foreach (MeshRenderer _meshRenderer in Object.FindObjectsOfType<MeshRenderer>(true))
        {
            _meshRenderer.sharedMaterial = _defaultMaterial;
            _amount++;
        }

        Debug.Log($"Reset {_amount} mesh's material in current scene!");
    }

    [MenuItem("Tools/Editor Utility/Remove all colliders from scene", false, 0)]
    public static void RemoveAllCollidersFromScene()
    {
        int _amount = 0;

        foreach (Collider _collider in Object.FindObjectsOfType<Collider>(true))
        {
            Object.DestroyImmediate(_collider);
            _amount++;
        }

        Debug.Log($"Destroyed {_amount} colliders!");
    }
}
