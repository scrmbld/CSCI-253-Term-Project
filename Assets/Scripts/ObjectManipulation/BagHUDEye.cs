using UnityEngine;
using UnityEngine.InputSystem;

public class BagHUD_Input_Debug : MonoBehaviour
{
    [Header("Eye tracking")]
    public MetaEyeTrackingBridge eyeBridge;
    public float gazeMaxDistance = 5f;
    public LayerMask bagLayer;

    [Header("UI Roots")]
    public GameObject bagIconRoot;     // BagIconRoot
    public GameObject bagPanelRoot;    // BagPanelRoot
    public Vector3 panelLocalOffset = new Vector3(0f, 0f, 1.5f);

    [Header("Right trigger (Input System)")]
    public InputActionProperty rightTriggerAction;   // bind to RIGHT trigger action

    [Header("Visual feedback")]
    public Renderer bagIconRenderer;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;

    bool isLookingAtIcon = false;
    bool wasLookingLastFrame = false;

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
        UpdateGazeOnIcon();

        if (rightTriggerAction.action != null)
        {
            float val = rightTriggerAction.action.ReadValue<float>();
            if (val > 0.05f)
            {
                Debug.Log($"[BagHUD] Trigger value: {val:F2}, isLookingAtIcon={isLookingAtIcon}");
            }

            if (rightTriggerAction.action.WasPressedThisFrame())
            {
                bool panelOpen = bagPanelRoot != null && bagPanelRoot.activeSelf;
                Debug.Log($"[BagHUD] Right trigger PRESSED – panelOpen={panelOpen}, isLookingAtIcon={isLookingAtIcon}");

                if (panelOpen)
                {
                    // Close from anywhere
                    TogglePanel();
                }
                else if (isLookingAtIcon)
                {
                    // Only open if you’re looking at the bag
                    TogglePanel();
                }
            }
        }
    }


    void UpdateGazeOnIcon()
    {
        bool gazeOnIcon = false;
        isLookingAtIcon = false;

        if (eyeBridge == null)
        {
            if (wasLookingLastFrame)
                Debug.LogWarning("[BagHUD] eyeBridge is null now.");
            SetIconHighlight(false);
            wasLookingLastFrame = false;
            return;
        }

        Ray ray = new Ray(eyeBridge.GazeOrigin, eyeBridge.GazeDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, gazeMaxDistance, bagLayer))
        {
            if (hit.collider.CompareTag("BagIcon"))
            {
                gazeOnIcon = true;
                isLookingAtIcon = true;
            }
        }

        // Log only when state changes, so Console isn’t spammed
        if (gazeOnIcon && !wasLookingLastFrame)
        {
            Debug.Log("[BagHUD] Gaze ENTER bag icon.");
        }
        else if (!gazeOnIcon && wasLookingLastFrame)
        {
            Debug.Log("[BagHUD] Gaze EXIT bag icon.");
        }

        wasLookingLastFrame = gazeOnIcon;
        SetIconHighlight(gazeOnIcon);
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
