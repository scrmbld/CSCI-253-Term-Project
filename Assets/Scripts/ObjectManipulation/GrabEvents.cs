using UnityEngine;
using UnityEngine.Events;

public static class GrabEventSystem
{
    // Static, readonly UnityEvents: who was grabbed and by which hand ("Left"/"Right")
    public static readonly UnityEvent<GameObject, string> OnGrab = new UnityEvent<GameObject, string>();
    public static readonly UnityEvent<GameObject, string> OnRelease = new UnityEvent<GameObject, string>();

    // ✅ New event with grab distance
    public static UnityEvent<GameObject, string, float> OnGrabWithDistance = new UnityEvent<GameObject, string, float>();

    // Optional global flag
    public static bool IsObjectGrabbed { get; private set; }

    // Old version — still here for backward compatibility
    public static void TriggerGrab(GameObject obj, string hand)
    {
        IsObjectGrabbed = true;
        OnGrab.Invoke(obj, hand);
    }

    // ✅ New version — includes grab distance
    public static void TriggerGrab(GameObject obj, string hand, float distance)
    {
        IsObjectGrabbed = true;

        // Call both so control & experiment groups still work
        OnGrab.Invoke(obj, hand);
        OnGrabWithDistance.Invoke(obj, hand, distance);
    }

    public static void TriggerRelease(GameObject obj, string hand)
    {
        IsObjectGrabbed = false;
        OnRelease.Invoke(obj, hand);
    }
}
