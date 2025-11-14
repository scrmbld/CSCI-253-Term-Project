using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class UndoManagerExperimental : UndoManager
{
    // Serialized for debugging
    [SerializeField] private List<ObjectState> objectHistory = new List<ObjectState>();

    protected override void Awake()
    {
        // Guarantees only one UndoManager can exist at once (so control and experimental can't conflict)
        base.Awake();
    }
    // Update is called once per frame
    void Update()
    {
        // TODO : Scrub through object history while grabbing
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
        // Save state code
    }

    public override void Undo()
    {
        // Move backwards through the object history list
    }

    public override void Redo()
    {
        // Move forwards through the object history lsit
    }
    
    private void LogState(string message, ObjectState newState)
    {
        Debug.Log($"{message}\nPosition: {newState.savedPosition}; Rotation: {newState.savedRotation}; Scale: {newState.savedScale}");
    }
}
