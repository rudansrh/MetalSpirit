using System;
using System.Text;
using UnityEngine;

public enum SpeakerType
{
    Player,
    Npc
}

[Serializable]
public class DialogueTextElement
{
    [TextArea(1, 3)]
    public string text;
    public bool bold;
    public Color color = Color.black;
}

[Serializable]
public class DialogueLine
{
    public SpeakerType speaker;
    public DialogueTextElement[] textElements;

    [HideInInspector] public string text;
    [HideInInspector] public bool textBold;
    [HideInInspector] public Color color = Color.black;

    public string BuildRichText()
    {
        EnsureTextElements();

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

            AppendStyledText(builder, element);
        }

        return builder.ToString();
    }

    public bool TryUpgradeLegacyData()
    {
        if (textElements != null && textElements.Length > 0)
        {
            return false;
        }

        if (string.IsNullOrEmpty(text) && !textBold && color == Color.black)
        {
            return false;
        }

        textElements = new[]
        {
            new DialogueTextElement
            {
                text = text ?? string.Empty,
                bold = textBold,
                color = color
            }
        };

        text = string.Empty;
        textBold = false;
        color = Color.black;

        return true;
    }

    void EnsureTextElements()
    {
        if (textElements == null || textElements.Length == 0)
        {
            TryUpgradeLegacyData();
        }
    }

    static void AppendStyledText(StringBuilder builder, DialogueTextElement element)
    {
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
