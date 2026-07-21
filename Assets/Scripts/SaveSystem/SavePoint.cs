using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();

            // 전에 만드신 DocumentUIManager를 활용해서 "저장되었습니다." 텍스트를 띄우면 아주 좋습니다.
            if (DocumentUIManager.Instance != null)
            {
                DocumentUIManager.Instance.ShowDocument("게임이 안전하게 저장되었습니다.");
            }
        }
    }
}