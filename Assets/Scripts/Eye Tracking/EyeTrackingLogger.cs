using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class EyeTrackingLogger : MonoBehaviour
{
    [Header("Eye Anchors")]
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;
    public Transform combinedEyeAnchor;

    [Header("Gaze Raycast")]
    public float maxGazeDistance = 25f;
    public LayerMask gazeLayerMask = ~0; // everything by default

    [Header("Fixation Detection (for Combined Eye)")]
    [Tooltip("Max change in gaze direction (degrees) to still count as the same fixation.")]
    public float fixationMaxAngle = 1.5f;

    [Tooltip("Minimum duration (seconds) before a gaze is counted as a fixation.")]
    public float fixationMinDuration = 0.1f;

    private string rawSamplesPath;
    private string fixationPath;

    private Fixation currentFixation;

    private class Fixation
    {
        public string aoiName;
        public float startTime;
        public Vector3 lastDir;
    }

    void Awake()
    {
        // Create ExperimentalData folder in a safe location on device
        string folder = Path.Combine(Application.persistentDataPath, "ExperimentalData");
        Directory.CreateDirectory(folder);

        string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        rawSamplesPath = Path.Combine(folder, $"GazeSamples_{ts}.csv");
        fixationPath = Path.Combine(folder, $"Fixations_{ts}.csv");

        // CSV headers
        File.WriteAllText(rawSamplesPath,
            "time,eye,origin_x,origin_y,origin_z,dir_x,dir_y,dir_z,hit_object,hit_x,hit_y,hit_z\n");

        File.WriteAllText(fixationPath,
            "aoi,start_time,end_time,duration\n");
    }

    void Update()
    {
        float t = Time.time;

        LogEyeSample(t, "Left", leftEyeAnchor);
        LogEyeSample(t, "Right", rightEyeAnchor);
        LogEyeSample(t, "Combined", combinedEyeAnchor, doFixationLogic: true);
    }

    private void LogEyeSample(float t, string eyeLabel, Transform eyeAnchor, bool doFixationLogic = false)
    {
        if (eyeAnchor == null) return;

        Vector3 origin = eyeAnchor.position;
        Vector3 dir = eyeAnchor.forward;

        RaycastHit hit;
        string hitObjectName = "";
        Vector3 hitPoint = Vector3.zero;

        if (Physics.Raycast(origin, dir, out hit, maxGazeDistance, gazeLayerMask))
        {
            hitObjectName = hit.collider.gameObject.name;
            hitPoint = hit.point;
        }

        // Append raw gaze sample
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0:F4},{1},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8},{9:F4},{10:F4},{11:F4}\n",
            t, eyeLabel,
            origin.x, origin.y, origin.z,
            dir.x, dir.y, dir.z,
            hitObjectName,
            hitPoint.x, hitPoint.y, hitPoint.z);

        File.AppendAllText(rawSamplesPath, line);

        if (doFixationLogic)
        {
            UpdateFixation(t, dir, hitObjectName);
        }
    }

    private void UpdateFixation(float t, Vector3 currentDir, string currentAOI)
    {
        // If we're not hitting anything, end any current fixation
        if (string.IsNullOrEmpty(currentAOI))
        {
            EndCurrentFixation(t);
            return;
        }

        if (currentFixation == null)
        {
            // Start new fixation candidate
            currentFixation = new Fixation
            {
                aoiName = currentAOI,
                startTime = t,
                lastDir = currentDir
            };
        }
        else
        {
            float angle = Vector3.Angle(currentFixation.lastDir, currentDir);

            // Still looking at same AOI and direction hasn't changed too much
            if (currentAOI == currentFixation.aoiName && angle <= fixationMaxAngle)
            {
                currentFixation.lastDir =
                    Vector3.Lerp(currentFixation.lastDir, currentDir, 0.5f);
            }
            else
            {
                // Gaze jumped: finish old fixation, start a new one
                EndCurrentFixation(t);
                currentFixation = new Fixation
                {
                    aoiName = currentAOI,
                    startTime = t,
                    lastDir = currentDir
                };
            }
        }
    }

    private void EndCurrentFixation(float tNow)
    {
        if (currentFixation == null) return;

        float dur = tNow - currentFixation.startTime;
        if (dur >= fixationMinDuration)
        {
            // Log fixation to CSV
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0},{1:F4},{2:F4},{3:F4}\n",
                currentFixation.aoiName,
                currentFixation.startTime,
                tNow,
                dur);

            File.AppendAllText(fixationPath, line);
        }

        currentFixation = null;
    }

    void OnApplicationQuit()
    {
        EndCurrentFixation(Time.time);
    }

    void OnDestroy()
    {
        EndCurrentFixation(Time.time);
    }
}
