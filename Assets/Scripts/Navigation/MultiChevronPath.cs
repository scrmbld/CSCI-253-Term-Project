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
        public GameObject container;
    }

    public GameObject chevronPrefab;
    public List<ObjectPair> objectPairs = new List<ObjectPair>();

    void Update()
    {
        UpdateAllPairs();
    }

    void UpdateAllPairs()
    {
        foreach (var pair in objectPairs)
        {
            if (pair.startPoint == null || pair.endPoint == null || chevronPrefab == null)
                continue;

            // Create container for this pair if missing
            if (pair.container == null)
            {
                pair.container = new GameObject("ChevronPath_" + pair.startPoint.name);
                pair.container.transform.parent = pair.startpoint;

                // Spawn arrows initially
                for (int i = 0; i < pair.arrowCount; i++)
                {
                    GameObject arrow = Instantiate(chevronPrefab, pair.container.transform);
                }
            }

            UpdatePair(pair);
        }
    }

    void UpdatePair(ObjectPair pair)
    {
        int childCount = pair.container.transform.childCount;
        Vector3 dir = (pair.endPoint.position - pair.startPoint.position).normalized;

        for (int i = 0; i < childCount; i++)
        {
            Transform arrow = pair.container.transform.GetChild(i);

            float distance = pair.spacing * (i + 1);
            Vector3 pos = pair.startPoint.position + dir * distance;

            arrow.position = pos;

            // Rotate arrow to face the target
            arrow.rotation = Quaternion.LookRotation(dir);
        }
    }
}
