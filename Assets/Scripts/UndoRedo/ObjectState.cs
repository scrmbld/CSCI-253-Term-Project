using UnityEngine;

[System.Serializable]

// Stores object transform states for undo/redo access
public struct ObjectState
{

    // Name for testing/debugging
    public string name;
    public UndoableObject targetObject;
    public Vector3 savedPosition;
    public Quaternion savedRotation;
    public Vector3 savedScale;


    // Constructor
    public ObjectState(UndoableObject gameObject)
    {
        targetObject = gameObject;

        if (gameObject != null)
        {
            name = gameObject.name;
            savedPosition = gameObject.transform.position;
            savedRotation = gameObject.transform.rotation;
            savedScale = gameObject.transform.localScale;
        }
        else
        {
            name = "null object";
            savedPosition = Vector3.zero;
            savedRotation = Quaternion.identity;
            savedScale = Vector3.one;
        }

    }

    // Sets the target object current state to be the stored object state
    public void RestoreState()
    {
        if (targetObject == null) { return; }

        Transform current = targetObject.transform;
        current.position = savedPosition;
        current.rotation = savedRotation;
        current.localScale = savedScale;
    }
}
