using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class UndoableObject : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    // Stack
    private Stack<ObjectState> undoStack = new Stack<ObjectState>();
    private Stack<ObjectState> redoStack = new Stack<ObjectState>();

    // Undoable Object
    void Awake()
    {
        if (interactable == null)
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        }

        // Saves the object's initial state
        SaveState();

        // Subscribe to interaction events
        interactable.selectEntered.AddListener(OnGrabbed);
        interactable.selectExited.AddListener(OnReleased);
    }

    // Unsubscribe to interaction events
    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnGrabbed);
            interactable.selectExited.RemoveListener(OnReleased);
        }
    }

    // Called when the object is grabbed, saves the current state for future undo
    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // SaveState();
    }

    // Called when the object is released, saves the current state for future undo
    private void OnReleased(SelectExitEventArgs args)
    {
        SaveState();
    }

    private void SaveState()
    {
        undoStack.Push(new ObjectState(transform));
        // For testing
        LogState("Object state saved at:", undoStack.Peek());

        // Remove any redo history
        redoStack.Clear();
    }

    void Start()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Need to map to controller buttons and/or UI
        // Currently only works w/ keyboard for testing
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            Undo();
        }
        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            Redo();
        }
    }

    // Undo/Redo
    public void Undo()
    {
        if (undoStack.Count <= 1) { return; }

        // Store the object's current state (transform) and move to redoStack
        ObjectState currState = new ObjectState(transform);
        redoStack.Push(currState);

        // Pop current state from the stack
        undoStack.Pop();

        // Restore previous state
        ObjectState prevState = undoStack.Peek();
        prevState.ApplyTo(transform);

        // For testing
        LogState("Object state reverted back to:", prevState);
    }

    public void Redo()
    {
        if (redoStack.Count == 0) { return; }

        // Store the object's current state (transform) and move to undoStack
        ObjectState currState = new ObjectState(transform);
        undoStack.Push(currState);

        // Pop the current state from the redo stack
        ObjectState redoState = redoStack.Pop();

        // Restore previous redo state
        redoState.ApplyTo(transform);

        // For testing
        LogState("Object state redone to:", redoState);
    }

    private void LogState(string message, ObjectState newState)
    {
        Debug.Log($"{message}\nPosition: {newState.position}; Rotation: {newState.rotation}; Scale: {newState.scale}");
    }
}

[System.Serializable]
public struct ObjectState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public ObjectState(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    public void ApplyTo(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }
}