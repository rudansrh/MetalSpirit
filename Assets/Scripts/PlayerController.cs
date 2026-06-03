using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField] PlayerAbilityManager abilityManager;

    [SerializeField] float speed;
    [SerializeField] float jumpForce;
    [SerializeField] float wallJumpDelay;

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
        if(rigid.linearVelocityY == 0)
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
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && (!isJump || (TouchingWallCnt>0 && abilityManager.canWallJump && canWallJumpAgain)))
        {
            canWallJumpAgain=false;
            Invoke("enableWallJump", wallJumpDelay);

            rigid.linearVelocityY = 0;
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJump = true;
        }
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
