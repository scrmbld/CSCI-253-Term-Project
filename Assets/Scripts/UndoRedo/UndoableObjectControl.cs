using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoableObjectControl : MonoBehaviour
{
    public ManipulationControl undoableObject;

    // Initialize object state
    void Awake()
    {
        if (undoableObject == null)
        {
            undoableObject = GetComponent<ManipulationControl>();
        }

        UndoManager.Instance.SaveState(undoableObject.gameObject);
    }

    // Subscribe to GrabEvents
    private void OnEnable()
    {
        // GrabEventSystem.OnGrab.AddListener(OnObjectGrab);
        GrabEventSystem.OnRelease.AddListener(OnObjectRelease);
    }

    // Unsubscribe to GrabEvents
    private void OnDisable()
    {
        // GrabEventSystem.OnGrab.RemoveListener(OnObjectGrab);
        GrabEventSystem.OnRelease.RemoveListener(OnObjectRelease);
    }

    // private void OnObjectGrab(GameObject grabbedObject, string hand)
    // {
    //     if (grabbedObject == undoableObject.GameObject)
    //     {
    //         SaveState();
    //     }
    // }

    private void OnObjectRelease(GameObject grabbedObject, string hand)
    {
        if (grabbedObject == gameObject)
        {
            UndoManager.Instance.SaveState(grabbedObject);
        }
        else
        {
            Debug.Log($"{undoableObject.name} != {grabbedObject.name}, Save State not recorded");
        }
    }
}