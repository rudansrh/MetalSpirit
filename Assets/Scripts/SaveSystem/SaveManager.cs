using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;
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

        saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    //게임 저장
    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.savedSceneName = SceneManager.GetActiveScene().name;

        if (PlayerController.Instance != null)
        {
            Vector3 pos = PlayerController.Instance.transform.position;
            data.playerPosX = pos.x;
            data.playerPosY = pos.y;

            // Health와 Stamina 컴포넌트를 가져와서 저장
            if (PlayerController.Instance.TryGetComponent<Health>(out var health))
            {
                data.playerHp = health.CurrentHealth;
            }
            if (PlayerController.Instance.TryGetComponent<Stamina>(out var stamina))
            {
                data.playerStamina = stamina.CurrentStamina;
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"게임 저장 완료! 경로: {saveFilePath}");
    }

    //파일이 존재하는지 체크
    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    //게임 불러오기
    public void LoadGame()
    {
        if (HasSaveFile())
        {
            string json = File.ReadAllText(saveFilePath);
            currentLoadData = JsonUtility.FromJson<SaveData>(json);

            SceneManager.sceneLoaded += OnSceneLoaded;

            SceneManager.LoadScene(currentLoadData.savedSceneName);
        }
        else
        {
            Debug.LogWarning("세이브 파일이 존재하지 않습니다.");
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerController.Instance != null && currentLoadData != null)
        {
            PlayerController.Instance.transform.position = new Vector2(currentLoadData.playerPosX, currentLoadData.playerPosY);

            if (PlayerController.Instance.TryGetComponent<Health>(out var health))
            {
                health.LoadHealthData(currentLoadData.playerHp);
            }
            if (PlayerController.Instance.TryGetComponent<Stamina>(out var stamina))
            {
                stamina.LoadStaminaData(currentLoadData.playerStamina);
            }

            Debug.Log("플레이어 상태 및 위치 복구 완료");
        }

        currentLoadData = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}