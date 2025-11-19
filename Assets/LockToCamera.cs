using UnityEngine;

public class LockToCamera : MonoBehaviour
{
    public Transform cameraTransform;  // Main Camera

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        transform.position = cameraTransform.position;
        transform.rotation = cameraTransform.rotation;
    }
}
