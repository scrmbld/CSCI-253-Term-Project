using UnityEngine;

public class EyeGazeTimeTracker : MonoBehaviour
{
    [Header("Eye tracking")]
    [Tooltip("OVREyeGaze component on this eye gaze object")]
    public OVREyeGaze eyeGaze;

    [Tooltip("Layers that the eye ray can hit (Default = everything)")]
    public LayerMask gazeLayers = ~0;

    [Header("Target tagging")]
    [Tooltip("Tag used by all objects we care about (sphere, cube, etc.)")]
    public string targetTag = "GazeTarget";

    private float totalSessionTime = 0f;
    private float timeLookingAtTargets = 0f;

    private void Awake()
    {
        // Auto-grab OVREyeGaze if not wired in Inspector
        if (eyeGaze == null)
        {
            eyeGaze = GetComponent<OVREyeGaze>();
        }
    }

    private void Update()
    {
        // 1) Always accumulate total time while the game is running
        totalSessionTime += Time.deltaTime;

        // 2) If eye tracking is not available, don't accumulate target time
        if (eyeGaze == null || !eyeGaze.EyeTrackingEnabled)
            return;

        // Optional: ignore low-confidence frames
        if (eyeGaze.Confidence < eyeGaze.ConfidenceThreshold)
            return;

        // 3) Raycast from eye gaze object forward
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, gazeLayers))
        {
            // 4) Check if we hit a gaze target (sphere, cube, etc.)
            if (hit.collider.CompareTag(targetTag))
            {
                timeLookingAtTargets += Time.deltaTime;
            }
        }
    }

    public float GetGazeRatio()
    {
        if (totalSessionTime <= 0f)
            return 0f;

        return timeLookingAtTargets / totalSessionTime;
    }

    private void OnApplicationQuit()
    {
        // Log result at the end of the run
        float ratio = GetGazeRatio();
        Debug.Log($"Eye gaze stats: " +
                  $"targetTime = {timeLookingAtTargets:F2}s, " +
                  $"totalTime = {totalSessionTime:F2}s, " +
                  $"ratio = {ratio:F3}");
    }
}
