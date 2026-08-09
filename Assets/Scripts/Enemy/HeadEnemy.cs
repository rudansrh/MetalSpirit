using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HeadEnemy : MonoBehaviour
{
    [Header("Flight & AI Settings")]
    [SerializeField] private float flySpeed = 2.5f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float hoverDistance = 4f; // 플레이어와 유지하려는 최소 거리

    [Header("Laser Attack Settings")]
    [SerializeField] private float laserRange = 5f;       // 공격 범위
    [SerializeField] private float laserDamage = 50f;     // 데미지
    [SerializeField] private float laserThickness = 1f;   // 레이저 두께
    [SerializeField] private float attackCooldown = 3f;   // 공격 쿨타임
    [SerializeField] private float attackDelay = 0.8f;    // 발사 전 경고 시간
    [SerializeField] private Vector2 headOffset = new Vector2(0f, 0.5f); // 머리 위치 오프셋

    [Header("Enemy Hp")]
    [SerializeField] private float enemyHp = 30f;

    private Rigidbody2D rb;
    private Collider2D col;
    private LineRenderer lineRenderer;
    private PlayerController playerController;
    private PlayerAbilityManager playerAbility;

    [SerializeField] private float facingDirection = 1f;

    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    public bool isPossessed = false;
    public GameObject nearbyEnemy;
    private bool found = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        lineRenderer = GetComponent<LineRenderer>();

        playerController = PlayerController.Instance;
        playerAbility = playerController.GetComponent<PlayerAbilityManager>();

        rb.gravityScale = 0f;

        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
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
                if (hit.collider.gameObject == gameObject) continue;

                nearbyEnemy = hit.collider.gameObject;
                if (!found) playerController.canInteractUI.showInterectUI(hit.transform, "e", "대화");
                found = true;
                break;
            }

            if (!found)
            {
                nearbyEnemy = null;
                playerController.canInteractUI.hideInterectUI();
            }
            return;
        }

        if (playerAbility.isSoul)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 playerPos = playerController.transform.position;
        float distanceToPlayer = Vector2.Distance(transform.position, playerPos);

        if (distanceToPlayer <= laserRange && !playerController.isPossessing)
        {
            rb.linearVelocity = Vector2.zero;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(LaserRoutine());
            }
        }
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > hoverDistance && !playerController.isPossessing)
        {
            FlyTowardsPlayer(playerPos);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FlyTowardsPlayer(Vector2 playerPos)
    {
        Vector2 direction = (playerPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * flySpeed;

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            facingDirection = Mathf.Sign(direction.x);
            if ((facingDirection > 0 && transform.localScale.x < 0) || (facingDirection < 0 && transform.localScale.x > 0))
            {
                Flip();
            }
        }
    }

    // 레이저 발사 코루틴
    private IEnumerator LaserRoutine()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        Vector2 firePoint = (Vector2)transform.position + new Vector2(headOffset.x * facingDirection, headOffset.y);
        Vector2 playerCenter = playerController.transform.position;

        Vector2 aimDirection = (playerCenter - firePoint).normalized;

        lineRenderer.enabled = true;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;

        lineRenderer.SetPosition(0, firePoint);
        lineRenderer.SetPosition(1, firePoint + aimDirection * laserRange);

        Debug.Log("레이저 조준 중...");

        yield return new WaitForSeconds(attackDelay);


        lineRenderer.startWidth = laserThickness;
        lineRenderer.endWidth = laserThickness;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        lineRenderer.SetPosition(0, firePoint);
        lineRenderer.SetPosition(1, firePoint + aimDirection * laserRange);

        float radius = laserThickness / 2f;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(firePoint, radius, aimDirection, laserRange);

        bool hitPlayer = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(laserDamage, DamageType.Normal);
                    hitPlayer = true;
                    Debug.Log("레이저 적중! 데미지: 50");
                }
            }
        }

        if (!hitPlayer) Debug.Log("레이저 빗나감");

        yield return new WaitForSeconds(0.3f);

        lineRenderer.enabled = false;
        isAttacking = false;
    }
    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, laserRange);

        Gizmos.color = Color.cyan;
        Vector2 firePoint = (Vector2)transform.position + new Vector2(headOffset.x * facingDirection, headOffset.y);
        Gizmos.DrawSphere(firePoint, 0.2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var pc) && pc.isInvincibility) return;
        if (collision.gameObject.TryGetComponent<PlayerAbilityManager>(out var pa) && pa.isSoul) return;

        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(15f, DamageType.Normal);
        }
    }

    public void Attacked(float playerDamage)
    {
        enemyHp -= playerDamage;
        if (enemyHp <= 0)
        {
            Debug.Log("Flying Laser Enemy killed");
            if (TryGetComponent<DropItem>(out var drop)) drop.dropItem();
            this.gameObject.SetActive(false);
        }
    }
}