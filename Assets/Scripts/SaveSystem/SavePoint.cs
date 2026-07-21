using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        if (DocumentUIManager.Instance == null) return;

        if (DocumentUIManager.Instance.isOpen)
        {
            DocumentUIManager.Instance.CloseDocument();
        }

        else
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.canMove = false;
                PlayerController.Instance.StopMovement();
                PlayerController.Instance.StopDash();
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }

            DocumentUIManager.Instance.ShowDocument("게임이 안전하게 저장되었습니다.");
        }
    }
}