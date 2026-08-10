using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerProgressionManager progressionManager;

    [Header("State")]
    public bool isSoul = true;
    public bool canPossess = true;

    [Header("Abilities")]
    public bool canDash = false;
    public bool canWallJump = false;
    public bool canLowAttack = false;
    public bool canHighAttack = false;

    [Header("Inventory")]
    public bool canUseInventory = false;

    private void Awake()
    {
        if (progressionManager == null)
        {
            progressionManager = GetComponent<PlayerProgressionManager>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    public void PossessBody()
    {
        if (progressionManager != null)
        {
            progressionManager.SetSoulState(false);
        }
        else
        {
            isSoul = false;
        }

        playerController.isWallAttatching = false;

        Debug.Log("Possess body.");
    }

    public void DepossessBody()
    {
        if (progressionManager != null)
        {
            progressionManager.SetSoulState(true);
        }
        else
        {
            isSoul = true;
        }

        Debug.Log("Return to soul form.");
    }

    public bool ApplyResolvedState(
        bool soulState,
        bool dashEnabled,
        bool wallJumpEnabled,
        bool lowAttackEnabled,
        bool highAttackEnabled,
        bool inventoryEnabled)
    {
        bool changed = isSoul != soulState
            || canDash != dashEnabled
            || canWallJump != wallJumpEnabled
            || canLowAttack != lowAttackEnabled
            || canHighAttack != highAttackEnabled
            || canUseInventory != inventoryEnabled;

        isSoul = soulState;
        canDash = dashEnabled;
        canWallJump = wallJumpEnabled;
        canLowAttack = lowAttackEnabled;
        canHighAttack = highAttackEnabled;
        canUseInventory = inventoryEnabled;

        return changed;
    }
}
