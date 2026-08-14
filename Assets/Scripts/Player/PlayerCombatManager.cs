using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerCombatManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private LineRenderer headAttackLineRenderer;

    [Header("Attack Settings")]
    public bool isAttackVisualize = false;
    [SerializeField] private float legAttackCoolTime = 0.1f;
    [SerializeField] private float legAttackDamage = 30f;
    [SerializeField] private Vector2 legAttackOffset = new Vector2(1f, 0.2f);
    [SerializeField] private Vector2 legAttackSize = new Vector2(1f, 0.1f);
    [SerializeField] private float armAttackCoolTime = 0.1f;
    [SerializeField] private float armAttackDamage = 50f;
    [SerializeField] private Vector2 armAttackOffset = new Vector2(1f, -0.2f);
    [SerializeField] private Vector2 armAttackSize = new Vector2(1f, 0.1f);
    [SerializeField] private float bodyAttackCoolTime = 0.1f;
    [SerializeField] private float bodyAttackDamage = 70f;
    [SerializeField] private float bodyAttackRange = 2f;
    [SerializeField] private float bodyAttackDuration = 0.15f;
    [SerializeField] private float headAttackCoolTime = 0.1f;
    [SerializeField] private float headAttackDamage = 100f;
    [SerializeField] private float headAttackRange = 5f;
    [SerializeField] private float headAttackThickness = 0.15f;
    [SerializeField] private float headAttackVisualDuration = 0.15f;
    [SerializeField] private Vector2 headAttackOffset = new Vector2(0.4f, 0f);

    [Header("Attack Visualization Settings")]
    [SerializeField] private float attackVisualDuration = 0.2f;
    [SerializeField] private float attackVisualLineWidth = 0.04f;
    [SerializeField] private Color attackVisualColor = Color.yellow;

    private bool isAttacking;
    private float curTime_leg;
    private float curTime_arm;
    private float curTime_body;
    private float curTime_head;
    private Coroutine attackVisualCoroutine;
    private LineRenderer attackVisualLineRenderer;

    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (abilityManager == null)
        {
            abilityManager = GetComponent<PlayerAbilityManager>();
        }

        if (headAttackLineRenderer == null)
        {
            headAttackLineRenderer = GetComponent<LineRenderer>();
        }

        if (headAttackLineRenderer != null)
        {
            headAttackLineRenderer.enabled = false;
            headAttackLineRenderer.positionCount = 2;
        }

        if (bodyAttackRange <= 0f) bodyAttackRange = 2f;
        if (bodyAttackDuration <= 0f) bodyAttackDuration = 0.15f;
        if (headAttackRange <= 0f) headAttackRange = 5f;
        if (headAttackThickness <= 0f) headAttackThickness = 0.15f;
        if (headAttackVisualDuration <= 0f) headAttackVisualDuration = 0.15f;
        if (attackVisualDuration <= 0f) attackVisualDuration = 0.2f;
        if (attackVisualLineWidth <= 0f) attackVisualLineWidth = 0.04f;
    }

    private void Update()
    {
        curTime_leg += Time.deltaTime;
        curTime_arm += Time.deltaTime;
        curTime_body += Time.deltaTime;
        curTime_head += Time.deltaTime;
    }

    private void OnDisable()
    {
        isAttacking = false;
        HideAttackVisual();

        if (headAttackLineRenderer != null)
        {
            headAttackLineRenderer.enabled = false;
        }
    }

    public void OnLegAttack(InputValue value)
    {
        if (!value.isPressed || !CanStartCommonAttack()) return;
        if (abilityManager == null || !abilityManager.canLegAttack || legAttackCoolTime > curTime_leg) return;

        curTime_leg = 0f;

        Vector2 center = GetAttackCenter(legAttackOffset);
        ApplyDamageInBox(center, legAttackSize, legAttackDamage);
        ShowAttackBoxVisual(center, legAttackSize);
    }

    public void OnArmAttack(InputValue value)
    {
        if (!value.isPressed || !CanStartCommonAttack()) return;
        if (abilityManager == null || !abilityManager.canArmAttack || armAttackCoolTime > curTime_arm) return;

        curTime_arm = 0f;

        Vector2 center = GetAttackCenter(armAttackOffset);
        ApplyDamageInBox(center, armAttackSize, armAttackDamage);
        ShowAttackBoxVisual(center, armAttackSize);
    }

    public void OnBodyAttack(InputValue value)
    {
        if (!value.isPressed || !CanStartCommonAttack()) return;
        if (abilityManager == null || !abilityManager.canBodyAttack || bodyAttackCoolTime > curTime_body) return;
        if (playerController.IsDashing || !playerController.canMove) return;

        curTime_body = 0f;
        StartCoroutine(BodyAttackRoutine());
    }

    public void OnHeadAttack(InputValue value)
    {
        if (!value.isPressed || !CanStartCommonAttack()) return;
        if (abilityManager == null || !abilityManager.canHeadAttack || headAttackCoolTime > curTime_head) return;
        if (playerController.IsDashing || !playerController.canMove) return;

        curTime_head = 0f;
        StartCoroutine(HeadAttackRoutine());
    }

    private bool CanStartCommonAttack()
    {
        return playerController != null
            && !playerController.IsUiOpen
            && !playerController.IsPossessing
            && !isAttacking;
    }

    private IEnumerator BodyAttackRoutine()
    {
        isAttacking = true;

        Vector2 direction = new Vector2(playerController.FacingDirection, 0f);
        ApplyDamageAlongBoxCast(direction, bodyAttackRange, bodyAttackDamage);

        float bodyAttackSpeed = bodyAttackRange / Mathf.Max(bodyAttackDuration, 0.01f);
        float elapsed = 0f;

        while (elapsed < bodyAttackDuration)
        {
            Rigidbody2D rb = playerController.CurrentRigidbody;
            if (rb == null)
            {
                break;
            }

            rb.linearVelocity = playerController.FilterVelocityForBounds(new Vector2(
                direction.x * bodyAttackSpeed,
                rb.linearVelocity.y));
            elapsed += Time.deltaTime;
            yield return null;
        }

        Rigidbody2D endRb = playerController.CurrentRigidbody;
        if (endRb != null)
        {
            endRb.linearVelocity = new Vector2(0f, endRb.linearVelocity.y);
        }

        isAttacking = false;
    }

    private IEnumerator HeadAttackRoutine()
    {
        isAttacking = true;

        Rigidbody2D rb = playerController.CurrentRigidbody;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Vector2 start = (Vector2)transform.position + new Vector2(headAttackOffset.x * playerController.FacingDirection, headAttackOffset.y);
        Vector2 direction = new Vector2(playerController.FacingDirection, 0f);
        Vector2 end = start + direction * headAttackRange;

        if (headAttackLineRenderer != null)
        {
            headAttackLineRenderer.enabled = true;
            headAttackLineRenderer.positionCount = 2;
            headAttackLineRenderer.startWidth = headAttackThickness;
            headAttackLineRenderer.endWidth = headAttackThickness;
            headAttackLineRenderer.SetPosition(0, start);
            headAttackLineRenderer.SetPosition(1, end);
        }

        ApplyDamageAlongLaser(start, direction, headAttackRange, headAttackThickness, headAttackDamage);

        yield return new WaitForSeconds(headAttackVisualDuration);

        if (headAttackLineRenderer != null)
        {
            headAttackLineRenderer.enabled = false;
        }

        isAttacking = false;
    }

    private Vector2 GetAttackCenter(Vector2 baseOffset)
    {
        float x = baseOffset.x * playerController.FacingDirection;
        return (Vector2)transform.position + new Vector2(x, baseOffset.y);
    }

    private void ApplyDamageInBox(Vector2 center, Vector2 size, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);
        ApplyDamageToColliders(hits, damage);
    }

    private void ApplyDamageAlongBoxCast(Vector2 direction, float distance, float damage)
    {
        Collider2D currentCollider = playerController.CurrentCollider;
        if (currentCollider == null)
        {
            return;
        }

        Vector2 hitboxSize = new Vector2(
            Mathf.Max(currentCollider.bounds.size.x, 0.5f),
            Mathf.Max(currentCollider.bounds.size.y, 0.5f));
        RaycastHit2D[] hits = Physics2D.BoxCastAll(currentCollider.bounds.center, hitboxSize, 0f, direction, distance);
        ApplyDamageToHits(hits, damage);
    }

    private void ApplyDamageAlongLaser(Vector2 start, Vector2 direction, float distance, float thickness, float damage)
    {
        float radius = Mathf.Max(thickness * 0.5f, 0.01f);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(start, radius, direction, distance);
        ApplyDamageToHits(hits, damage);
    }

    private void ApplyDamageToHits(RaycastHit2D[] hits, float damage)
    {
        HashSet<IEnemyDamageReceiver> damagedEnemies = new HashSet<IEnemyDamageReceiver>();

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || !hit.collider.CompareTag("Enemy"))
            {
                continue;
            }

            IEnemyDamageReceiver enemy = hit.collider.GetComponent<IEnemyDamageReceiver>();
            if (enemy == null || !damagedEnemies.Add(enemy))
            {
                continue;
            }

            enemy.Attacked(damage);
        }
    }

    private void ApplyDamageToColliders(Collider2D[] hits, float damage)
    {
        HashSet<IEnemyDamageReceiver> damagedEnemies = new HashSet<IEnemyDamageReceiver>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.CompareTag("Enemy"))
            {
                continue;
            }

            IEnemyDamageReceiver enemy = hit.GetComponent<IEnemyDamageReceiver>();
            if (enemy == null || !damagedEnemies.Add(enemy))
            {
                continue;
            }

            enemy.Attacked(damage);
        }
    }

    private void ShowAttackBoxVisual(Vector2 center, Vector2 size)
    {
        if (!isAttackVisualize)
        {
            HideAttackVisual();
            return;
        }

        EnsureAttackVisualLineRenderer();
        if (attackVisualLineRenderer == null)
        {
            return;
        }

        Vector2 halfSize = size * 0.5f;
        Vector3[] positions =
        {
            new Vector3(center.x - halfSize.x, center.y - halfSize.y, 0f),
            new Vector3(center.x - halfSize.x, center.y + halfSize.y, 0f),
            new Vector3(center.x + halfSize.x, center.y + halfSize.y, 0f),
            new Vector3(center.x + halfSize.x, center.y - halfSize.y, 0f),
            new Vector3(center.x - halfSize.x, center.y - halfSize.y, 0f)
        };

        attackVisualLineRenderer.enabled = true;
        attackVisualLineRenderer.positionCount = positions.Length;
        attackVisualLineRenderer.startWidth = attackVisualLineWidth;
        attackVisualLineRenderer.endWidth = attackVisualLineWidth;
        attackVisualLineRenderer.startColor = attackVisualColor;
        attackVisualLineRenderer.endColor = attackVisualColor;
        attackVisualLineRenderer.SetPositions(positions);

        if (attackVisualCoroutine != null)
        {
            StopCoroutine(attackVisualCoroutine);
        }

        attackVisualCoroutine = StartCoroutine(HideAttackVisualRoutine());
    }

    private IEnumerator HideAttackVisualRoutine()
    {
        yield return new WaitForSeconds(attackVisualDuration);
        HideAttackVisual();
    }

    private void HideAttackVisual()
    {
        if (attackVisualCoroutine != null)
        {
            StopCoroutine(attackVisualCoroutine);
            attackVisualCoroutine = null;
        }

        if (attackVisualLineRenderer != null)
        {
            attackVisualLineRenderer.enabled = false;
        }
    }

    private void EnsureAttackVisualLineRenderer()
    {
        if (attackVisualLineRenderer != null)
        {
            return;
        }

        GameObject visualObject = new GameObject("AttackVisualLine");
        visualObject.transform.SetParent(transform, false);

        attackVisualLineRenderer = visualObject.AddComponent<LineRenderer>();
        attackVisualLineRenderer.enabled = false;
        attackVisualLineRenderer.useWorldSpace = true;
        attackVisualLineRenderer.loop = false;
        attackVisualLineRenderer.positionCount = 5;
        attackVisualLineRenderer.numCapVertices = 0;
        attackVisualLineRenderer.numCornerVertices = 0;
        attackVisualLineRenderer.sortingOrder = 10;

        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            attackVisualLineRenderer.material = new Material(spriteShader);
        }
    }
}
