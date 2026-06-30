using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid;
    Collider2D col;
    [SerializeField] PlayerAbilityManager abilityManager;

    //스태미나 컴포넌트 참조 추가
    Stamina stamina;

    [Header("Movement Settings")]
    [SerializeField] float speed;
    [SerializeField] float soulSpeed = 10f;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpDelay;
    [SerializeField] float wallClimbSpeed = 0.25f;
    [SerializeField] float wallClimbHoldStaminaCostPerSecond = 2f;
    [SerializeField] float wallClimbMoveStaminaCostPerSecond = 3f;
    [SerializeField] float wallClimbFallOffDistance = 0.1f;
    [SerializeField] float wallClimbFallSpeed = 0.25f;
    [SerializeField] float jumpStaminaCost = 10f;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 20f;     // 돌진 속도
    [SerializeField] float dashDuration = 0.2f; // 돌진 유지 시간
    [SerializeField] float dashCooldown = 1f;   // 돌진 쿨타임
    //대시할 때 스태미나 소모량
    [SerializeField] float dashStaminaCost = 20f;


    [Header("Slow Effect Settings")]
    [SerializeField] float speedMultiplier = 1f; // 느려지는 효과 수치
    [SerializeField] int slowEffectCount = 0;    // 느려지는 효과 중첩 카운트
    [SerializeField] float cobwebMaxRiseSpeed = 2.5f;
    [SerializeField] float cobwebMaxFallSpeed = 0.1f;

    [Header("Attack Settings")]
    bool isAttacking = false;
    float curTime_low = 0f;
    [SerializeField] float lowAttackCoolTime = 0.6f;
    [SerializeField] float lowAttackDamage = 10f;
    float curTime_high = 0f;
    [SerializeField] float highAttackCoolTime = 0.6f;
    [SerializeField] float highAttackDamage = 10f;



    bool isDashing = false;
    bool canDashAgain = true;
    Coroutine DashCoroutine;
    float facingDirection = 1f; // 바라보는 방향 (기본값: 오른쪽 1)
    float originalGravity = 1f;

    bool isJump = false;
    bool canWallJumpAgain = true;
    int TouchingWallCnt = 0;
    bool isWallClimbing = false;
    bool wallClimbLockedUntilExit = false;
    float wallClimbDetachDirection = 0f;

    Vector2 moveInput;

    public bool canMove = true;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        originalGravity = rigid.gravityScale;

        stamina = GetComponent<Stamina>();

        col = GetComponent<Collider2D>();
        UpdateFormState();
    }

    void UpdateFormState()
    {
        if (abilityManager.isSoul)
        {
            rigid.gravityScale = 0f;
            col.isTrigger = true;
        }
        else
        {
            rigid.gravityScale = originalGravity;
            col.isTrigger = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curTime_high += Time.deltaTime;
        curTime_low += Time.deltaTime;

        // 대시 중일 땐 이동과 중력 무시
        if (isDashing || !canMove)
            return;

        // 영혼 상태일 때의 이동
        if (abilityManager.isSoul)
        {
            Vector2 soulMoveDir = moveInput;
            if (soulMoveDir.magnitude > 1f)
            {
                soulMoveDir.Normalize();
            }

            rigid.linearVelocity = soulMoveDir * soulSpeed;
            return;
        }

        // 빙의 상태일 때
        if (rigid.linearVelocityY == 0)
        {
            isJump = false;
        }

        if (UpdateWallClimbState())
        {
            return;
        }

        ApplyCobwebVerticalLimit();
        rigid.linearVelocityX = moveInput.x * speed * speedMultiplier;
    }

    //적 공격 (발차기)
    public void OnLowAttack(InputValue value)
    {
        if (!abilityManager.canLowAttack || lowAttackCoolTime > curTime_low) return;

        curTime_low = 0f;
        Vector2 pos = transform.position + transform.up*transform.localScale.y*0.2f + transform.right*facingDirection;
        Vector2 size = new Vector2(1.0f, 0.1f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, 0);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("발차기 공격");
                hit.GetComponent<SimpleEnemy>().Attacked(lowAttackDamage);
            }
        }
    }

    //적 공격 (주먹)
    public void OnHighAttack(InputValue value) 
    {
        if (!abilityManager.canHighAttack || highAttackCoolTime > curTime_high) return;

        curTime_high = 0f;
        Vector2 pos = transform.position + transform.up * transform.localScale.y * -0.2f + transform.right * facingDirection;
        Vector2 size = new Vector2(1.0f, 0.1f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, size, 0);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("주먹 공격");
                hit.GetComponent<SimpleEnemy>().Attacked(highAttackDamage);
            }
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector2(
            input.x,
            abilityManager.isSoul || TouchingWallCnt > 0 ? input.y : 0f);

        // 바라보는 방향을 업데이트
        if (moveInput.x != 0)
        {
            facingDirection = Mathf.Sign(moveInput.x);
        }
    }

#region Cobweb Slow Effect
    public void SetSpeedMultiplier(float multiplier)
    {
        slowEffectCount++;
        speedMultiplier = Mathf.Clamp(multiplier, 0f, 1f);
        ApplyCobwebVerticalLimit();
    }

    public void ResetSpeedMultiplier()
    {
        slowEffectCount = Mathf.Max(0, slowEffectCount - 1);

        if (slowEffectCount == 0)
        {
            speedMultiplier = 1f;
        }
    }

    void ApplyCobwebVerticalLimit()
    {
        if (slowEffectCount <= 0) return;

        float clampedY = rigid.linearVelocityY;

        if (clampedY > cobwebMaxRiseSpeed)
        {
            clampedY = cobwebMaxRiseSpeed;
        }
        else if (clampedY < -cobwebMaxFallSpeed)
        {
            clampedY = -cobwebMaxFallSpeed;
        }

        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, clampedY);
    }
#endregion


    public void OnJump(InputValue value)
    {
        // 점프 불가
        if (isDashing || abilityManager.isSoul) return;

        if (value.isPressed && (!isJump || (TouchingWallCnt > 0 && abilityManager.canWallJump && canWallJumpAgain)))
        {
            if (stamina != null && !stamina.UseStamina(jumpStaminaCost))
            {
                return;
            }

            canWallJumpAgain = false;
            Invoke("EnableWallJump", wallJumpDelay);

            StopWallClimb();
            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJump = true;
        }
    }
    void EnableWallJump()
    {
        canWallJumpAgain = true;
    }

    // 대쉬 액션
    public void OnDash(InputValue value)
    {

        if (abilityManager.isSoul) return;

        if (abilityManager.canDash && canDashAgain && !isDashing && canMove && !isAttacking)
        {
            if (stamina != null && stamina.UseStamina(dashStaminaCost))
            {
                DashCoroutine = StartCoroutine(DashRoutine());
            }
        }
    }

    IEnumerator DashRoutine()
    {
       
        canDashAgain = false;
        isDashing = true;
        rigid.gravityScale = 0f;

        rigid.linearVelocityX = facingDirection * dashSpeed * speedMultiplier;
        rigid.linearVelocityY = 0.0000001f; 

        yield return new WaitForSeconds(dashDuration);

        rigid.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDashAgain = true;
    }

    public void StopDash()
    {
        if (DashCoroutine == null) return;

        StopCoroutine(DashCoroutine);
        rigid.gravityScale = originalGravity;
        isDashing = false;
        canDashAgain = true;
    }

#region Wall Climb
    // 벽 타기 상태를 업데이트
    bool UpdateWallClimbState()
    {
        if (!CanWallClimb())
        {
            StopWallClimb();
            return false;
        }

        float climbInput = moveInput.y;
        bool isMovingVertically = Mathf.Abs(climbInput) > 0.01f;
        float staminaCostPerSecond = isMovingVertically
            ? wallClimbMoveStaminaCostPerSecond
            : wallClimbHoldStaminaCostPerSecond;
        float staminaCost = staminaCostPerSecond * Time.fixedDeltaTime;

        if (stamina != null && !stamina.UseStaminaSilently(staminaCost))
        {
            ForceWallClimbFall();
            return true;
        }

        isWallClimbing = true;
        rigid.gravityScale = 0f;
        rigid.linearVelocity = new Vector2(0f, isMovingVertically ? climbInput * wallClimbSpeed : 0f);
        return true;
    }

    // 벽 타기 가능 여부
    bool CanWallClimb()
    {
        return abilityManager.canWallJump
            && !abilityManager.isSoul
            && TouchingWallCnt > 0
            && !isDashing
            && canMove
            && !wallClimbLockedUntilExit
            && (isWallClimbing
                || rigid.linearVelocityY < -0.01f
                || (Mathf.Abs(moveInput.y) > 0.01f && rigid.linearVelocityY <= 0.01f));
    }

    // 벽 타기 중지
    void StopWallClimb()
    {
        if (!isWallClimbing)
        {
            return;
        }

        isWallClimbing = false;
        rigid.gravityScale = originalGravity;
    }

    // 벽 타기 중 스태미나 부족 시 강제 낙하 처리
    void ForceWallClimbFall()
    {
        StopWallClimb();
        wallClimbLockedUntilExit = true;

        float detachDirection = wallClimbDetachDirection;
        if (Mathf.Abs(detachDirection) <= 0.01f)
        {
            detachDirection = moveInput.x != 0f ? -Mathf.Sign(moveInput.x) : -facingDirection;
        }

        transform.position += Vector3.right * detachDirection * wallClimbFallOffDistance;
        rigid.linearVelocity = new Vector2(0f, -wallClimbFallSpeed);
    }

    // 벽과의 충돌에서 떨어지는 방향을 결정
    void UpdateWallClimbDetachDirection(Collision2D collision)
    {
        if (collision.contactCount <= 0)
        {
            return;
        }

        ContactPoint2D contact = collision.GetContact(0);
        if (Mathf.Abs(contact.normal.x) > 0.01f)
        {
            wallClimbDetachDirection = Mathf.Sign(contact.normal.x);
            return;
        }

        float fallbackDirection = transform.position.x - collision.transform.position.x;
        if (Mathf.Abs(fallbackDirection) > 0.01f)
        {
            wallClimbDetachDirection = Mathf.Sign(fallbackDirection);
        }
    }
#endregion

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            TouchingWallCnt++;
            UpdateWallClimbDetachDirection(collision);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            UpdateWallClimbDetachDirection(collision);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            TouchingWallCnt--;
            if (TouchingWallCnt <= 0)
            {
                TouchingWallCnt = 0;
                wallClimbDetachDirection = 0f;
                wallClimbLockedUntilExit = false;
                StopWallClimb();
            }
        }
    }

    // 빙의 처리 로직
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (abilityManager.isSoul && abilityManager.canPossess)
        {
            if (collision.TryGetComponent<SimpleEnemy>(out var enemy))
            {
                transform.position = enemy.transform.position;

                Destroy(enemy.gameObject);

                abilityManager.PossessBody();
                UpdateFormState();
            }
        }
    }
}
