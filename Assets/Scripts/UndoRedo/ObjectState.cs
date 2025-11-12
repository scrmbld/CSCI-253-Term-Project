using UnityEngine;

[System.Serializable]

// Stores object transform states for undo/redo access
public struct ObjectState
{
    public GameObject targetObject;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public ObjectState(GameObject gameObject)
    {
        targetObject = gameObject;

        if (gameObject != null)
        {
            position = gameObject.transform.position;
            rotation = gameObject.transform.rotation;
            scale = gameObject.transform.localScale;
        }
        else
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
        }

    }

    public void RestoreState()
    {
        if (targetObject == null) { return; }

        Transform t = targetObject.transform;
        t.position = position;
        t.rotation = rotation;
        t.localScale = scale;
    }
}
