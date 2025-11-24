using System;
using System.Threading.Tasks.Sources;
using UnityEngine;
using TaskShape;

public class UndoTestManager : MonoBehaviour
{
    public static UndoTestManager Instance { get; private set; }
    public UndoMetrics metrics = new UndoMetrics();
    
    // Track checkpoints here??
    // Dictionary<GameObject, int> itemCheckpointIndex;

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        taskStarted = true;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
