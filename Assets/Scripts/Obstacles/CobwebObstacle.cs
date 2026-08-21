using UnityEngine;

public class CobwebObstacle : MonoBehaviour
{
    [SerializeField] private float slowDebuffRate = 0.4f; // 60% 느려짐 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var controller))
            {
                controller.SetSpeedMultiplier(slowDebuffRate);
                controller.StopDash();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var controller))
            {
                controller.ResetSpeedMultiplier();
            }
        }
    }
}