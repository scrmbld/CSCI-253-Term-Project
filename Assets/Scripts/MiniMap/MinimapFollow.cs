using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // Drag XR Origin (VR) or your Player root here

    [Header("Follow Settings")]
    public float height = 2f;                // Fixed Y height for the ICON (not 50!)
    public Vector2 offsetXZ = Vector2.zero;  // Optional map offset in meters
    public float followSmooth = 10f;         // Higher = snappier follow

    [Header("Rotation")]
    public bool rotateWithTarget = true;     // Arrow points with player's heading
    public float rotationSmooth = 10f;       // Higher = snappier rotation

    [Header("Startup")]
    public bool snapOnStart = true;          // Snap immediately on enable to avoid 1-frame drift

    void Awake()
    {
        // Prefer explicit drag in Inspector; auto-find only if empty
        if (!target)
        {
            var go = GameObject.FindWithTag("Player");
            if (go) target = go.transform;
        }
    }

    void OnEnable()
    {
        if (snapOnStart && target)
        {
            var p = target.position;
            transform.position = new Vector3(p.x + offsetXZ.x, height, p.z + offsetXZ.y);
            transform.rotation = Quaternion.Euler(90f, rotateWithTarget ? target.eulerAngles.y : 0f, 0f);
        }
    }

    void LateUpdate()
    {
        if (!target) return;

        // --- POSITION (lock Y so the icon never flies to 49/50) ---
        Vector3 desired = new Vector3(
            target.position.x + offsetXZ.x,
            height,
            target.position.z + offsetXZ.y
        );

        float pf = (followSmooth <= 0f) ? 1f : 1f - Mathf.Exp(-followSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, pf);

        // --- ROTATION: flat sprite (X=90), yaw follows player if enabled ---
        float yaw = rotateWithTarget ? target.eulerAngles.y : 0f;
        Quaternion desiredRot = Quaternion.Euler(90f, yaw, 0f);
        float rf = (rotationSmooth <= 0f) ? 1f : 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rf);
    }

    void OnValidate()
    {
        if (height < 0.5f) height = 0.5f;    // guard against accidental tiny/negative heights
        if (followSmooth < 0f) followSmooth = 0f;
        if (rotationSmooth < 0f) rotationSmooth = 0f;
    }
}
