using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class PathConnector : MonoBehaviour
{
    public enum PathMode { StraightLine, NavMeshRoad }

    [Header("General")]
    public PathMode mode = PathMode.StraightLine;
    public Transform startPoint;
    public Transform targetPoint;
    public float yOffset = 0.02f;

    LineRenderer lineRenderer;
    NavMeshPath navPath;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.08f;

        navPath = new NavMeshPath();
    }

    void Update()
    {
        if (startPoint == null || targetPoint == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        switch (mode)
        {
            case PathMode.StraightLine:
                DrawStraightLine();
                break;

            case PathMode.NavMeshRoad:
                DrawNavMeshPath();
                break;
        }
    }

    void DrawStraightLine()
    {
        lineRenderer.positionCount = 2;

        Vector3 a = startPoint.position;
        Vector3 b = targetPoint.position;
        a.y += yOffset;
        b.y += yOffset;

        lineRenderer.SetPosition(0, a);
        lineRenderer.SetPosition(1, b);
    }

    void DrawNavMeshPath()
    {
        // Snap start/target to nearest point ON the NavMesh
        if (!NavMesh.SamplePosition(startPoint.position, out NavMeshHit startHit, 2f, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(targetPoint.position, out NavMeshHit targetHit, 2f, NavMesh.AllAreas))
        {
            lineRenderer.positionCount = 0;
            return;
        }

        bool gotPath = NavMesh.CalculatePath(startHit.position, targetHit.position,
                                             NavMesh.AllAreas, navPath);

        if (!gotPath || navPath.status != NavMeshPathStatus.PathComplete || navPath.corners.Length == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        lineRenderer.positionCount = navPath.corners.Length;

        for (int i = 0; i < navPath.corners.Length; i++)
        {
            Vector3 p = navPath.corners[i];
            p.y += yOffset;
            lineRenderer.SetPosition(i, p);
        }
    }
}
