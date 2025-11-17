using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ControlHaptics : MonoBehaviour
{
    [Header("Grab bump (one-shot)")]
    [Range(0f, 1f)] public float grabAmplitude = 0.6f;
    [Range(0.01f, 0.3f)] public float grabDuration  = 0.08f;

    [Header("While held (looped pulses)")]
    [Range(0f, 1f)] public float holdAmplitude = 0.2f;
    [Range(0.01f, 0.5f)] public float holdPulseDuration = 0.06f;
    [Range(0.03f, 0.5f)] public float holdInterval = 0.12f;

    [Header("Release bump (one-shot)")]
    [Range(0f, 1f)] public float releaseAmplitude = 0.6f;
    [Range(0.01f, 0.3f)] public float releaseDuration  = 0.08f;

    [Header("Options")]
    public bool useXRNodeFallback = true;     // fallback to device nodes
    public float recacheTimeoutSeconds = 2f;  // how long to wait for controllers to appear on startup
    public int resendAttempts = 3;            // retries when nothing is found yet
    public float resendDelay = 0.35f;         // delay between retries

    [SerializeField] UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor[] interactorRefs;
    [SerializeField] XRBaseController[] controllerRefs;

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor[] _interactorTargets;
    XRBaseController[] _controllerTargets;

    Coroutine _holdRoutine;

    void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnGrab);
        GrabEventSystem.OnRelease.AddListener(OnRelease);
        StartCoroutine(BootstrapCache());
    }

    void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnGrab);
        GrabEventSystem.OnRelease.RemoveListener(OnRelease);

        if (_holdRoutine != null) { StopCoroutine(_holdRoutine); _holdRoutine = null; }
    }

    IEnumerator BootstrapCache()
    {
        float endTime = Time.time + recacheTimeoutSeconds;
        do
        {
            CacheTargets();
            if (HasAnyTargets())
            {
                Debug.Log($"[Haptics] Ready. Interactors={_interactorTargets.Length}, Controllers={_controllerTargets.Length}");
                yield break;
            }
            yield return null;
        } while (Time.time < endTime);

        Debug.LogWarning("[Haptics] No XR interactors/controllers found at startup (will try on demand & use XRNode fallback).");
    }

    void CacheTargets()
    {
        _interactorTargets = (interactorRefs != null && interactorRefs.Length > 0)
            ? FilterEnabled(interactorRefs)
            : FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>(true);

        _controllerTargets = (controllerRefs != null && controllerRefs.Length > 0)
            ? FilterEnabled(controllerRefs)
            : FindObjectsOfType<XRBaseController>(true);
    }

    static T[] FilterEnabled<T>(T[] src) where T : Behaviour
    {
        var list = new List<T>();
        foreach (var x in src) if (x != null) list.Add(x);
        return list.ToArray();
    }

    bool HasAnyTargets()
    {
        return (_interactorTargets != null && _interactorTargets.Length > 0) ||
               (_controllerTargets  != null && _controllerTargets.Length  > 0);
    }

    void EnsureTargets()
    {
        if (!HasAnyTargets())
        {
            CacheTargets();
            if (!HasAnyTargets())
                Debug.LogWarning("[Haptics] No XR interactors/controllers found yet. Will try fallback.");
        }
    }

    // -------- immediate-hand buzz helper (lowest-latency path) --------
    void ImmediateBuzzByHand(string hand, float amplitude, float duration)
    {
        if (!useXRNodeFallback) return;

        XRNode node = (hand == "Left") ? XRNode.LeftHand : XRNode.RightHand;
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;

        if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            device.SendHapticImpulse(0u, amplitude, duration); // channel 0
            // Debug.Log($"[Haptics] Immediate buzz: {hand}");
        }
    }

    void OnGrab(GameObject obj, string hand)
    {
        // 1) fire immediate buzz to the grabbing hand (no waiting, no caching)
        ImmediateBuzzByHand(hand, grabAmplitude, grabDuration);

        // 2) proceed with normal send; this will hit interactors/controllers (or fallback) as they appear
        SendHapticsAll(grabAmplitude, grabDuration);

        if (_holdRoutine == null) _holdRoutine = StartCoroutine(HoldPulseLoop());

        // 3) if nothing exists yet, schedule quick retries
        if (!HasAnyTargets()) StartCoroutine(ResendAfterDelay(grabAmplitude, grabDuration));
    }

    void OnRelease(GameObject obj, string hand)
    {
        SendHapticsAll(releaseAmplitude, releaseDuration);
        if (_holdRoutine != null) { StopCoroutine(_holdRoutine); _holdRoutine = null; }
    }

    IEnumerator HoldPulseLoop()
    {
        while (GrabEventSystem.IsObjectGrabbed)
        {
            SendHapticsAll(holdAmplitude, holdPulseDuration);
            yield return new WaitForSeconds(holdInterval);
        }
        _holdRoutine = null;
    }

    IEnumerator ResendAfterDelay(float amplitude, float duration)
    {
        int attempts = resendAttempts;
        while (attempts-- > 0 && !HasAnyTargets())
        {
            yield return new WaitForSeconds(resendDelay);
            CacheTargets();
            SendHapticsAll(amplitude, duration);
        }
    }

    void SendHapticsAll(float amplitude, float duration)
    {
        EnsureTargets();

        bool sentAny = false;

        // Interactors first
        if (_interactorTargets != null)
        {
            foreach (var it in _interactorTargets)
            {
                if (it != null && it.isActiveAndEnabled)
                {
                    it.SendHapticImpulse(amplitude, duration);
                    sentAny = true;
                }
            }
        }

        // Controllers next
        if (!sentAny && _controllerTargets != null)
        {
            foreach (var ctrl in _controllerTargets)
            {
                if (ctrl != null && ctrl.isActiveAndEnabled)
                {
                    ctrl.SendHapticImpulse(amplitude, duration);
                    sentAny = true;
                }
            }
        }

        // XRNode fallback (already used for immediate buzz, but keep here for non-hand cases)
        if (!sentAny && useXRNodeFallback)
        {
            TryBuzzNode(XRNode.LeftHand, amplitude, duration, ref sentAny);
            TryBuzzNode(XRNode.RightHand, amplitude, duration, ref sentAny);
        }

        if (!sentAny) Debug.LogWarning("[Haptics] No active targets to send to.");
    }

    void TryBuzzNode(XRNode node, float amplitude, float duration, ref bool sentAny)
    {
        var device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid) return;

        if (device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
        {
            device.SendHapticImpulse(0u, amplitude, duration);
            sentAny = true;
        }
    }
}
