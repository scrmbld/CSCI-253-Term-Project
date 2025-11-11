using System.Collections;
using UnityEngine;
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

    // FIX: use XRBaseControllerInteractor (Ray/Direct) not XRBaseInputInteractor
    UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor[] _interactorTargets;
    XRBaseController[] _controllerTargets;
    Coroutine _holdRoutine;

    void OnEnable()
    {
        GrabEventSystem.OnGrab.AddListener(OnGrab);
        GrabEventSystem.OnRelease.AddListener(OnRelease);
        CacheTargets();
    }

    void OnDisable()
    {
        GrabEventSystem.OnGrab.RemoveListener(OnGrab);
        GrabEventSystem.OnRelease.RemoveListener(OnRelease);

        if (_holdRoutine != null)
        {
            StopCoroutine(_holdRoutine);
            _holdRoutine = null;
        }
    }

    void CacheTargets()
    {
        // Interactors cover both Direct & Ray interactors
        _interactorTargets = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>(true);

        // Fallback to raw controllers
        _controllerTargets = FindObjectsOfType<XRBaseController>(true);
    }

    void OnGrab(GameObject obj, string hand)
    {
        Debug.Log($"Global event: {hand} hand grabbed {obj.name}");
        SendHapticsAll(grabAmplitude, grabDuration);

        if (_holdRoutine == null)
            _holdRoutine = StartCoroutine(HoldPulseLoop());
    }

    void OnRelease(GameObject obj, string hand)
    {
        Debug.Log($"Global event: {hand} hand released {obj.name}");

        if (_holdRoutine != null)
        {
            StopCoroutine(_holdRoutine);
            _holdRoutine = null;
        }
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

    void SendHapticsAll(float amplitude, float duration)
    {
        bool sentAny = false;

        if (_interactorTargets != null && _interactorTargets.Length > 0)
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

        if (!sentAny && _controllerTargets != null && _controllerTargets.Length > 0)
        {
            foreach (var ctrl in _controllerTargets)
            {
                if (ctrl != null && ctrl.isActiveAndEnabled)
                {
                    ctrl.SendHapticImpulse(amplitude, duration);
                }
            }
        }
    }
}
