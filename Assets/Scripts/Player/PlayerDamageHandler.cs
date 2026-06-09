using UnityEngine;
using System;
using System.Collections;

public class PlayerDamageHandler : MonoBehaviour, IDamageable
{
    private Health _health;
    private PlayerController _controller;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _controller = GetComponent<PlayerController>();
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
        yield return new WaitForSeconds(0.4f);
        _controller.canMove = true;
        _controller.StopDash();
    }
}