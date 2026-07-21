using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;
    private SaveData currentLoadData; // 불러올 때 임시로 데이터를 쥐고 있을 변수

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

        // Application.persistentDataPath: OS별로 데이터가 지워지지 않는 안전한 영구 저장 경로를 반환합니다.
        saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    // 1. 게임 저장 (세이브 포인트에서 호출)
    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 현재 씬 이름 저장
        data.savedSceneName = SceneManager.GetActiveScene().name;

        // 플레이어 정보 저장 (PlayerController 싱글톤 활용)
        if (PlayerController.Instance != null)
        {
            Vector3 pos = PlayerController.Instance.transform.position;
            data.playerPosX = pos.x;
            data.playerPosY = pos.y;

            // TODO: 실제 체력/스태미나 변수명으로 변경해 주세요.
            // data.playerHp = PlayerController.Instance.currentHp; 
            // data.playerStamina = PlayerController.Instance.currentStamina;
        }

        // 데이터를 JSON 문자열로 변환하고 디스크에 쓰기
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log($"게임 저장 완료! 경로: {saveFilePath}");
    }

    // 파일이 존재하는지 체크 (시작 화면에서 '이어하기' 버튼 활성화 여부에 사용)
    public bool HasSaveFile()
    {
        return File.Exists(saveFilePath);
    }

    // 2. 게임 불러오기 (시작 화면에서 호출)
    public void LoadGame()
    {
        if (HasSaveFile())
        {
            // JSON 파일을 읽어서 객체로 역직렬화
            string json = File.ReadAllText(saveFilePath);
            currentLoadData = JsonUtility.FromJson<SaveData>(json);

            // 씬 로드가 끝날 때까지 기다려야 하므로 이벤트를 구독합니다.
            SceneManager.sceneLoaded += OnSceneLoaded;

            // 세이브 파일에 기록된 씬으로 이동
            SceneManager.LoadScene(currentLoadData.savedSceneName);
        }
        else
        {
            Debug.LogWarning("세이브 파일이 존재하지 않습니다.");
        }
    }

    // 씬 로드가 완료된 직후에 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerController.Instance != null && currentLoadData != null)
        {
            // 저장했던 위치로 플레이어 이동
            PlayerController.Instance.transform.position = new Vector2(currentLoadData.playerPosX, currentLoadData.playerPosY);

            // TODO: 저장했던 체력과 스태미나 복구
            // PlayerController.Instance.currentHp = currentLoadData.playerHp;

            Debug.Log("플레이어 상태 및 위치 복구 완료");
        }

        // 임시 데이터 비우기 및 이벤트 구독 해제 (중복 실행 방지)
        currentLoadData = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}