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

    // Awake is called at start
    protected override void Awake()
    {
        // Guarantees only one UndoManager can exist at once (so control and experimental can't conflict)
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        // Reads keyboard controls to trigger undo/redo (for simulator testing)
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            Undo();
        }
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            Redo();
        }
    }
    // Reads Meta Controller input to trigger undo/redo
    public override void OnUndoInput(bool isHeld, bool justPressed)
    {
        // Only react to a single press
        if (justPressed)
        {
            Undo();
        }
    }
    public override void OnRedoInput(bool isHeld, bool justPressed)
    {
        if (justPressed)
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

    // Grab Events for object grab
    private void OnObjectGrab(GameObject grabbedObject, string hand)
    {
        // Check if grabbed object is undoable and saves state
        UndoableObject undoableObject = grabbedObject.GetComponentInParent<UndoableObject>();
        if (undoableObject != null)
        {
            SaveState(undoableObject);
        }
    }

    // Undo and Redo functions
    public void Undo()
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

    public void Redo()
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

    private void LogState(string message, ObjectState newState)
    {
        Debug.Log($"{message}\nPosition: {newState.savedPosition}; Rotation: {newState.savedRotation}; Scale: {newState.savedScale}");
    }
}
