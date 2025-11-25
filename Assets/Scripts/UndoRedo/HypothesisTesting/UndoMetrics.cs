using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UndoMetrics 
{
    private const string fileName = "undo_quantitative_metrics.csv";

    // Quantitative metrics
    public int totalUndoCount = 0;
    private int totalRedoCount = 0;
    private int totalGrabCount = 0;
    private float errorCorrectionRate = 0f; // Lower = fewer error corrections, Higher = more error corrections
    private float startTime;
    private float scrubTime = 0;
    private float scrubStart;

    // Increment/update functions
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
        string folder = Application.dataPath;
        string path = Path.Combine(folder, fileName);

        if (!File.Exists(path))
        {
            string header = "TaskNumber,Condition,CompletionTimeSeconds,TotalScrubTime,TotalGrabs,TotalUndo,TotalRedo\n";
            File.WriteAllText(path, header);
        }
        string condition = scrubTime > 0 ? "Experimental" : "Control";

        string row = $"{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name},{condition},{completionTime:F2},{scrubTime:F2},{totalGrabCount},{totalUndoCount},{totalRedoCount}";

        File.AppendAllText(path, row);
        Debug.Log($"{fileName} wrote to {path}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
    }
}
