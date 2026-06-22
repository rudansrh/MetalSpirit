using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;           // 이동 속도
    [SerializeField] private Transform leftWaypoint;     // 좌측 이동 한계점
    [SerializeField] private Transform rightWaypoint;    // 우측 이동 한계점

    private bool movingRight = true;

    [Header("Damage Settings")]
    [SerializeField] private float damage = 15f;         // 적 접촉 시 데미지
    [SerializeField] private float knockbackForce = 7f;  // 넉백 힘

    private void Update()
    {
        MovePatrol();
    }

    // 좌우 순찰 로직
    private void MovePatrol()
    {
        // 웨이포인트가 할당되지 않았다면 이동하지 않음
        if (leftWaypoint == null || rightWaypoint == null) return;

        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= rightWaypoint.position.x)
            {
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= leftWaypoint.position.x)
            {
                Flip();
            }
        }
    }

    // 방향 전환 및 스프라이트 반전
    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // 플레이어와 충돌 시 데미지 및 넉백 처리 (SpikeObstacle 참고)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 데미지 적용
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Normal);
        }

        // 2. 물리 넉백 적용
        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 knockbackDir = collision.transform.position - transform.position;

            // X축 방향 결정 및 Y축을 띄워주는 효과
            knockbackDir = new Vector2(Mathf.Sign(knockbackDir.x) * 0.4f, 1f).normalized;

            // 하강 중일 때 넉백 방향 보정
            if (rb.linearVelocityY > 0) knockbackDir.y = -1;

            rb.linearVelocity = Vector2.zero; // 기존 속도 초기화
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}
