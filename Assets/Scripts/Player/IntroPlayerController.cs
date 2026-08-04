using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class IntroPlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerVisualManager visualManager;
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerInput playerInput;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (visualManager == null)
        {
            visualManager = GetComponent<PlayerVisualManager>();
        }

        if (abilityManager == null)
        {
            abilityManager = GetComponent<PlayerAbilityManager>();
        }

        if (rigidbody2D == null)
        {
            rigidbody2D = GetComponent<Rigidbody2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
    }

    public void PrepareForIntro(Vector3 startPoint)
    {
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        if (rigidbody2D != null)
        {
            rigidbody2D.linearVelocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
            rigidbody2D.simulated = false;
        }

        transform.position = new Vector3(startPoint.x, startPoint.y, transform.position.z);
        SetIdleAnimation();
    }

    public IEnumerator MoveTo(Vector3 targetPoint, float speed)
    {
        Vector3 targetPosition = new Vector3(targetPoint.x, targetPoint.y, transform.position.z);

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            Vector3 previousPosition = transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            Vector3 delta = transform.position - previousPosition;
            float horizontal = Mathf.Abs(delta.x) > 0.001f ? Mathf.Sign(delta.x) : 0f;
            UpdateAnimation(horizontal, true, false, 0f);

            yield return null;
        }

        transform.position = targetPosition;
        SetIdleAnimation();
    }

    public IEnumerator Fall(float distance, float duration)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * distance;
        float elapsed = 0f;

        PlayFallAnimation();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = progress * progress;

            transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);
            UpdateFallAnimationState();

            yield return null;
        }

        transform.position = endPosition;
    }

    public void SetIdleAnimation()
    {
        UpdateAnimation(0f, false, false, 0f);
    }

    private void UpdateAnimation(float horizontal, bool isMoving, bool isFalling, float yVelocity)
    {
        if (visualManager != null)
        {
            visualManager.UpdateFacingDirection(horizontal);
            visualManager.UpdateAnimationState(
                isMoving ? 1f : 0f,
                !isFalling,
                yVelocity,
                false,
                false,
                abilityManager != null && abilityManager.isSoul);
            return;
        }

        if (spriteRenderer != null && Mathf.Abs(horizontal) > 0.001f)
        {
            spriteRenderer.flipX = horizontal > 0f;
        }
    }

    private void PlayFallAnimation()
    {
        if (animator != null)
        {
            animator.Play("player1_air", 0, 0f);
        }
    }

    private void UpdateFallAnimationState()
    {
        if (visualManager != null)
        {
            visualManager.UpdateAnimationState(
                0f,
                false,
                -8f,
                false,
                false,
                abilityManager != null && abilityManager.isSoul);
        }
    }
}
