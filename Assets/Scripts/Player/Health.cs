using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour // 플레이어 체력 컴포넌트
{ 
    public float MaxHealth { get; set; } = 200f;        // 최대 체력
    public float CurrentHealth { get; set; }    // 현재 체력
    public bool PlayerIsDead { get; set; } = false; // 플레이어 사망 여부

    public event Action OnHealthChanged;    // 체력 변화 이벤트
    public event Action OnDeath;            // 사망 이벤트

    private PlayerAbilityManager abilityManager;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
        abilityManager = GetComponent<PlayerAbilityManager>();
        Debug.Log($"Player Health Initialized: {CurrentHealth}/{MaxHealth}");
    }

    private void Start()
    {
        OnDeath += SaveManager.Instance.YouDied;
    }

    private void Update()
    {
        // 체력(기름)은 상시 감소 
        // 영혼 상태이거나 특정 조건일 때 감소하지 않도록 추후 분기 처리
        if (abilityManager != null && abilityManager.isSoul) return;
        ReduceHealth(Time.deltaTime * 1.0f); 
    }

    // 체력 감소 메서드, 외부에서 공격 등으로 호출
    public void ReduceHealth(float amount)
    {
        if(PlayerController.Instance != null && PlayerController.Instance.isInvincibility) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke();

        if (CurrentHealth <= 0 && !PlayerIsDead)
        {
            PlayerIsDead = true;
            AudioManager.instance?.PlaySfx(AudioManager.Sfx.GameOver); //***
            OnDeath?.Invoke();
            return;
        }

        //Debug.Log($"Player Health Reduced: {CurrentHealth}/{MaxHealth}");
    }

    // 체력 회복 메서드, 외부에서 회복 등으로 호출
    public void RestoreHealth(float amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke();
    }
    public void LoadHealthData(float savedHealth)
    {

        CurrentHealth = Mathf.Clamp(savedHealth, 0, MaxHealth);

        OnHealthChanged?.Invoke();

        Debug.Log($"체력 로드 완료: {CurrentHealth} / {MaxHealth}");
    }
}
