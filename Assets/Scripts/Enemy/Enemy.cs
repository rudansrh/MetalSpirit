using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IEnemyDamageReceiver
{
    [SerializeField] protected float facingDirection = -1f;
    [Header("Hit Flash Settings")]
    [SerializeField] private SpriteRenderer[] hitFlashRenderers;
    [SerializeField] private Color hitFlashColor = new Color32(201, 190, 172, 150);
    [SerializeField] private float hitFlashDuration = 0.2f;

    public bool isPossessed = false;
    public GameObject nearbyEnemy;

    protected bool isDying = false;
    protected EnemyAnimationController animationController;
    private Coroutine hitFlashCoroutine;
    private Color[] hitFlashOriginalColors;

    protected PlayerController playerController;
    protected PlayerAbilityManager playerAbility;

    private void Awake()
    {
        playerController = PlayerController.Instance;
        playerAbility = playerController.GetComponent<PlayerAbilityManager>();
    }

    protected void InitializeEnemyBase()
    {
        animationController = GetComponent<EnemyAnimationController>();
        EnsureHitFlashSetup();
        facingDirection = transform.localScale.x < 0f ? 1f : -1f;
        UpdateFacingVisual();
    }

    private void OnEnable()
    {
        EnsureHitFlashSetup();
        RestoreHitFlashVisuals();
    }

    private void OnDisable()
    {
        RestoreHitFlashVisuals();
        hitFlashCoroutine = null;
    }

    public void SetPossessed(bool possessed)
    {
        if (isPossessed == possessed)
        {
            return;
        }

        isPossessed = possessed;
        animationController?.TriggerStun();
    }

    protected void UpdateFacingVisual()
    {
        Vector3 localScale = transform.localScale;
        float absX = Mathf.Abs(localScale.x);
        localScale.x = facingDirection > 0f ? -absX : absX;
        transform.localScale = localScale;
    }

    protected void PlayHitFlash()
    {
        EnsureHitFlashSetup();

        if (hitFlashRenderers == null || hitFlashRenderers.Length == 0 || hitFlashDuration <= 0f)
        {
            return;
        }

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
            RestoreHitFlashVisuals();
        }

        CacheHitFlashColors();
        SetHitFlashColor(hitFlashColor);
        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    protected void CancelHitFlash()
    {
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
        }

        RestoreHitFlashVisuals();
    }

    private IEnumerator HitFlashRoutine()
    {
        yield return new WaitForSeconds(hitFlashDuration);
        RestoreHitFlashVisuals();
        hitFlashCoroutine = null;
    }

    private void EnsureHitFlashSetup()
    {
        if (hitFlashRenderers == null || hitFlashRenderers.Length == 0)
        {
            hitFlashRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if (hitFlashOriginalColors == null || hitFlashOriginalColors.Length != hitFlashRenderers.Length)
        {
            CacheHitFlashColors();
        }
    }

    private void CacheHitFlashColors()
    {
        if (hitFlashRenderers == null)
        {
            hitFlashOriginalColors = null;
            return;
        }

        hitFlashOriginalColors = new Color[hitFlashRenderers.Length];
        for (int i = 0; i < hitFlashRenderers.Length; i++)
        {
            hitFlashOriginalColors[i] = hitFlashRenderers[i] != null
                ? hitFlashRenderers[i].color
                : Color.white;
        }
    }

    private void RestoreHitFlashVisuals()
    {
        if (hitFlashRenderers == null || hitFlashOriginalColors == null)
        {
            return;
        }

        int count = Mathf.Min(hitFlashRenderers.Length, hitFlashOriginalColors.Length);
        for (int i = 0; i < count; i++)
        {
            if (hitFlashRenderers[i] == null)
            {
                continue;
            }

            hitFlashRenderers[i].color = hitFlashOriginalColors[i];
        }
    }

    private void SetHitFlashColor(Color color)
    {
        if (hitFlashRenderers == null)
        {
            return;
        }

        for (int i = 0; i < hitFlashRenderers.Length; i++)
        {
            if (hitFlashRenderers[i] == null)
            {
                continue;
            }

            hitFlashRenderers[i].color = color;
        }
    }

    public abstract void Attacked(float playerDamage);

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(isPossessed && collision.gameObject.tag == "Wall")
        {
            playerController.isWallAttatching = true;
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (isPossessed && playerController.isJump && collision.gameObject.tag == "Wall")
        {
            playerController.UpdateWallClimbDetachDirection(collision);
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.1f)
                {
                    playerController.isJump = false;
                    return;
                }
            }
        }
    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (isPossessed && collision.gameObject.tag == "Wall")
            playerController.isWallAttatching = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPossessed) return;

        // 아이템 등 상호작용 객체 감지 로직
        playerController.touchInteractable(collision);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (!isPossessed) return;

        // 아이템 등 상호작용 객체 감지 로직
        playerController.fallFromInteractable(collision);
    }
}
