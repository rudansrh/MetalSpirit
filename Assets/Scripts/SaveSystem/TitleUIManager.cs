using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleUIManager : MonoBehaviour
{
    private void Awake()
    {
        // 타이틀 화면에 플레이어가 남아있다면 파괴
        if (PlayerController.Instance != null)
        {
            Destroy(PlayerController.Instance.gameObject);
        }
    }

    // 이어하기 버튼
    public void OnClickLoadGame()
    {
        if (SaveManager.Instance != null)
        {
            //SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("SaveManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    // 새 게임 버튼
    public void OnClickNewGame()
    {
        if (SaveManager.Instance != null)
        {
            //SaveManager.Instance.NewGame("LegZone");
        }
        else
        {
            SceneManager.LoadScene("Stage1");
        }
    }

    public void OnClickQuitGame()
    {
        Debug.Log("게임을 종료합니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}