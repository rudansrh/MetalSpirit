using UnityEngine;

public class PlayerProgressionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private PlayerVisualManager visualManager;

    [Header("Progression")]
    [SerializeField] private PlayerStage unlockedStage = PlayerStage.Soul;

    public PlayerStage UnlockedStage => unlockedStage;
    public PlayerStage CurrentVisualStage => IsSoulForm ? PlayerStage.Soul : unlockedStage;
    public bool IsSoulForm => abilityManager == null || abilityManager.isSoul;

    private PlayerStage lastAppliedVisualStage = (PlayerStage)(-1);

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
        SyncState(forceVisualRefresh: true);
    }

    private void LateUpdate()
    {
        SyncState();
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

    public void LoadState(bool isSoulState, PlayerStage stage)
    {
        unlockedStage = ClampStage(stage);

        if (abilityManager != null)
        {
            abilityManager.isSoul = isSoulState;
        }

        SyncState(forceVisualRefresh: true);
    }

    private void SyncState(bool forceVisualRefresh = false)
    {
        SyncAbilities();

        PlayerStage currentVisualStage = CurrentVisualStage;
        if (forceVisualRefresh || currentVisualStage != lastAppliedVisualStage)
        {
            lastAppliedVisualStage = currentVisualStage;

            if (visualManager != null)
            {
                visualManager.ApplyVisualStage(currentVisualStage);
            }
        }
    }

    private void SyncAbilities()
    {
        if (abilityManager == null)
        {
            return;
        }

        if (abilityManager.isSoul)
        {
            abilityManager.canDash = false;
            abilityManager.canWallJump = false;
            abilityManager.canLowAttack = false;
            abilityManager.canHighAttack = false;
            abilityManager.canUseInventory = false;
            return;
        }

        abilityManager.canDash = unlockedStage >= PlayerStage.Legs;
        abilityManager.canWallJump = unlockedStage >= PlayerStage.Legs;
        abilityManager.canLowAttack = unlockedStage >= PlayerStage.Arms;
        abilityManager.canHighAttack = unlockedStage >= PlayerStage.Arms;
        abilityManager.canUseInventory = unlockedStage >= PlayerStage.FullBody;
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
