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

    // ���� ����
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
            TurotialUIManager.Instance.OpenTutorial(0);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSaveFilePath(slotIndex), json);
        Debug.Log($"���� {slotIndex}�� ���� ���� �Ϸ�!");
        Debug.Log($"���̺� ���: {GetSaveFilePath(slotIndex)}");
    }

    //���� �ҷ����� 
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
            Debug.LogWarning($"{slotIndex}�� ������ ����ֽ��ϴ�.");
        }
    }

    // �� ����
    public void StartNewGame(string firstSceneName)
    {
        // ù ��° ���� �ε��ϸ� ���� �ִ� �÷��̾ �⺻ ����(�ִ� ü��/���¹̳�)�� �ʱ�ȭ�Ǿ� ���۵˴ϴ�.
        lastSlot = 0;
        newGameStarted = true;

        if (PlayerController.Instance.TryGetComponent<Health>(out var health))
        {
            health.PlayerIsDead = false; // ü�� �ε� �� ��� ���� �ʱ�ȭ
            health.gameOver = false; // ���� ���� ���� �ʱ�ȭ
        }

        SceneManager.LoadScene(firstSceneName);
        Debug.Log($"�� ���� ����: {firstSceneName} �� �ε�");
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
                health.PlayerIsDead = false; // ü�� �ε� �� ��� ���� �ʱ�ȭ
                health.gameOver = false; // ���� ���� ���� �ʱ�ȭ

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
                    Debug.Log("�κ��丮 �ε� �Ϸ�");
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

        PlayerController.Instance.Invincibility(1f); // 플레이어 부활 후 1초 무적
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