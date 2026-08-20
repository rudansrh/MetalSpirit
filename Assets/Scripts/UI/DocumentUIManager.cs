using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Text;

public class DocumentUIManager : MonoBehaviour
{
    public static DocumentUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject documentPanel;
    [SerializeField] private TextMeshProUGUI documentText;

    public bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        documentPanel.SetActive(false);
    }

    public void ShowDocument(string content)
    {
        ShowDocument(content, null);
    }

    public void ShowDocument(string fallbackContent, DialogueTextElement[] textElements)
    {
        string content = BuildRichText(textElements);
        if (string.IsNullOrEmpty(content))
        {
            content = fallbackContent ?? string.Empty;
        }

        documentText.text = content;
        documentPanel.SetActive(true);
        isOpen = true;
        AudioManager.instance?.PlaySfx(AudioManager.Sfx.PaperFlip); //***

        // 문서를 읽는 동안 플레이어 조작 막기
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = false;
            PlayerController.Instance.StopMovement();
            PlayerController.Instance.StopDash();
        }
    }

    public void CloseDocument()
    {
        documentPanel.SetActive(false);
        isOpen = false;

        // 플레이어 조작 다시 활성화
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = true;
        }
    }

    private void Update()
    {
        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseDocument();
        }
    }

    static string BuildRichText(DialogueTextElement[] textElements)
    {
        if (textElements == null || textElements.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();

        foreach (DialogueTextElement element in textElements)
        {
            if (element == null)
            {
                continue;
            }

            builder.Append("<color=#");
            builder.Append(ColorUtility.ToHtmlStringRGBA(element.color));
            builder.Append(">");

            if (element.bold)
            {
                builder.Append("<b>");
            }

            builder.Append(EscapeRichText(element.text));

            if (element.bold)
            {
                builder.Append("</b>");
            }

            builder.Append("</color>");
        }

        return builder.ToString();
    }

    static string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
