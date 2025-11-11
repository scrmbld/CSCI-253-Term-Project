using UnityEngine;
using TaskShape;

public class ItemObject : MonoBehaviour
{
    public ShapeType type;
    public GameObject goalObject;
    private Shape shape;
    private Shape goal;

    void Start()
    {
        // initialize our shape objects based on the ShapeType we have been given
        // assume that both us and the goal have the same ShapeType because it would be stupid if we didn't
        switch (type)
        {
            case ShapeType.Cube:
                shape = new Cube(gameObject);
                goal = new Cube(goalObject);
                break;
            case ShapeType.Sphere:
                shape = new Sphere(gameObject);
                goal = new Sphere(goalObject);
                break;
                // TODO: Other shapes
        }
    }

    void Update()
    {
        if (shape.Equivalent(goal))
        {
            Debug.Log($"{name}: Goal!");
        } else
        {
            Debug.Log($"{name}: No Goal!");
        }
    }
}