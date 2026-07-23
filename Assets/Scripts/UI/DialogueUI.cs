using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public RectTransform bubble;
    public TMP_Text text;

    Transform target;

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

    public void Show(string message, Transform targetTransform)
    {
        target = targetTransform;

        text.text = message;

        bubble.gameObject.SetActive(true);
    }

    public void Hide()
    {
        bubble.gameObject.SetActive(false);
        target = null;
    }
}