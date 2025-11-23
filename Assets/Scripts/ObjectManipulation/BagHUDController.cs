using UnityEngine;

/// <summary>
/// Small bag icon that sticks to the view. When you LOOK at it (eye gaze ray)
/// and press the RIGHT index trigger, it toggles a big panel in front of you.
/// Uses MetaEyeTrackingBridge for gaze and OVRInput for trigger.
/// </summary>
public class BagHUDControllerOVR : MonoBehaviour
{
    [Header("Eye tracking (from MetaEyeTrackingBridge)")]
    public MetaEyeTrackingBridge eyeBridge;
    public float gazeMaxDistance = 5f;
    public LayerMask bagLayer;     // layer where the bag icon lives

    [Header("UI Roots (children of camera)")]
    public GameObject bagIconRoot;     // small icon in corner
    public GameObject bagPanelRoot;    // big panel in front when open
    public Vector3 panelLocalOffset = new Vector3(0f, 0f, 1.2f); // in front of camera

    [Header("Visual Feedback")]
    public Renderer bagIconRenderer;       // Renderer on the quad / mesh
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;


    private bool isLookingAtIcon = false;



    void Start()
    {
        if (bagIconRoot != null) bagIconRoot.SetActive(true);
        if (bagPanelRoot != null) bagPanelRoot.SetActive(false);

        SetIconHighlight(false);
    }

    void Update()
    {
        UpdateGazeOnIcon();
        HandleTrigger();
    }

    void SetIconHighlight(bool highlight)
    {
        if (bagIconRenderer == null) return;

        // .material makes an instance for this renderer (fine for one HUD icon)
        var mat = bagIconRenderer.material;
        mat.color = highlight ? hoverColor : normalColor;
    }


    void UpdateGazeOnIcon()
    {
        bool gazeOnIcon = false;
        isLookingAtIcon = false;

        if (eyeBridge == null)
        {
            SetIconHighlight(false);
            return;
        }

        Vector3 origin = eyeBridge.GazeOrigin;
        Vector3 dir = eyeBridge.GazeDirection;

        Ray ray = new Ray(origin, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, gazeMaxDistance, bagLayer))
        {
            if (hit.collider.CompareTag("BagIcon"))
            {
                gazeOnIcon = true;
                isLookingAtIcon = true;
            }
        }

        SetIconHighlight(gazeOnIcon);
    }


    void HandleTrigger()
    {
        // Right index trigger (OVR)
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && isLookingAtIcon)
        {
            TogglePanel();
        }
    }

    void TogglePanel()
    {
        bool open = !bagPanelRoot.activeSelf;

        bagPanelRoot.SetActive(open);
        bagIconRoot.SetActive(!open);

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
            // icon just became visible again
            SetIconHighlight(false);
        }
    }

}
