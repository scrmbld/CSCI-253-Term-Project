using System;
using System.Threading.Tasks.Sources;
using UnityEngine;
using TaskShape;

public class UndoTestManager : MonoBehaviour
{
    public static UndoTestManager Instance { get; private set; }
    public UndoMetrics metrics = new UndoMetrics();
    
    [Header("Item Checkpoint Sequences")]
    public CheckpointSequence[] checkpointSequences;


    private bool taskStarted = false;
    private bool taskCompleted = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"More than one LevelManager exists in the scene. Destroying {name}, keeping {Instance.name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"{name} initialized.");

        taskStarted = true;   

        foreach(CheckpointSequence sequence in checkpointSequences)
        {
            if (sequence.item == null || sequence.goal == null)
            {
                continue;
            }

            // Set the ItemObject goal
            ItemObject itemObject = sequence.item.GetComponent<ItemObject>();
            if (itemObject != null)
            {
                itemObject.goalObject = sequence.goal;
            }

            sequence.currIndex = 0;
            sequence.isComplete = false;
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
        ItemEventSystem.GoalReached.AddListener(OnGoalReached);
    }
    void OnDisable()
    {
        ItemEventSystem.GoalReached.RemoveListener(OnGoalReached);
    }
    private void OnGoalReached(GameObject item, GameObject goal)
    {
        // update checkpoint state for the item
        // move goal to next checkpoint/mark this task as finished
        // when all required tasks are done -> end level and save metrics:

        // float completionTime = 
        // metrics.SaveToCSV(completionTime);

        if (!taskStarted || taskCompleted)
        {
            return;
        }

        foreach (CheckpointSequence sequence in checkpointSequences)
        {
            // Reached goal is a checkpoint, but not necessarily the final goal
            if (sequence.item == item && sequence.goal == goal && !sequence.isComplete)
            {
                CheckpointReached(sequence);
                break;
            }

            // After checkpoint reached, check if it was the final goal (all tasks complete)
            if (AllSequencesComplete())
            {
                CompleteTask();
            }
        }
    }

    private void CheckpointReached(CheckpointSequence sequence)
    {
        Debug.Log($"Checkpoint {sequence.currIndex} reached for {sequence.item.name}");

        // Check if this is the last checkpoint
        if (sequence.currIndex >= sequence.checkpoints.Length - 1)
        {
            sequence.isComplete = true;
            Debug.Log($"{sequence.item.name} task complete");
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
