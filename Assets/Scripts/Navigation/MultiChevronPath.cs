using System.Collections.Generic;
using UnityEngine;

public class MultiChevronPath : MonoBehaviour
{
    [System.Serializable]
    public class ObjectPair
    {
        public Transform startPoint;
        public Transform endPoint;
        public int arrowCount = 5;
        public float spacing = 1f;

        [HideInInspector]
        public GameObject container; // Stores arrows for this pair
    }

    public GameObject chevronPrefab;
    public List<ObjectPair> objectPairs = new List<ObjectPair>();

    void Start()
    {
        GenerateAllArrows();
    }

    public void GenerateAllArrows()
    {
        foreach (var pair in objectPairs)
        {
            GenerateForPair(pair);
        }
    }

    void GenerateForPair(ObjectPair pair)
    {
        if (pair.startPoint == null || pair.endPoint == null || chevronPrefab == null)
            return;

        // Create a container for this pair (if not already)
        if (pair.container == null)
        {
            pair.container = new GameObject("ChevronGroup_" + pair.startPoint.name + "_" + pair.endPoint.name);
            pair.container.transform.parent = this.transform;
        }

        // Clear old arrows
        foreach (Transform child in pair.container.transform)
        {
            Destroy(child.gameObject);
        }

        Vector3 direction = (pair.endPoint.position - pair.startPoint.position).normalized;

        float totalDistance = Vector3.Distance(pair.startPoint.position, pair.endPoint.position);
        float stepDistance = Mathf.Clamp(pair.spacing, 0.1f, totalDistance / 2f);

        for (int i = 0; i < pair.arrowCount; i++)
        {
            float t = (i + 1) * stepDistance;
            if (t >= totalDistance) break;

            Vector3 pos = pair.startPoint.position + direction * t;

            GameObject arrow = Instantiate(chevronPrefab, pos, Quaternion.identity, pair.container.transform);

            // Auto-rotate toward target
            arrow.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
