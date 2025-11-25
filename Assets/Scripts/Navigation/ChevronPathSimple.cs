using UnityEngine;

public class ChevronPathSimple : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public GameObject chevronPrefab;
    public int arrowCount = 5;
    public float spacing = 1f;

    private GameObject[] arrows;

    void Start()
    {
        arrows = new GameObject[arrowCount];

        for (int i = 0; i < arrowCount; i++)
        {
            arrows[i] = Instantiate(chevronPrefab, transform);
        }
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        Vector3 direction = (endPoint.position - startPoint.position).normalized;

        for (int i = 0; i < arrowCount; i++)
        {
            Vector3 pos = startPoint.position + direction * spacing * (i + 1);
            arrows[i].transform.position = pos;
            arrows[i].transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
