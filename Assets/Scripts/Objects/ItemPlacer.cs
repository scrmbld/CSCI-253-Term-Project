using UnityEngine;
using System.Collections.Generic;
using TaskShape;

public class ItemPlacer : MonoBehaviour
{
    public int objectMax;
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public Transform CameraOffset;
    public GameObject leftController;
    public GameObject rightController;

    private float minY = 0.0f;
    private float maxY = 0.3f;
    private int objectCount = 0;
    private int nextID = 0;

    private Dictionary<ShapeType, Mesh> ShapeTypeMeshes = new Dictionary<ShapeType, Mesh>();
    private Dictionary<ShapeColor, Color> ShapeColors = new Dictionary<ShapeColor, Color>();

    void Start()
    {
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
        // randomly select shape type and color
        var types = System.Enum.GetValues(typeof(ShapeType));
        ShapeType newShape = (ShapeType)types.GetValue((int)Random.Range(0, types.Length));
        var colors = System.Enum.GetValues(typeof(ShapeColor));
        ShapeColor newColor = (ShapeColor)colors.GetValue((int)Random.Range(0, colors.Length));

        // place the item

        Vector3 itemPos = GeneratePosition();
        Debug.Log(itemPos);
        GameObject newItem = new GameObject($"Item{nextID}");
        newItem.transform.position = itemPos;
        newItem.transform.rotation = Random.rotation;
        if (newShape == ShapeType.Cylinder)
        {
            newItem.transform.localScale = new Vector3(0.7f, 0.3f, 0.7f);
        } else
        {
            newItem.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }

        Mesh itemMesh = ShapeTypeMeshes[newShape];

        newItem.AddComponent<MeshFilter>().mesh = itemMesh;
        newItem.AddComponent<MeshRenderer>();
        newItem.GetComponent<MeshRenderer>().material.color = ShapeColors[newColor];

        // place the goal

        Vector3 goalPos;

        do
        {
            goalPos = GeneratePosition();
        } while (Vector3.Distance(goalPos, itemPos) < 1.5f);

        Debug.Log(goalPos);
        GameObject newGoal = new GameObject($"Goal{nextID}");
        newGoal.transform.position = goalPos;
        newGoal.transform.rotation = Random.rotation;
        if (newShape == ShapeType.Cylinder)
        {
            newGoal.transform.localScale = new Vector3(0.7f, 0.3f, 0.7f);
        } else
        {
            newGoal.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        }

        newGoal.AddComponent<MeshFilter>().mesh = itemMesh;
        newGoal.AddComponent<MeshRenderer>();
        newGoal.GetComponent<MeshRenderer>().material.color =  new Color(0.14f, 0.13f, 0.14f);

        nextID++;
        objectCount++;

        // add the ItemObject component to the item object

        newItem.AddComponent<ItemObject>();
        newItem.GetComponent<ItemObject>().type = newShape;
        newItem.GetComponent<ItemObject>().color = ShapeColor.Green;
        newItem.GetComponent<ItemObject>().goalObject = newGoal;

        // add the manipulation component to the item obejct
        newItem.AddComponent<ManipulationControl>();
        newItem.GetComponent<ManipulationControl>().grabRadius = 1.0f;
        newItem.GetComponent<ManipulationControl>().leftController = leftController;
        newItem.GetComponent<ManipulationControl>().rightController = rightController;

        // subscribe to the item's goalReached event
        newItem.GetComponent<ItemObject>().goalReached.AddListener(GoalReached);
    }

    Vector3 GeneratePosition()
    {
        // select a random x,z point inside of our range
        // do a raycast from way up in the sky to make sure it works
        // if it doesn't, repeat
        // if it does, radomly pick a height above the mesh at that point

        Vector3 position;
        float? groundHeight;
        do
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            position = new Vector3(x, 0.0f, z);

            groundHeight = CheckPosition(position);
        } while (groundHeight == null);

        if (groundHeight is { } h)
        {
            position.y = h + Random.Range(0.2f, 1.3f);
        }

        return position;
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
    void GoalReached(GameObject item, GameObject goal)
    {
        GameObject.Destroy(item);
        GameObject.Destroy(goal);

        objectCount--;
    }
}
