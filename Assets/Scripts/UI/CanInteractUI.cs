using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CanInteractUI : MonoBehaviour
{
    [SerializeField] GameObject bubble;
    [SerializeField] Transform interactKeys;

    [Header("UI Offset & Clamp")]
    [Tooltip("UI가 나타날 위치를 미세 조정 (X: 좌우, Y: 상하)")]
    [SerializeField] Vector2 offset = new Vector2(0f, 0.5f);

    [Tooltip("화면 끝에서 UI가 잘리지 않도록 보호할 여백")]
    [SerializeField] float paddingX = 150f;
    [SerializeField] float paddingY = 50f;

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
            Vector3 screenPos = Camera.main.WorldToScreenPoint(keyPos); 

            screenPos.x = Mathf.Clamp(screenPos.x, paddingX, Screen.width - paddingX);
            screenPos.y = Mathf.Clamp(screenPos.y, paddingY, Screen.height - paddingY);

            bubble.transform.position = screenPos;
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