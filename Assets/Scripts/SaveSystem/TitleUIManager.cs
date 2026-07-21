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

    // '이어하기' 버튼이 클릭되었을 때 실행될 메서드
    public void OnClickLoadGame()
    {
        // 씬 어딘가에 살아남아 있는 진짜 SaveManager.Instance를 찾아서 명령을 내립니다.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("SaveManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    // (참고용) '새 게임' 버튼에 연결할 메서드도 여기에 만들어두면 편합니다.
    public void OnClickNewGame()
    {
        // 첫 번째 스테이지 씬의 이름을 적어주세요.
        SceneManager.LoadScene("test1"); 
    }
}