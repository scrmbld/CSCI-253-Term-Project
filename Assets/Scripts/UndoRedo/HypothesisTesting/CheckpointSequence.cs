using System;
using UnityEngine;
using TaskShape;

[Serializable]
public class CheckpointSequence
{
    [Header("Item and its Goal")]
    public GameObject item; // Item must have ItemObject, Manipulation, UndoableObject
    public GameObject goal; // ItemObject.goal

    [Header("Checkpoints in order")]
    public Transform[] checkpoints; // Each transform is the new checkpoint spot the checkpoint will move to

    [HideInInspector] public int currIndex = 0;
    [HideInInspector] public bool isComplete = false;

    public void MoveGoalToCurrentCheckpoint()
    {
        if (goal == null || checkpoints == null || checkpoints.Length == 0)
        {
            return;
        }
        currIndex = Mathf.Clamp(currIndex, 0, checkpoints.Length - 1);
        Transform checkpoint = checkpoints[currIndex];
        goal.transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
    }
}