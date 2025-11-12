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
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        if (startPoint != null && targetPoint != null)
        {
            lineRenderer.SetPosition(0, startPoint.position);
            lineRenderer.SetPosition(1, targetPoint.position);
        }
    }
}
