using UnityEngine;

public class FireFlyHints : MonoBehaviour
{
    [SerializeField] private GameObject Hints;
    PlayerProgressionManager progressionManager;

    private void Start()
    {
        if(PlayerController.Instance != null)
        {
            progressionManager = PlayerController.Instance.GetComponent<PlayerProgressionManager>();
        }
    }

    private void Update()
    {
        if (progressionManager != null && progressionManager.UnlockedStage == PlayerStage.FullBody)
        {
            Hints.SetActive(true);
            this.enabled = false;
        }
    }
}
