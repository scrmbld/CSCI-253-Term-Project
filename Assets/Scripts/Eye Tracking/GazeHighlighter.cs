using UnityEngine;

/// <summary>
/// Central gaze raycaster:
/// - Shoots a ray from MetaEyeTrackingBridge gaze
/// - Finds a GazeHighlightable under the hit
/// - Turns highlight on for that one, off for the previous one
/// </summary>
public class GazeHighlighter : MonoBehaviour
{
    [Header("Eye tracking (from MetaEyeTrackingBridge)")]
    public MetaEyeTrackingBridge eyeBridge;
    public float maxDistance = 20f;
    public LayerMask gazeLayerMask = ~0;  // default: everything

    private GazeHighlightable currentHighlighted;

    void Update()
    {
        if (eyeBridge == null)
        {
            ClearHighlight();
            return;
        }

        Vector3 origin = eyeBridge.GazeOrigin;
        Vector3 dir = eyeBridge.GazeDirection;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, gazeLayerMask))
        {
            GazeHighlightable candidate =
                hit.collider.GetComponentInParent<GazeHighlightable>();

            if (candidate != currentHighlighted)
            {
                // Turn off previous
                if (currentHighlighted != null)
                    currentHighlighted.SetHighlighted(false);

                currentHighlighted = candidate;

                // Turn on new
                if (currentHighlighted != null)
                    currentHighlighted.SetHighlighted(true);
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void ClearHighlight()
    {
        if (currentHighlighted != null)
        {
            currentHighlighted.SetHighlighted(false);
            currentHighlighted = null;
        }
    }
}
