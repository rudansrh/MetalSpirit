using System.Collections;
using UnityEngine;

public class LegEnemy : MonoBehaviour
{
    [Header("Movement & AI Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Stomp Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;       // 발구르기를 시작할 거리
    [SerializeField] private float attackCooldown = 2.5f;    // 공격 쿨타임
    [SerializeField] private float attackDelay = 0.6f;       // 다리를 위로 올리고 있는 대기 시간
    [SerializeField] private float stompDamage = 15f;        // 밟기 데미지
    [SerializeField] private float stompWidth = 1.0f;        // 밟기 판정 너비
    [SerializeField] private Vector2 legOffset = new Vector2(0.5f, -0.5f); // 몸체 중심 기준 밟는 다리의 위치

    [Header("Detection (Raycast) Settings")]
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float pitCheckDistance = 2f;

    [Header("Damage Settings")]
    [SerializeField] private float bodyDamage = 10f;         // 접촉 데미지
    [SerializeField] private float knockbackForce = 7f;

    [Header("Enemy Hp")]
    [SerializeField] private float enemyHp = 30f;

    private Rigidbody2D rb;
    private Collider2D col;
    private PlayerController playerController;
    private PlayerAbilityManager playerAbility;
    private bool isGrounded;
    [SerializeField] private float facingDirection = 1f;

    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private Vector2 playerPos;

    // 빙의 관련 변수[cite: 24]
    public bool isPossessed = false;
    public GameObject nearbyEnemy;
    private bool found = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerController = PlayerController.Instance;
        playerAbility = playerController.GetComponent<PlayerAbilityManager>();
    }

    private void FixedUpdate()
    {
        if (playerController == null) return;

        if (isPossessed)
        {
            if (rb.linearVelocityX > 0.1f) facingDirection = 1f;
            else if (rb.linearVelocityX < -0.1f) facingDirection = -1f;

            if (playerController.isTalking) return;

            LayerMask enemyLayer = LayerMask.GetMask("Enemy");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right * facingDirection, 2f, enemyLayer);
            found = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    continue;
                }

                nearbyEnemy = hit.collider.gameObject;

                if (!found)
                {
                    playerController.canInteractUI.showInterectUI(hit.transform, "e", "대화");
                }
                found = true;
                break;
            }

            if (!found)
            {
                nearbyEnemy = null;
                if (!found) playerController.canInteractUI.hideInterectUI();
            }

            return;
        }

        if (playerAbility.isSoul)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        CheckGrounded();

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        playerPos = playerController.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos);

        if (distanceToPlayer <= attackRange && !playerController.isPossessing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(StompRoutine());
            }
        }
        else if (distanceToPlayer <= detectionRange && !playerController.isPossessing)
        {
            ChasePlayer(playerPos);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // 발구르기 공격 코루틴
    private IEnumerator StompRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Debug.Log("다리 올림! (발구르기 준비)");

        yield return new WaitForSeconds(attackDelay);

        Debug.Log("쾅! (내리찍기)");

        Vector2 stompPos = (Vector2)transform.position + new Vector2(legOffset.x * facingDirection, legOffset.y);

        Vector2 stompBoxSize = new Vector2(stompWidth, 1f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(stompPos, stompBoxSize, 0);

        bool hitPlayer = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(stompDamage, DamageType.Normal);
                hitPlayer = true;
                Debug.Log($"발구르기 적중! 데미지: {stompDamage}");
            }
        }

        if (!hitPlayer) Debug.Log("발구르기 빗나감!");

        yield return new WaitForSeconds(0.4f);

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1, 0, 0, 0.4f);
        Vector2 stompPos = (Vector2)transform.position + new Vector2(legOffset.x * facingDirection, legOffset.y);
        Vector2 stompBoxSize = new Vector2(stompWidth, 1f);
        Gizmos.DrawCube(stompPos, stompBoxSize);
    }

    private void ChasePlayer(Vector2 playerPos)
    {
        float dirX = playerPos.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.1f)
        {
            facingDirection = Mathf.Sign(dirX);
        }

        if ((facingDirection > 0 && transform.localScale.x < 0) ||
            (facingDirection < 0 && transform.localScale.x > 0))
        {
            Flip();
        }

        rb.linearVelocity = new Vector2(facingDirection * speed, rb.linearVelocity.y);
        CheckJumpObstacle();
    }

    private void CheckJumpObstacle()
    {
        if (!isGrounded) return;

        float startOffsetX = (col.bounds.extents.x - 0.05f) * facingDirection;
        Vector2 rayOrigin = new Vector2(transform.position.x + startOffsetX, transform.position.y);

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
        if (collision.gameObject.TryGetComponent<PlayerController>(out var pc) && pc.isInvincibility) return;
        if (collision.gameObject.TryGetComponent<PlayerAbilityManager>(out var pa) && pa.isSoul) return;

        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(bodyDamage, DamageType.Normal);
        }

        if (collision.gameObject.TryGetComponent<Rigidbody2D>(out var rb2d))
        {
            Vector2 knockbackDir = collision.transform.position - transform.position;
            knockbackDir = new Vector2(Mathf.Sign(knockbackDir.x) * 0.4f, 1f).normalized;

            rb2d.linearVelocity = Vector2.zero;
            rb2d.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isPossessed && playerController.isJump && collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.1f)
                {
                    playerController.isJump = false;
                    return;
                }
            }
        }
    }

    public void Attacked(float playerDamage)
    {
        enemyHp -= playerDamage;
        if (enemyHp <= 0)
        {
            Debug.Log("Stomp Enemy killed");
            if (TryGetComponent<DropItem>(out var drop)) drop.dropItem();
            this.gameObject.SetActive(false);
        }
    }
}