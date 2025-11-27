using UnityEngine;

public class PickableItem : MonoBehaviour
{
    public string itemName = "Item";
    public Sprite icon;         // Icon to show in panel

    [HideInInspector] public bool inInventory = false;
}
