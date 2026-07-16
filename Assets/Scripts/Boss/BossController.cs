using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BossPhase
{
    Phase1,
    Phase2
}

public enum BossState
{
    Idle,
    Telegraph,
    Attack,
    Recovery,
    Transition,
    Defeated
}

public enum BossWeakPointType
{
    LeftArm,
    RightArm,
    UpperBody,
    LowerBody
}

public enum BossAttackType
{
    LeftPunch,
    RightPunch,
    Charge
}

public class BossController : MonoBehaviour
{
    [Header("Phase Settings")]
    [SerializeField] BossPhase startPhase = BossPhase.Phase1;
    [SerializeField] float phase1MaxHealth = 100f;
    [SerializeField] float phase2MaxHealth = 150f;
    [SerializeField] bool autoStartBattle = true;

    [Header("Transition Settings")]
    [SerializeField] bool loadPhase2SceneOnTransition = true;
    [SerializeField] string phase2SceneName = "BossPhase2";

    [Header("Combat Test Settings")]
    [SerializeField] bool usePassiveTestDamage = true;
    [SerializeField] float passiveDamagePerSecond = 10f;

    [Header("Component References")]
    [SerializeField] BossWeakPointManager weakPointManager;
    [SerializeField] BossAttackController attackController;
    [SerializeField] BossArenaController arenaController;

    public event Action<BossPhase> OnPhaseChanged;
    public event Action<BossState> OnStateChanged;
    public event Action<float, float> OnHealthChanged;
    public event Action OnBossDefeated;

    public BossPhase CurrentPhase { get; private set; }
    public BossState CurrentState { get; private set; } = BossState.Idle;
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsBattleActive { get; private set; }
    public bool IsDefeated => CurrentState == BossState.Defeated;

    void Reset()
    {
        CacheReferences();
    }

    void Awake()
    {
        CacheReferences();
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

    public void SetState(BossState nextState)
    {
        if (CurrentState == nextState)
        {
            return;
        }

        CurrentState = nextState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void ApplyDamage(float amount)
    {
        if (!IsBattleActive || IsDefeated || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

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

        if (!silent)
        {
            OnPhaseChanged?.Invoke(CurrentPhase);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
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
}
