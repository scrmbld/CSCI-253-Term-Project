using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Processes raw samples from EyeDataRecorder and computes AOI-level metrics:
/// - fixation count
/// - total dwell time (ms)
/// - mean fixation duration (ms)
/// - time to first fixation (ms)
/// - revisits
///
/// Uses a simple consecutive-sample angular threshold clustering to form fixations.
/// Attach to the same GameObject as EyeDataRecorder or call manually after a session.
/// </summary>
public class MetricsAggregator : MonoBehaviour
{
    [Tooltip("Reference to the recorder that collected samples.")]
    public EyeDataRecorder recorder;

    [Header("Fixation detection (tweakable)")]
    [Tooltip("Angle (degrees) threshold between consecutive gaze directions to consider same fixation.")]
    public float angleThresholdDegrees = 1.5f;

    [Tooltip("Minimum fixation duration in ms. Set to 0 for no minimum.")]
    public float minFixationDurationMs = 60f;

    [Header("Output")]
    [Tooltip("Max distance to consider an AOI hit part of the sample (ms).")]
    public float maxSampleToAOIDistance = 0.5f;

    /// <summary>
    /// Call (manually or from UI) to compute and save summary CSV.
    /// </summary>
    [ContextMenu("Compute & Save Summary")]
    public void ComputeAndSave()
    {
        if (recorder == null)
        {
            Debug.LogError("[MetricsAggregator] No recorder assigned.");
            return;
        }

        // Group samples by eye or combine? You asked to log both eyes separately and no fused metrics for now.
        // We'll compute AOI metrics by considering any sample (left or right) hitting the AOI.
        var samples = recorder.samples.OrderBy(s => s.timestampMs).ToList();
        if (samples.Count == 0)
        {
            Debug.LogWarning("[MetricsAggregator] No samples found.");
            return;
        }

        // Build a set of AOI names that actually occurred
        var aois = new HashSet<string>(samples.Select(s => s.hitAOI).Where(n => !string.IsNullOrEmpty(n)));

        // Build per-AOI sample lists
        var samplesByAOI = new Dictionary<string, List<EyeDataRecorder.EyeSample>>();
        foreach (var a in aois) samplesByAOI[a] = new List<EyeDataRecorder.EyeSample>();
        foreach (var s in samples)
        {
            if (!string.IsNullOrEmpty(s.hitAOI))
            {
                samplesByAOI[s.hitAOI].Add(s);
            }
        }

        // For each AOI, compute fixations and dwell
        List<AOIMetrics> results = new List<AOIMetrics>();
        foreach (var kv in samplesByAOI)
        {
            string aName = kv.Key;
            var sList = kv.Value.OrderBy(x => x.timestampMs).ToList();
            // Detect fixations using simple angular clustering across samples (global)
            var fixations = DetectFixations(sList, angleThresholdDegrees, minFixationDurationMs);

            double totalDwellMs = fixations.Sum(f => f.durationMs);
            double avgFixMs = fixations.Count > 0 ? fixations.Average(f => f.durationMs) : 0.0;
            int revisitCount = Math.Max(0, fixations.Count - 1);

            // time to first fixation relative to first sample in entire session
            double sessionStart = samples.First().timestampMs;
            double timeToFirstFix = fixations.Count > 0 ? fixations[0].startMs - sessionStart : -1.0;

            results.Add(new AOIMetrics()
            {
                aoiName = aName,
                fixationCount = fixations.Count,
                totalDwellMs = totalDwellMs,
                avgFixationDurationMs = avgFixMs,
                timeToFirstFixMs = timeToFirstFix,
                revisits = revisitCount
            });
        }

        // Save CSV
        SaveSummaryCsv(results);
    }

    void SaveSummaryCsv(List<AOIMetrics> results)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string outDir = Path.Combine(projectRoot, recorder != null ? recorder.outputFolderName : "ExperimentalData");
        Directory.CreateDirectory(outDir);

        string ts = DateTime.Now.ToString("MM-dd_h_mm_tt");
        string fileName = $"Participant_{recorder.participantId}_{ts}_summary.csv";
        string path = Path.Combine(outDir, fileName);

        var sb = new StringBuilder();
        sb.AppendLine("participantId,aoiName,fixationCount,totalDwellMs,avgFixationDurationMs,timeToFirstFixMs,revisits");
        foreach (var r in results)
        {
            sb.AppendLine($"{recorder.participantId},{EscapeCsv(r.aoiName)},{r.fixationCount},{r.totalDwellMs:F1},{r.avgFixationDurationMs:F1},{r.timeToFirstFixMs:F1},{r.revisits}");
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log($"[MetricsAggregator] Summary CSV written: {path}");
    }

    static string EscapeCsv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        if (s.Contains(",") || s.Contains("\""))
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    /// <summary>
    /// Simple fixation struct for detected clusters.
    /// </summary>
    class Fixation
    {
        public double startMs;
        public double endMs;
        public double durationMs => endMs - startMs;
    }

    /// <summary>
    /// Detect fixations from sample list belonging to the same AOI.
    /// Uses consecutive-sample angular difference threshold.
    /// </summary>
    List<Fixation> DetectFixations(List<EyeDataRecorder.EyeSample> list, float angleThresholdDeg, float minDurationMs)
    {
        List<Fixation> fixs = new List<Fixation>();
        if (list.Count == 0) return fixs;

        Fixation current = null;
        Vector3 lastDir = Vector3.zero;
        bool haveLast = false;

        foreach (var s in list)
        {
            if (!haveLast)
            {
                // start new candidate
                current = new Fixation() { startMs = s.timestampMs, endMs = s.timestampMs };
                lastDir = s.direction;
                haveLast = true;
                continue;
            }

            float angle = Vector3.Angle(lastDir, s.direction); // degrees between frames
            if (angle <= angleThresholdDeg)
            {
                // extend fixation
                current.endMs = s.timestampMs;
            }
            else
            {
                // close current
                if (current.durationMs >= minDurationMs)
                    fixs.Add(current);

                // start new
                current = new Fixation() { startMs = s.timestampMs, endMs = s.timestampMs };
            }

            lastDir = s.direction;
        }

        // finalize last
        if (current != null && current.durationMs >= minDurationMs)
            fixs.Add(current);

        return fixs;
    }

    class AOIMetrics
    {
        public string aoiName;
        public int fixationCount;
        public double totalDwellMs;
        public double avgFixationDurationMs;
        public double timeToFirstFixMs;
        public int revisits;
    }
}
