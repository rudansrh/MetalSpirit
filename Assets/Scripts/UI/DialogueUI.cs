using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public RectTransform bubble;
    public TMP_Text text;
    [SerializeField] Image portraitImage;
    [SerializeField] Image portraitBackgroundImage;
    [SerializeField] Vector2 portraitSize = new Vector2(96f, 96f);
    [SerializeField] Vector2 portraitBackgroundPadding = new Vector2(20f, 20f);
    [SerializeField] Vector2 portraitOffset = new Vector2(-24f, 0f);

    [Header("Screen Bounds Clamp")]
    [SerializeField] float paddingX = 500f;
    [SerializeField] float paddingY = 100f;

    Transform target;
    HorizontalLayoutGroup bubbleLayout;
    LayoutElement portraitLayoutElement;
    LayoutElement portraitBackgroundLayoutElement;

    [SerializeField] float typingSpeed = 0.03f;
    Coroutine typingCoroutine;
    string currentMessage;
    bool isTyping = false;

    public bool IsTyping => isTyping;

    void Awake()
    {
        Instance = this;
        ResolveReferences();

        if (text != null)
        {
            text.richText = true;
        }

        ApplyPortrait(null);
        bubble.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 1.5f);
        targetPos.x += (Mathf.Max(0, bubble.rect.width - 268 - portraitSize.x) / 2 + 80) * bubble.localScale.x;

        targetPos.x = Mathf.Clamp(targetPos.x, paddingX, Screen.width - paddingX);
        targetPos.y = Mathf.Clamp(targetPos.y, paddingY, Screen.height - paddingY);

        bubble.position = targetPos;
    }

    public void Show(DialogueLine line, Transform targetTransform)
    {
        string message = line != null ? line.BuildRichText() : string.Empty;
        Sprite portraitSprite = line != null ? line.portraitSprite : null;
        Show(message, targetTransform, portraitSprite);
    }

    public void Show(string message, Transform targetTransform, Sprite portraitSprite = null)
    {
        target = targetTransform;
        bubble.gameObject.SetActive(true);
        ApplyPortrait(portraitSprite);

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
        ApplyPortrait(null);
        bubble.gameObject.SetActive(false);
        target = null;
    }

    void ResolveReferences()
    {
        if (bubble == null)
        {
            return;
        }

        if (text == null)
        {
            text = bubble.GetComponentInChildren<TMP_Text>(true);
        }

        if (bubbleLayout == null)
        {
            bubbleLayout = bubble.GetComponent<HorizontalLayoutGroup>();
        }

        if (portraitImage == null)
        {
            Transform portraitTransform = bubble.Find("Portrait");
            if (portraitTransform != null)
            {
                portraitImage = portraitTransform.GetComponent<Image>();
            }
        }

        if (portraitBackgroundImage == null)
        {
            Transform portraitBackgroundTransform = bubble.Find("PortraitBackground");
            if (portraitBackgroundTransform != null)
            {
                portraitBackgroundImage = portraitBackgroundTransform.GetComponent<Image>();
            }
        }

        if (portraitImage != null)
        {
            portraitLayoutElement = portraitImage.GetComponent<LayoutElement>();
            if (portraitLayoutElement == null)
            {
                portraitLayoutElement = portraitImage.gameObject.AddComponent<LayoutElement>();
            }

            portraitLayoutElement.ignoreLayout = true;
        }

        if (portraitBackgroundImage != null)
        {
            portraitBackgroundLayoutElement = portraitBackgroundImage.GetComponent<LayoutElement>();
            if (portraitBackgroundLayoutElement == null)
            {
                portraitBackgroundLayoutElement = portraitBackgroundImage.gameObject.AddComponent<LayoutElement>();
            }

            portraitBackgroundLayoutElement.ignoreLayout = true;
        }
    }

    void ApplyPortrait(Sprite portraitSprite)
    {
        if (portraitImage == null)
        {
            return;
        }

        bool hasPortrait = portraitSprite != null;
        portraitImage.sprite = portraitSprite;
        portraitImage.preserveAspect = true;
        RectTransform portraitRect = portraitImage.rectTransform;
        portraitRect.anchorMin = new Vector2(0f, 0.5f);
        portraitRect.anchorMax = new Vector2(0f, 0.5f);
        portraitRect.pivot = new Vector2(1f, 0.5f);
        portraitRect.anchoredPosition = portraitOffset;
        portraitRect.sizeDelta = portraitSize;
        portraitImage.gameObject.SetActive(hasPortrait);

        if (portraitBackgroundImage != null)
        {
            RectTransform portraitBackgroundRect = portraitBackgroundImage.rectTransform;
            portraitBackgroundRect.anchorMin = portraitRect.anchorMin;
            portraitBackgroundRect.anchorMax = portraitRect.anchorMax;
            portraitBackgroundRect.pivot = portraitRect.pivot;
            portraitBackgroundRect.anchoredPosition = portraitRect.anchoredPosition;
            portraitBackgroundRect.sizeDelta = portraitSize + portraitBackgroundPadding;
            portraitBackgroundImage.gameObject.SetActive(hasPortrait);
        }

        if (bubbleLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(bubble);
        }
    }
}
