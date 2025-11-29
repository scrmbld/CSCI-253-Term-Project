using UnityEngine;

public class LineBetweenTwoObjects : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
    }

    void LateUpdate()
    {
        if (startPoint == null || endPoint == null)
            return;

        // Use world positions — required for VR accuracy.
        line.SetPosition(0, startPoint.position);
        line.SetPosition(1, endPoint.position);
    }
}
