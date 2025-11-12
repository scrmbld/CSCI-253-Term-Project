using UnityEngine;

public class ItemNavigation : MonoBehaviour
{
    [Header("References")]
    public GroundLineNavigation lineNav;  // drag NavigationLine here
    public Transform itemObject;          // drag TestItem here
    public Transform goalObject;          // drag TestTarget here

    void Start()
    {
        if (itemObject == null || goalObject == null)
        {
            Debug.LogError($"{name}: Missing item or goal object reference!");
            return;
        }

        if (lineNav == null)
        {
            Debug.LogError($"{name}: GroundLineNavigation reference missing!");
            return;
        }

        // Assign navigation points dynamically
        lineNav.startPoint = itemObject;
        lineNav.targetPoint = goalObject;
    }

    void Update()
    {
        if (lineNav != null && itemObject != null && goalObject != null)
        {
            // Update line positions as objects move
            lineNav.startPoint = itemObject;
            lineNav.targetPoint = goalObject;
        }
    }
}
