using UnityEngine;

public class ReadableDocument : MonoBehaviour, IInteractable
{
    [Header("Document Settings")]
    [TextArea(5, 10)]
    public string documentContent = "이곳에 보고서나 문서의 내용을 입력하세요.";

    public void Interact(GameObject interactor)
    {
        if (DocumentUIManager.Instance != null)
        {
            DocumentUIManager.Instance.ShowDocument(documentContent);
        }
        else
        {
            Debug.LogWarning("씬에 DocumentUIManager가 없습니다!");
        }
    }
}