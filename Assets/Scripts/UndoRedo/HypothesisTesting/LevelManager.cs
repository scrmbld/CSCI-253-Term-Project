using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Target[] targets;
    private float startTime;
    private bool levelStarted = false;
    private bool levelCompleted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
        levelStarted = true;   
    }

    // Update is called once per frame
    void Update()
    {
        if (!levelStarted || levelCompleted) { return; }
        bool taskComplete = true;

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
            float completionTime = Time.time - startTime;
            Debug.Log($"LEVEL COMPLETE IN {completionTime:F2} seconds");
            // Debug.Log($"Total Undo: {UndoManager.Instance.TotalUndoCount}, Total Redo: {UndoManager.Instance.TotalRedoCount}");
            // Consider logging total object manipulation count and possibly a comparison of undo/redo vs object manipulation count
        }
        
    }
}
