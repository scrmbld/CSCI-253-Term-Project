using UnityEngine;

public class Target : MonoBehaviour
{
    // Assign in inspector (this will be the object the goal/checkpoint expects to recieve)
    public GameObject expectedObject; 

    // Tolerance for how accurate object placement must be. Can be adjusted per level in the inspector
    public float positionTolerance = 0.15f;
    public float rotationToleranceDegrees = 25f;

    // public Transform targetTransform;

    // Uncomment if you want to see satified condition in debugging
    // [HideInInspector]
    public bool isSatisfied = false;

    private void OnTriggerStay(Collider other)
    {
        if (isSatisfied) return;

        if (other.gameObject == expectedObject)
        {
            // Compare difference from object placement to expected placement
            Transform t = other.transform;
            float distance = Vector3.Distance(t.position, transform.position);
            float angle = Quaternion.Angle(t.rotation, transform.rotation);

            if (distance <= positionTolerance && angle <= rotationToleranceDegrees)
            {
                isSatisfied = true;
                Debug.Log($"{name} satisfied by {other.name}");

                // TODO: Change pad color to green
            }
            else
            {
                // TODO: Change pad color to red? or maybe leave at red always until satisfied?
            }
        }
    }
}
