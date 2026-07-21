using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel; // 일시정지 메뉴 UI 패널
    [SerializeField] private string titleSceneName = "SpaceStar_Title"; // 시작 화면 씬의 정확한 이름

    private bool isPaused = false;

    private void Start()
    {
        // 게임이 시작될 때 일시정지 메뉴는 숨겨둡니다.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // ESC 키를 눌렀을 때 일시정지 토글
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // 일시정지 상태를 켜고 끄는 메서드
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // 게임 내 모든 시간 흐름 정지 (적, 애니메이션 등)
        }
        else
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // 시간 흐름 정상화
        }
    }

    // '계속하기' 버튼에 연결할 메서드
    public void ResumeGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    // '시작 화면으로' 버튼에 연결할 메서드
    public void GoToTitleScreen()
    {
        // 씬을 이동하기 전에 반드시 시간을 원래대로 돌려놓아야 합니다! (안 그러면 다음 씬도 멈춰있음)
        Time.timeScale = 1f;

        // 씬 이동
        SceneManager.LoadScene(titleSceneName);
    }
}