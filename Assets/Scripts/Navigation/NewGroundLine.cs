using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class NewGroundLine : MonoBehaviour
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
            Vector3 startpos = startPoint.position;
            Vector3 targetpos = targetPoint.position;

            startpos.y = startPoint.position.y + 0.02f;
            targetpos.y = targetPoint.position.y + 0.02f;

            lineRenderer.SetPosition(0, startpos);
            lineRenderer.SetPosition(1, targetpos);
        }
    }
}