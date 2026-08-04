using UnityEngine;

public class SaveSlotUIManager : MonoBehaviour
{
    public static SaveSlotUIManager Instance { get; private set; }

    [Header("UI Settings")]
    public GameObject slotPanel;
    public bool isTitleScreen;

    public bool isOpen = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (slotPanel != null) slotPanel.SetActive(false);

        // 타이틀 화면일 때 인게임에서 넘어온 객체들을 파괴
        if (isTitleScreen)
        {
            if (PlayerController.Instance != null)
            {
                Destroy(PlayerController.Instance.gameObject);
            }

            if (cameraFollow.Instance != null)
            {
                Destroy(cameraFollow.Instance.gameObject);
            }

            if (DocumentUIManager.Instance != null)
            {
                Destroy(DocumentUIManager.Instance.gameObject);
            }

            PlayerStatsUIManager statsUI = FindAnyObjectByType<PlayerStatsUIManager>();
            if (statsUI != null)
            {
                Destroy(statsUI.gameObject);
            }
        }
    }

    public void OpenSlotUI()
    {
        slotPanel.SetActive(true);
        isOpen = true;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = false;
            PlayerController.Instance.isUIopen = true;
            PlayerController.Instance.StopMovement();
            PlayerController.Instance.StopDash();
        }
    }

    public void CloseSlotUI()
    {
        slotPanel.SetActive(false);
        isOpen = false;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = true;
            PlayerController.Instance.isUIopen = false;
        }
    }

    // 새 게임 버튼을 눌렀을 때
    public void OnClickNewGameDirectly()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.StartNewGame("LegZone");
        }
    }

    // 슬롯을 누를 때
    public void OnClickSlot(int slotIndex)
    {
        if (isTitleScreen)
        {
            // [타이틀 화면 - Load 전용] 
            if (SaveManager.Instance.HasSaveFile(slotIndex))
            {
                SaveManager.Instance.LoadGame(slotIndex);
            }
            else
            {
                Debug.LogWarning($"{slotIndex}번 슬롯은 비어있습니다. 로드할 수 없습니다.");
            }
        }
        else
        {
            // [인게임 - Save 전용] 
            SaveManager.Instance.SaveGame(slotIndex);
            CloseSlotUI();

        }
    }
}