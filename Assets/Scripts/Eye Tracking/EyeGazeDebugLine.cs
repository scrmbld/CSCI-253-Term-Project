using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EyeGazeDebugLine : MonoBehaviour
{
    public float lineLength = 5f;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    void LateUpdate()
    {
        // start at eye / combined eye position
        lr.SetPosition(0, transform.position);
        // end some units forward in gaze direction
        lr.SetPosition(1, transform.position + transform.forward * lineLength);
    }
}