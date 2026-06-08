using UnityEngine;

public class CobwebObstacle : MonoBehaviour
{
    [SerializeField] private float slowDebuffRate = 0.4f; // 60% 느려짐 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() is PlayerController controller)
        {
            controller.SetSpeedMultiplier(slowDebuffRate);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() is PlayerController controller)
        {
            controller.ResetSpeedMultiplier();
        }
    }
}
