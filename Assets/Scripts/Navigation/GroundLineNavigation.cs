using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GroundLineNavigation : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (startPoint != null && targetPoint != null)
        {
            // Get positions
            Vector3 startPos = startPoint.position;
            Vector3 targetPos = targetPoint.position;

            // Slightly above ground so it doesn’t Z-fight
            startPos.y += 0.05f;
            targetPos.y += 0.05f;

            // Apply to line
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, targetPos);
        }
    }
}