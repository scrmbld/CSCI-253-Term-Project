using UnityEngine;
using System.Collections.Generic;

public class DynamicChevronPath : MonoBehaviour
{
    [System.Serializable]
    public class ObjectPair
    {
        public Transform startPoint;
        public Transform endPoint;
        public int arrowCount = 5;
        public float spacing = 0.5f;
        public GameObject arrowPrefab;

        [HideInInspector] public List<Transform> spawnedArrows = new List<Transform>();
    }

    public List<ObjectPair> pairs = new List<ObjectPair>();

    void Start()
    {
        foreach (var p in pairs)
        {
            // Clear old arrows
            foreach (var a in p.spawnedArrows)
                Destroy(a.gameObject);

            p.spawnedArrows.Clear();

            // Spawn fresh arrows
            for (int i = 0; i < p.arrowCount; i++)
            {
                GameObject newArrow = Instantiate(p.arrowPrefab);
                p.spawnedArrows.Add(newArrow.transform);
            }
        }
    }

    void Update()
    {
        foreach (var p in pairs)
        {
            if (p.startPoint == null || p.endPoint == null)
                continue;

            Vector3 direction = (p.endPoint.position - p.startPoint.position).normalized;
            float fullDistance = Vector3.Distance(p.startPoint.position, p.endPoint.position);

            // AUTO-CONTRACT / EXPAND
            float dynamicSpacing = fullDistance / (p.arrowCount + 1);

            for (int i = 0; i < p.spawnedArrows.Count; i++)
            {
                float dist = dynamicSpacing * (i + 1);
                Vector3 pos = p.startPoint.position + direction * dist;

                p.spawnedArrows[i].position = pos;
                p.spawnedArrows[i].rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}