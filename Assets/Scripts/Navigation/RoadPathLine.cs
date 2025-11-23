using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class RoadPathLine : MonoBehaviour
{
    public Transform startPoint;   // TestItem
    public Transform targetPoint;  // TestTarget
    public float yOffset = 0.02f;

    private LineRenderer lineRenderer;
    private NavMeshPath navPath;

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

        // Snap start/target to nearest point ON the NavMesh
        if (!NavMesh.SamplePosition(startPoint.position, out NavMeshHit startHit, 2f, NavMesh.AllAreas) ||
            !NavMesh.SamplePosition(targetPoint.position, out NavMeshHit targetHit, 2f, NavMesh.AllAreas))
        {
            // Nothing near the NavMesh – probably NavMesh missing here
            lineRenderer.positionCount = 0;
            return;
        }

        // Ask NavMesh for a path
        bool gotPath = NavMesh.CalculatePath(startHit.position, targetHit.position,
                                             NavMesh.AllAreas, navPath);

        if (!gotPath || navPath.status != NavMeshPathStatus.PathComplete || navPath.corners.Length == 0)
        {
            // Uncomment this if you want spammy logs:
            // Debug.Log($"No valid path. gotPath={gotPath}, status={navPath.status}, corners={navPath.corners.Length}");
            lineRenderer.positionCount = 0;
            return;
        }

        // Draw line along the path corners
        lineRenderer.positionCount = navPath.corners.Length;

        for (int i = 0; i < navPath.corners.Length; i++)
        {
            Vector3 p = navPath.corners[i];
            p.y += yOffset;
            lineRenderer.SetPosition(i, p);
        }
    }
}
