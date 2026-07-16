using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossAttackDefinition
{
    public BossAttackType attackType;
    public string displayName;
    public Transform attackOrigin;
    public Vector2 hitboxSize = new Vector2(3f, 2f);
    public float damage = 20f;
    public float activeDuration = 0.2f;
    public GameObject telegraphIndicator;
    public GameObject attackIndicator;

    [Header("Charge Only")]
    public Transform chargeStartPoint;
    public Transform chargeEndPoint;
    public float chargeDuration = 0.75f;
}

public class BossAttackController : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] float telegraphDuration = 3f;
    [SerializeField] float recoveryDuration = 1f;
    [SerializeField] float idleDelayBetweenPatterns = 0.75f;

    [Header("Phase 1 Attacks")]
    [SerializeField] BossAttackDefinition leftPunchAttack = new BossAttackDefinition
    {
        attackType = BossAttackType.LeftPunch,
        displayName = "Left Punch"
    };
    [SerializeField] BossAttackDefinition rightPunchAttack = new BossAttackDefinition
    {
        attackType = BossAttackType.RightPunch,
        displayName = "Right Punch"
    };

    [Header("Phase 2 Attacks")]
    [SerializeField] BossAttackDefinition chargeAttack = new BossAttackDefinition
    {
        attackType = BossAttackType.Charge,
        displayName = "Charge"
    };

    BossController bossController;
    Coroutine attackLoopCoroutine;

    public void Begin(BossController controller)
    {
        bossController = controller;
        StopAttacks();
        attackLoopCoroutine = StartCoroutine(AttackLoopRoutine());
    }

    public void StopAttacks()
    {
        if (attackLoopCoroutine != null)
        {
            StopCoroutine(attackLoopCoroutine);
            attackLoopCoroutine = null;
        }

        SetIndicators(leftPunchAttack, false, false);
        SetIndicators(rightPunchAttack, false, false);
        SetIndicators(chargeAttack, false, false);
    }

    IEnumerator AttackLoopRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (bossController != null && bossController.IsBattleActive && !bossController.IsDefeated)
        {
            BossAttackDefinition nextAttack = SelectNextAttack();
            if (nextAttack == null)
            {
                yield return null;
                continue;
            }

            yield return ExecuteAttackRoutine(nextAttack);
            yield return new WaitForSeconds(idleDelayBetweenPatterns);
        }
    }

    BossAttackDefinition SelectNextAttack()
    {
        if (bossController == null)
        {
            return null;
        }

        if (bossController.CurrentPhase == BossPhase.Phase1)
        {
            return Random.value < 0.5f ? leftPunchAttack : rightPunchAttack;
        }

        int attackIndex = Random.Range(0, 3);
        switch (attackIndex)
        {
            case 0:
                return leftPunchAttack;
            case 1:
                return rightPunchAttack;
            default:
                return chargeAttack;
        }
    }

    IEnumerator ExecuteAttackRoutine(BossAttackDefinition attack)
    {
        bossController.SetState(BossState.Telegraph);
        SetIndicators(attack, true, false);
        Debug.Log($"Boss telegraph: {attack.displayName}");
        yield return new WaitForSeconds(telegraphDuration);

        bossController.SetState(BossState.Attack);
        SetIndicators(attack, false, true);

        if (attack.attackType == BossAttackType.Charge)
        {
            yield return ExecuteChargeRoutine(attack);
        }
        else
        {
            ResolveAreaAttack(attack);
            yield return new WaitForSeconds(attack.activeDuration);
        }

        SetIndicators(attack, false, false);

        bossController.SetState(BossState.Recovery);
        yield return new WaitForSeconds(recoveryDuration);
        bossController.SetState(BossState.Idle);
    }

    IEnumerator ExecuteChargeRoutine(BossAttackDefinition attack)
    {
        Transform startPoint = attack.chargeStartPoint != null ? attack.chargeStartPoint : transform;
        Transform endPoint = attack.chargeEndPoint != null ? attack.chargeEndPoint : transform;
        HashSet<Collider2D> damagedTargets = new HashSet<Collider2D>();

        if (attack.chargeStartPoint != null)
        {
            transform.position = attack.chargeStartPoint.position;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, attack.chargeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            ResolveAreaAttack(attack, damagedTargets);
            yield return null;
        }
    }

    void ResolveAreaAttack(BossAttackDefinition attack, HashSet<Collider2D> alreadyDamaged = null)
    {
        Vector2 center = attack.attackOrigin != null ? attack.attackOrigin.position : transform.position;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attack.hitboxSize, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.CompareTag("Player"))
            {
                continue;
            }

            if (alreadyDamaged != null && alreadyDamaged.Contains(hit))
            {
                continue;
            }

            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attack.damage, DamageType.Normal);
                alreadyDamaged?.Add(hit);
            }
        }
    }

    void SetIndicators(BossAttackDefinition attack, bool telegraphActive, bool attackActive)
    {
        if (attack == null)
        {
            return;
        }

        if (attack.telegraphIndicator != null)
        {
            attack.telegraphIndicator.SetActive(telegraphActive);
        }

        if (attack.attackIndicator != null)
        {
            attack.attackIndicator.SetActive(attackActive);
        }
    }

    void OnDrawGizmosSelected()
    {
        DrawAttackGizmo(leftPunchAttack, Color.red);
        DrawAttackGizmo(rightPunchAttack, Color.blue);
        DrawAttackGizmo(chargeAttack, Color.yellow);
    }

    void DrawAttackGizmo(BossAttackDefinition attack, Color color)
    {
        if (attack == null)
        {
            return;
        }

        Vector3 center = attack.attackOrigin != null ? attack.attackOrigin.position : transform.position;
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, attack.hitboxSize);
    }
}
