using UnityEngine;

/// <summary>
/// Raycaster for controller:
/// - Shoots a ray from the controller forward
/// - Finds a GazeHighlightable under the hit
/// - Turns highlight on for that one, off for the previous one
/// </summary>
public class ControllerHighlighter : MonoBehaviour
{
    [Header("Controller ray source")]
    public Transform controllerTransform;   // assign Right Controller here in the Inspector
    public float maxDistance = 20f;
    public LayerMask controllerLayerMask = ~0;  // default: everything

    private GazeHighlightable currentHighlighted;

    void Update()
    {
        if (controllerTransform == null)
        {
            ClearHighlight();
            return;
        }

        Vector3 origin = controllerTransform.position;
        Vector3 dir = controllerTransform.forward;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, controllerLayerMask))
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
