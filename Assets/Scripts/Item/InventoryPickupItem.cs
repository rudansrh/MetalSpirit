using UnityEngine;

public class InventoryPickupItem : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    [SerializeField] ItemType itemType = ItemType.Empty;
    [SerializeField] float itemAmount = 0f;
    [SerializeField] int itemCount = 1;
    [SerializeField] bool requireInventoryAbility = true;
    [SerializeField] bool destroyOnPickup = true;

    [Header("Document Settings")]
    [TextArea(5, 10)]
    [SerializeField] string documentText = "";

    private string purpose = "아이템 획득";
    public string Purpose => purpose;

    public void Interact(GameObject interactor)
    {
        if (itemType == ItemType.Empty)
        {
            Debug.LogWarning($"{name}: Empty item type cannot be picked up.");
            return;
        }

        if (!interactor.TryGetComponent<InventoryManager>(out var inventoryManager))
        {
            Debug.LogWarning($"{name}: InventoryManager was not found on interactor.");
            return;
        }

        if (requireInventoryAbility
            && (!interactor.TryGetComponent<PlayerAbilityManager>(out var abilityManager) || !abilityManager.canUseInventory))
        {
            Debug.Log($"{name}: Inventory use is not available yet.");
            return;
        }

        if (!inventoryManager.AddItem(itemType, itemAmount, itemCount, documentText))
        {
            Debug.Log($"{name}: Failed to add {itemType} to inventory.");
            return;
        }

        Debug.Log($"{itemType} item acquired x{itemCount}.");

        if (destroyOnPickup)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }
}
