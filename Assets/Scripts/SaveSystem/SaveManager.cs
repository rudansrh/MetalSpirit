using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private SaveData currentLoadData;

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
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSaveFilePath(slotIndex), json);
        Debug.Log($"슬롯 {slotIndex}에 게임 저장 완료!");
    }

    //게임 불러오기 
    public void LoadGame(int slotIndex)
    {
        if (HasSaveFile(slotIndex))
        {
            string json = File.ReadAllText(GetSaveFilePath(slotIndex));
            currentLoadData = JsonUtility.FromJson<SaveData>(json);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(currentLoadData.savedSceneName);
        }
        else
        {
            Debug.LogWarning($"{slotIndex}번 슬롯은 비어있습니다.");
        }
    }

    // 새 게임
    public void StartNewGame(string firstSceneName)
    {
        // 첫 번째 씬을 로드하면 씬에 있는 플레이어가 기본 상태(최대 체력/스태미나)로 초기화되어 시작됩니다.
        SceneManager.LoadScene(firstSceneName);
        Debug.Log($"새 게임 시작: {firstSceneName} 씬 로드");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerController.Instance != null && currentLoadData != null)
        {
            PlayerController.Instance.transform.position = new Vector2(currentLoadData.playerPosX, currentLoadData.playerPosY);

            if (PlayerController.Instance.TryGetComponent<Health>(out var health))
                health.LoadHealthData(currentLoadData.playerHp);

            if (PlayerController.Instance.TryGetComponent<Stamina>(out var stamina))
                stamina.LoadStaminaData(currentLoadData.playerStamina);

            Debug.Log(currentLoadData);
        }
        currentLoadData = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}