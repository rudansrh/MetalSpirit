using System;
using UnityEngine;

[Serializable]
public class PlayerStageVisualSet
{
    public PlayerStage stage;
    public RuntimeAnimatorController animatorController;
    public Sprite previewSprite;
}

public class PlayerVisualManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerProgressionManager progressionManager;

    [Header("Stage Visuals")]
    [SerializeField] private PlayerStageVisualSet[] stageVisuals;

    [Header("Animator Parameters")]
    [SerializeField] private string stageParameter = "Stage";
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string groundedParameter = "IsGrounded";
    [SerializeField] private string yVelocityParameter = "YVelocity";
    [SerializeField] private string dashingParameter = "IsDashing";
    [SerializeField] private string wallClimbParameter = "IsWallClimbing";
    [SerializeField] private string soulParameter = "IsSoul";
    [SerializeField] private string legAttackTriggerParameter = "doLegAtk";
    [SerializeField] private string armAttackTriggerParameter = "doArmAtk";
    [SerializeField] private string bodyAttackTriggerParameter = "doBodyAtk";
    [SerializeField] private string headAttackTriggerParameter = "doHeadAtk";

    [Header("Facing Settings")]
    [SerializeField] private bool facesLeftByDefault = true;

    private int stageHash;
    private int speedHash;
    private int groundedHash;
    private int yVelocityHash;
    private int dashingHash;
    private int wallClimbHash;
    private int soulHash;
    private int legAttackTriggerHash;
    private int armAttackTriggerHash;
    private int bodyAttackTriggerHash;
    private int headAttackTriggerHash;
    private bool isInitialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void ApplyCurrentVisual()
    {
        EnsureInitialized();

        if (progressionManager == null)
        {
            return;
        }

        ApplyVisualStage(progressionManager.CurrentVisualStage);
    }

    public void ApplyVisualStage(PlayerStage stage)
    {
        EnsureInitialized();

        PlayerStageVisualSet visualSet = FindVisualSet(stage);
        bool controllerChanged = false;

        if (visualSet != null)
        {
            if (animator != null && visualSet.animatorController != null)
            {
                controllerChanged = animator.runtimeAnimatorController != visualSet.animatorController;
                animator.runtimeAnimatorController = visualSet.animatorController;
            }

            if (spriteRenderer != null && visualSet.previewSprite != null)
            {
                spriteRenderer.sprite = visualSet.previewSprite;
            }
        }

        if (animator != null)
        {
            animator.SetInteger(stageHash, (int)stage);

            if (controllerChanged)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }
    }

    public void UpdateAnimationState(float speed, bool isGrounded, float yVelocity, bool isDashing, bool isWallClimbing, bool isSoul)
    {
        EnsureInitialized();

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetFloat(speedHash, speed);
        animator.SetBool(groundedHash, isGrounded);
        animator.SetFloat(yVelocityHash, yVelocity);
        animator.SetBool(dashingHash, isDashing);
        animator.SetBool(wallClimbHash, isWallClimbing);
        animator.SetBool(soulHash, isSoul);
    }

    public void UpdateFacingDirection(float horizontalDirection)
    {
        EnsureInitialized();

        if (spriteRenderer == null || Mathf.Abs(horizontalDirection) < 0.01f)
        {
            return;
        }

        bool faceRight = horizontalDirection > 0f;
        spriteRenderer.flipX = facesLeftByDefault ? faceRight : !faceRight;
    }

    public void PlayLegAttackAnimation()
    {
        TriggerAnimation(legAttackTriggerHash);
    }

    public void PlayArmAttackAnimation()
    {
        TriggerAnimation(armAttackTriggerHash);
    }

    public void PlayBodyAttackAnimation()
    {
        TriggerAnimation(bodyAttackTriggerHash);
    }

    public void PlayHeadAttackAnimation()
    {
        TriggerAnimation(headAttackTriggerHash);
    }

    private void CacheParameterHashes()
    {
        stageHash = Animator.StringToHash(stageParameter);
        speedHash = Animator.StringToHash(speedParameter);
        groundedHash = Animator.StringToHash(groundedParameter);
        yVelocityHash = Animator.StringToHash(yVelocityParameter);
        dashingHash = Animator.StringToHash(dashingParameter);
        wallClimbHash = Animator.StringToHash(wallClimbParameter);
        soulHash = Animator.StringToHash(soulParameter);
        legAttackTriggerHash = Animator.StringToHash(legAttackTriggerParameter);
        armAttackTriggerHash = Animator.StringToHash(armAttackTriggerParameter);
        bodyAttackTriggerHash = Animator.StringToHash(bodyAttackTriggerParameter);
        headAttackTriggerHash = Animator.StringToHash(headAttackTriggerParameter);
    }

    private void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (progressionManager == null)
        {
            progressionManager = GetComponent<PlayerProgressionManager>();
        }

        if (string.IsNullOrWhiteSpace(stageParameter)) stageParameter = "Stage";
        if (string.IsNullOrWhiteSpace(speedParameter)) speedParameter = "Speed";
        if (string.IsNullOrWhiteSpace(groundedParameter)) groundedParameter = "IsGrounded";
        if (string.IsNullOrWhiteSpace(yVelocityParameter)) yVelocityParameter = "YVelocity";
        if (string.IsNullOrWhiteSpace(dashingParameter)) dashingParameter = "IsDashing";
        if (string.IsNullOrWhiteSpace(wallClimbParameter)) wallClimbParameter = "IsWallClimbing";
        if (string.IsNullOrWhiteSpace(soulParameter)) soulParameter = "IsSoul";
        if (string.IsNullOrWhiteSpace(legAttackTriggerParameter)) legAttackTriggerParameter = "doLegAtk";
        if (string.IsNullOrWhiteSpace(armAttackTriggerParameter)) armAttackTriggerParameter = "doArmAtk";
        if (string.IsNullOrWhiteSpace(bodyAttackTriggerParameter)) bodyAttackTriggerParameter = "doBodyAtk";
        if (string.IsNullOrWhiteSpace(headAttackTriggerParameter)) headAttackTriggerParameter = "doHeadAtk";

        CacheParameterHashes();
        isInitialized = true;
    }

    private void TriggerAnimation(int triggerHash)
    {
        EnsureInitialized();

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetTrigger(triggerHash);
    }

    private PlayerStageVisualSet FindVisualSet(PlayerStage stage)
    {
        if (stageVisuals == null)
        {
            return null;
        }

        foreach (PlayerStageVisualSet stageVisual in stageVisuals)
        {
            if (stageVisual.stage == stage)
            {
                return stageVisual;
            }
        }

        return null;
    }
}
