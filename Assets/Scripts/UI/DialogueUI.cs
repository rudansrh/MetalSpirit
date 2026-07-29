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
        bubble.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target == null) return;

        bubble.position = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1f);
    }

    public void Show(string message, Transform targetTransform, Color32 color, bool isBold)
    {
        target = targetTransform;
        text.color = color;
        text.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
        bubble.gameObject.SetActive(true);

        currentMessage = message;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        isTyping = true;
        text.text = "";

        foreach (char c in currentMessage)
        {
            text.text += c;
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
        isTyping = false;
    }

    public void Hide()
    {
        bubble.gameObject.SetActive(false);
        target = null;
    }
}