using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private SaveData currentLoadData;
    private int lastSlot = 0;
    private bool playerRevive = false;
    private bool newGameStarted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"savefile_{slotIndex}.json");
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetSaveFilePath(slotIndex));
    }

    // 게임 저장
    public void SaveGame(int slotIndex)
    {
        SaveData data = new SaveData();
        data.savedSceneName = SceneManager.GetActiveScene().name;

        if (PlayerController.Instance != null)
        {
            Vector3 pos = PlayerController.Instance.transform.position;
            data.playerPosX = pos.x;
            data.playerPosY = pos.y;

            if (PlayerController.Instance.TryGetComponent<Health>(out var health))
                data.playerHp = health.CurrentHealth;

            if (PlayerController.Instance.TryGetComponent<Stamina>(out var stamina))
                data.playerStamina = stamina.CurrentStamina;

            if (PlayerController.Instance.TryGetComponent<PlayerProgressionManager>(out var progressionManager))
                data.currentPlayerStage = progressionManager.UnlockedStage;

            if (PlayerController.Instance.TryGetComponent<PlayerAbilityManager>(out var playerAbility))
            {
                if (playerAbility.canUseInventory && PlayerController.Instance.TryGetComponent<InventoryManager>(out var inventory))
                {
                    data.inventoryItems = inventory.items;
                    Debug.Log(data.inventoryItems.Length);
                }
                data.isSoulState = playerAbility.isSoul;
            }
            data.unlockedPassword = PlayerController.Instance.unlockedPassword;
            PlayerController.Instance.lastSavedSlot = slotIndex;
        }

        if(newGameStarted)
        {
            newGameStarted = false;
            if (TurotialUIManager.Instance != null)
            {
                TurotialUIManager.Instance.OpenTutorial(0);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSaveFilePath(slotIndex), json);
        Debug.Log($"{slotIndex}번 슬롯 저장 완료!");
        Debug.Log($"세이브 경로: {GetSaveFilePath(slotIndex)}");
    }

    // 게임 불러오기
    public void LoadGame(int slotIndex)
    {
        if (HasSaveFile(slotIndex))
        {
            string json = File.ReadAllText(GetSaveFilePath(slotIndex));
            currentLoadData = JsonUtility.FromJson<SaveData>(json);
            lastSlot = slotIndex;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(currentLoadData.savedSceneName);
        }
        else
        {
            Debug.LogWarning($"{slotIndex}번 슬롯이 비어 있습니다.");
        }
    }

    // 새 게임
    public void StartNewGame(string firstSceneName)
    {
        // 첫 번째 씬을 로드하면 기존 플레이어가 기본 상태로 초기화되어 시작됩니다.
        lastSlot = 0;
        newGameStarted = true;

        if (PlayerController.Instance != null &&
            PlayerController.Instance.TryGetComponent<Health>(out var health))
        {
            health.PlayerIsDead = false; // 체력 로드 전 사망 상태 초기화
            health.gameOver = false; // 게임 오버 상태 초기화
        }

        SceneManager.LoadScene(firstSceneName);
        Debug.Log($"새 게임 시작: {firstSceneName} 씬 로드");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (PlayerController.Instance != null && currentLoadData != null)
        {
            PlayerController.Instance.transform.position = new Vector2(currentLoadData.playerPosX, currentLoadData.playerPosY);
            PlayerController.Instance.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            if (PlayerController.Instance.TryGetComponent<Health>(out var health))
            {
                health.LoadHealthData(currentLoadData.playerHp);
                health.PlayerIsDead = false; // 체력 로드 전 사망 상태 초기화
                health.gameOver = false; // 게임 오버 상태 초기화

                if (playerRevive)
                {
                    health.LoadHealthData(health.MaxHealth);
                }
            }

            if (PlayerController.Instance.TryGetComponent<Stamina>(out var stamina))
            {
                stamina.LoadStaminaData(currentLoadData.playerStamina);

                if (playerRevive)
                {
                    stamina.LoadStaminaData(stamina.MaxStamina);
                }
            }

            if (PlayerController.Instance.TryGetComponent<PlayerProgressionManager>(out var progressionManager))
                progressionManager.SetUnlockedStage(currentLoadData.currentPlayerStage);

            if (PlayerController.Instance.TryGetComponent<PlayerAbilityManager>(out var playerAbility))
            {
                if (PlayerController.Instance.TryGetComponent<InventoryManager>(out var inventory))
                {
                    inventory.LoadInventory(currentLoadData.inventoryItems);
                    Debug.Log("인벤토리 로드 완료");
                }
                playerAbility.isSoul = currentLoadData.isSoulState;
            }

            PlayerController.Instance.lastSavedSlot = lastSlot;
            PlayerController.Instance.unlockedPassword = currentLoadData.unlockedPassword;
            PlayerController.Instance.isUIopen = false;
            PlayerController.Instance.canMove = true;
        }

        currentLoadData = null;
        playerRevive = false;
    }

    public void YouDied()
    {
        Debug.Log("You Died");
        if (PlayerController.Instance.isPossessing)
        {
            PlayerController.Instance.DepossessFromEnemy();
        }
        lastSlot = PlayerController.Instance.lastSavedSlot;
        string json = File.ReadAllText(GetSaveFilePath(PlayerController.Instance.lastSavedSlot));
        currentLoadData = JsonUtility.FromJson<SaveData>(json);
        playerRevive = true;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(currentLoadData.savedSceneName);
    }
}
