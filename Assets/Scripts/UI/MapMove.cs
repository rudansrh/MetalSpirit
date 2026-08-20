using UnityEngine;
using UnityEngine.SceneManagement;
public class ScenePortal : MonoBehaviour
{
    [Header("Transition Settings")]
    public string targetSceneName; // 이동할 다음 씬의 이름
    public string spawnPointID;    // 다음 씬에서 플레이어가 등장할 위치 ID

    public static string TargetSpawnPointID;

    [SerializeField]bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.TryGetComponent<PlayerAbilityManager>(out var playerAbility))
        {
            if (playerAbility.isSoul) return;
        }
        */

        if (!isActivated) return;

        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            Debug.Log($"{targetSceneName} 씬으로 이동합니다...");

            player.isMovingToNextScene = true;
            TargetSpawnPointID = spawnPointID;
            SceneManager.LoadScene(targetSceneName);
        }
    }

    public void activatePortal()
    {
        isActivated = true;
    }
}