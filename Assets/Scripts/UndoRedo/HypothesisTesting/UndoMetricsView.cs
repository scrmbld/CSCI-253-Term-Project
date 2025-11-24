using System;
using System.Threading.Tasks.Sources;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using UnityEngine;

public class UndoMetricsView : MonoBehaviour
{
    // References the original task metrics, copies them into serialize fields for easy viewing in Unity
    UndoMetrics metrics;

    [SerializeField] private string timeElapsed = "0 seconds";
    [SerializeField] private string scrubTime = "0 seconds";
    [SerializeField] private int grabCount = 0;
    [SerializeField] private int undoCount = 0;
    [SerializeField] private int redoCount = 0;
    [SerializeField] private float errorCorrectionRate = 0.0f;

    void Start()
    {
        metrics = UndoTestManager.Instance.metrics;
    }
    void Update()
    {
        SyncMetrics();
    }

    // Keeps the views updated
    private void SyncMetrics()
    {
        timeElapsed = ($"{Math.Round(Time.time - metrics.StartTime())} seconds");
        scrubTime = ($"{metrics.ReturnScrubTime()} seconds");
        grabCount = metrics.ReturnGrabCount();
        undoCount = metrics.ReturnUndoCount();
        redoCount = metrics.ReturnRedoCount();
        errorCorrectionRate = 1f * metrics.ReturnErrorCorrectionRate();
    }
}