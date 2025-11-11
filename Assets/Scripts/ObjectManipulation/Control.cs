//using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ManipulationControl : MonoBehaviour
{
    public static bool IsGrabbedGlobal = false;

    public float grabRadius;
    public GameObject leftController;
    public GameObject rightController;

    // events
    public GrabEventSystem rightHandGrab = new GrabEventSystem();

    private XRIDefaultInputActions controls;

    void Awake()
    {
        controls = new XRIDefaultInputActions();
    }

    void OnEnable()
    {
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;

        leftGripAction.started += LeftGripStarted;
        leftGripAction.canceled += LeftGripCanceled;

        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;

        rightGripAction.started += RightGripStarted;
        rightGripAction.canceled += RightGripCanceled;

        controls.Enable();
    }

    void OnDisable()
    {
        InputAction leftGripAction = controls.XRILeftLocomotion.GrabMove;

        leftGripAction.started -= LeftGripStarted;
        leftGripAction.canceled -= LeftGripCanceled;

        InputAction rightGripAction = controls.XRIRightLocomotion.GrabMove;

        rightGripAction.started -= RightGripStarted;
        rightGripAction.canceled -= RightGripCanceled;

        controls.Enable();
    }

    /// <summary>
    /// Left grip button pressed callback. Checks for grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void LeftGripStarted(InputAction.CallbackContext ctx)
    {
        float delta = (transform.position - leftController.transform.position).magnitude;
        //Debug.Log("Grip button pressed!");
        Debug.Log("Grip button pressed!");
        Debug.Log($"Distance to left controller: {delta}");
        if (delta < grabRadius && transform.parent == null)
        {
            Debug.Log($"Grabbed {name} (left hand)");
            transform.SetParent(leftController.transform);

            IsGrabbedGlobal = true;
        }
    }
    /// <summary>
    /// Left grip button released callback. Releases grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void LeftGripCanceled(InputAction.CallbackContext ctx)
    {
        if (transform.parent != null)
        {
            Debug.Log($"Released {name} (left hand)");
            transform.parent = null;

            IsGrabbedGlobal = false;
        }
    }
    /// <summary>
    /// Right grip button pressed callback. Checks for grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void RightGripStarted(InputAction.CallbackContext ctx)
    {
        float delta = (transform.position - rightController.transform.position).magnitude;
        if (delta < grabRadius && transform.parent == null)
        {
            Debug.Log($"Grabbed {name} (right hand)");
            transform.SetParent(rightController.transform);

            IsGrabbedGlobal = true;
        }
    }
    /// <summary>
    /// Right grip button released callback. Releases grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void RightGripCanceled(InputAction.CallbackContext ctx)
    {
        if (transform.parent != null)
        {
            Debug.Log($"Released {name} (right hand)");
            transform.parent = null;

            // >>> CHANGED: clear global grab flag on release
            IsGrabbedGlobal = false; // <<< CHANGED
        }
    }
}
