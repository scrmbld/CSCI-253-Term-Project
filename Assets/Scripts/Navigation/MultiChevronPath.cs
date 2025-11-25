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
        public List<Transform> arrows = new List<Transform>();
    }

    public GameObject chevronPrefab;
    public List<ObjectPair> objectPairs = new List<ObjectPair>();

    void Start()
    {
        InitializePairs();
    }

    void Update()
    {
        UpdatePairs();
    }

    void InitializePairs()
    {
        foreach (var pair in objectPairs)
        {
            // Delete old arrows
            foreach (var oldArrow in pair.arrows)
            {
                if (oldArrow != null)
                    Destroy(oldArrow.gameObject);
            }

            pair.arrows.Clear();

            // Spawn new arrows
            for (int i = 0; i < pair.arrowCount; i++)
            {
                GameObject arrowObj = Instantiate(chevronPrefab);
                arrowObj.transform.SetParent(transform); // keep hierarchy clean
                pair.arrows.Add(arrowObj.transform);
            }
        }
    }

    void UpdatePairs()
    {
        foreach (var pair in objectPairs)
        {
            if (pair.startPoint == null || pair.endPoint == null)
                continue;

            Vector3 dir = (pair.endPoint.position - pair.startPoint.position).normalized;

            for (int i = 0; i < pair.arrows.Count; i++)
            {
                float dist = i * pair.spacing;
                Vector3 pos = pair.startPoint.position + dir * dist;

                Transform arrow = pair.arrows[i];
                arrow.position = pos;
                arrow.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180, 0);
            }
        }
    }
}