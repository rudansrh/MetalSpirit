using UnityEngine;

public class PlayerPossessChecker : MonoBehaviour
{
    PlayerAbilityManager abilityManager;
    PlayerController playerController;

    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        abilityManager = GetComponentInParent<PlayerAbilityManager>();
        playerController.possessChecker = this.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 빙의 대상 감지 로직
        if (abilityManager.canPossess)
        {
            if (collision.TryGetComponent<Enemy>(out var enemy))
            {
                playerController.targetEnemyToPossess = enemy;
                playerController.canInteractUI.showInterectUI(collision.transform, "v", "빙의");
            }
        }

        playerController.touchInteractable(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 빙의 대상 해제 로직
        if (collision.TryGetComponent<Enemy>(out var enemy))
        {
            if (playerController.targetEnemyToPossess == enemy)
            {
                playerController.targetEnemyToPossess = null;
            }
            playerController.canInteractUI.hideInterectUI();
        }

        playerController.fallFromInteractable(collision);
    }
}
