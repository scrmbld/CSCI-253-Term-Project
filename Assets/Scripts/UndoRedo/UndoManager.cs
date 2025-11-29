using UnityEngine;
using UnityEngine.PlayerLoop;

public abstract class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }
    public string condition;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"More than one UndoManager exists in the scene. Destroying {name}, keeping {Instance.name}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        condition = name == "Experiment (disable Control)" ? "Experiment" : "Control";
        Debug.Log($"{condition} manager initialized.");
        
    }

    // ButtonManager.cs will call these every frame with button info
    public abstract void OnUndoInput(bool isHeld, bool justPressed, bool wasReleased);
    public abstract void OnRedoInput(bool isHeld, bool justPressed, bool wasReleased);
}
