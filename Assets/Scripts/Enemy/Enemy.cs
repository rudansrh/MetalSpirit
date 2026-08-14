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
}
