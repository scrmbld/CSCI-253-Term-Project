using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.Events; // only needed if you create local UnityEvents

public class ManipulationControl : MonoBehaviour
{
    // Optional global: not used below, but keep if others read it
    public static bool IsGrabbedGlobal = false;

    [Header("Grab Settings")]
    public float grabRadius = 0.2f;

    [Header("Controllers")]
    public GameObject leftController;
    public GameObject rightController;

    private ProjectInputActions controls;

    void Awake()
    {
        controls = new ProjectInputActions();
    }

    void OnEnable()
    {
        // Subscribe input callbacks
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;
        leftGripAction.started += LeftGripStarted;
        leftGripAction.canceled += LeftGripCanceled;

        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;
        rightGripAction.started += RightGripStarted;
        rightGripAction.canceled += RightGripCanceled;

        controls.Enable();

        // Example: if you want to react globally elsewhere, subscribe here:
        // GrabEventSystem.OnGrab.AddListener(OnAnyGrab);
        // GrabEventSystem.OnRelease.AddListener(OnAnyRelease);
    }

    void OnDisable()
    {
        // Unsubscribe input callbacks
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;
        leftGripAction.started -= LeftGripStarted;
        leftGripAction.canceled -= LeftGripCanceled;

        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;
        rightGripAction.started -= RightGripStarted;
        rightGripAction.canceled -= RightGripCanceled;

        controls.Disable();

        transform.SetParent(null, true);

        // If you subscribed to global events above, unsubscribe here:
        // GrabEventSystem.OnGrab.RemoveListener(OnAnyGrab);
        // GrabEventSystem.OnRelease.RemoveListener(OnAnyRelease);
    }

    // LEFT
    private void LeftGripStarted(InputAction.CallbackContext ctx)
    {
        if (leftController == null) return;

        float delta = Vector3.Distance(transform.position, leftController.transform.position);
        Debug.Log($"Grip (Left) pressed. Distance={delta:F3}");

        if (delta < grabRadius && transform.parent == null)
        {
            transform.SetParent(leftController.transform, true);
            IsGrabbedGlobal = true;
            GrabEventSystem.TriggerGrab(gameObject, "Left");
            Debug.Log($"Grabbed {name} (left hand)");
        }
    }

    private void LeftGripCanceled(InputAction.CallbackContext ctx)
    {
        // only release if this object is currently parented to the left controller
        if (transform.parent == leftController?.transform)
        {
            transform.SetParent(null, true);
            IsGrabbedGlobal = false;
            GrabEventSystem.TriggerRelease(gameObject, "Left");
            Debug.Log($"Released {name} (left hand)");
        }
    }

    // RIGHT
    private void RightGripStarted(InputAction.CallbackContext ctx)
    {
        if (rightController == null) return;

        float delta = Vector3.Distance(transform.position, rightController.transform.position);
        if (delta < grabRadius && transform.parent == null)
        {
            transform.SetParent(rightController.transform, true);
            IsGrabbedGlobal = true;
            GrabEventSystem.TriggerGrab(gameObject, "Right");
            Debug.Log($"Grabbed {name} (right hand)");
        }
    }

    private void RightGripCanceled(InputAction.CallbackContext ctx)
    {
        // only release if this object is currently parented to the right controller
        if (transform.parent == rightController?.transform)
        {
            transform.SetParent(null, true);
            IsGrabbedGlobal = false;
            GrabEventSystem.TriggerRelease(gameObject, "Right");
            Debug.Log($"Released {name} (right hand)");
        }
    }

    // Example global listeners if you want them:
    // private void OnAnyGrab(GameObject obj, string hand) { /* ... */ }
    // private void OnAnyRelease(GameObject obj, string hand) { /* ... */ }
}