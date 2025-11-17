using System.Collections;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;

public class ExperimentalHaptics : MonoBehaviour
{
    // ---------- Tunables: Grab / Hold / Release ----------
    [Header("Grab bump (one-shot, base)")]
    [Range(0f, 1f)] public float grabAmplitude = 0.6f;
    [Range(0.01f, 0.3f)] public float grabDuration  = 0.08f;

    [Header("While held (looped pulses)")]
    [Range(0f, 1f)] public float holdAmplitude = 0.2f;
    [Range(0.01f, 0.5f)] public float holdPulseDuration = 0.06f;
    [Range(0.03f, 0.5f)] public float holdInterval = 0.12f;

    [Header("Release bump (one-shot)")]
    [Range(0f, 1f)] public float releaseAmplitude = 0.6f;
    [Range(0.01f, 0.3f)] public float releaseDuration  = 0.08f;

    // ---------- Intensity cue: distance → amplitude ----------
    [Header("Intensity cue (distance → amplitude)")]
    [Tooltip("Left controller Transform (tip or grip). Used to measure grab distance.")]
    public Transform leftController;
    [Tooltip("Right controller Transform (tip or grip). Used to measure grab distance.")]
    public Transform rightController;

    [Tooltip("Distances mapped into amplitude multiplier via the curve below.")]
    public float distanceMin = 0.01f;   // touching / very close
    public float distanceMax = 0.30f;   // arm's length-ish

    [Tooltip("Multiplier at far (0) and near (1) ends of the curve.")]
    public float ampMultiplierMin = 0.6f;   // far
    public float ampMultiplierMax = 1.8f;   // very close (make this bigger to feel it more)

    [Tooltip("Non-linear curve: input x=0 (far) .. 1 (very close) → y multiplier [0..1], then remapped to [ampMin..ampMax].")]
    public AnimationCurve distanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // Tip: Try a steeper curve for more punch: new Keyframe[]{ new(0,0), new(0.35f,0.25f), new(0.7f,0.75f), new(1,1) }

    [Header("Options")]
    public bool useXRNodeFallback = true;     // XRNode path for immediate buzz
    public bool scaleHoldPulsesToo = true;    // also scale the while-held pulses

    // ---------- Internal state ----------
    public static ExperimentalHaptics Instance { get; private set; }
    Coroutine _holdRoutine;
    string _currentHand = null; // "Left" or "Right"
    GameObject _currentObj = null;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GrabEventSystem.OnGrabWithDistance.AddListener(OnGrabWithDistance);
        GrabEventSystem.OnRelease.AddListener(OnRelease);
    }


    void OnDisable()
    {
        GrabEventSystem.OnGrabWithDistance.RemoveListener(OnGrabWithDistance);
        GrabEventSystem.OnRelease.RemoveListener(OnRelease);

        if (_holdRoutine != null)
        {
            StopCoroutine(_holdRoutine);
            _holdRoutine = null;
        }
    }

    // ---------- Static helper any script can call ----------
    public static void BuzzHandStatic(string hand, float amp, float dur)
    {
        if (Instance != null)
            Instance.ImmediateBuzzByHand(hand, amp, dur);
    }

    // ---------- Event handlers ----------
    void OnGrabWithDistance(GameObject obj, string hand, float grabDistance)
    {
        _currentHand = hand;
        _currentObj = obj;
        Debug.Log($"[Haptics] OnGrabWithDistance: hand={hand}, distance={grabDistance:F3}m");

        // Map distance (grabDistance) directly to amplitude multiplier
        float t = 1f - Mathf.InverseLerp(distanceMin, distanceMax, grabDistance);
        float mult = Mathf.Lerp(ampMultiplierMin, ampMultiplierMax, Mathf.Clamp01(t));
        float scaledAmp = grabAmplitude * mult;

        Debug.Log($"[Haptics] GrabDistance={grabDistance:F3}m  → mult={mult:F2}  amp={scaledAmp:F2}");

        // Immediate buzz
        ImmediateBuzzByHand(hand, scaledAmp, grabDuration);

        // Start the hold loop
        if (_holdRoutine == null)
            _holdRoutine = StartCoroutine(HoldPulseLoop());
    }


    void OnRelease(GameObject obj, string hand)
    {
        ImmediateBuzzByHand(hand, releaseAmplitude, releaseDuration);

        if (!GrabEventSystem.IsObjectGrabbed && _holdRoutine != null)
        {
            StopCoroutine(_holdRoutine);
            _holdRoutine = null;
            _currentHand = null;
            _currentObj  = null;
        }
    }

    // ---------- Loop: gentle pulses while held (optionally scaled) ----------
    IEnumerator HoldPulseLoop()
    {
        while (GrabEventSystem.IsObjectGrabbed)
        {
            float amp = holdAmplitude;
            if (scaleHoldPulsesToo && _currentObj != null && !string.IsNullOrEmpty(_currentHand))
                amp = ScaleBySurfaceDistance(_currentHand, holdAmplitude, _currentObj);

            if (!string.IsNullOrEmpty(_currentHand))
                ImmediateBuzzByHand(_currentHand, amp, holdPulseDuration);

            yield return new WaitForSeconds(holdInterval);
        }

        _holdRoutine = null;
        _currentHand = null;
        _currentObj  = null;
    }

    // ---------- Surface distance → amplitude ----------
    float ScaleBySurfaceDistance(string hand, float baseAmplitude, GameObject obj)
    {
        // 1) pick the controller transform
        Transform ctrl = (hand == "Left") ? leftController : rightController;
        if (ctrl == null) return baseAmplitude; // fallback if not wired

        Vector3 p = ctrl.position;

        // 2) compute distance to the object's collider surface if possible
        float d = Vector3.Distance(p, obj.transform.position);
        bool usedDefault = true;

        // Try to get a collider distance instead of pivot distance
        Collider[] cols = obj.GetComponentsInChildren<Collider>(true);
        if (cols.Length > 0)
        {
            float best = float.PositiveInfinity;
            foreach (var c in cols)
            {
                if (c == null || !c.enabled) continue;
                Vector3 cp = c.ClosestPoint(p);
                float dist = Vector3.Distance(p, cp);
                if (dist < best) best = dist;
            }

            if (best < float.PositiveInfinity)
            {
                d = best;
                usedDefault = false;
            }
        }

        // Log which path is used
        if (usedDefault)
            Debug.Log($"[Haptics] Using DEFAULT amplitude (no colliders found) → dist={d:F3}m");
        else
            Debug.Log($"[Haptics] Using SCALED amplitude (surface distance={d:F3}m)");


        // 3) normalize distance into 0..1 where 0=far, 1=very close
        float tClose = 1f - Mathf.InverseLerp(distanceMin, distanceMax, d);
        tClose = Mathf.Clamp01(tClose);

        // 4) apply non-linear curve
        float curved = Mathf.Clamp01(distanceCurve.Evaluate(tClose));

        // 5) remap to multiplier range and scale
        float mult = Mathf.Lerp(ampMultiplierMin, ampMultiplierMax, curved);

        // Optional: debug to tune values (comment out when done)
        // Debug.Log($"[Haptics] d={d:F3} tClose={tClose:F2} curved={curved:F2} mult={mult:F2}");

        return baseAmplitude * mult;
    }

    // ---------- Lowest-latency haptic path via XRNode ----------
    void ImmediateBuzzByHand(string hand, float amplitude, float duration)
    {
        if (!useXRNodeFallback) return;

        XRNode node = (hand == "Left") ? XRNode.LeftHand : XRNode.RightHand;
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;

        if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
            device.SendHapticImpulse(0u, amplitude, duration); // channel 0
    }
}


/*
 * ============================================================
 *  PROXIMITY AURA (per-grabbable)
 *  ------------------------------------------------------------
 *  - Adds/uses a trigger SphereCollider around the object.
 *  - When a controller enters the aura, it ticks a light haptic
 *    on that controller (pre-contact cue).
 *  - Uses ExperimentalHaptics.BuzzHandStatic(...)
 * ============================================================
 */

[RequireComponent(typeof(Collider))]
public class ProximityAura : MonoBehaviour
{
    [Header("Near-zone trigger")]
    [Tooltip("Radius of the proximity aura (a trigger SphereCollider).")]
    public float radius = 0.22f;

    [Header("Pre-contact tick")]
    [Range(0f, 0.5f)] public float preContactAmplitude = 0.12f;
    [Range(0.01f, 0.1f)] public float preContactDuration = 0.03f;

    SphereCollider _aura; // we keep our own trigger sphere so we don't touch main colliders

    void Reset()      { EnsureTrigger(); }
    void OnValidate() { EnsureTrigger(); if (_aura) _aura.radius = Mathf.Max(0.01f, radius); }
    void Awake()      { EnsureTrigger(); }

    void EnsureTrigger()
    {
        // If this object already has a trigger sphere, reuse it. Otherwise add one.
        var col = GetComponent<Collider>();
        if (col is SphereCollider sc && sc.isTrigger)
        {
            _aura = sc;
            _aura.radius = Mathf.Max(0.01f, radius);
            return;
        }

        if (_aura == null)
        {
            _aura = gameObject.AddComponent<SphereCollider>();
            _aura.isTrigger = true;
            _aura.radius = Mathf.Max(0.01f, radius);
            _aura.center = Vector3.zero;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Try to infer hand from the entering collider name (simple and fast)
        string lower = other.name.ToLower();
        string hand = lower.Contains("left") ? "Left" :
                      lower.Contains("right") ? "Right" : null;

        // If that failed, climb parents and check names there
        if (hand == null)
        {
            var t = other.transform;
            for (int i = 0; i < 4 && t != null; i++)
            {
                string n = t.name.ToLower();
                if (n.Contains("left"))  { hand = "Left";  break; }
                if (n.Contains("right")) { hand = "Right"; break; }
                t = t.parent;
            }
        }

        // Send a light tick to whichever hand we detected
        if (hand != null)
        {
            ExperimentalHaptics.BuzzHandStatic(hand, preContactAmplitude, preContactDuration);
        }
    }

    // Editor gizmo so you can see the aura when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, Mathf.Max(0.01f, radius));
    }
}
