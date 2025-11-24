using UnityEngine;
using UnityEngine.InputSystem;

public class ManipulationExperimentA : MonoBehaviour
{

    public float grabRadius;
    public GameObject leftController;
    public GameObject rightController;

    private ProjectInputActions controls;

    private bool grabbedLeft = false;
    private bool grabbedRight = false;

    private Vector3 previousRightPos;

    Quaternion rotOffset;


    void Awake()
    {
        controls = new ProjectInputActions();
    }

    void Start()
    {
        previousRightPos = rightController.transform.position;
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

        controls.Disable();

        grabbedLeft = false;
        grabbedRight = false;
    }

    void Update()
    {
        LeftHandInteraction();
        RightHandInteraction();
    }

    /// <summary>
    /// Rotates the object based on left hand behavior. If left hand is grabbing,
    /// the object's rotation is mapped to the left hand's rotation.
    /// </summary>
    private void LeftHandInteraction()
    {
        if (grabbedLeft)
        {
            transform.rotation = leftController.transform.rotation * rotOffset;
        }
    }

    /// <summary>
    /// Translates the obejct based on right hand behavior. If the right hand is grabbing,
    /// the obejct's translation is mapped to the right hand's translation.
    /// </summary>
    private void RightHandInteraction()
    {
        if (grabbedRight)
        {
            Vector3 displacement = rightController.transform.position - previousRightPos;
            transform.position += displacement;
        }
        previousRightPos = rightController.transform.position;
    }

    /// <summary>
    /// Left grip button pressed callback. Checks for grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void LeftGripStarted(InputAction.CallbackContext ctx)
    {
        float delta = (transform.position - leftController.transform.position).magnitude;
        if (delta < grabRadius)
        {
            Debug.Log($"Grabbed {name} (left hand, rotation)"); // create an event
            rotOffset = transform.rotation * Quaternion.Inverse(leftController.transform.rotation);
            grabbedLeft = true;
            GrabEventSystem.TriggerGrab(gameObject, "Left");
        }
    }
    /// <summary>
    /// Left grip button released callback. Releases grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void LeftGripCanceled(InputAction.CallbackContext ctx)
    {
        if (grabbedLeft)
        {
            Debug.Log($"Released {name} (left hand, rotation)");
            grabbedLeft = false;
        }
            GrabEventSystem.TriggerRelease(gameObject, "Left");
    }
    /// <summary>
    /// Right grip button pressed callback. Checks for grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void RightGripStarted(InputAction.CallbackContext ctx)
    {
        float delta = (transform.position - rightController.transform.position).magnitude;
        if (delta < grabRadius)
        {
            Debug.Log($"Grabbed {name} (right hand, translation)");
            grabbedRight = true;
            GrabEventSystem.TriggerGrab(gameObject, "Right");
        }
    }
    /// <summary>
    /// Right grip button released callback. Releases grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void RightGripCanceled(InputAction.CallbackContext ctx)
    {
        if (grabbedRight)
        {
            Debug.Log($"Released {name} (right hand, translation)");
            grabbedRight = false;
            GrabEventSystem.TriggerRelease(gameObject, "Right");
        }
    }
}