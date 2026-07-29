using UnityEngine;

public class WaterObstacle : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 5f; // 초당 데미지

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            // 초당 데미지 적용
            damageable.TakeDamage(damagePerSecond * Time.deltaTime, DamageType.Water);
        }
    }
}