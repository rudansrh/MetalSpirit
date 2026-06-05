using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] PlayerAbilityManager abilityManager;

    [SerializeField] float speed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpDelay;

    //대쉬 관련 변수
    [SerializeField] float dashSpeed = 20f;     // 돌진 속도
    [SerializeField] float dashDuration = 0.2f; // 돌진 유지 시간
    [SerializeField] float dashCooldown = 1f;   // 돌진 쿨타임
    
    bool isDashing = false;
    bool canDashAgain = true;
    float facingDirection = 1f; // 바라보는 방향 (기본값: 오른쪽 1)

    bool isJump = false;
    bool canWallJumpAgain = true;
    int TouchingWallCnt = 0;

    Vector2 moveInput;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 대시 중일 땐 이동과 중력 무시
        if (isDashing)
            return;

        if (rigid.linearVelocityY == 0)
        {
            isJump = false;
        }
        rigid.linearVelocityX = moveInput.x * speed;
    }

    void enableWallJump()
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

    public void OnJump(InputValue value)
    {
        // 대시 중에는 점프 불가
        if (isDashing) return;

        if (value.isPressed && (!isJump || (TouchingWallCnt>0 && abilityManager.canWallJump && canWallJumpAgain)))
        {
            canWallJumpAgain=false;
            Invoke("enableWallJump", wallJumpDelay);

            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJump = true;
        }
    }

    // 대쉬 액션
    public void OnDash(InputValue value)
    {
        if (value.isPressed && abilityManager.canDash && canDashAgain && !isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
       
        canDashAgain = false;
        isDashing = true;
        float originalGravity = rigid.gravityScale;
        rigid.gravityScale = 0f;

        rigid.linearVelocityX = facingDirection * dashSpeed;
        rigid.linearVelocityY = 0.0000001f; 

        yield return new WaitForSeconds(dashDuration);

        rigid.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
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
