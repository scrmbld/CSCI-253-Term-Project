using UnityEngine;
using UnityEngine.InputSystem;

public class BagHUD_Input_Debug2 : MonoBehaviour
{
    [Header("Controller aiming")]
    public Transform controllerTransform;      // RIGHT controller transform
    public float maxDistance = 5f;
    public LayerMask bagLayer;                // e.g. BagUI

    [Header("UI Roots")]
    public GameObject bagIconRoot;            // BagIconRoot
    public GameObject bagPanelRoot;           // BagPanelRoot
    public Vector3 panelLocalOffset = new Vector3(0f, 0f, 1.5f);

    [Header("Right trigger (Input System)")]
    public InputActionProperty rightTriggerAction;   // bind to RIGHT trigger action

    [Header("Visual feedback")]
    public Renderer bagIconRenderer;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    bool isPointingAtIcon = false;
    bool wasPointingLastFrame = false;

    void OnEnable()
    {
        if (rightTriggerAction.action != null)
        {
            rightTriggerAction.action.Enable();
            Debug.Log("[BagHUD] Enabled rightTriggerAction: " + rightTriggerAction.action.name);
        }
        else
        {
            Debug.LogWarning("[BagHUD] RightTriggerAction is NULL – nothing will fire.");
        }
    }

    void OnDisable()
    {
        if (rightTriggerAction.action != null)
            rightTriggerAction.action.Disable();
    }

    void Start()
    {
        Debug.Log("[BagHUD] Start – icon=" + bagIconRoot + ", panel=" + bagPanelRoot);

        if (bagIconRoot != null) bagIconRoot.SetActive(true);
        if (bagPanelRoot != null) bagPanelRoot.SetActive(false);

        SetIconHighlight(false);
    }

    void Update()
    {
        UpdateControllerAimOnIcon();

        if (rightTriggerAction.action != null &&
            rightTriggerAction.action.WasPressedThisFrame())
        {
            bool panelOpen = bagPanelRoot != null && bagPanelRoot.activeSelf;
            Debug.Log($"[BagHUD] Right trigger PRESSED – panelOpen={panelOpen}, isPointingAtIcon={isPointingAtIcon}");

            if (panelOpen)
            {
                // Close from anywhere
                TogglePanel();
            }
            else if (isPointingAtIcon)
            {
                // Only open if the controller is pointing at the bag icon
                TogglePanel();
            }
        }
    }

    void UpdateControllerAimOnIcon()
    {
        bool pointingNow = false;
        isPointingAtIcon = false;

        if (controllerTransform == null)
        {
            if (wasPointingLastFrame)
                Debug.LogWarning("[BagHUD] controllerTransform is null now.");
            SetIconHighlight(false);
            wasPointingLastFrame = false;
            return;
        }

        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, bagLayer))
        {
            if (hit.collider.CompareTag("BagIcon"))
            {
                pointingNow = true;
                isPointingAtIcon = true;
            }
        }

        // Log only when state changes
        if (pointingNow && !wasPointingLastFrame)
        {
            Debug.Log("[BagHUD] AIM ENTER bag icon.");
        }
        else if (!pointingNow && wasPointingLastFrame)
        {
            Debug.Log("[BagHUD] AIM EXIT bag icon.");
        }

        wasPointingLastFrame = pointingNow;
        SetIconHighlight(pointingNow);
    }

    void TogglePanel()
    {
        if (!bagPanelRoot || !bagIconRoot)
        {
            Debug.LogWarning("[BagHUD] TogglePanel but roots not set.");
            return;
        }

        bool open = !bagPanelRoot.activeSelf;

        bagPanelRoot.SetActive(open);
        bagIconRoot.SetActive(!open);

        Debug.Log(open ? "[BagHUD] Panel OPENED" : "[BagHUD] Panel CLOSED");

        if (open)
        {
            Transform cam = Camera.main.transform;
            if (bagPanelRoot.transform.parent != cam)
                bagPanelRoot.transform.SetParent(cam);

            bagPanelRoot.transform.localPosition = panelLocalOffset;
            bagPanelRoot.transform.localRotation = Quaternion.identity;
        }
        else
        {
            SetIconHighlight(false);
        }
    }

    void SetIconHighlight(bool highlight)
    {
        if (!bagIconRenderer) return;
        bagIconRenderer.material.color = highlight ? hoverColor : normalColor;
    }
}
