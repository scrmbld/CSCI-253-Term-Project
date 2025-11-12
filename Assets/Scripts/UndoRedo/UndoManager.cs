using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }
    private Stack<ObjectState> undoStack = new Stack<ObjectState>();
    private Stack<ObjectState> redoStack = new Stack<ObjectState>();

    // ------------------------------------------------------------------------------------
    // TODO: Debug Undo/Redo Logic
    // Viewable stacks in Unity for debugging
    [SerializeField] private List<ObjectState> debugUndoStack = new List<ObjectState>();        
    [SerializeField] private List<ObjectState> debugRedoStack = new List<ObjectState>();
    private void SyncDebugStacks()
    {
        debugUndoStack = new List<ObjectState>(undoStack);
        debugRedoStack = new List<ObjectState>(redoStack);
    }
    // ------------------------------------------------------------------------------------

    // Prevents SaveState during undo/redo
    private bool isRestoring;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("UndoManager initialized.");
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
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            Redo();
        }
    }

    public void SaveState(GameObject gameObject)
    {
        Debug.Log(gameObject);
        if (gameObject == null) { return; }

        ObjectState state = new ObjectState(gameObject);
        undoStack.Push(state);

        // Remove any redo history
        redoStack.Clear();
        // For testing
        LogState($"{gameObject.name} saved at:", undoStack.Peek());
        SyncDebugStacks();
    }

    public void Undo()
    {
        Debug.Log("Undo() triggered.");
        if (undoStack.Count < 1)
        {
            Debug.Log($"Nothing to undo.");
            return; 
        }
        isRestoring = true;

        // Pop object's current state
        ObjectState currState = undoStack.Pop();
        redoStack.Push(currState);

        // Restore previous state
        ObjectState prevState = undoStack.Peek();
        prevState.RestoreState();
        isRestoring = false;


        // For testing
        LogState($"{prevState.targetObject?.name} state reverted back to:", prevState);
        Debug.Log($"Undo Stack size: {undoStack.Count}");
        SyncDebugStacks();
    }

    public void Redo()
    {
        Debug.Log("Redo() triggered.");
        if (redoStack.Count < 1)
        {
            Debug.Log("Redo stack empty. Nothing to redo.");
            return;
        }

        isRestoring = true;

        // Pop the current state from the redo stack to restore
        ObjectState redoState = redoStack.Pop();
        redoState.RestoreState();
        isRestoring = false;

        // Push undo state to undo stack
        undoStack.Push(redoState);

        // For testing
        LogState($"{redoState.targetObject.name} state redone to:", redoState);
        Debug.Log($"Redo Stack size: {redoStack.Count}");
        SyncDebugStacks();

    }

    private void LogState(string message, ObjectState newState)
    {
        Debug.Log($"{message}\nPosition: {newState.position}; Rotation: {newState.rotation}; Scale: {newState.scale}");
    }
}
