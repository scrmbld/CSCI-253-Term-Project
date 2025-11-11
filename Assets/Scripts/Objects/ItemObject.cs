using UnityEngine;
using TaskShape;

public class ItemObject : MonoBehaviour
{
    public ShapeType type;
    public ShapeColor color;
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
                shape = new Cube(gameObject, color);
                goal = new Cube(goalObject, color);
                break;
            case ShapeType.Sphere:
                shape = new Sphere(gameObject, color);
                goal = new Sphere(goalObject, color);
                break;
            case ShapeType.Cylinder:
                shape = new Cylinder(gameObject, color);
                goal = new Cylinder(goalObject, color);
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