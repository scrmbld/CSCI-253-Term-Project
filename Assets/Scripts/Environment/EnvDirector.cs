using UnityEngine;
using UnityEngine.Rendering;

public class EnvDirector : MonoBehaviour
{
    [Header("References")]
    public Light sun;
    public Light moon;
    [Tooltip("Assign your Skybox/Procedural material here")]
    public Material skyboxMat;

    [Header("Time of Day")]
    [Tooltip("Full day length in real-time minutes")]
    public float dayLengthMinutes = 5f;
    [Range(0f, 1f)] public float time01 = 0.25f; // 0 dawn, 0.5 noon, 0.75 dusk, 1 night
    public bool autoAdvance = true;

    [Header("Sun/Moon")]
    public AnimationCurve sunIntensity = AnimationCurve.EaseInOut(0, 0, 0.5f, 1);
    public AnimationCurve moonIntensity = AnimationCurve.EaseInOut(0.5f, 0, 1f, 0.4f);
    public Gradient sunColor;
    public Gradient ambientColor;

    [Header("Fog")]
    public bool enableFog = true;
    public Gradient fogColor;
    public AnimationCurve fogDensity = AnimationCurve.EaseInOut(0, 0.004f, 1, 0.02f);

    [Header("Ambient Audio (optional)")]
    public AudioSource dayLoop;   // birds/wind
    public AudioSource nightLoop; // crickets/low wind
    [Range(0f, 1f)] public float audioCrossfadeSharpness = 4f;

    [Header("Debug Controls")]
    public KeyCode toggleAutoKey = KeyCode.F2;
    public KeyCode fasterTimeKey = KeyCode.F1;
    public KeyCode toggleFogKey = KeyCode.F3;

    float daySpeed => (dayLengthMinutes <= 0.01f) ? 0f : (1f / (dayLengthMinutes * 60f));

    void Reset()
    {
        sun = GameObject.Find("Sun")?.GetComponent<Light>();
        moon = GameObject.Find("Moon")?.GetComponent<Light>();
    }

    void Update()
    {
        // Controls
        if (Input.GetKeyDown(toggleAutoKey)) autoAdvance = !autoAdvance;
        if (Input.GetKeyDown(toggleFogKey)) RenderSettings.fog = !(RenderSettings.fog);
        if (Input.GetKey(toggleAutoKey) && Input.GetKey(fasterTimeKey)) time01 += Time.deltaTime * daySpeed * 10f; // combo
        else if (autoAdvance) time01 += Time.deltaTime * daySpeed;
        time01 = Mathf.Repeat(time01, 1f);

        UpdateLighting(time01);
        UpdateFog(time01);
        UpdateAudio(time01);
    }

    void UpdateLighting(float t)
    {
        if (!sun) return; // only require the Sun, not the skybox

        // Sun
        float sunAngle = (t * 360f) - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 30f, 0f);
        sun.intensity = Mathf.Max(0f, sunIntensity.Evaluate(t));
        sun.color     = sunColor.Evaluate(t);

        // Moon (always enabled; intensity drives visibility)
        if (moon)
        {
            moon.transform.rotation = Quaternion.Euler(sunAngle + 180f, 30f, 0f);
            moon.intensity = Mathf.Max(0f, moonIntensity.Evaluate(t)); // clamp to avoid negatives
            // no moon.enabled toggle here (Option A)
        }

        // Ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor.Evaluate(t);

        // Skybox exposure (optional)
        if (skyboxMat)
        {
            float exposure = Mathf.Lerp(0.9f, 1.3f, Mathf.InverseLerp(0.1f, 0.6f, sun.intensity));
            if (skyboxMat.HasProperty("_Exposure")) skyboxMat.SetFloat("_Exposure", exposure);
        }
    }


    void UpdateFog(float t)
    {
        RenderSettings.fog = enableFog;
        if (!enableFog) return;

        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor.Evaluate(t);
        RenderSettings.fogDensity = fogDensity.Evaluate(t);
    }

    void UpdateAudio(float t)
    {
        if (!dayLoop && !nightLoop) return;

        // Day defined loosely as sun above horizon around t ~ 0.1..0.6
        float dayWeight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 0.55f, t));
        float nightWeight = 1f - dayWeight;

        if (dayLoop)
        {
            if (!dayLoop.isPlaying) dayLoop.Play();
            dayLoop.volume = Mathf.Pow(dayWeight, audioCrossfadeSharpness);
        }
        if (nightLoop)
        {
            if (!nightLoop.isPlaying) nightLoop.Play();
            nightLoop.volume = Mathf.Pow(nightWeight, audioCrossfadeSharpness);
        }
    }
}

