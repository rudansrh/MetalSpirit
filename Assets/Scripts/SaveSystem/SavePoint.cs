using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    private string purpose = "세이브";
    public string Purpose => purpose;
    public void Interact(GameObject interactor)
    {
        if (SaveSlotUIManager.Instance == null) return;

        // 이미 슬롯 창이 열려있다면 닫기 (토글 방식)
        if (SaveSlotUIManager.Instance.isOpen)
        {
            SaveSlotUIManager.Instance.CloseSlotUI();
        }
        // 닫혀있다면 슬롯 선택 창 열기
        else
        {
            SaveSlotUIManager.Instance.OpenSlotUI();
        }
    }
}