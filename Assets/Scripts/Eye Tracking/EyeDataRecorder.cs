using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Records eye samples (Left / Right) by polling the provided eye anchor Transforms.
/// Performs a physics raycast per sample to detect AOIs (colliders on gazeLayerMask).
/// Writes a RAW CSV as it runs and keeps samples in-memory for aggregation.
/// Suitable for Editor with Quest connected; saves to ProjectRoot/ExperimentalData by default.
/// </summary>
public class EyeDataRecorder : MonoBehaviour
{
    [Header("Session")]
    [Tooltip("Simple participant id string (e.g. 1 or P01) used for filenames.")]
    public string participantId = "1";

    [Header("Sampling")]
    [Tooltip("Target sample rate (Hz) for polling eye anchors.")]
    public float targetSampleRateHz = 70f;
    public float maxSampleDistance = 20f;

    [Header("Eye anchors (assign your eye anchor transforms)")]
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;

    [Header("Raycast / AOI")]
    [Tooltip("Layer mask for AOI objects (GazeTarget or similar).")]
    public LayerMask gazeLayerMask = ~0;

    [Header("Output")]
    [Tooltip("Folder relative to project root. Default: ExperimentalData")]
    public string outputFolderName = "ExperimentalData";

    // Internal
    float sampleInterval => 1f / Mathf.Max(1f, targetSampleRateHz);
    double lastSampleTime = 0f;
    StringBuilder rawCsvSb;
    StreamWriter rawWriter;
    string rawFilePath;

    // In-memory samples for later aggregation
    public List<EyeSample> samples = new List<EyeSample>();

    bool isRecording = false;

    void Start()
    {
        StartRecording();
    }

    void OnDisable()
    {
        // Auto-save on disable / scene stop
        if (isRecording) FinishAndSave();
    }

    /// <summary>
    /// Call to start recording.
    /// </summary>
    public void StartRecording()
    {
        // Prepare folder
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(projectRoot, outputFolderName);
        Directory.CreateDirectory(outDir);

        // Build filename
        string ts = DateTime.Now.ToString("MM-dd_h_mm_tt"); // e.g. 11-26_5_48_PM
        string fileName = $"Participant_{participantId}_{ts}_raw.csv";
        rawFilePath = Path.Combine(outDir, fileName);

        rawCsvSb = new StringBuilder();
        // Header
        rawCsvSb.AppendLine("participantId,timestampMs,eye,origin_x,origin_y,origin_z,dir_x,dir_y,dir_z,confidence,hitAOI,hitDistance");

        // Open writer
        rawWriter = new StreamWriter(rawFilePath, false, Encoding.UTF8);
        rawWriter.Write(rawCsvSb.ToString());
        rawWriter.Flush();

        samples.Clear();
        lastSampleTime = Time.realtimeSinceStartupAsDouble;
        isRecording = true;

        Debug.Log($"[EyeDataRecorder] Recording started. Writing raw CSV to: {rawFilePath}");
    }

    /// <summary>
    /// Call to stop and save. Also called automatically on Disable.
    /// </summary>
    [ContextMenu("Finish And Save")]
    public void FinishAndSave()
    {
        if (!isRecording) return;
        isRecording = false;

        // Close raw writer (we've been writing incrementally).
        if (rawWriter != null)
        {
            rawWriter.Flush();
            rawWriter.Close();
            rawWriter = null;
        }

        Debug.Log($"[EyeDataRecorder] Recording finished. Samples: {samples.Count}");
    }

    void Update()
    {
        if (!isRecording) return;

        double now = Time.realtimeSinceStartupAsDouble;
        if (now - lastSampleTime >= sampleInterval)
        {
            // We may sample multiple times if the update lagged
            int n = Mathf.FloorToInt((float)((now - lastSampleTime) / sampleInterval));
            for (int i = 0; i < Mathf.Max(1, n); ++i)
            {
                SampleOnce();
                lastSampleTime += sampleInterval;
            }
        }
    }

    void SampleOnce()
    {
        double timestampMs = Time.realtimeSinceStartupAsDouble * 1000.0;

        if (leftEyeAnchor != null) GatherEyeSample("L", leftEyeAnchor, timestampMs);
        if (rightEyeAnchor != null) GatherEyeSample("R", rightEyeAnchor, timestampMs);
    }

    void GatherEyeSample(string eyeLabel, Transform eyeAnchor, double timestampMs)
    {
        Vector3 origin = eyeAnchor.position;
        Vector3 dir = eyeAnchor.forward;

        // Attempt to read confidence from a component named OVREyeGaze if available (best-effort)
        float confidence = -1f;
        var ovrComp = eyeAnchor.GetComponentInChildren<Component>();
        if (ovrComp != null)
        {
            // try to reflectively read a 'confidence' or 'eyeConfidence' float field/property
            var t = ovrComp.GetType();
            var prop = t.GetProperty("confidence");
            if (prop != null && prop.PropertyType == typeof(float))
            {
                confidence = (float)prop.GetValue(ovrComp);
            }
            else
            {
                var field = t.GetField("confidence");
                if (field != null && field.FieldType == typeof(float))
                {
                    confidence = (float)field.GetValue(ovrComp);
                }
            }
        }

        // Raycast for AOI hits
        string hitName = "";
        float hitDistance = -1f;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxSampleDistance, gazeLayerMask))
        {
            hitDistance = hit.distance;
            var highlight = hit.collider.GetComponentInParent<MonoBehaviour>();
            // Prefer GazeHighlightable if present
            var gh = hit.collider.GetComponentInParent(typeof(Component));
            // We use collider's GameObject name as AOI label
            hitName = hit.collider.gameObject.name;
            // If parent has GazeHighlightable component, use its gameobject name (same)
        }

        // Build sample
        EyeSample s = new EyeSample()
        {
            participantId = participantId,
            timestampMs = timestampMs,
            eye = eyeLabel,
            origin = origin,
            direction = dir,
            confidence = confidence,
            hitAOI = hitName,
            hitDistance = hitDistance
        };

        samples.Add(s);

        // Append CSV line and flush quickly to ensure we don't lose data
        string line = $"{participantId},{timestampMs:F3},{eyeLabel},{origin.x:F6},{origin.y:F6},{origin.z:F6},{dir.x:F6},{dir.y:F6},{dir.z:F6},{confidence:F3},{EscapeCsv(hitName)},{hitDistance:F3}";
        rawWriter.WriteLine(line);
        rawWriter.Flush();
    }

    static string EscapeCsv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\""))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    /// <summary>
    /// Simple in-memory sample struct for later aggregation.
    /// </summary>
    [Serializable]
    public class EyeSample
    {
        public string participantId;
        public double timestampMs;
        public string eye; // "L" or "R"
        public Vector3 origin;
        public Vector3 direction;
        public float confidence;
        public string hitAOI;
        public float hitDistance;
    }
}
