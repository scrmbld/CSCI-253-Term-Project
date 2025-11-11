using UnityEngine;
using UnityEngine.InputSystem;

public class ManipulationExperimentA : MonoBehaviour
{

    public float grabRadius;
    public Transform leftController;
    public Transform rightController;

    private XRIDefaultInputActions controls;

    private bool grabbedLeft = false;
    private bool grabbedRight = false;

    private Vector3 previousRightPos;

    Quaternion rotOffset;


    void Awake()
    {
        controls = new XRIDefaultInputActions();
    }

    void Start()
    {
        previousRightPos = rightController.position;
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
            transform.rotation = leftController.rotation * rotOffset;
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
            Vector3 displacement = rightController.position - previousRightPos;
            transform.position += displacement;
        }
        previousRightPos = rightController.position;
    }

    /// <summary>
    /// Left grip button pressed callback. Checks for grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void LeftGripStarted(InputAction.CallbackContext ctx)
    {
        float delta = (transform.position - leftController.position).magnitude;
        if (delta < grabRadius)
        {
            Debug.Log($"Grabbed {name} (left hand, rotation)"); // create an event
            rotOffset = transform.rotation * Quaternion.Inverse(leftController.rotation);
            grabbedLeft = true;
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
    }
    /// <summary>
    /// Right grip button pressed callback. Checks for grab.
    /// </summary>
    /// <param name="ctx"></param>
    private void RightGripStarted(InputAction.CallbackContext ctx)
    {
        float delta = (transform.position - rightController.position).magnitude;
        if (delta < grabRadius)
        {
            Debug.Log($"Grabbed {name} (right hand, translation)");
            grabbedRight = true;
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
        }
    }
}