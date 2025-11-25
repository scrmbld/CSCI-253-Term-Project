using UnityEngine;

public class ChevronPath : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public GameObject chevronPrefab;

    public int arrowCount = 6;
    public float spacing = 1f;

    private GameObject[] arrows;

    void Start()
    {
        GenerateArrows();
    }

    void GenerateArrows()
    {
        // Clean old arrows
        if (arrows != null)
        {
            foreach (var a in arrows)
                Destroy(a);
        }

        arrows = new GameObject[arrowCount];

        Vector3 dir = (endPoint.position - startPoint.position).normalized;
        float totalDist = Vector3.Distance(startPoint.position, endPoint.position);

        for (int i = 0; i < arrowCount; i++)
        {
            float t = (i + 1) / (float)(arrowCount + 1); // evenly spaced
            Vector3 pos = Vector3.Lerp(startPoint.position, endPoint.position, t);

            arrows[i] = Instantiate(chevronPrefab, pos, Quaternion.LookRotation(dir, Vector3.up));

            // adjust scale if needed
            arrows[i].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
    }
}
