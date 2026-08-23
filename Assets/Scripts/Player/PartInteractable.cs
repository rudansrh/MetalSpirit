using UnityEngine;

public class PartInteractable : MonoBehaviour, IInteractable
{
    [Header("Part Settings")]
    public PlayerStage targetStage; // 유니티 인스펙터에서 Legs, Arms, FullBody 중 하나를 선택

    [Header("UI Settings")]
    [TextArea(3, 5)]
    public string unlockMessage = "새로운 파츠를 획득했습니다!";

    private string purpose = "파츠 획득";
    public string Purpose => purpose;

    [TextArea]
    [SerializeField] string binary = "";

    public void Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent<PlayerProgressionManager>(out var progressionManager))
        {

            progressionManager.SetUnlockedStage(targetStage);

            if (interactor.TryGetComponent<PlayerAbilityManager>(out var abilityManager))
            {
                if (abilityManager.isSoul)
                {
                    abilityManager.PossessBody();
                }
            }

            if(targetStage == PlayerStage.Legs)
            {
                if (TurotialUIManager.Instance != null)
                {
                    TurotialUIManager.Instance.OpenTutorial(3);
                }
            }

            if (targetStage == PlayerStage.Arms)
            {
                if (TurotialUIManager.Instance != null)
                {
                    TurotialUIManager.Instance.OpenTutorial(6);
                }
            }

            if (targetStage == PlayerStage.FullBody)
            {
                if (interactor.TryGetComponent<Health>(out var health))
                    health.RestoreHealth(health.MaxHealth);

                if (interactor.TryGetComponent<Stamina>(out var stamina))
                    stamina.RestoreStamina(stamina.MaxStamina);
                if(interactor.TryGetComponent<InventoryManager>(out var inventoryManager))
                {
                    inventoryManager.AddItem(ItemType.BinaryCode, 0, 1, binary);
                    Debug.Log("Binary Code 1개 획득");
                }

                if(TurotialUIManager.Instance != null)
                {
                    TurotialUIManager.Instance.OpenTutorial(8);
                }
            }

            Debug.Log($"[{targetStage}] 파츠 획득: {unlockMessage}");

            Destroy(gameObject);
        }
    }
}