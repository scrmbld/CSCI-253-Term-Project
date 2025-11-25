using System.Collections;
using UnityEngine;

public class CheckpointTravel : MonoBehaviour
{
    public float moveDuration = 1.5f;

    private Coroutine moveRoutine;

    public void MoveTo(Transform target)
    {
        if (target == null) return;

        // If it's already moving, stop that first
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // Snap exactly at the end to avoid tiny floating–point drift
        transform.position = endPos;
        transform.rotation = endRot;

        moveRoutine = null;
    }
}