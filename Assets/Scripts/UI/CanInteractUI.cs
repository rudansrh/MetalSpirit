using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanInteractUI : MonoBehaviour
{
    [SerializeField] GameObject bubble;
    [SerializeField] Transform interactKeys;

    [Header("UI Offset")]
    [Tooltip("UI가 나타날 위치를 미세 조정 (X: 좌우, Y: 상하)")]
    [SerializeField] Vector2 offset = new Vector2(0f, 0.5f);

    TMP_Text keyText;
    Transform target;

    private void Start()
    {
        PlayerController.Instance.canInteractUI = this;
        bubble = Instantiate(bubble, interactKeys);
        bubble.SetActive(false);
        keyText = bubble.GetComponentInChildren<TMP_Text>();
    }

    private void LateUpdate()
    {
        if (bubble.activeSelf)
        {
            Vector2 keyPos = new Vector2(target.position.x + offset.x, target.position.y - target.localScale.y + offset.y);
            bubble.transform.position = Camera.main.WorldToScreenPoint(keyPos);
        }
    }

    public void showInterectUI(Transform targetPos, string key, string purpose)
    {
        target = targetPos;
        keyText.text = $"[{key}]키를 눌러 {purpose}";
        bubble.SetActive(true);
    }

    public void hideInterectUI()
    {
        bubble.SetActive(false);
    }
}