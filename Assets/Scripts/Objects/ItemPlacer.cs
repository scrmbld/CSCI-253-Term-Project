using UnityEngine;
using System.Collections.Generic;
using TaskShape;
using System.Linq;

public class ItemPlacer : MonoBehaviour
{
    [Header("Item Count")]
    // the maximum number of items that can be placed in the world at one time
    public int objectMax;

    [Header("Item Position Bounds")]
    public float minX = -3.0f;
    public float maxX = 3.0f;
    public float minZ = -3.0f;
    public float maxZ = 3.0f;
    public bool relativeToPlayer = true;

    [Header("Goal Position (relative to item)")]
    public float minDistance = 1.5f;
    public float maxDistance = 220.0f;

    [Header("Item Type Constraints -- ignored unless useHardcodedTypes is true")]
    public bool useHardcodedTypes = false;
    public ShapeType hardcodedShape;
    public ShapeColor hardcodedColor;

    [Header("Manipulation Type")]
    public bool useExperimentalManip = false;

    [Header("Player Information")]
    public GameObject leftController;
    public GameObject rightController;

    private float minY = -0.1f;
    private float maxY = 0.3f;
    private int objectCount = 0;
    private int nextID = 0;

    // the absolute maximum bounds on item position imposed by the world
    // anything that exceeds these does not make sense
    private (float, float, float, float) worldBounds = (-15.0f, 250.0f, -15.0f, 220.0f);

    private Dictionary<ShapeType, Mesh> ShapeTypeMeshes = new Dictionary<ShapeType, Mesh>();
    private Dictionary<ShapeColor, Color> ShapeColors = new Dictionary<ShapeColor, Color>();

    private HashSet<(ShapeType, ShapeColor)> availableShapes = new HashSet<(ShapeType, ShapeColor)>();

    /// <summary>
    /// Get a random (ShapeType, ShapeColor) from the set of available shapes and then remove it from the set.
    /// If useHardcodedTypes is enabled, it will use the hardcoded type instead.
    /// </summary>
    /// <returns>The selected shape type x color tuple.</returns>
    (ShapeType, ShapeColor) UseRandomShape()
    {
        // if (useHardcodedTypes)
        // {
        //     return (hardcodedShape, hardcodedColor);
        // }
        // get a random shape from the set and then remove it from the set
        Debug.Log($"Number of item types available: {availableShapes.Count}");
        int usedIndex = (int)Random.Range(0, availableShapes.Count);
        (ShapeType, ShapeColor) chosen = availableShapes.ElementAt(usedIndex);
        availableShapes.Remove(chosen);

        return chosen;
    }

    void Start()
    {
        // get all shape + color combinations and put them into availableShapes
        var shapes = System.Enum.GetValues(typeof(ShapeType)).Cast<ShapeType>();
        var colors = System.Enum.GetValues(typeof(ShapeColor)).Cast<ShapeColor>();
        foreach (ShapeType s in shapes)
        {
            foreach (ShapeColor c in colors)
            {
                availableShapes.Add((s, c));
            }
        }

        // get all the meshes and put them into the Dictionary
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cube = obj.GetComponent<MeshFilter>().mesh;
        GameObject.Destroy(obj);

        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphere = obj.GetComponent<MeshFilter>().mesh;
        GameObject.Destroy(obj);

        obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Mesh cylinder = obj.GetComponent<MeshFilter>().mesh;
        GameObject.Destroy(obj);

        ShapeTypeMeshes[ShapeType.Cube] = cube;
        ShapeTypeMeshes[ShapeType.Sphere] = sphere;
        ShapeTypeMeshes[ShapeType.Cylinder] = cylinder;

        // put colors in the dictionary as well
        ShapeColors[ShapeColor.Red] = new Color(0.77f, 0.12f, 0.23f);
        ShapeColors[ShapeColor.Green] = new Color(0.18f, 0.55f, 0.34f);
        ShapeColors[ShapeColor.Blue] = new Color(0.0f, 0.2f, 0.38f);
        ShapeColors[ShapeColor.Purple] = new Color(0.4f, 0.2f, 0.6f);

        // subscribe to goal reached events
        ItemEventSystem.ItemDestroyed.AddListener(ItemDestroyed);
    }

    // Update is called once per frame
    void Update()
    {
        if (objectCount < objectMax)
        {
            GenerateItem();
        }
    }

    void GenerateItem()
    {
        Debug.Log($"Generating item...");


        // HACK: use the left controller as a proxy for the player's position
        float worldMinX = minX + leftController.transform.position.x;
        float worldMaxX = maxX + leftController.transform.position.x;
        float worldMinZ = minZ + leftController.transform.position.z;
        float worldMaxZ = maxZ + leftController.transform.position.z;

        // get the item position
        if (GeneratePosition(worldMinX, worldMaxX, worldMinZ, worldMaxZ) is { } itemPos)
        {
            // get the goal position

            for (int i = 0; i < 100; i++)
            {
                Vector3? maybeGoalPos = GeneratePosition(worldBounds.Item1, worldBounds.Item2, worldBounds.Item3, worldBounds.Item4);
                if (
                    // check that our goal position is legal
                    maybeGoalPos is { } goalPos
                    && Vector3.Distance(goalPos, itemPos) >= minDistance
                    && Vector3.Distance(goalPos, itemPos) <= maxDistance
                   )
                {
                    // randomly select shape type and color
                    // Debug.Log("Getting random shape (number)");
                    var (newShape, newColor) = UseRandomShape();

                    // create the item object
                    GameObject newItem = new GameObject($"Item{nextID}");
                    newItem.transform.position = itemPos;
                    newItem.transform.rotation = Random.rotation;
                    if (newShape == ShapeType.Cylinder)
                    {
                        newItem.transform.localScale = new Vector3(0.7f, 0.3f, 0.7f);
                    }
                    else
                    {
                        newItem.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    }

                    Mesh itemMesh = ShapeTypeMeshes[newShape];

                    newItem.AddComponent<MeshFilter>().mesh = itemMesh;
                    newItem.AddComponent<MeshRenderer>();
                    newItem.GetComponent<MeshRenderer>().material.color = ShapeColors[newColor];

                    // create the goal object
                    GameObject newGoal = new GameObject($"Goal{nextID}");
                    newGoal.transform.position = goalPos;
                    newGoal.transform.rotation = Random.rotation;
                    if (newShape == ShapeType.Cylinder)
                    {
                        newGoal.transform.localScale = new Vector3(0.7f, 0.3f, 0.7f);
                    }
                    else
                    {
                        newGoal.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    }

                    newGoal.AddComponent<MeshFilter>().mesh = itemMesh;
                    newGoal.AddComponent<MeshRenderer>();
                    newGoal.GetComponent<MeshRenderer>().material.color = new Color(0.14f, 0.13f, 0.14f);

                    nextID++;
                    objectCount++;

                    // add the ItemObject component to the item object

                    newItem.AddComponent<ItemObject>();
                    newItem.GetComponent<ItemObject>().type = newShape;
                    newItem.GetComponent<ItemObject>().color = ShapeColor.Green;
                    newItem.GetComponent<ItemObject>().goalObject = newGoal;

                    // add the manipulation component to the item obejct
                    if (useExperimentalManip)
                    {
                        newItem.AddComponent<ManipulationExperimentA>();
                        newItem.GetComponent<ManipulationExperimentA>().grabRadius = 1.0f;
                        newItem.GetComponent<ManipulationExperimentA>().leftController = leftController;
                        newItem.GetComponent<ManipulationExperimentA>().rightController = rightController;

                    } else
                    {
                        newItem.AddComponent<ManipulationControl>();
                        newItem.GetComponent<ManipulationControl>().grabRadius = 1.0f;
                        newItem.GetComponent<ManipulationControl>().leftController = leftController;
                        newItem.GetComponent<ManipulationControl>().rightController = rightController;
                    }

                    Debug.Log($"Generated {newColor} {newShape} at {itemPos} with goal at {goalPos}.");

                    // invoke the ItemPlaced event
                    ItemEventSystem.ItemPlaced.Invoke(newItem, newGoal);
                    return;
                }
            }
        }
    }

    Vector3? GeneratePosition(float minX, float maxX, float minZ, float maxZ)
    {
        // select a random x,z point inside of our range
        // do a raycast from way up in the sky to make sure it works
        // if it doesn't, repeat
        // if it does, radomly pick a height above the mesh at that point

        Vector3 position;
        float? groundHeight;
        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            position = new Vector3(x, 0.0f, z);

            groundHeight = CheckPosition(position);

            if (groundHeight is { } h)
            {
                position.y = h + Random.Range(0.2f, 1.3f);
                return position;
            }
        }

        return null;
    }

    float? CheckPosition(Vector3 position)
    {
        // raycast
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(new Vector3(position.x, 500.0f, position.z), Vector3.up * -1.0f, out hitInfo);

        float minDistance = 500.0f - maxY;
        float maxDistance = 500.0f - minY;

        if (hitInfo.distance > minDistance && hitInfo.distance < maxDistance)
        {
            return 500.0f - hitInfo.distance;
        }

        return null;
    }

    /// <summary>
    /// Callback used for when an ItemObject reaches its goal transform (the ItemObject.goalReached event).
    /// </summary>
    /// <param name="item">A reference to the item object.</param>
    /// <param name="goal">A reference to the goal object.</param>
    void ItemDestroyed(GameObject item, GameObject goal)
    {
        availableShapes.Add((item.GetComponent<ItemObject>().type, item.GetComponent<ItemObject>().color));
        objectCount--;
    }
}
