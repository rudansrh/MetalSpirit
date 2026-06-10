using UnityEngine;
using System;

public class Stamina : MonoBehaviour
{
    public float MaxStamina { get; set; } = 100f;
    public float CurrentStamina { get; private set; }

    [Header("Settings")]
    public float staminaRegenRate = 15f;

    public event Action OnStaminaChanged;

    private void Start()
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
            
        }
    }

    public bool UseStamina(float amount)
    {
        // 스태미나가 충분한지 확인
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            OnStaminaChanged?.Invoke();

            // 스태미나 감소 로그
            Debug.Log($"Stamina Reduced: {CurrentStamina}/{MaxStamina}");
            return true;
        }

        Debug.Log("스태미나 부족");
        return false;
    }
}