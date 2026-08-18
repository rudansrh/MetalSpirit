using UnityEngine;

public class ReadableDocument : MonoBehaviour, IInteractable
{
    [Header("Document Settings")]
    [TextArea(5, 10)]
    public string documentContent = "이곳에 보고서나 문서의 내용을 입력하세요.";
    public DialogueTextElement[] documentTextElements;

    private string purpose = "문서 읽기";
    public string Purpose => purpose;

    public void Interact(GameObject interactor)
    {
        if (DocumentUIManager.Instance == null)
        {
            return;
        }

        if (DocumentUIManager.Instance.isOpen)
        {
            DocumentUIManager.Instance.CloseDocument();
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.isUIopen = false;
            }
        }
        else
        {
            DocumentUIManager.Instance.ShowDocument(documentContent, documentTextElements);
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.isUIopen = true;
            }
        }
    }
}
