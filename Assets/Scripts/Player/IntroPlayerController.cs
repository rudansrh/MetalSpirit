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
    [SerializeField] private Sprite fallSprite;

    private Vector3 simulatedRoutePosition;
    private Sprite defaultSprite;

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

        simulatedRoutePosition = new Vector3(startPoint.x, startPoint.y, transform.position.z);
        transform.position = simulatedRoutePosition;
        CacheDefaultSprite();
        SetIdleAnimation();
    }

    public IEnumerator MoveTo(Vector3 targetPoint, float speed)
    {
        Vector3 targetPosition = new Vector3(targetPoint.x, targetPoint.y, simulatedRoutePosition.z);
        Vector3 routeDelta = targetPosition - simulatedRoutePosition;
        float distance = routeDelta.magnitude;

        if (distance <= 0.01f || speed <= 0f)
        {
            simulatedRoutePosition = targetPosition;
            SetIdleAnimation();
            yield break;
        }

        float horizontal = Mathf.Abs(routeDelta.x) > 0.001f ? Mathf.Sign(routeDelta.x) : 0f;
        float duration = distance / speed;
        float elapsed = 0f;

        RestoreAnimationControl();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            UpdateAnimation(horizontal, true, false, 0f);
            yield return null;
        }

        simulatedRoutePosition = targetPosition;
        SetIdleAnimation();
    }

    public IEnumerator Fall(float distance, float duration)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.down * distance;
        float elapsed = 0f;

        EnableFallSprite();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = progress * progress;

            transform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);
            yield return null;
        }

        transform.position = endPosition;
    }

    public void SetIdleAnimation()
    {
        RestoreAnimationControl();
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

    private void CacheDefaultSprite()
    {
        if (spriteRenderer != null && defaultSprite == null)
        {
            defaultSprite = spriteRenderer.sprite;
        }
    }

    private void EnableFallSprite()
    {
        CacheDefaultSprite();

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (spriteRenderer != null && fallSprite != null)
        {
            spriteRenderer.sprite = fallSprite;
        }
    }

    private void RestoreAnimationControl()
    {
        if (animator != null && !animator.enabled)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }

        if (spriteRenderer != null && defaultSprite != null && fallSprite != null && spriteRenderer.sprite == fallSprite)
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }
}
