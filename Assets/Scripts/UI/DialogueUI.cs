using TMPro;
using UnityEngine;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public RectTransform bubble;
    public TMP_Text text;

    Transform target;

    [SerializeField] float typingSpeed = 0.03f;
    Coroutine typingCoroutine;
    string currentMessage;
    bool isTyping = false;

    public bool IsTyping => isTyping;

    void Awake()
    {
        Instance = this;
        text.richText = true;
        bubble.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.5f);
        targetPos.x += (Mathf.Max(0, text.rectTransform.rect.width - 268) / 2 + 80) * bubble.localScale.x; //말풍선 꼬리위치 고정코드 수정 필요(하드코딩)
        bubble.position = targetPos;
    }

    public void Show(string message, Transform targetTransform)
    {
        target = targetTransform;
        bubble.gameObject.SetActive(true);

        currentMessage = message ?? string.Empty;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        text.text = currentMessage;
        text.maxVisibleCharacters = 0;
        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        text.text = currentMessage;
        text.maxVisibleCharacters = 0;
        text.ForceMeshUpdate();

        int visibleCharacterCount = text.textInfo.characterCount;

        for (int i = 1; i <= visibleCharacterCount; i++)
        {
            text.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void FinishTyping()
    {
        if (!isTyping) return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        text.text = currentMessage;
        text.ForceMeshUpdate();
        text.maxVisibleCharacters = text.textInfo.characterCount;
        isTyping = false;
    }

    public void Hide()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        text.text = string.Empty;
        text.maxVisibleCharacters = 0;
        bubble.gameObject.SetActive(false);
        target = null;
    }
}
