using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoManagerControl : UndoManager
{
    private Stack<ObjectState> undoStack = new Stack<ObjectState>();
    private Stack<ObjectState> redoStack = new Stack<ObjectState>();

    // Prevents save states during undo/redo
    private bool isRestoring = false;


    // Viewable stacks in Unity for debugging
    [SerializeField] private List<ObjectState> debugUndoStack = new List<ObjectState>();        
    [SerializeField] private List<ObjectState> debugRedoStack = new List<ObjectState>();
    private void SyncDebugStacks()
    {
        debugUndoStack = new List<ObjectState>(undoStack);
        debugRedoStack = new List<ObjectState>(redoStack);
        Debug.Log($"undoStack size: {undoStack.Count}, redoStack size: {redoStack.Count}");
    }

    protected override void Awake()
    {
        // Guarantees only one UndoManager can exist at once (so control and experimental can't conflict)
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        // Keyboard controls for simulator testing
        // Meta Controller mappings in Assets/Scripts/ObjectManipulation/Control.cs
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            Undo();
        }
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            Redo();
        }
    }

    // Subscribe and Unsubscribe to GrabEvents
    private void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnObjectGrab);
    }
    private void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnObjectGrab);
    }

    private void OnObjectGrab(GameObject grabbedObject, string hand)
    {
        // Check if grabbed object is undoable and saves state
        UndoableObject undoableObject = grabbedObject.GetComponentInParent<UndoableObject>();
        if (undoableObject != null)
        {
            SaveState(undoableObject);
        }
    }

    public void SaveState(UndoableObject gameObject)
    {
        if (gameObject == null) { return; }
        if (isRestoring) { return; }
        ObjectState state = new ObjectState(gameObject);
        undoStack.Push(state);

        // Remove any redo history
        redoStack.Clear();

        // For testing
        LogState($"{gameObject.name} saved at:", undoStack.Peek());
        SyncDebugStacks();
    }

    public override void Undo()
    {
        if (undoStack.Count < 1)
        {
            Debug.Log($"Nothing to undo.");
            return; 
        }
        isRestoring = true;

        // Pop the undo stack 
        ObjectState undoState = undoStack.Pop();
        
        // Push current state of target object to undoStack
        ObjectState currState = new ObjectState(undoState.targetObject);
        redoStack.Push(currState);

        // Restore the undo state
        undoState.RestoreState();

        isRestoring = false;

        // For testing
        LogState($"{undoState.targetObject?.name} state reverted back to:", undoState);
        SyncDebugStacks();
    }

    public override void Redo()
    {
        if (redoStack.Count < 1)
        {
            Debug.Log("Nothing to redo.");
            return;
        }

        isRestoring = true;

        // Pop the current state from the redo stack to restore
        ObjectState redoState = redoStack.Pop();

        // Save the current state of target object and push to undoStack 
        ObjectState currState = new ObjectState(redoState.targetObject);
        undoStack.Push(currState);

        // Restore the previous state
        redoState.RestoreState();

        isRestoring = false;

        // For testing
        LogState($"{redoState.targetObject.name} state redone to:", redoState);
        SyncDebugStacks();

    }

    private void LogState(string message, ObjectState newState)
    {
        Debug.Log($"{message}\nPosition: {newState.savedPosition}; Rotation: {newState.savedRotation}; Scale: {newState.savedScale}");
    }
}
