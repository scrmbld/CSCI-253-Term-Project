using UnityEngine;

/// <summary>
/// Simple bridge that combines left & right eye into one "gaze" anchor
/// and optionally draws a debug ray.
/// Assumes LeftEyeAnchor & RightEyeAnchor are driven by OVREyeGaze.
/// </summary>
public class MetaEyeTrackingBridge : MonoBehaviour
{
    [Header("Eye anchors driven by OVREyeGaze")]
    public Transform leftEye;
    public Transform rightEye;

    [Header("Output")]
    public Transform combinedEye;     // e.g. CombinedEyeAnchor

    [Header("Debug")]
    public bool drawDebugRay = true;
    public float debugRayLength = 5f;

    public Vector3 GazeOrigin =>
        combinedEye != null ? combinedEye.position :
        leftEye != null ? leftEye.position :
        Vector3.zero;

    public Vector3 GazeDirection =>
        combinedEye != null ? combinedEye.forward :
        leftEye != null ? leftEye.forward :
        Vector3.forward;

    void LateUpdate()
    {
        if (leftEye == null || rightEye == null || combinedEye == null)
            return;

        // Position halfway between the two eyes
        combinedEye.position = (leftEye.position + rightEye.position) * 0.5f;

        // Orientation halfway between the two eye rotations
        combinedEye.rotation = Quaternion.Slerp(
            leftEye.rotation,
            rightEye.rotation,
            0.5f);

        if (drawDebugRay)
        {
            Debug.DrawRay(
                combinedEye.position,
                combinedEye.forward * debugRayLength,
                Color.green);
        }
    }
}
