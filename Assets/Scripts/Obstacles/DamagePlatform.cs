using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DamagePlatform : MonoBehaviour
{
    [Header("Cycle Settings")]
    [SerializeField] private float activeTime = 2f;
    [SerializeField] private float inactiveTime = 2f;
    [SerializeField] private float alphaLerpSpeed = 8f;

    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 2f;
    [SerializeField] private float damageInterval = 1f;

    [Header("Alpha Settings")]
    [SerializeField] private float activeAlpha = 0.75f;

    private float timer;
    private float damageTimer;
    private bool isActive;

    private Collider2D platformCollider;
    private SpriteRenderer spriteRenderer;
    private IDamageable currentDamageable;
    private PlayerController currentPlayerController;
    private PlayerAbilityManager currentAbilityManager;

    private Color baseColor;

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer.color;
    }

    private void Start()
    {
        isActive = false;
        timer = 0f;
        damageTimer = 0f;

        SetAlpha(0f);
        platformCollider.enabled = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float targetAlpha = isActive ? activeAlpha : 0f;
        float nextAlpha = Mathf.Lerp(spriteRenderer.color.a, targetAlpha, alphaLerpSpeed * Time.deltaTime);
        SetAlpha(nextAlpha);

        if (isActive)
        {
            if (currentDamageable != null)
            {
                damageTimer += Time.deltaTime;

                if (damageTimer >= damageInterval)
                {
                    damageTimer -= damageInterval;
                    ApplyDamage();
                }
            }

            if (timer >= activeTime)
            {
                SetPlatformState(false);
            }

            return;
        }

        if (timer >= inactiveTime)
        {
            SetPlatformState(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CachePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        CachePlayer(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (currentPlayerController != null && collision.gameObject == currentPlayerController.gameObject)
        {
            ClearPlayer();
        }
    }

    private void SetPlatformState(bool nextState)
    {
        isActive = nextState;
        timer = 0f;

        platformCollider.enabled = isActive;

        if (!isActive)
        {
            ClearPlayer();
        }
    }

    private void CachePlayer(GameObject target)
    {
        if (!isActive)
        {
            return;
        }

        if (!target.TryGetComponent<PlayerController>(out var playerController))
        {
            return;
        }

        if (!target.TryGetComponent<IDamageable>(out var damageable))
        {
            return;
        }

        currentPlayerController = playerController;
        currentDamageable = damageable;
        currentAbilityManager = target.GetComponent<PlayerAbilityManager>();
    }

    private void ClearPlayer()
    {
        currentDamageable = null;
        currentPlayerController = null;
        currentAbilityManager = null;
        damageTimer = 0f;
    }

    private void ApplyDamage()
    {
        if (currentDamageable == null || currentPlayerController == null)
        {
            return;
        }

        if (currentPlayerController.isInvincibility)
        {
            return;
        }

        if (currentAbilityManager != null && currentAbilityManager.isSoul)
        {
            return;
        }

        currentDamageable.TakeDamage(damageAmount, DamageType.Water);
    }

    private void SetAlpha(float alpha)
    {
        spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
