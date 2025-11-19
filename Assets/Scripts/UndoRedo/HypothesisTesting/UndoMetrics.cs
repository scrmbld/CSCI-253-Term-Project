using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


// TODO : ADD METRIC INCREMENTS FOR UNDO/REDO (both control and experimental)
// Then check list in ChatGPT for what's next

public class UndoMetrics //: MonoBehaviour
{
    // Grab/Undo/Redo counts for testing metrics:
    public int totalUndoCount = 0;
    private int totalRedoCount = 0;
    private int totalGrabCount = 0;
    private float errorCorrectionRate = 0f; // Lower = fewer error corrections, Higher = more error corrections
    private float startTime;

    // Increment functions
    public void AddGrab()
    {   
        totalGrabCount++;
    }

    public void AddUndoCount()
    {
        totalUndoCount++;
    }

    public void AddRedoCount()
    {
        totalRedoCount++;
    }

    // Return funtions
    public int ReturnGrabCount()
    {
        return totalGrabCount;
    }
    public int ReturnUndoCount()
    {
        return totalUndoCount;
    }
    public int ReturnRedoCount()
    {
        return totalRedoCount;
    }
    public float ReturnErrorCorrectionRate()
    {
        if (totalGrabCount == 0)
        {
            return 0.0f;
        }
        else
        {
            errorCorrectionRate = 1f * (totalUndoCount + totalRedoCount) / totalGrabCount;
            return errorCorrectionRate;
        }
    }
    public float StartTime()
    {
        return startTime;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
