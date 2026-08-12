using UnityEngine;

public class PartInteractable : MonoBehaviour, IInteractable
{
    [Header("Part Settings")]
    public PlayerStage targetStage; // À¯´ÏÆ¼ ÀÎ½ºÆåÅÍ¿¡¼­ Legs, Arms, FullBody Áß ÇÏ³ª¸¦ ¼±ÅÃ

    [Header("UI Settings")]
    [TextArea(3, 5)]
    public string unlockMessage = "»õ·Î¿î ÆÄÃ÷¸¦ È¹µæÇß½À´Ï´Ù!";

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

            if (targetStage == PlayerStage.FullBody)
            {
                if (interactor.TryGetComponent<Health>(out var health))
                    health.RestoreHealth(health.MaxHealth);

                if (interactor.TryGetComponent<Stamina>(out var stamina))
                    stamina.RestoreStamina(stamina.MaxStamina);
            }

            Debug.Log($"[{targetStage}] ÆÄÃ÷ È¹µæ: {unlockMessage}");

            Destroy(gameObject);
        }
    }
}