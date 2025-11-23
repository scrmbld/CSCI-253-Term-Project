using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;


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
    private float scrubTime = 0;
    private float scrubStart;

    private const string fileName = "undo_quantitative_metrics.csv";

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

    public void RecordScrubCount()
    {
        scrubStart = Time.time;
        Debug.Log($"Scrub start time; {scrubStart}");
    }
    public void SaveScrubTime()
    {
        scrubTime += Time.time - scrubStart;
        Debug.Log($"Scrub end time; {Time.time}");
        Debug.Log($"Time scrubbing: {scrubTime}");
    }

    // Return funtions
    public float ReturnScrubTime()
    {
        return scrubTime;
    }
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

    public void SaveToCSV(float completionTime)
    {
        string folder = Application.persistentDataPath;
        string path = Path.Combine(folder, fileName);

        if (!File.Exists(path))
        {
            string header = "Condition,CompletionTimeSeconds,TotalScrubTime,TotalGrabs,TotalUndo,TotalRedo\n";
            File.WriteAllText(path, header);
        }
        string condition = scrubTime > 0 ? "Experimental" : "Control";

        string row = $"{condition},{completionTime:F2},{scrubTime:F2},{totalGrabCount},{totalUndoCount},{totalRedoCount}";

        File.AppendAllText(path, row);
        Debug.Log($"{fileName} wrote to {path}");
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
