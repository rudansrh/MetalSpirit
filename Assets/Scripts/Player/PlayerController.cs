using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] PlayerAbilityManager abilityManager;

    [Header("Movement Settings")]
    [SerializeField] float speed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpDelay;

    [Header("Dash Settings")]
    [SerializeField] float dashSpeed = 20f;     // 돌진 속도
    [SerializeField] float dashDuration = 0.2f; // 돌진 유지 시간
    [SerializeField] float dashCooldown = 1f;   // 돌진 쿨타임

    [Header("Slow Effect Settings")]
    [SerializeField] float speedMultiplier = 1f; // 느려지는 효과 수치
    [SerializeField] int slowEffectCount = 0;    // 느려지는 효과 중첩 카운트
    [SerializeField] float cobwebMaxRiseSpeed = 2.5f;
    [SerializeField] float cobwebMaxFallSpeed = 0.1f;
    
    bool isDashing = false;
    bool canDashAgain = true;
    Coroutine DashCoroutine;
    float facingDirection = 1f; // 바라보는 방향 (기본값: 오른쪽 1)
    float originalGravity = 1f;

    bool isJump = false;
    bool canWallJumpAgain = true;
    int TouchingWallCnt = 0;

    Vector2 moveInput;

    public bool canMove = true;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        originalGravity = rigid.gravityScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 대시 중일 땐 이동과 중력 무시
        if (isDashing || !canMove)
            return;

        if (rigid.linearVelocityY == 0)
        {
            isJump = false;
        }

        ApplyCobwebVerticalLimit();
        rigid.linearVelocityX = moveInput.x * speed * speedMultiplier;
    }

    void EnableWallJump()
    {
        canWallJumpAgain = true;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

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
        // 대시 중에는 점프 불가
        if (isDashing) return;

        if (value.isPressed && (!isJump || (TouchingWallCnt > 0 && abilityManager.canWallJump && canWallJumpAgain)))
        {
            canWallJumpAgain = false;
            Invoke("EnableWallJump", wallJumpDelay);

            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJump = true;
        }
    }

    // 대쉬 액션
    public void OnDash(InputValue value)
    {
        if (value.isPressed && abilityManager.canDash && canDashAgain && !isDashing && canMove)
        {
            DashCoroutine = StartCoroutine(DashRoutine());
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            TouchingWallCnt++;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            TouchingWallCnt--;
        }
    }
}
