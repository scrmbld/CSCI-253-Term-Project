using System;
using System.Threading.Tasks.Sources;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public Target[] targets;
    
    private bool levelStarted = false;
    private bool levelCompleted = false;

    public UndoMetrics metrics = new UndoMetrics();

    // Metrics data
    [SerializeField] private string timeElapsed = "0 seconds";
    [SerializeField] private int grabCount = 0;
    [SerializeField] private int undoCount = 0;
    [SerializeField] private int redoCount = 0;
    [SerializeField] private float errorCorrectionRate = 0.0f;

    private void SyncDebugMetrics()
    {
        timeElapsed = ($"{Math.Round(Time.time - metrics.StartTime())} seconds");
        grabCount = metrics.ReturnGrabCount();
        undoCount = metrics.ReturnUndoCount();
        redoCount = metrics.ReturnRedoCount();
        errorCorrectionRate = 1f * metrics.ReturnErrorCorrectionRate();
    }
    
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelStarted = true;   
    }

    // Update is called once per frame
    void Update()
    {
        if (!levelCompleted)
        {
            SyncDebugMetrics();     
        }
        if (!levelStarted || levelCompleted) { return; }
        bool taskComplete = true;
        taskComplete = false; // for testing, delete when task conditions are added

        foreach (Target t in targets)
        {
            if (!t.isSatisfied)
            {
                taskComplete = false;
                break;
            }
        }

        if (taskComplete)
        {
            levelCompleted = true;
            float completionTime = Time.time - metrics.StartTime();
            Debug.Log($"LEVEL COMPLETE IN {completionTime:F2} seconds");
            Debug.Log($"Total Object Grabs: {metrics.ReturnGrabCount()}");
            Debug.Log($"Total Undo: {metrics.ReturnUndoCount()}");
            Debug.Log($"Total Redo: {metrics.ReturnRedoCount()}");
            Debug.Log($"Error Correction Rate: {metrics.ReturnErrorCorrectionRate()}");
        }
    }
}
