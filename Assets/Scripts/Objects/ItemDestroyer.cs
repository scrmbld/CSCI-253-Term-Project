using UnityEngine;
using TaskShape;

/// <summary>
/// Destroys the item and goal GameObjects when the item reaches the goal.
/// </summary>
public class ItemDestroyer : MonoBehaviour
{
    void Start()
    {
        // listen for goal reached events
        ItemEventSystem.GoalReached.AddListener(GoalReached);
    }

    void GoalReached(GameObject item, GameObject goal)
    {
        // these don't actually happen until after the current update loop
        GameObject.Destroy(item);
        GameObject.Destroy(goal);

        // therefore I can reference the destroyed items here and still be fine
        ItemEventSystem.ItemDestroyed.Invoke(item, goal);
    }
}
