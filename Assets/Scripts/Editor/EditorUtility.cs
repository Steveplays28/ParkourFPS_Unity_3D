using UnityEditor;
using UnityEngine;

public class EditorUtility
{
    [MenuItem("Tools/Editor Utility/Remove all colliders from scene", false, 0)]
    static public void RemoveAllCollidersFromScene()
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
