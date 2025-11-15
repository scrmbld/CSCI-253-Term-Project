using UnityEngine;
using UnityEngine.Events;

public static class GrabEventSystem
{
    // Static, readonly UnityEvents: who was grabbed and by which hand ("Left"/"Right")
    public static readonly UnityEvent<GameObject, string> OnGrab = new UnityEvent<GameObject, string>();
    public static readonly UnityEvent<GameObject, string> OnRelease = new UnityEvent<GameObject, string>();

    // Optional global flag
    public static bool IsObjectGrabbed { get; private set; }

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
