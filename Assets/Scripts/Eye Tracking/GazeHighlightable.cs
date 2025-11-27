using UnityEngine;

/// <summary>
/// Put this on any object that should glow yellow when looked at.
/// Requires a Renderer (or child renderers) and a collider.
/// </summary>
public class GazeHighlightable : MonoBehaviour
{
    [Header("Renderers to tint (leave empty = auto)")]
    public Renderer[] renderers;

    [Header("Base Colors")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    [Header("Glow (emission)")]
    public Color glowColor = Color.yellow;
    public float glowIntensity = 3f;     // higher = brighter glow

    private bool isHighlighted = false;

    void Reset()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();
    }

    public void SetHighlighted(bool highlight)
    {
        if (isHighlighted == highlight) return;
        isHighlighted = highlight;

        if (renderers == null) return;

        foreach (var r in renderers)
        {
            if (r == null) continue;

            // Instance material for this renderer
            var mat = r.material;

            // 1. Base color
            mat.color = highlight ? highlightColor : normalColor;

            // 2. Emission (glow)
            if (mat.HasProperty("_EmissionColor"))
            {
                // Enable emission keyword so URP uses it
                mat.EnableKeyword("_EMISSION");

                if (highlight)
                {
                    // HDR color = color * intensity
                    Color finalGlow = glowColor * glowIntensity;
                    mat.SetColor("_EmissionColor", finalGlow);
                }
                else
                {
                    // Turn emission off (or very low)
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}
