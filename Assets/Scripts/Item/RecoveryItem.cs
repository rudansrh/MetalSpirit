using UnityEngine;

public class RecoveryItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    public ItemType type;
    public float recoveryAmount = 20f;

    public void Interact(GameObject interactor)
    {
        if(interactor.TryGetComponent<PlayerAbilityManager>(out var abilityManager)
            && abilityManager.canUseInventory
            && interactor.GetComponent<InventoryManager>().AddItem(type, recoveryAmount))
        {   
            Debug.Log($"{type} item acquired.");
            Destroy(gameObject);
            return;
        }

        if (type == ItemType.Health)
        {
            if (interactor.TryGetComponent<Health>(out var health))
            {
                health.RestoreHealth(recoveryAmount);
                Debug.Log($"Health restored. Current Health: {health.CurrentHealth}");
            }
        }
        else if (type == ItemType.Stamina)
        {
            if (interactor.TryGetComponent<Stamina>(out var stamina))
            {
                stamina.RestoreStamina(recoveryAmount);
                Debug.Log($"Stamina restored. Current Stamina: {stamina.CurrentStamina}");
            }
        }

        Destroy(gameObject);
    }
}
