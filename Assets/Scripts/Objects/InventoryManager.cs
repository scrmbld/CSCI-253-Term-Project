using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Input")]
    // we'll drag the LeftHand Activate action here
    public InputActionProperty leftActivateAction;

    [Header("Raycast")]
    public Transform cameraTransform;      // Main Camera
    public float rayDistance = 10f;
    public LayerMask worldLayers;
    public LayerMask inventoryUILayers;

    [Header("UI")]
    public Transform bagPanelRoot;         // Panel
    public GameObject itemIconPrefab;      // ItemIcon prefab

    private readonly List<PickableItem> _items = new List<PickableItem>();

    private void OnEnable()
    {
        if (leftActivateAction.action != null)
        {
            leftActivateAction.action.performed += OnLeftActivate;
            leftActivateAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (leftActivateAction.action != null)
        {
            leftActivateAction.action.performed -= OnLeftActivate;
        }
    }

    private void OnLeftActivate(InputAction.CallbackContext ctx)
    {
        Debug.Log("Left trigger pressed");

        // 1) Try to hit an inventory icon
        if (TryHitInventorySlot(out InventorySlot slot))
        {
            Debug.Log("Hit inventory slot: " + slot.name);
            TakeItemOutOfInventory(slot);
            return;
        }

        // 2) Otherwise, try to hit a world item
        if (TryHitWorldItem(out PickableItem item))
        {
            Debug.Log("Hit world item: " + item.name);
            PutItemIntoInventory(item);
        }
        else
        {
            Debug.Log("Did not hit any pickable item.");
        }
    }

    bool TryHitWorldItem(out PickableItem item)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, worldLayers))
        {
            item = hit.collider.GetComponentInParent<PickableItem>();
            if (item != null && !item.inInventory)
                return true;
        }

        item = null;
        return false;
    }

    bool TryHitInventorySlot(out InventorySlot slot)
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.blue, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, inventoryUILayers))
        {
            slot = hit.collider.GetComponentInParent<InventorySlot>();
            if (slot != null && slot.item != null)
                return true;
        }

        slot = null;
        return false;
    }

    void PutItemIntoInventory(PickableItem item)
    {
        if (_items.Contains(item)) return;

        _items.Add(item);
        item.inInventory = true;

        item.gameObject.SetActive(false);   // hide world object

        GameObject iconGO = Instantiate(itemIconPrefab, bagPanelRoot);
        iconGO.name = $"Inv_{_items.Count}_{item.itemName}";

        Image img = iconGO.GetComponent<Image>();
        if (img != null && item.icon != null)
            img.sprite = item.icon;

        InventorySlot slot = iconGO.GetComponent<InventorySlot>();
        slot.item = item;

        Debug.Log("Item stored: " + item.itemName);
    }

    void TakeItemOutOfInventory(InventorySlot slot)
    {
        PickableItem item = slot.item;
        if (item == null) return;

        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * 1.5f;
        item.transform.position = spawnPos;
        item.transform.rotation = Quaternion.LookRotation(-cameraTransform.forward);
        item.gameObject.SetActive(true);
        item.inInventory = false;

        _items.Remove(item);
        Destroy(slot.gameObject);

        Debug.Log("Item spawned back out: " + item.itemName);
    }
}
