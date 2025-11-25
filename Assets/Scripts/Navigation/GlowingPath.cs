using UnityEngine;

public class GlowingPath : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        if (line == null)
            line = gameObject.AddComponent<LineRenderer>();

        line.positionCount = 2;
    }

    void Update()
    {
        if (startPoint == null || endPoint == null)
            return;

        // Update the line positions EVERY FRAME
        line.SetPosition(0, startPoint.position);
        line.SetPosition(1, endPoint.position);
    }
}