using UnityEngine;

public class ReadableDocument : MonoBehaviour, IInteractable
{
    [Header("Document Settings")]
    [TextArea(5, 10)]
    public string documentContent = "이곳에 보고서나 문서의 내용을 입력하세요.";

    public void Interact(GameObject interactor)
    {
        if (DocumentUIManager.Instance == null) return;

        // 이미 문서 창이 열려있으면 닫기
        if (DocumentUIManager.Instance.isOpen)
        {
            DocumentUIManager.Instance.CloseDocument();
            PlayerController.Instance.isUIopen = false;
        }
        // 닫혀있으면 문서 내용 띄우기
        else
        {
            DocumentUIManager.Instance.ShowDocument(documentContent);
            PlayerController.Instance.isUIopen = true;
        }
    }
}