using UnityEngine;

public class BossPartHitbox : MonoBehaviour, IEnemyDamageReceiver
{
    [Header("Part Settings")]
    [SerializeField] BossWeakPointType weakPointType;   // 약점 포인트 유형
    [SerializeField] Collider2D hitboxCollider;         // 타격 범위 콜라이더
    [SerializeField] float damageMultiplier = 1f;       // 플레이어 원본 공격력에 곱해질 부위 배율
    [SerializeField] float weakPointBonusMultiplier = 2f; // 약점 활성화 시 추가 배율

    [Header("Visual Feedback")]
    [SerializeField] SpriteRenderer targetRenderer;                     // 시각적 피드백을 위한 Sprite Renderer
    [SerializeField] Color activeColor = new Color(1f, 0.4f, 0.4f, 1f); // 활성화 상태 색상
    [SerializeField] Color inactiveColor = Color.white;                 // 비활성화 상태 색상
    [SerializeField] GameObject activeMarker;                           // 활성화 상태 표시기
    [SerializeField] GameObject inactiveMarker;                         // 비활성화 상태 표시기

    BossWeakPointManager weakPointManager;
    BossController bossController;
    bool isWeakPointActive;

    public BossWeakPointType WeakPointType => weakPointType;
    public bool IsWeakPointActive => isWeakPointActive;

    void Reset()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Awake()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }

        if (bossController == null)
        {
            bossController = GetComponentInParent<BossController>();
        }
    }

    public void Configure(BossWeakPointManager manager)
    {
        weakPointManager = manager;

        if (bossController == null)
        {
            bossController = GetComponentInParent<BossController>();
        }
    }

    public void SetWeakPointActive(bool isActive)
    {
        isWeakPointActive = isActive;

        if (targetRenderer != null)
        {
            targetRenderer.color = isActive ? activeColor : inactiveColor;
        }

        if (activeMarker != null)
        {
            activeMarker.SetActive(isActive);
        }

        if (inactiveMarker != null)
        {
            inactiveMarker.SetActive(!isActive);
        }
    }

    public bool ApplyDamage(float amount, BossController bossController)
    {
        if (bossController == null || amount <= 0f)
        {
            return false;
        }

        float finalDamage = amount * Mathf.Max(0f, damageMultiplier);
        if (isWeakPointActive)
        {
            finalDamage *= Mathf.Max(1f, weakPointBonusMultiplier);
        }

        if (finalDamage <= 0f)
        {
            return false;
        }

        bossController.ApplyDamage(finalDamage);
        Debug.Log(
            $"Boss part hit: {weakPointType}, base={amount:0.##}, multiplier={damageMultiplier:0.##}, " +
            $"weakPoint={(isWeakPointActive ? "x" + weakPointBonusMultiplier.ToString("0.##") : "x1")}, " +
            $"final={finalDamage:0.##}");
        return true;
    }

    public void Attacked(float playerDamage)
    {
        ApplyDamage(playerDamage, bossController);
    }

    void OnValidate()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider2D>();
        }

        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        weakPointBonusMultiplier = Mathf.Max(1f, weakPointBonusMultiplier);
    }
}
