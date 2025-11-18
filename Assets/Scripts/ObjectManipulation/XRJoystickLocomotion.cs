using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class XRJoystickLocomotion : MonoBehaviour
{
    [Header("References")]
    public Transform head;          // Main Camera / CenterEyeAnchor

    [Header("Movement")]
    public XRNode inputSource = XRNode.LeftHand; // or RightHand if you prefer
    public float moveSpeed = 3.0f;  // <--- CHANGE THIS TO WALK FASTER
    public float gravity = -9.81f;

    private CharacterController controller;
    private InputDevice device;
    private Vector3 fallingSpeed = Vector3.zero;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Ensure we have a valid device
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(inputSource);
            return;
        }

        // Read thumbstick (primary2DAxis)
        if (!device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input))
            return;

        // No input -> only gravity
        Vector3 move = Vector3.zero;
        if (input.sqrMagnitude > 0.0001f)
        {
            // Move relative to head direction, on the horizontal plane
            Vector3 forward = head.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = head.right;
            right.y = 0;
            right.Normalize();

            move = forward * input.y + right * input.x;
            move *= moveSpeed;  // <--- SPEED APPLIED HERE
        }

        // Simple gravity so controller stays grounded
        if (controller.isGrounded && fallingSpeed.y < 0)
            fallingSpeed.y = 0;
        fallingSpeed.y += gravity * Time.deltaTime;

        controller.Move((move + fallingSpeed) * Time.deltaTime);
    }
}
