using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class EyeTrackingLogger : MonoBehaviour
{
    [Header("Participant Info")]
    [Tooltip("Name that will be prefixed to the CSV files (e.g. palavi, rajesh).")]
    public string participantName = "participant";

    [Header("Eye Anchors")]
    public Transform leftEyeAnchor;
    public Transform rightEyeAnchor;
    public Transform combinedEyeAnchor;

    [Header("Gaze Raycast")]
    [Tooltip("Only layers in this mask will be considered as gaze targets. " +
             "Set this to include your 'Gazing' layer (and any others you care about).")]
    public LayerMask gazeLayerMask = ~0;
    public float maxGazeDistance = 25f;

    [Header("Fixation Detection (Combined Eye)")]
    public float fixationMaxAngle = 1.5f;
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
        string rootFolder;

#if UNITY_EDITOR
        // In editor: write inside the project so you can see it easily
        rootFolder = Path.Combine(Application.dataPath, "ExperimentalData");
#else
        // In builds / on device: use persistentDataPath
        rootFolder = Path.Combine(Application.persistentDataPath, "ExperimentalData");
#endif

        Directory.CreateDirectory(rootFolder);

        // Clean participant name for filenames
        string cleanName = string.IsNullOrWhiteSpace(participantName)
            ? "participant"
            : participantName.Replace(" ", "_");

        string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        rawSamplesPath = Path.Combine(rootFolder, $"{cleanName}_GazeSamples_{ts}.csv");
        fixationPath = Path.Combine(rootFolder, $"{cleanName}_Fixations_{ts}.csv");

        Debug.Log($"[EyeTrackingLogger] Writing gaze samples to: {rawSamplesPath}");
        Debug.Log($"[EyeTrackingLogger] Writing fixations to: {fixationPath}");

        // NOTE: added hit_layer column
        File.WriteAllText(rawSamplesPath,
            "time,eye,origin_x,origin_y,origin_z," +
            "dir_x,dir_y,dir_z," +
            "hit_object,hit_layer,hit_x,hit_y,hit_z\n");

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
        string hitLayerName = "";
        Vector3 hitPoint = Vector3.zero;

        // Raycast only against layers in gazeLayerMask
        if (Physics.Raycast(origin, dir, out hit, maxGazeDistance, gazeLayerMask))
        {
            GameObject hitObj = hit.collider.gameObject;
            hitObjectName = hitObj.name;
            hitLayerName = LayerMask.LayerToName(hitObj.layer);
            hitPoint = hit.point;
        }

        // Append raw gaze sample (now includes hit_layer)
        string line = string.Format(CultureInfo.InvariantCulture,
            "{0:F4},{1}," +         // time, eye
            "{2:F4},{3:F4},{4:F4}," + // origin
            "{5:F4},{6:F4},{7:F4}," + // direction
            "{8},{9},{10:F4},{11:F4},{12:F4}\n", // hit_object, hit_layer, hit position
            t, eyeLabel,
            origin.x, origin.y, origin.z,
            dir.x, dir.y, dir.z,
            hitObjectName,
            hitLayerName,
            hitPoint.x, hitPoint.y, hitPoint.z);

        File.AppendAllText(rawSamplesPath, line);

        if (doFixationLogic)
        {
            UpdateFixation(t, dir, hitObjectName);
        }
    }

    private void UpdateFixation(float t, Vector3 currentDir, string currentAOI)
    {
        // If we're not hitting anything (no AOI), end current fixation
        if (string.IsNullOrEmpty(currentAOI))
        {
            EndCurrentFixation(t);
            return;
        }

        if (currentFixation == null)
        {
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

            if (currentAOI == currentFixation.aoiName && angle <= fixationMaxAngle)
            {
                currentFixation.lastDir =
                    Vector3.Lerp(currentFixation.lastDir, currentDir, 0.5f);
            }
            else
            {
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
