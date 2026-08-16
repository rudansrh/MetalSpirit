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

    [Header("Facing Settings")]
    [SerializeField] private bool facesLeftByDefault = true;

    private int stageHash;
    private int speedHash;
    private int groundedHash;
    private int yVelocityHash;
    private int dashingHash;
    private int wallClimbHash;
    private int soulHash;

    private void Awake()
    {
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

        CacheParameterHashes();
    }

    public void ApplyCurrentVisual()
    {
        if (progressionManager == null)
        {
            return;
        }

        ApplyVisualStage(progressionManager.CurrentVisualStage);
    }

    public void ApplyVisualStage(PlayerStage stage)
    {
        PlayerStageVisualSet visualSet = FindVisualSet(stage);

        if (visualSet != null)
        {
            if (animator != null && visualSet.animatorController != null)
            {
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
        }
    }

    public void UpdateAnimationState(float speed, bool isGrounded, float yVelocity, bool isDashing, bool isWallClimbing, bool isSoul)
    {
        if (animator == null)
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
        if (spriteRenderer == null || Mathf.Abs(horizontalDirection) < 0.01f)
        {
            return;
        }

        bool faceRight = horizontalDirection > 0f;
        spriteRenderer.flipX = facesLeftByDefault ? faceRight : !faceRight;
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
