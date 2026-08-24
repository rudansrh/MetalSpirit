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
    [SerializeField] private float damagePerSecond = 5f; // 초당 데미지

    [Header("Alpha Settings")]
    [SerializeField] private float activeAlpha = 0.75f;

    private float timer;
    private float damageTimer;
    private bool isActive;

    private Collider2D platformCollider;
    private SpriteRenderer spriteRenderer;

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

    private void SetPlatformState(bool nextState)
    {
        isActive = nextState;
        timer = 0f;

        platformCollider.enabled = isActive;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            // 초당 데미지 적용
            damageable.TakeDamage(damagePerSecond * Time.deltaTime, DamageType.Water);

            Debug.Log($"Damage applied to {other.gameObject.name}: {damagePerSecond * Time.deltaTime} damage.");
        }
    }

    private void SetAlpha(float alpha)
    {
        spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
    }
}
