using UnityEngine;
using UnityEngine.Events;

public static class GrabEventSystem
{
    // UnityEvent that passes which object was grabbed and by which hand
    public UnityEvent<GameObject, string> OnGrab = new UnityEvent<GameObject, string>();
    public UnityEvent<GameObject, string> OnRelease = new UnityEvent<GameObject, string>();

    // Optional: simple global flag
    public static bool IsObjectGrabbed { get; private set; } = false;

    public static void TriggerGrab(GameObject obj, string hand)
    {
        IsObjectGrabbed = true;
        OnGrab.Invoke(obj, hand);
    }

    public static void TriggerRelease(GameObject obj, string hand)
    {
        IsObjectGrabbed = false;
        OnRelease.Invoke(obj, hand);
    }
}