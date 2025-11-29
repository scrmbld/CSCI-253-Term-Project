using System;
using System.Threading.Tasks.Sources;
using UnityEngine;
using TaskShape;
using System.Collections.Generic;

public class UndoTestManager : MonoBehaviour
{
    public static UndoTestManager Instance { get; private set; }
    public UndoMetrics metrics = new UndoMetrics();
    
    [Header("Item Checkpoint Sequences")]
    public CheckpointSequence[] checkpointSequences;

    // Tracks when an object is initially grabbed or released for precise checkpoint placement 
    private HashSet<GameObject> objectsBeingHeld = new HashSet<GameObject>();
    private bool allowGoalCheck = false;
    private float allowGoalCheckTimer = 0f;
    private const float window = 0.1f;

    private bool taskStarted = false;
    private bool taskCompleted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"More than one UndoTestManager exists in the scene. Destroying {name}, keeping {Instance.name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"{name} initialized.");

        taskStarted = true;   

        // Initializes each checkpoint sequence
            // Each sequence: An item object, its goal item, and each checkpoint the goal is placed at
        foreach(CheckpointSequence sequence in checkpointSequences)
        {
            if (sequence.item == null || sequence.goal == null)
            {
                continue;
            }

            // Set the ItemObject's goal
            ItemObject itemObject = sequence.item.GetComponent<ItemObject>();
            if (itemObject != null)
            {
                itemObject.goalObject = sequence.goal;
            }

            // currIndex = current checkpoint in the sequence (starts at 0)
            sequence.currIndex = 0;
            sequence.isComplete = false;

            // Sets the current checkpoint position to transform data at currIndex
            sequence.MoveGoalToCurrentCheckpoint();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Event listeners
    void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnGrab);
        GrabEventSystem.OnRelease.AddListener(OnRelease);
        ItemEventSystem.GoalReached.AddListener(OnGoalReached);
    }
    void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnGrab);
        GrabEventSystem.OnRelease.RemoveListener(OnRelease);
        ItemEventSystem.GoalReached.RemoveListener(OnGoalReached);
    }
    private void OnGrab(GameObject grabbedObj, string hand)
    {
        objectsBeingHeld.Add(grabbedObj);
    }

    private void OnRelease(GameObject releasedObj, string hand)
    {
        objectsBeingHeld.Remove(releasedObj);
    }
    private void OnGoalReached(GameObject item, GameObject goal)
    {
        if (!taskStarted || taskCompleted) 
        { 
            Debug.Log($"taskStarted: {taskStarted}, taskCompleted: {taskCompleted}");
            return; 
        }

        // Rejects goal if player is still holding object 
        // (Must place the object to count)
        if (objectsBeingHeld.Contains(item)) { return; }

        // Rejects goal if placement isn't a recent undo/redo restoration 
        // (Can't undo into the path of a moving checkpoint and wait for the checkpoint to meet the object)
        if (!allowGoalCheck || Time.time - allowGoalCheckTimer > window) { return; }

        allowGoalCheck = false;

        // Item was correctly placed on the goal -> accept and process checkpoint
        ProcessCheckpoint(item, goal);
    }

    private void ProcessCheckpoint(GameObject item, GameObject goal)
    {
        foreach (CheckpointSequence sequence in checkpointSequences)
        {
            // Reached goal is a checkpoint, but not necessarily the final goal
            if (sequence.item == item && sequence.goal == goal && !sequence.isComplete)
            {
                CheckpointReached(sequence);
                break;
            }
        }
    }

    public void AllowGoalCheck()
    {
        allowGoalCheck = true;
        allowGoalCheckTimer = Time.time;
    }

    private void CheckpointReached(CheckpointSequence sequence)
    {
        Debug.Log($"Checkpoint {sequence.currIndex} reached for {sequence.item.name}");

        // Check if this is the last checkpoint
        if (sequence.currIndex >= sequence.checkpoints.Length - 1)
        {
            sequence.Complete();
            Debug.Log($"{sequence.item.name} task complete");
            sequence.HideItem(sequence.item);
            if (AllSequencesComplete())
            {
                CompleteTask();
            }

            return;
        }

        // Not the final checkpoint -> move to next checkpoint
        sequence.currIndex++;
        sequence.MoveGoalToCurrentCheckpoint();
    }

    private bool AllSequencesComplete()
    {
        foreach (CheckpointSequence sequence in checkpointSequences)
        {
            if (!sequence.isComplete) { return false; }
        }
        return true;
    }

    private void CompleteTask()
    {
        taskCompleted = true;
        float completionTime = Time.time - metrics.StartTime();

        Debug.Log($"Task Complete in {completionTime} seconds");

        Debug.Log($"Total Grab Count: {metrics.ReturnGrabCount()}");
        Debug.Log($"Total Undo Count: {metrics.ReturnUndoCount()}");
        Debug.Log($"Total Redo Count: {metrics.ReturnRedoCount()}");
        if (metrics.ReturnScrubTime() > 0) { Debug.Log($"Total Scrub Time (Experimental only): {metrics.ReturnScrubTime()}"); }
        Debug.Log($"Error Correction Rate: {metrics.ReturnErrorCorrectionRate()}");

        metrics.SaveToCSV(completionTime);
    }
}
