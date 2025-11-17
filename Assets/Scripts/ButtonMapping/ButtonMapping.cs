using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonMapping : MonoBehaviour
{
    // X/Y Button Mappings
    private bool xPressed = false;
    private bool yPressed = false;

// Check X and Y button presses, activate X for undo, Y for redo
    void LateUpdate()
    {
        // Get the left controller device
        UnityEngine.XR.InputDevice leftHand =
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);

        if (!leftHand.isValid || UndoManager.Instance == null)
            return;

        // --- Read X button (primary) ---
        bool xDown = leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool xValue) && xValue;

        bool xJustPressed = xDown && !xPressed;

        // Report to UndoManager
        UndoManager.Instance.OnUndoInput(xDown, xJustPressed);

        // Track previous state for edge detection
        xPressed = xDown;

        // --- Read Y button (secondary) ---
        bool yDown = leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool yValue) && yValue;

        bool yJustPressed = yDown && !yPressed;

        // Report to UndoManager
        UndoManager.Instance.OnRedoInput(yDown, yJustPressed);

        // Track previous state
        yPressed = yDown;
    }
}
