using UnityEngine;

public class NavExperimentManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] shapePrefabs;        // sphere, cube, cylinder, etc.
    public PathConnector pathConnectorPrefab; // prefab with LineRenderer + PathConnector

    [Header("Experiment settings")]
    public int pairCount = 2;
    public PathConnector.PathMode pathMode = PathConnector.PathMode.StraightLine;

    [Header("Spawn area")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(10f, 0f, 10f);   // XZ area on ground

    void Start()
    {
        SpawnPairs();
    }

    void SpawnPairs()
    {
        if (shapePrefabs == null || shapePrefabs.Length == 0)
        {
            Debug.LogError("[NavExperiment] No shapePrefabs assigned!");
            return;
        }

        for (int i = 0; i < pairCount; i++)
        {
            // --- Spawn TestItem ---
            GameObject testPrefab = RandomShape();
            GameObject testItem = Instantiate(testPrefab,
                RandomPointInArea(),
                Quaternion.identity);

            testItem.name = $"TestItem_{i}_{testPrefab.name}";

            // --- Spawn TargetItem ---
            GameObject targetPrefab = RandomShape();
            GameObject targetItem = Instantiate(targetPrefab,
                RandomPointInArea(),
                Quaternion.identity);

            targetItem.name = $"TargetItem_{i}_{targetPrefab.name}";

            // --- Create connector line ---
            PathConnector connector = Instantiate(
                pathConnectorPrefab,
                Vector3.zero,
                Quaternion.identity);

            connector.mode = pathMode;
            connector.startPoint = testItem.transform;
            connector.targetPoint = targetItem.transform;
            connector.gameObject.name = $"Connector_{i}_{pathMode}";
        }
    }

    GameObject RandomShape()
    {
        int idx = Random.Range(0, shapePrefabs.Length);
        return shapePrefabs[idx];
    }

    Vector3 RandomPointInArea()
    {
        return areaCenter + new Vector3(
            Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
            0f,
            Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f)
        );
    }
}
