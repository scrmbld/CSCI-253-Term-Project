using UnityEngine;

public class EyeGazeDebugger : MonoBehaviour
{
    public float length = 10f;

    void Update()
    {
        // 1) Draw a ray every frame
        Debug.DrawRay(transform.position, transform.forward * length);

        // 2) Log sometimes so we know it's running
        if (Time.frameCount % 2 == 0)
        {
            Debug.Log($"[EyeGazeDebugger] pos={transform.position} forward={transform.forward}");
        }
    }

    void OnDrawGizmos()
    {
        // 3) Also draw a gizmo line so you can see it even when paused
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * length);
    }
}
