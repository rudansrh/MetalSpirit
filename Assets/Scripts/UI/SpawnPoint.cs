using UnityEngine;

public class PortalSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    public string spawnPointID; // 이 도착 지점의 고유 ID

    private void Start()
    {
        if (ScenePortal.TargetSpawnPointID == spawnPointID)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.transform.position = this.transform.position;
                Debug.Log("맵 이동 완료");
            }
        }
    }
}