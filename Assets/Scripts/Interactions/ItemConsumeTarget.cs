using UnityEngine;
using UnityEngine.Events;

public class ItemConsumeTarget : MonoBehaviour, IInteractable
{
    [Header("Item Requirements")]
    [SerializeField] ItemType requiredItemType = ItemType.Empty;
    [SerializeField] int requiredItemCount = 1;
    [SerializeField] bool consumeItemOnResolve = true;
    [SerializeField] bool requireInventoryAbility = true;

    [Header("Resolve Behaviour")]
    [SerializeField] bool resolveOnlyOnce = true;
    [SerializeField] bool destroyOnResolve = false;
    [SerializeField] GameObject targetToResolve;

    [Header("Feedback")]
    [SerializeField] string successLogMessage = "Target resolved.";
    [SerializeField] string missingItemLogMessage = "Required item is missing.";
    [SerializeField] string inventoryLockedLogMessage = "Inventory use is not available yet.";
    [SerializeField] UnityEvent onResolved;
    [SerializeField] UnityEvent onFailed;

    bool isResolved;

    public bool IsResolved => isResolved;

    public void Interact(GameObject interactor)
    {
        if (resolveOnlyOnce && isResolved)
        {
            return;
        }

        if (interactor == null)
        {
            return;
        }

        if (requireInventoryAbility
            && (!interactor.TryGetComponent<PlayerAbilityManager>(out var abilityManager) || !abilityManager.canUseInventory))
        {
            Debug.Log($"{name}: {inventoryLockedLogMessage}");
            onFailed?.Invoke();
            return;
        }

        if (requiredItemType != ItemType.Empty)
        {
            if (!interactor.TryGetComponent<InventoryManager>(out var inventoryManager))
            {
                Debug.LogWarning($"{name}: InventoryManager was not found on interactor.");
                onFailed?.Invoke();
                return;
            }

            bool resolved = consumeItemOnResolve
                ? inventoryManager.TryConsumeItem(requiredItemType, requiredItemCount)
                : inventoryManager.HasItem(requiredItemType, requiredItemCount);

            if (!resolved)
            {
                Debug.Log($"{name}: {missingItemLogMessage} ({requiredItemType} x{requiredItemCount})");
                onFailed?.Invoke();
                return;
            }
        }

        ResolveTarget();
    }

    [ContextMenu("Resolve Target")]
    public void ResolveTarget()
    {
        if (resolveOnlyOnce && isResolved)
        {
            return;
        }

        isResolved = true;
        Debug.Log($"{name}: {successLogMessage}");
        onResolved?.Invoke();

        GameObject resolvedTarget = targetToResolve != null ? targetToResolve : gameObject;
        if (destroyOnResolve)
        {
            Destroy(resolvedTarget);
            return;
        }

        resolvedTarget.SetActive(false);
    }

    [ContextMenu("Reset Resolved State")]
    public void ResetResolvedState()
    {
        isResolved = false;
    }
}
