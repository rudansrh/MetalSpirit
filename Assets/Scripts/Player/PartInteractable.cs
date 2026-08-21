using UnityEngine;

public class PartInteractable : MonoBehaviour, IInteractable
{
    [Header("Part Settings")]
    public PlayerStage targetStage; // À¯´ÏÆ¼ ÀÎ½ºÆåÅÍ¿¡¼­ Legs, Arms, FullBody Áß ÇÏ³ª¸¦ ¼±ÅÃ

    [Header("UI Settings")]
    [TextArea(3, 5)]
    public string unlockMessage = "»õ·Î¿î ÆÄÃ÷¸¦ È¹µæÇß½À´Ï´Ù!";

    private string purpose = "ÆÄÃ÷ È¹µæ";
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
                    TurotialUIManager.Instance.OpenTutorial(5);
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
                    Debug.Log("Binary Code 1°³ È¹µæ");
                }

                if(TurotialUIManager.Instance != null)
                {
                    TurotialUIManager.Instance.OpenTutorial(8);
                }
            }

            Debug.Log($"[{targetStage}] ÆÄÃ÷ È¹µæ: {unlockMessage}");

            Destroy(gameObject);
        }
    }
}