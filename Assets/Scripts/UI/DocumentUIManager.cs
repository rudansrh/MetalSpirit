using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DocumentUIManager : MonoBehaviour
{
    public static DocumentUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject documentPanel; // 문서를 보여줄 배경
    [SerializeField] private TextMeshProUGUI documentText;

    private bool isOpen = false;

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
        documentText.text = content;
        documentPanel.SetActive(true);
        isOpen = true;

        // 문서를 읽는 동안 플레이어 조작 막기
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = false;
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
        // 문서가 열려있을 때 ESC 키를 누르면 닫기
        if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseDocument();
        }
    }
}