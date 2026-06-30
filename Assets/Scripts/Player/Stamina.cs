using UnityEngine;
using System;

public class Stamina : MonoBehaviour
{
    public float MaxStamina { get; set; } = 100f;
    public float CurrentStamina { get; private set; }

    [Header("Settings")]
    public float staminaRegenRate = 15f;

    public event Action OnStaminaChanged;

    private void Awake()
    {
        CurrentStamina = MaxStamina;
        Debug.Log($"Player Stamina Initialized: {CurrentStamina}/{MaxStamina}");
    }

    private void Update()
    {
        // 스태미나 자동 회복
        if (CurrentStamina < MaxStamina)
        {
            CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + staminaRegenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke();
        }
    }

    public bool UseStamina(float amount)
    {
        return TryConsumeStamina(amount, true, true);
    }

    public bool UseStaminaSilently(float amount)
    {
        return TryConsumeStamina(amount, false, false);
    }

    bool TryConsumeStamina(float amount, bool logUsage, bool logFailure)
    {
        // 스태미나가 충분한지 확인
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            OnStaminaChanged?.Invoke();

            if (logUsage)
            {
                Debug.Log($"Stamina Reduced: {CurrentStamina}/{MaxStamina}");
            }
            return true;
        }

        if (logFailure)
        {
            Debug.Log("스태미나 부족");
        }
        return false;
    }
}
