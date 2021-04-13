using System;
using UnityEngine;

public class LevelEditorObject : MonoBehaviour
{
    [Header("References")]
    public GameObject cubePrefab;
    public GameObject cylinderPrefab;
    public GameObject spherePrefab;

    [Serializable]
    public enum ObjectType { Cube, Cylinder, Sphere };

    [Serializable]
    public struct Data
    {
        public Vector3 position;
        public Vector3 rotation;
        public ObjectType objectType;
    }
    public Data data;

    public void Initialize(Vector3 newPosition, Vector3 newRotation, ObjectType newObjectType)
    {
        data.position = newPosition;
        data.rotation = newRotation;
        data.objectType = newObjectType;

        if (newObjectType == ObjectType.Cube)
        {
            Instantiate(cubePrefab);
        }
    }
}