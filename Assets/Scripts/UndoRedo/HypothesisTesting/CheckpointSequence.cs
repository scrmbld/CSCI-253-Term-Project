using System;
using UnityEngine;
using TaskShape;
using Oculus.Platform;
using Oculus.Interaction.PoseDetection;

[Serializable]
public class CheckpointSequence
{
    [Header("Item and its Goal")]
    public GameObject item; // Item must have ItemObject, Manipulation, UndoableObject
    public GameObject goal; // ItemObject.goal

    [Header("Checkpoints in order")]
    public Transform[] checkpoints; // Each transform is the new checkpoint spot the checkpoint will move to

    public int currIndex = 0; // Current checkpoint in the sequence
    public bool isComplete = false;

    public Material completeMaterial; // Assign this in the inspector

    public void MoveGoalToCurrentCheckpoint()
    {
        if (goal == null || checkpoints == null || checkpoints.Length == 0)
        {
            return;
        }

        currIndex = Mathf.Clamp(currIndex, 0, checkpoints.Length - 1);

        // Move the goal transform to current checkpoint
        Transform checkpoint = checkpoints[currIndex];

        // Sets the initial position without movement animation
        if (currIndex == 0) 
        { 
            goal.transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        }
        else
        {
            // If the goal has a MovingGoal script, animate to the checkpoint
            CheckpointTravel mover = goal.GetComponent<CheckpointTravel>();
            if (mover != null)
            {
                mover.MoveTo(checkpoint);
            }
            else
            {
                // Fallback: instant teleport
                goal.transform.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
            }
        }
        

    }

    public void Complete()
    {
        isComplete = true;
        MarkGoalComplete(goal);
       
    }

    private void MarkGoalComplete(GameObject goalObject)
    {
        MeshRenderer renderer = goalObject.GetComponentInChildren<MeshRenderer>();

        if (renderer != null)
        {
            renderer.material = completeMaterial;
        }
    }

    public void HideItem(GameObject item)
    {
        Debug.Log($"Hiding item {item}");
        Vector3 newPos = new Vector3 (0, -100, 0);
        item.transform.position += newPos;
    }
}