using UnityEngine;
using UnityEngine.InputSystem;   // same as your other script, ok to keep

public class MapButtonMapping : MonoBehaviour
{
    [Header("Map")]
    public GameObject mapCanvas;   // assign your MapCanvas here
    public Transform head;         // your VR camera / CenterEyeAnchor
    public float showDistance = 2f;

    // B button state
    private bool bPressed = false;
    private bool isVisible = false;

    void LateUpdate()
    {
        // Get the right controller device
        UnityEngine.XR.InputDevice rightHand =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);

        if (!rightHand.isValid || mapCanvas == null || head == null)
            return;

        // --- Read B button (secondary) ---
        bool bDown = rightHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton,
                                                  out bool bValue) && bValue;

        bool bJustPressed = bDown && !bPressed;

        if (bJustPressed)
        {
            ToggleMap();
        }

        // Track previous state
        bPressed = bDown;
    }

    private void ToggleMap()
    {
        isVisible = !isVisible;

        mapCanvas.SetActive(isVisible);

        if (!isVisible)
            return;

        // Place map in front of the head when opened
        Vector3 forwardFlat = head.forward;
        forwardFlat.y = 0;
        forwardFlat.Normalize();

        mapCanvas.transform.position = head.position + forwardFlat * showDistance;

        // Make the canvas face the player
        Vector3 lookPos = head.position;
        lookPos.y = mapCanvas.transform.position.y;
        mapCanvas.transform.LookAt(lookPos);
        mapCanvas.transform.Rotate(0, 180f, 0);
    }
}
