using UnityEngine;
using System;

public class PlayerDamageHandler : MonoBehaviour, IDamageable
{
    private Health _health;

    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    public void TakeDamage(float damage, DamageType type)
    {
        // 데미지 적용
        _health.ReduceHealth(damage);
    }
}