using UnityEngine;

public class SpikeObstacle : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float knockbackForce = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 데미지 적용
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Normal);
        }

        // 가시 효과: 튕겨내기
        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 knockbackDir = collision.transform.position - transform.position;
            knockbackDir = new Vector2(Mathf.Sign(knockbackDir.x)*0.4f, 1);
            if (rb.linearVelocityY > 0) knockbackDir.y = -1;

            rb.linearVelocity = Vector2.zero; // 기존 속도 초기화
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
