using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"More than one UndoManager exists in the scene. Destroying {name}, keeping {Instance.name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"{name} initialized.");
    }

    public abstract void Undo();
    public abstract void Redo();
}
