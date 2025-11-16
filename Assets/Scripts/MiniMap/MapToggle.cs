using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MapToggle : MonoBehaviour
{
    [Header("Hook these up in Inspector")]
    [Tooltip("The UI panel (Canvas/RawImage) that displays the RenderTexture")]
    public GameObject miniMapCanvas;                 // <- ONLY the UI panel

    [Tooltip("The camera that renders the minimap (top-down)")]
    public Camera miniMapCamera;                     // <- Assign your MiniMapCamera here

#if ENABLE_INPUT_SYSTEM
    [Tooltip("Input Action Reference to Gameplay/ToggleMinimap (Press Only)")]
    public InputActionReference toggleActionRef;     // <- your Input Action
    private InputAction _toggle;
#endif

    [Header("Behaviour")]
    [Tooltip("Start with the minimap hidden?")]
    public bool startHidden = true;

    [Tooltip("Name of the layer used for minimap-only sprites (e.g., PlayerIcon, object icons)")]
    public string minimapLayerName = "MiniMapIcon";

    [Tooltip("Also disable the MiniMapCamera when hidden (tiny perf win, optional)")]
    public bool disableMiniMapCameraWhenHidden = false;

    private bool isVisible;
    private int minimapLayerMask;

    void Awake()
    {
        // Resolve camera if not assigned (optional convenience)
        if (!miniMapCamera)
        {
            var byName = GameObject.Find("MiniMapCamera");
            if (byName) miniMapCamera = byName.GetComponent<Camera>();
            if (!miniMapCamera) Debug.LogWarning("[MapToggle] MiniMapCamera not assigned.");
        }

        // Cache the layer mask (0 if layer doesn’t exist yet)
        minimapLayerMask = LayerMask.GetMask(minimapLayerName);

        SetVisible(!startHidden);
    }

#if ENABLE_INPUT_SYSTEM
    void OnEnable()
    {
        if (toggleActionRef != null)
        {
            _toggle = toggleActionRef.action;
            if (_toggle != null)
            {
                _toggle.Enable();
                _toggle.performed += OnPerformed;
                // Debug.Log($"[MapToggle] Enabled. Action={_toggle.name}, enabled={_toggle.enabled}");
            }
        }
        else
        {
            Debug.LogWarning("[MapToggle] No InputActionReference assigned. Using KeyCode.M fallback.");
        }
    }

    void OnDisable()
    {
        if (_toggle != null)
        {
            _toggle.performed -= OnPerformed;
            _toggle.Disable();
        }
    }

    private void OnPerformed(InputAction.CallbackContext ctx) => Toggle();
#endif

    void Update()
    {
#if !ENABLE_INPUT_SYSTEM
        // Old Input fallback if new system disabled
        if (Input.GetKeyDown(KeyCode.M)) Toggle();
#else
        // Fallback if user forgot to assign the action
        if (toggleActionRef == null && Input.GetKeyDown(KeyCode.M)) Toggle();
#endif
    }

    public void Toggle()
    {
        SetVisible(!isVisible);
        // Debug.Log($"[MapToggle] Map toggled -> {isVisible}");
    }

    private void SetVisible(bool show)
    {
        isVisible = show;

        // 1) Toggle the UI panel
        if (miniMapCanvas) miniMapCanvas.SetActive(isVisible);

        // 2) Toggle rendering of the MinimapIcon layer on the MiniMapCamera
        if (miniMapCamera)
        {
            if (minimapLayerMask == 0)
            {
                // Layer not found or not set up—skip quietly
            }
            else
            {
                if (isVisible)
                    miniMapCamera.cullingMask |= minimapLayerMask;   // include layer
                else
                    miniMapCamera.cullingMask &= ~minimapLayerMask;  // exclude layer
            }

            // 3) (Optional) Toggle the camera component itself
            if (disableMiniMapCameraWhenHidden)
                miniMapCamera.enabled = isVisible;
        }
    }
}
