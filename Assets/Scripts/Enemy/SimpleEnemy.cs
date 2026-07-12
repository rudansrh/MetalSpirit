using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 3f;           // 추적 이동 속도
    [SerializeField] private float jumpForce = 12f;      // 점프력
    [SerializeField] private float detectionRange = 10f; // 플레이어 인식 거리

    [Header("Detection Settings")]
    [SerializeField] private float wallCheckDistance = 1f; // 앞의 벽을 감지할 거리
    [SerializeField] private float pitCheckDistance = 2f;  // 앞의 낭떠러지를 감지할 거리

    [Header("Damage Settings")]
    [SerializeField] private float damage = 15f;         // 충돌 시 데미지
    [SerializeField] private float knockbackForce = 7f;  // 넉백 힘

    [Header("Enemy Hp")]
    [SerializeField] private float enemyHp = 30f;

    private Rigidbody2D rb;
    private Collider2D col;
    private bool isGrounded;
    private float facingDirection = 1f; // 바라보는 방향 (1: 오른쪽, -1: 왼쪽)

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (PlayerController.Instance == null) return;

        // 영혼인지 체크
        if (PlayerController.Instance.TryGetComponent<PlayerAbilityManager>(out var playerAbility))
        {
            if (playerAbility.isSoul)
            {

                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                return;
            }
        }

        Vector2 playerPos = PlayerController.Instance.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos);

        CheckGrounded();

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(playerPos);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void ChasePlayer(Vector2 playerPos)
    {
        // 이동할 방향 계산
        float dirX = playerPos.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.1f)
        {
            facingDirection = Mathf.Sign(dirX);
        }

        // 스프라이트 방향 뒤집기
        if ((facingDirection > 0 && transform.localScale.x < 0) ||
            (facingDirection < 0 && transform.localScale.x > 0))
        {
            Flip();
        }

        // 플레이어 방향으로 이동
        rb.linearVelocity = new Vector2(facingDirection * speed, rb.linearVelocity.y);

        // 이동 중 점프해야 할 장애물이 있는지 체크
        CheckJumpObstacle();
    }

    private void CheckJumpObstacle()
    {
        if (!isGrounded) return;

        Vector2 rayOrigin = new Vector2(transform.position.x + (facingDirection * col.bounds.extents.x), transform.position.y);

        // 벽 감지
        bool hasWall = false;
        RaycastHit2D[] wallHits = Physics2D.RaycastAll(rayOrigin, Vector2.right * facingDirection, wallCheckDistance);
        foreach (var hit in wallHits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                hasWall = true;
                break;
            }
        }

        // 낭떠러지 감지
        bool hasPit = true;
        RaycastHit2D[] pitHits = Physics2D.RaycastAll(rayOrigin + new Vector2(facingDirection * 1f, 0), Vector2.down, pitCheckDistance);
        foreach (var hit in pitHits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                hasPit = false;
                break;
            }
        }

        Debug.DrawRay(rayOrigin, Vector2.right * facingDirection * wallCheckDistance, Color.red);
        Debug.DrawRay(rayOrigin + new Vector2(facingDirection * 1f, 0), Vector2.down * pitCheckDistance, Color.blue);

        if (hasWall || hasPit)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void CheckGrounded()
    {
        float extraHeight = 0.1f;
        RaycastHit2D[] hits = Physics2D.RaycastAll(col.bounds.center, Vector2.down, col.bounds.extents.y + extraHeight);

        isGrounded = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 영혼 상태, 무적 상태일 때 충돌 무시
        if (collision.gameObject.TryGetComponent<PlayerController>(out var playerController))
        {
            if (playerController.isInvincibility) return;
        }

        if (collision.gameObject.TryGetComponent<PlayerAbilityManager>(out var playerAbility))
        {
            if (playerAbility.isSoul) return;
        }

        // 1. 데미지 적용
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, DamageType.Normal);
        }

        // 2. 피격 넉백 적용
        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 knockbackDir = collision.transform.position - transform.position;
            knockbackDir = new Vector2(Mathf.Sign(knockbackDir.x) * 0.4f, 1f).normalized;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void Attacked(float playerDamage)
    {
        enemyHp -= playerDamage;
        if (enemyHp <= 0)
        {
            this.gameObject.SetActive(false);
            Debug.Log("Enemy killed");
        }
    }
}