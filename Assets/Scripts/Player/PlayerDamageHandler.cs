using UnityEngine;
using System;
using System.Collections;

public class PlayerDamageHandler : MonoBehaviour, IDamageable
{
    private Health _health;
    private PlayerController _controller;
    private SpriteRenderer playerColor;
    private Color orginColor;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _controller = GetComponent<PlayerController>();
        playerColor = GetComponent<SpriteRenderer>();

        orginColor = playerColor.color;
    }

    public void TakeDamage(float damage, DamageType type)
    {
        // 데미지 적용
        _health.ReduceHealth(damage);

        if (type == DamageType.Normal)
        { 
            StartCoroutine(playerKnockBack());
        }
    }

    IEnumerator playerKnockBack() // 플레이어 넉백시 다른 입력 멈춤
    {
        _controller.canMove = false;
        _controller.isInvincibility = true;
        _controller.StopDash();
        StartCoroutine(damagedEffect());
        yield return new WaitForSeconds(0.3f);
        _controller.canMove = true;
        _controller.isInvincibility = false;
    }

    IEnumerator damagedEffect() //임시 피격 효과
    { 
        for (int i = 0; i < 1; i++)
        {
            playerColor.color = new Color32(201, 190, 172, 150);
            yield return new WaitForSeconds(0.2f);

            playerColor.color = new Color32(201, 149, 94, 150);
            yield return new WaitForSeconds(0.2f);
        }

        playerColor.color = orginColor;
    }
}