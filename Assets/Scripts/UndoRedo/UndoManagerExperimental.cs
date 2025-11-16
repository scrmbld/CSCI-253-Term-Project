using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class UndoManagerExperimental : UndoManager
{
    // Serialized for debugging
    [SerializeField] private List<ObjectState> objectHistory = new List<ObjectState>();
    [SerializeField] private int currIndex = 0;
    private UndoableObject grabbedObject = null;

    private bool objectIsBeingGrabbed = false;
    private bool isRestoring = false;

    protected override void Awake()
    {
        // Guarantees only one UndoManager can exist at once (so control and experimental can't conflict)
        base.Awake();
    }
    // Update is called once per frame
    void Update()
    {
        if (objectIsBeingGrabbed && !isRestoring)
        {
            SaveState(grabbedObject);
        }

        // Reads keyboard controls to trigger undo/redo (for simulator testing)
        if (Keyboard.current.zKey.isPressed && !Keyboard.current.xKey.isPressed)
        {
            Undo();
        }
        if (Keyboard.current.zKey.wasReleasedThisFrame)
        {
            isRestoring = false;
        }
        if (Keyboard.current.xKey.isPressed && !Keyboard.current.zKey.isPressed)
        {
            Redo();
        }
        if (Keyboard.current.xKey.wasReleasedThisFrame)
        {
            isRestoring = false;
        }
    }

    // Reads Meta Controller input to trigger undo/redo
    public override void OnUndoInput(bool isHeld, bool justPressed)
    {
        // Reacts to holding down X
        if (!isHeld)
        {
            Undo();
        }
    }

    public override void OnRedoInput(bool isHeld, bool justPressed)
    {
        // Reacts to holding down Y
        if (!isHeld)
        {
            Redo();
        }
    }
    
    // Subscribe and Unsubscribe to GrabEvents
    private void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnObjectGrab);
        GrabEventSystem.OnRelease.AddListener(OnObjectRelease);
    }
    private void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnObjectGrab);
        GrabEventSystem.OnRelease.RemoveListener(OnObjectRelease);
    }

    // Grab Events for object grab and object release
    private void OnObjectGrab(GameObject grabbedObject, string hand)
    {
        // Check if grabbed object is undoable
        UndoableObject undoableObject = grabbedObject.GetComponentInParent<UndoableObject>();
        if (undoableObject != null)
        {   
            objectIsBeingGrabbed = true;
            // Clears object history if grabbed object is not object in current history
            if (undoableObject != this.grabbedObject)
            {
                objectHistory.Clear();
                SaveInitialState(undoableObject);
            }
        }
    }
    private void OnObjectRelease(GameObject grabbedObject, string hand)
    {
        objectIsBeingGrabbed = false;
    }

    // Undo and Redo functions
    public void Undo()
    {
        if (currIndex < 0)
        {
            Debug.Log($"Nothing to undo.");
            return; 
        }
        isRestoring = true;

        // Iterate backwards through the Object History
        ObjectState savedState = objectHistory[--currIndex];
        savedState.RestoreState();
    }

    public void Redo()
    {
        if (currIndex > objectHistory.Count - 1)
        {
            Debug.Log($"Nothing to redo.");
            return; 
        }
        
        // Iterate forwards through the object history list
        ObjectState savedState = objectHistory[++currIndex];
        savedState.RestoreState();
    }

    // Save state functions
    public void SaveInitialState(UndoableObject gameObject)
    {
        Debug.Log($"NEW object {gameObject} grabbed");
        this.grabbedObject = gameObject;

        // Add initial state to object history
        objectHistory.Add(new ObjectState(gameObject));
        currIndex = objectHistory.Count - 1;
    }

    public void SaveState(UndoableObject gameObject)
    {   
        // Compare current state with most recent saved state
        ObjectState current = new ObjectState(gameObject);
        ObjectState recent = objectHistory[objectHistory.Count - 1];
        // Save if object state is different from the most current save state
        if (current.savedPosition != recent.savedPosition || current.savedRotation != recent.savedRotation || current.savedScale != recent.savedScale)
        {
            objectHistory.Add(current);
            currIndex = objectHistory.Count - 1;
        }
    }
}
