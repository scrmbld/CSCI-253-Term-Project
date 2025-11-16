using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float lookSensitivity = 2f;
    [SerializeField] float jumpHeight = 1.1f;
    [SerializeField] float gravity = -9.81f;

    CharacterController cc;
    Transform cam;
    float yVel;
    float pitch;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>().transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Look (mouse) ---
        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;
        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, -85f, 85f);
        cam.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // --- Move (WASD) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = (transform.right * h + transform.forward * v).normalized;

        Vector3 vel = input * moveSpeed;

        // --- Ground / jump ---
        if (cc.isGrounded)
        {
            yVel = -2f; // stick to ground
            if (Input.GetButtonDown("Jump"))
                yVel = Mathf.Sqrt(2f * jumpHeight * -gravity);
        }
        else
        {
            yVel += gravity * Time.deltaTime;
        }

        vel.y = yVel;
        cc.Move(vel * Time.deltaTime);
    }
}
