using UnityEngine;

public enum ItemType {Empty, Health, Stamina }

public class RecoveryItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    public ItemType type;               // 인스펙터에서 아이템 종류 선택
    public float recoveryAmount = 20f;  // 회복량

    public void Interact(GameObject interactor)
    {
        if(interactor.TryGetComponent<PlayerAbilityManager>(out var abilityManager)
            && abilityManager.canUseInventory
            && interactor.GetComponent<InventoryManager>().AddItem(type, recoveryAmount))
        {   
            Debug.Log($"{type} 아이템 획득");
            Destroy(gameObject);
            return;
        }

        if (type == ItemType.Health)
        {
            if (interactor.TryGetComponent<Health>(out var health))
            {
                health.RestoreHealth(recoveryAmount);
                Debug.Log($"체력 회복! 현재 체력: {health.CurrentHealth}");
            }
        }
        else if (type == ItemType.Stamina)
        {
            if (interactor.TryGetComponent<Stamina>(out var stamina))
            {
                stamina.RestoreStamina(recoveryAmount); // Stamina.cs에 추가할 메서드
                Debug.Log($"스태미나 회복! 현재 스태미나: {stamina.CurrentStamina}");
            }
        }

        // 아이템 습득 후 맵에서 제거
        Destroy(gameObject);
    }
}