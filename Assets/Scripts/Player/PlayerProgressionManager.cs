using System;
using UnityEngine;
using UnityEngine.LightTransport.PostProcessing;

public class PlayerProgressionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private PlayerVisualManager visualManager;

    [Header("Progression")]
    [SerializeField] private PlayerStage unlockedStage = PlayerStage.Soul;

    [Header("Debug Runtime Override")]
    [SerializeField] private bool useDebugOverride = false;
    [SerializeField] private bool debugIsSoul = true;
    [SerializeField] private PlayerStage debugUnlockedStage = PlayerStage.Soul;

    public PlayerStage UnlockedStage => unlockedStage;
    public PlayerStage EffectiveUnlockedStage => useDebugOverride ? ClampStage(debugUnlockedStage) : unlockedStage;
    public PlayerStage CurrentVisualStage => IsSoulForm ? PlayerStage.Soul : EffectiveUnlockedStage;
    public bool IsSoulForm => EffectiveIsSoul;
    public bool EffectiveIsSoul => useDebugOverride ? debugIsSoul : runtimeSoulState;

    public event Action StateChanged;

    private PlayerStage lastAppliedVisualStage = (PlayerStage)(-1);
    private bool runtimeSoulState = true;
    private bool hasAppliedState = false;
    private bool lastAppliedSoulState = true;

    private PossessGauge possess;
    private InventoryManager inventory;

    private void Awake()
    {
        if (abilityManager == null)
        {
            abilityManager = GetComponent<PlayerAbilityManager>();
        }

        if (visualManager == null)
        {
            visualManager = GetComponent<PlayerVisualManager>();
        }

        unlockedStage = ClampStage(unlockedStage);
        debugUnlockedStage = ClampStage(debugUnlockedStage);
        runtimeSoulState = abilityManager == null ? true : abilityManager.isSoul;
        SyncState(forceVisualRefresh: true);
    }

    private void Start()
    {
        CachePlayerDependencies();
    }
    private void Update()
    {
        if (!useDebugOverride && abilityManager != null && abilityManager.isSoul != runtimeSoulState)
        {
            runtimeSoulState = abilityManager.isSoul;
            SyncState(forceVisualRefresh: true);
            return;
        }

        SyncState();
    }

    private void OnValidate()
    {
        unlockedStage = ClampStage(unlockedStage);
        debugUnlockedStage = ClampStage(debugUnlockedStage);

        if (!Application.isPlaying)
        {
            return;
        }

        SyncState(forceVisualRefresh: true);
    }

    public void SetUnlockedStage(PlayerStage stage)
    {
        unlockedStage = ClampStage(stage);
        SyncState(forceVisualRefresh: true);
    }

    public bool UnlockNextStage()
    {
        if (unlockedStage >= PlayerStage.FullBody)
        {
            return false;
        }

        unlockedStage += 1;
        SyncState(forceVisualRefresh: true);
        return true;
    }

    public void SetSoulState(bool isSoulState)
    {
        runtimeSoulState = isSoulState;
        SyncState(forceVisualRefresh: true);
    }

    public void LoadState(bool isSoulState, PlayerStage stage)
    {
        unlockedStage = ClampStage(stage);
        runtimeSoulState = isSoulState;
        SyncState(forceVisualRefresh: true);
    }

    private void SyncState(bool forceVisualRefresh = false)
    {
        CachePlayerDependencies();

        bool effectiveSoulState = EffectiveIsSoul;
        PlayerStage currentVisualStage = effectiveSoulState ? PlayerStage.Soul : EffectiveUnlockedStage;
        bool hadAbilityChanges = ApplyResolvedAbilities();
        bool visualStageChanged = currentVisualStage != lastAppliedVisualStage;

        if (forceVisualRefresh || visualStageChanged)
        {
            lastAppliedVisualStage = currentVisualStage;

            if (visualManager != null)
            {
                visualManager.ApplyVisualStage(currentVisualStage);
            }
        }

        bool resolvedStateChanged = !hasAppliedState
            || effectiveSoulState != lastAppliedSoulState
            || visualStageChanged
            || hadAbilityChanges;

        if (!resolvedStateChanged)
        {
            return;
        }

        hasAppliedState = true;
        lastAppliedSoulState = effectiveSoulState;
        StateChanged?.Invoke();
    }

    private bool ApplyResolvedAbilities()
    {
        if (abilityManager == null)
        {
            return false;
        }

        if (EffectiveIsSoul)
        {
            return abilityManager.ApplyResolvedState(
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                false);
        }

        //빙의 제한시간 결정 + 인벤토리 칸수 결정
        if (possess != null)
        {
            possess.isInfinityPossess = false;

            if (unlockedStage == PlayerStage.Soul)
            {
                possess.isInfinityPossess = true;
            }
            else if (unlockedStage == PlayerStage.Legs)
            {
                possess.possessionLimitTime = 60;
            }
            else if (unlockedStage == PlayerStage.Arms)
            {
                possess.possessionLimitTime = 30;
            }
            else if (unlockedStage == PlayerStage.FullBody)
            {
                possess.possessionLimitTime = 5;
            }
        }

        if (inventory != null)
        {
            if (unlockedStage == PlayerStage.Arms && inventory.maxSlotCount != 3)
            {
                abilityManager.canUseInventory = true;
                inventory.AddSlot(3 - inventory.maxSlotCount);
                Debug.Log("슬롯칸수 변경3");
            }
            else if (unlockedStage == PlayerStage.FullBody && inventory.maxSlotCount != 5)
            {
                abilityManager.canUseInventory = true;
                inventory.AddSlot(5 - inventory.maxSlotCount);
                Debug.Log("슬롯칸수 변경5");
            }
        }

        return abilityManager.ApplyResolvedState(
            false,
            EffectiveUnlockedStage >= PlayerStage.Legs,
            EffectiveUnlockedStage >= PlayerStage.Legs,
            EffectiveUnlockedStage >= PlayerStage.Legs,
            EffectiveUnlockedStage >= PlayerStage.Arms,
            EffectiveUnlockedStage >= PlayerStage.FullBody,
            EffectiveUnlockedStage >= PlayerStage.FullBody,
            EffectiveUnlockedStage >= PlayerStage.Arms);
    }

    private void CachePlayerDependencies()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        if (possess == null)
        {
            possess = PlayerController.Instance.possessGauge;
        }

        if (inventory == null)
        {
            inventory = PlayerController.Instance.GetComponent<InventoryManager>();
        }
    }

    private PlayerStage ClampStage(PlayerStage stage)
    {
        if (stage < PlayerStage.Soul)
        {
            return PlayerStage.Soul;
        }

        if (stage > PlayerStage.FullBody)
        {
            return PlayerStage.FullBody;
        }

        return stage;
    }
}
