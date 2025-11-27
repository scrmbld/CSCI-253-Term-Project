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
    public LayerMask gazeLayerMask = ~0;

    [Header("Fixation Detection (Combined Eye)")]
    public float fixationMaxAngle = 1.5f;
    public float fixationMinDuration = 0.1f;

    [Header("Participant Info")]
    public string participantName = "default";   // Type the name in the Inspector

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
    rootFolder = Path.Combine(Application.dataPath, "ExperimentalData");
#else
        rootFolder = Path.Combine(Application.persistentDataPath, "ExperimentalData");
#endif

        Directory.CreateDirectory(rootFolder);

        // Clean participant name (optional)
        string cleanName = string.IsNullOrWhiteSpace(participantName)
            ? "participant"
            : participantName.Replace(" ", "_");

        string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        rawSamplesPath = Path.Combine(rootFolder, $"{cleanName}_GazeSamples_{ts}.csv");
        fixationPath = Path.Combine(rootFolder, $"{cleanName}_Fixations_{ts}.csv");

        Debug.Log($"[EyeTrackingLogger] Writing gaze samples to: {rawSamplesPath}");
        Debug.Log($"[EyeTrackingLogger] Writing fixations to: {fixationPath}");

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

        try
        {
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0:F4},{1},{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8},{9:F4},{10:F4},{11:F4}\n",
                t, eyeLabel,
                origin.x, origin.y, origin.z,
                dir.x, dir.y, dir.z,
                hitObjectName,
                hitPoint.x, hitPoint.y, hitPoint.z);

            File.AppendAllText(rawSamplesPath, line);
        }
        catch (Exception e)
        {
            Debug.LogError("[EyeTrackingLogger] Failed to write gaze sample: " + e);
        }

        if (doFixationLogic)
        {
            UpdateFixation(t, dir, hitObjectName);
        }
    }

    private void UpdateFixation(float t, Vector3 currentDir, string currentAOI)
    {
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
            try
            {
                string line = string.Format(CultureInfo.InvariantCulture,
                    "{0},{1:F4},{2:F4},{3:F4}\n",
                    currentFixation.aoiName,
                    currentFixation.startTime,
                    tNow,
                    dur);

                File.AppendAllText(fixationPath, line);
            }
            catch (Exception e)
            {
                Debug.LogError("[EyeTrackingLogger] Failed to write fixation: " + e);
            }
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
