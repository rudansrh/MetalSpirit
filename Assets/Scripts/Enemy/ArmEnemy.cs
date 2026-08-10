using System.Collections;
using UnityEngine;

public class ArmEnemy : Enemy
{
    [Header("Movement & AI Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float detectionRange = 10f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDelay = 0.3f;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1f);

    [Header("Detection (Raycast) Settings")]
    [SerializeField] private float wallCheckDistance = 1f;
    [SerializeField] private float pitCheckDistance = 2f;

    [Header("Damage Settings")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float knockbackForce = 7f;

    [Header("Enemy Hp")]
    [SerializeField] private float enemyHp = 30f;

    private Rigidbody2D rb;
    private Collider2D col;
    private PlayerController playerController;
    private PlayerAbilityManager playerAbility;
    private bool isGrounded;

    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private Vector2 playerPos;

    private bool found = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        playerController = PlayerController.Instance;
        playerAbility = playerController.GetComponent<PlayerAbilityManager>();
        InitializeEnemyBase();
    }

    private void FixedUpdate()
    {
        if (playerController == null) return;

        if (isDying)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateMoveAnimation(false);
            return;
        }

        if (isPossessed)
        {
            if (rb.linearVelocityX > 0.1f) facingDirection = 1f;
            else if(rb.linearVelocityX < -0.1f)facingDirection = -1f;
            UpdateFacingVisual();

            if (playerController.isTalking)
            {
                UpdateMoveAnimation(false);
                return;
            }

            LayerMask enemyLayer = LayerMask.GetMask("Enemy");
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right * facingDirection, 2f, enemyLayer);
            found = false;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == gameObject) //자기자신(빙의된 에너미) 제외
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

            UpdateMoveAnimation(Mathf.Abs(rb.linearVelocityX) > 0.05f);
            return;
        }

        if (playerAbility.isSoul)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateMoveAnimation(false);
            return;
        }

        CheckGrounded();

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateMoveAnimation(false);
            return;
        }

        playerPos = playerController.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos);

        if (distanceToPlayer <= attackRange && !playerController.isPossessing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateMoveAnimation(false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackRoutine());
            }
        }
        else if (distanceToPlayer <= detectionRange && !playerController.isPossessing)
        {
            ChasePlayer(playerPos);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateMoveAnimation(false);
        }
    }

    // 공격 코루틴
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        animationController?.TriggerAttack();

        Debug.Log("적 공격 준비!");

        yield return new WaitForSeconds(attackDelay);

        Vector2 pos = (Vector2)transform.position + new Vector2(facingDirection * (attackBoxSize.x / 2f), 0);
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, attackBoxSize, 0);

        bool hitPlayer = false;
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, DamageType.Normal);
                hitPlayer = true;
                Debug.Log("적 공격 적중");
            }
        }

        if (!hitPlayer) Debug.Log("적 공격 빗나감");

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Vector2 pos = (Vector2)transform.position + new Vector2(facingDirection * (attackBoxSize.x / 2f), 0);
        Gizmos.DrawCube(pos, attackBoxSize); 
    }

    private void ChasePlayer(Vector2 playerPos)
    {
        float dirX = playerPos.x - transform.position.x;

        if (Mathf.Abs(dirX) > 0.1f)
        {
            facingDirection = Mathf.Sign(dirX);
            UpdateFacingVisual();
        }

        rb.linearVelocity = new Vector2(facingDirection * speed, rb.linearVelocity.y);
        CheckJumpObstacle();
        UpdateMoveAnimation(Mathf.Abs(rb.linearVelocityX) > 0.05f);
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

    private void UpdateMoveAnimation(bool isMoving)
    {
        animationController?.SetMove(isMoving && !isAttacking && !isDying);
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        //빙의시 점프 판정 처리
        if (isPossessed && playerController.isJump && collision.gameObject.tag == "Wall")
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

    public override void Attacked(float playerDamage)
    {
        if (isDying)
        {
            return;
        }

        enemyHp -= playerDamage;

        if (enemyHp <= 0)
        {
            Debug.Log("Enemy killed");
            StartDeath();
            return;
        }

        animationController?.TriggerHit();
    }

    private void StartDeath()
    {
        if (isDying)
        {
            return;
        }

        isDying = true;
        isAttacking = false;
        StopAllCoroutines();
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        rb.linearVelocity = Vector2.zero;
        UpdateMoveAnimation(false);

        if (col != null)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.simulated = false;
        }

        animationController?.TriggerDeath();

        float deathDelay = animationController != null ? animationController.DeathDisableDelay : 0f;
        if (deathDelay > 0f)
        {
            yield return new WaitForSeconds(deathDelay);
        }

        if (TryGetComponent<DropItem>(out var drop))
        {
            drop.dropItem();
        }

        gameObject.SetActive(false);
    }
}
