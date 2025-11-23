# Item System Documentation

All code related to the item system can be found in `Assets/Scripts/Objects`. The script files contained therein account for all logic related to the creation of items and their goal checking, and expose a variety of important events for other scripts to use. Please see TaskShape Namespace/ItemEventSystem for these important public interfaces.

## TaskShape Namespace

The `TaskShape` namespace contains a variety of things that are used by the other elements of the item system, and exposes some useful functionality for outside components as well. It can be accessed from the global namespace.

Here are some examples of accessing `ShapeType`, a member of the `TaskShape` namespace, both with and without `using TaskShape`:

```cs
// without "using"
TaskShape.ShapeType shape = TaskShape.ShapeType
```

```cs
// with "using"
using TaskShape;
ShapeType shape = ShapeType
```

### ItemEvent Class

```cs
public class ItemEvent : UnityEvent<GameObject, GameObject> { }
```

The `TaskShape.ItemEvent` class is used to represent events related to the item system by `TaskShape.ItemEventSystem`. This inclues `ItemEventSystem.ItemPlaced`, `ItemEventSystem.GoalReached`, and `ItemEventSystem.ItemDestroyed`.

It is a derived instance of `UnityEvent` that holds two game objects: the item and its goal, respectively. Although one could create new instances of `ItemEvent`, this is generally not what you will want to do. Here is an example anyways, to demonstrate how `ItemEvent`s can be used by both invokers and callers:

```cs
// Create an instance of the event class
// This can be invoked multiple times by any number of other components,
// and can be listened to by any number of other components.
ItemEvent exampleEvent = new ItemEvent();

// add an event listener
exampleEvent.AddListener(exampleCallback);

// invoke the event, passing GameObject references exampleItem and exampleGoal
// results in "Example event invoked on..." being printed to the debug log
exampleEvent.Invoke(exampleItem, exampleGoal);

// example callback function
void exampleCallback(GameObject item, GameObject goal)
{
    Debug.Log($"Example event invoked on {item.name}, {goal.name}");
}
```

### ItemEventSystem Class

`TaskShape.ItemEventSystem` contains static members for global item events. The same `TaskShape.ItemEvent` instances are invoked for every single item object in the scene, so by listening to one, such as `ItemEventSystem.ItemPlaced`, you are listening for every item.

#### ItemPlaced ItemEvent

`ItemEventSystem.ItemPlaced` is invoked by the `ItemPlacer` object every time it creates a new item in the environment. The arguments passed to listener callbacks are the newly created item and goal `GameObject`s.

```cs
using TaskShape;

ItemEventSystem.ItemPlaced.AddListener(exampleCallback);

// example callback function
void exampleCallback(GameObject item, GameObject goal)
{
    ShapeType st = item.GetComponent<ItemObject>().type;
    ShapeColor c = item.GetComponent<ItemObject>().color;
    Debug.Log($"A {c} {st} has been created, with its goal at {goal.transform.position}");
}
```

#### GoalReached ItemEvent

`ItemEventSystem.GoalReached` is invoked by the `ItemObject` object when it finds that its position and rotation are equivalent to those if its goal object (see `TaskShape.Shape`). The `ItemObject` passes in itself and the goal object as arguments to listener callbacks are the item and goal `GameObject`s.

```cs
using TaskShape;

ItemEventSystem.GoalReached.AddListener(exampleCallback);

// example callback function
void exampleCallback(GameObject item, GameObject goal)
{
    ShapeType st = item.GetComponent<ItemObject>().type;
    ShapeColor c = item.GetComponent<ItemObject>().color;
    Debug.Log($"A {c} {st} has been created, with its goal at {goal.transform.position}");
}
```

#### ItemDestroyed ItemEvent

`ItemEventSystem.GoalReached` is to be used invoked whenever an `ItemObject` is destroyed. The invoker should pass the destroyed obejcts to the `GoalReached.Invoke` method call.

Any experiment that needs to define its own handling the `GoalReached` event will likely need to destroy the item and goal `GameObjects`  using `GameObject.Destroy` as a part of that handler. When this is done, it is necessary to invoke the `ItemDestroyed` event so that the `ItemPlacer` knows that it should create another item. The "default" `GoalReached` handling / item removal behavior is defined in the `ItemDestroyer` component, which can be used as a reference when for invoking this event yourself:

```cs
public class ItemDestroyer : MonoBehaviour
{
    void Start()
    {
        // listen for GoalReached invocations
        ItemEventSystem.GoalReached.AddListener(GoalReached);
    }

    // callback function
    void GoalReached(GameObject item, GameObject goal)
    {
        // these don't actually happen until after the current update loop
        GameObject.Destroy(item);
        GameObject.Destroy(goal);

        // therefore I can reference the destroyed items in the ItemDestroyed callbacks just fine
        ItemEventSystem.ItemDestroyed.Invoke(item, goal);
    }
}
```

If you instead need to listen for an `ItemDestroyed`, here an example of that:

```cs
using TaskShape;

ItemEventSystem.ItemDestroyed.AddListener(exampleCallback);

// example callback function
void exampleCallback(GameObject item, GameObject goal)
{
    // you can still access fields on the GameObjects on the same update tick that they are destroyed
    ShapeType st = item.GetComponent<ItemObject>().type;
    ShapeColor c = item.GetComponent<ItemObject>().color;
    Debug.Log($"A {c} {st} has been destroyed.");
}
```

### ShapeColor Enum

```cs
public enum ShapeColor
{
    Red,
    Green,
    Blue,
    Purple
}
```


This enum is used to distinguish between items of different colors.

### ShapeType Enum

```cs
    public enum ShapeType
    {
        Cube,
        Sphere,
        Cylinder
    }
```

This enum is used to distinguish between items of different shapes.


### Shape Interface

This interface is used by the `ItemObject` component in order to check that the goal and item transforms are equivalent. It exposes `Type`, `Color`, and `ShapeTransform` methods in order to facilitate the crucial `Equivalent` method, which checks for equivalence with another shape by comparing the types, colors, and transforms. This interface is not really useful for components other than `ItemObject`, which uses it to determine if its `Shape` is equivalent to that of its goal.

Implemented by `TaskShape.Cube`, `TaskShape.Sphere`, and `TaskShape.Cylinder`.

### Cube, Sphere, and Cylinder

Various implementations of the `TaskShape.Shape` interface. We need a different implementation for every `TaskShape.ShapeType` because each shape has different rotational symmetries.

### Angles Class

Contains helper functions for the various implementations of `Shape`. Generally not useful to the outside world.

## ItemPlacer Component

The `ItemPlacer` is used to place items at random locations in the world during runtime. Create an empty object in the scene and drag the component onto it in the inspector. However, it still needs a variety of values to be assigned in order to work correctly.

- `objectMax`: The maximum amount of items that the `ItemPlacer` can put in the world at one time
- `minX`: The minimum X coordinate at which items can be placed
- `maxX`: The maximum X coordinate at which items can be placed
- `minZ`: The minimum Z coordinate at which items can be placed
- `maxZ`: The maximum Z coordinate at which items can be placed
- `leftController`: the left controller's `GameObject`
    - needed so that they can be assigned to the generated items' Object Manipulation components
- `rightController`: the right controller's `GameObject`
    - needed so that they can be assigned to the generated items' Object Manipulation components

The `ItemPlacer` will invoke `TaskShape.ItemEventSystem.ItemPlaced` events whenever an item is placed. It will respond to `TaskShape.ItemEventSystem.ItemDestroyed` events by placing a new object.

## ItemObject Component

A component that, when placed on a `GameObject`, will check to see if it has reached its goal on every update. These are placed on every item generated by the `ItemPlacer`. However, they can also be created manually by creating a `GameObject` and dragging the `ItemObject` component onto it.

If an `ItemObject` is created manually, there are a few public values that must be set in order for it to function:

- `type`: the `TaskShape.ShapeType` value assigned to the `ItemObject`'s internal `Shape` 
    - IF THIS IS WRONG, THE GOAL CHECKING WILL ALSO BE WRONG
- `color`: the `TaskShape.ShapeColor` value assigned to the `ItemObejct`'s internal `Shape`
    - has no effect on goal checking, since the goal object is assumed to be the same shape/color as the item
- `goalObject`: the `GameObject` that represents the goal 
    - transform is compared against that of the `ItemObject`'s to check for goal completion

## ItemDestroyer Component

Can be added to world by creating an empty `GameObject` and dragging the component onto said `GameObject` in the inspector window. If active, it will respond to `TaskShape.ItemEventSystem.GoalReached` event invocations by destroying the item and goal objects.
