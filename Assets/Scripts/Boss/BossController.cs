using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BossPhase
{
    Phase1, // 페이즈 1
    Phase2  // 페이즈 2
}

public enum BossState
{
    Idle,       // 대기 상태
    Telegraph,  // 패턴 예고 상태
    Attack,     // 공격 상태
    Recovery,   // 패턴 후 회복 상태
    Transition, // 페이즈 전환 상태
    Defeated    // 처치 상태
}

public enum BossWeakPointType
{
    LeftArm,    // 왼쪽 팔
    RightArm,   // 오른쪽 팔
    UpperBody,  // 상체(머리)
    LowerBody   // 하체(몸통)
}

public enum BossAttackType
{
    LeftPunch,  // 왼팔 펀치
    RightPunch, // 오른팔 펀치
    Charge      // 돌진
}

public class BossController : MonoBehaviour
{
    const string BossHealthSliderName = "Slider_BossHealth";

    [Header("Phase Settings")]
    [SerializeField] BossPhase startPhase = BossPhase.Phase1;   // 시작 페이즈
    [SerializeField] float phase1MaxHealth = 100f;              // 페이즈 1 최대 체력
    [SerializeField] float phase2MaxHealth = 150f;              // 페이즈 2 최대 체력
    [SerializeField] bool autoStartBattle = true;               // 자동으로 전투 시작 여부

    [Header("Transition Settings")]
    [SerializeField] bool loadPhase2SceneOnTransition = true;   // 페이즈 2로 전환 시 씬 로드 여부
    [SerializeField] string phase2SceneName = "BossPhase2";     // 페이즈 2 씬 이름

    [Header("Combat Test Settings")]
    [SerializeField] bool usePassiveTestDamage = true;          // 테스트용 지속 피해 사용 여부
    [SerializeField] float passiveDamagePerSecond = 10f;        // 테스트용 지속 피해량 (초당)

    [Header("Component References")]
    [SerializeField] BossWeakPointManager weakPointManager;     // 약점 포인트 Manager
    [SerializeField] BossAttackController attackController;     // 공격 Controller
    [SerializeField] BossArenaController arenaController;       // 전투 Arena Controller

    [Header("UI")]
    [SerializeField] Slider bossHealthSlider;

    public event Action<BossPhase> OnPhaseChanged;              // 페이즈 변경 이벤트
    public event Action<BossState> OnStateChanged;              // 상태 변경 이벤트
    public event Action<float, float> OnHealthChanged;          // 체력 변경 이벤트 (현재 체력, 최대 체력)
    public event Action OnBossDefeated;                         // 보스 처치 이벤트

    public BossPhase CurrentPhase { get; private set; }                     // 현재 페이즈
    public BossState CurrentState { get; private set; } = BossState.Idle;   // 현재 상태
    public float CurrentHealth { get; private set; }                        // 현재 체력
    public float MaxHealth { get; private set; }                            // 최대 체력
    public bool IsBattleActive { get; private set; }                        // 전투 활성화 여부
    public bool IsDefeated => CurrentState == BossState.Defeated;           // 보스 처치 여부

    void Reset()
    {
        CacheReferences();
    }

    void Awake()
    {
        CacheReferences();
        CacheUiReferences();
        InitializePhase(startPhase, true);
    }

    void Start()
    {
        if (autoStartBattle)
        {
            StartBattle();
        }
    }

    void Update()
    {
        if (!IsBattleActive || IsDefeated || !usePassiveTestDamage)
        {
            return;
        }

        ApplyDamage(passiveDamagePerSecond * Time.deltaTime);
    }

    void CacheReferences()
    {
        if (weakPointManager == null)
        {
            weakPointManager = GetComponentInChildren<BossWeakPointManager>(true);
        }

        if (attackController == null)
        {
            attackController = GetComponentInChildren<BossAttackController>(true);
        }

        if (arenaController == null)
        {
            arenaController = GetComponentInChildren<BossArenaController>(true);
        }
    }

    void CacheUiReferences()
    {
        if (bossHealthSlider != null)
        {
            return;
        }

        GameObject sliderObject = GameObject.Find(BossHealthSliderName);
        if (sliderObject != null)
        {
            bossHealthSlider = sliderObject.GetComponent<Slider>();
        }
    }

    public void StartBattle()
    {
        if (IsBattleActive || IsDefeated)
        {
            return;
        }

        IsBattleActive = true;

        if (weakPointManager != null)
        {
            weakPointManager.Begin(this);
        }

        if (attackController != null)
        {
            attackController.Begin(this);
        }

        if (arenaController != null)
        {
            arenaController.Begin(this);
        }

        SetState(BossState.Idle);
    }

    public void StopBattle()
    {
        IsBattleActive = false;

        if (weakPointManager != null)
        {
            weakPointManager.StopCycle();
        }

        if (attackController != null)
        {
            attackController.StopAttacks();
        }

        if (arenaController != null)
        {
            arenaController.StopArenaLoop();
        }
    }

    public void SuppressAutoStartBattle()
    {
        autoStartBattle = false;
        StopBattle();
    }

    public void SetBossHealthUiVisible(bool isVisible)
    {
        if (bossHealthSlider == null)
        {
            CacheUiReferences();
        }

        if (bossHealthSlider == null)
        {
            return;
        }

        bossHealthSlider.gameObject.SetActive(isVisible);
    }

    public void SetState(BossState nextState)
    {
        if (CurrentState == nextState) return;

        CurrentState = nextState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void ApplyDamage(float amount)
    {
        if (!IsBattleActive || IsDefeated || amount <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        RefreshBossHealthUI();

        Debug.Log($"Boss HP reduced by {amount:0.##}. Current Health: {CurrentHealth:0.##}/{MaxHealth:0.##}");

        if (CurrentHealth <= 0f)
        {
            HandleHealthDepleted();
        }
    }

    public void InitializePhase(BossPhase phase, bool silent = false)
    {
        CurrentPhase = phase;
        MaxHealth = phase == BossPhase.Phase1 ? phase1MaxHealth : phase2MaxHealth;
        CurrentHealth = MaxHealth;
        RefreshBossHealthUI();

        if (!silent)
        {
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }

    void RefreshBossHealthUI()
    {
        if (bossHealthSlider == null)
        {
            CacheUiReferences();
        }

        if (bossHealthSlider == null)
        {
            return;
        }

        bossHealthSlider.minValue = 0f;
        bossHealthSlider.maxValue = MaxHealth;
        bossHealthSlider.value = CurrentHealth;
    }

    void HandleHealthDepleted()
    {
        StopBattle();

        if (CurrentPhase == BossPhase.Phase1)
        {
            SetState(BossState.Transition);
            BeginPhase2();
            return;
        }

        SetState(BossState.Defeated);
        OnBossDefeated?.Invoke();
        Debug.Log("Boss defeated.");
    }

    void BeginPhase2()
    {
        if (loadPhase2SceneOnTransition && !string.IsNullOrWhiteSpace(phase2SceneName))
        {
            SceneManager.LoadScene(phase2SceneName);
            return;
        }

        InitializePhase(BossPhase.Phase2);
        StartBattle();
    }

    void OnValidate()
    {
        CacheUiReferences();
        RefreshBossHealthUI();
    }
}
