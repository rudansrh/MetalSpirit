using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    static readonly int IsMoveHash = Animator.StringToHash("isMove");
    static readonly int DoAttackHash = Animator.StringToHash("doAttack");
    static readonly int DoHitHash = Animator.StringToHash("doHit");
    static readonly int DoStunHash = Animator.StringToHash("doStun");
    static readonly int DoDeathHash = Animator.StringToHash("doDeath");

    [SerializeField] private Animator animator;
    [SerializeField] private float deathDisableDelay = 1f;

    private bool isDead;
    public float DeathDisableDelay => deathDisableDelay;
    public bool IsDead => isDead;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        isDead = false;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        ResetAllTriggers();
        animator.SetBool(IsMoveHash, false);
    }

    public void SetMove(bool isMoving)
    {
        if (animator == null)
        {
            return;
        }

        animator.SetBool(IsMoveHash, !isDead && isMoving);
    }

    public void TriggerAttack()
    {
        if (animator == null || isDead)
        {
            return;
        }

        animator.SetBool(IsMoveHash, false);
        animator.ResetTrigger(DoHitHash);
        animator.ResetTrigger(DoStunHash);
        animator.SetTrigger(DoAttackHash);
    }

    public void TriggerHit()
    {
        if (animator == null || isDead)
        {
            return;
        }

        animator.SetBool(IsMoveHash, false);
        animator.ResetTrigger(DoAttackHash);
        animator.ResetTrigger(DoStunHash);
        animator.SetTrigger(DoHitHash);
    }

    public void TriggerStun()
    {
        if (animator == null || isDead)
        {
            return;
        }

        animator.SetBool(IsMoveHash, false);
        animator.ResetTrigger(DoAttackHash);
        animator.ResetTrigger(DoHitHash);
        animator.SetTrigger(DoStunHash);
    }

    public void TriggerDeath()
    {
        if (animator == null || isDead)
        {
            return;
        }

        isDead = true;

        ResetAllTriggers();
        animator.SetBool(IsMoveHash, false);
        animator.SetTrigger(DoDeathHash);
    }

    private void ResetAllTriggers()
    {
        if (animator == null)
        {
            return;
        }

        animator.ResetTrigger(DoAttackHash);
        animator.ResetTrigger(DoHitHash);
        animator.ResetTrigger(DoStunHash);
        animator.ResetTrigger(DoDeathHash);
    }
}
