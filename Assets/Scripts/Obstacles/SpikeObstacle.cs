using UnityEngine;

public class SpikeObstacle : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float knockbackForce = 5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 데미지 적용
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Normal);
        }

        // 가시 효과: 튕겨내기
        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
            knockbackDir.y = Mathf.Abs(knockbackDir.y) + 0.5f; // 위쪽으로 lerp하여 튕겨내기 방향 조정
            rb.linearVelocity = Vector2.zero; // 기존 속도 초기화
            rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
