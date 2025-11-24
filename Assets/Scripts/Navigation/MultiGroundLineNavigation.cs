using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MultiGroundLineNavigation : MonoBehaviour
{
    [Header("Pairs of objects (same index = same pair)")]
    public Transform[] startPoints;
    public Transform[] targetPoints;

    [Header("Line settings")]
    public float lineWidth = 0.03f;
    public float groundOffsetY = 0.05f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    void Update()
    {
        int pairCount = Mathf.Min(startPoints.Length, targetPoints.Length);

        if (pairCount <= 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Two points (start, target) per pair
        int pointCount = pairCount * 2;
        lineRenderer.positionCount = pointCount;

        int idx = 0;

        for (int i = 0; i < pairCount; i++)
        {
            Transform start = startPoints[i];
            Transform target = targetPoints[i];

            if (start == null || target == null)
            {
                // Keep positions but skip nulls
                continue;
            }

            Vector3 startPos = start.position;
            Vector3 targetPos = target.position;

            // Lift slightly above ground
            startPos.y += groundOffsetY;
            targetPos.y += groundOffsetY;

            // Add to line positions
            lineRenderer.SetPosition(idx++, startPos);
            lineRenderer.SetPosition(idx++, targetPos);
        }
    }
}

